using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class RatController : MonoBehaviour
{
    public float speed = 4f;

    public Vector2 MoveDirWorld { get; private set; } = Vector2.right;
    public bool IsBackingUp { get; private set; } = false;

    [Header("Graph")]
    public PathNode startNode;
    public float nodeArriveRadius = 0.06f;

    [Header("Direction matching")]
    [Range(-1f, 1f)] public float acceptDot = 0.7f;
    [Range(-1f, 1f)] public float moveAlignThreshold = 0.7f;

    [Header("Pre-turn snap")]
    public float preTurnRadius = 0.15f; // 接近节点时允许提前拐弯吸附

    Rigidbody2D rb;

    PathNode prevNode;
    PathNode currNode;
    PathNode nextNode;
    float edgeT;

    float tW = -1f, tA = -1f, tS = -1f, tD = -1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Start()
    {
        currNode = startNode;
        prevNode = null;
        nextNode = null;
        edgeT = 0f;

        if (currNode != null)
            rb.position = currNode.Pos2D;
    }

    private void OnEnable()
    {
        Start();
    }

    void Update()
    {
        UpdateKeyTimes();
    }

    void FixedUpdate()
    {
        if (!currNode) return;

        // 在节点处：等待输入选边
        if (!nextNode)
        {
            if (AnyMoveKeyPressed())
            {
                nextNode = ChooseNextAtNode(currNode, prevNode);
                edgeT = 0f;
            }

            MoveDirWorld = Vector2.zero;
            IsBackingUp = false;
            return;
        }

        Vector2 a = currNode.Pos2D;
        Vector2 b = nextNode.Pos2D;
        float len = Vector2.Distance(a, b);
        if (len < 1e-5f) { nextNode = null; return; }

        Vector2 edgeDir = (b - a).normalized;

        // ---------- 关键：即使停住，也允许“拐弯键 + 接近节点”触发吸附 ----------
        if (AnyMoveKeyPressed())
        {
            Vector2 turnDir = GetTurnIntentDir(edgeDir); // 与当前边不一致的方向键（最新按下优先）
            if (turnDir != Vector2.zero)
            {
                float distToA = Vector2.Distance(rb.position, a);
                float distToB = Vector2.Distance(rb.position, b);

                // 优先吸附更近的端点（通常你停在路口附近时会吸到B）
                if (distToB <= preTurnRadius && distToB <= distToA + 1e-4f)
                {
                    // 吸到 nextNode（路口），然后立刻按当前按键选分支
                    prevNode = currNode;
                    currNode = nextNode;
                    rb.position = currNode.Pos2D;

                    nextNode = null;
                    edgeT = 0f;

                    nextNode = ChooseNextAtNode(currNode, prevNode);
                    return;
                }
                else if (distToA <= preTurnRadius)
                {
                    // 吸到 currNode 端点（如果你在起点端附近想转）
                    rb.position = currNode.Pos2D;

                    nextNode = null;
                    edgeT = 0f;

                    nextNode = ChooseNextAtNode(currNode, prevNode);
                    return;
                }
            }
        }
        // ---------- 关键结束 ----------

        // 边中间可停：没按键不推进
        if (!AnyMoveKeyPressed())
        {
            MoveDirWorld = Vector2.zero;
            IsBackingUp = false;
            return;
        }

        // 沿边推进（只接受沿边/反向沿边）
        float moveSign = GetMoveSignAlongEdge(edgeDir);
        if (Mathf.Abs(moveSign) < 0.5f)
        {
            MoveDirWorld = Vector2.zero;
            IsBackingUp = false;
            return;
        }

        MoveDirWorld = (moveSign > 0f) ? edgeDir : -edgeDir;
        IsBackingUp = (moveSign < 0f);

        float deltaT = (speed * Time.fixedDeltaTime) / len;
        edgeT = Mathf.Clamp01(edgeT + Mathf.Sign(moveSign) * deltaT);

        Vector2 pos = Vector2.Lerp(a, b, edgeT);
        rb.MovePosition(pos);

        // 回到 currNode 端
        if (edgeT <= 0f + 1e-6f)
        {
            rb.position = currNode.Pos2D;
            nextNode = null;
            edgeT = 0f;
            return;
        }

        // 到达 nextNode 端
        if (edgeT >= 1f - 1e-6f || Vector2.Distance(pos, b) <= nodeArriveRadius)
        {
            prevNode = currNode;
            currNode = nextNode;
            rb.position = currNode.Pos2D;

            nextNode = null;
            edgeT = 0f;

            if (AnyMoveKeyPressed())
                nextNode = ChooseNextAtNode(currNode, prevNode);

            return;
        }
    }

    // ---------------- 输入：记录“是否按住 + 最近按下时间” ----------------

    void UpdateKeyTimes()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float now = Time.unscaledTime;

        if (kb.wKey.wasPressedThisFrame) tW = now;
        if (kb.aKey.wasPressedThisFrame) tA = now;
        if (kb.sKey.wasPressedThisFrame) tS = now;
        if (kb.dKey.wasPressedThisFrame) tD = now;

        if (kb.wKey.isPressed && tW < 0f) tW = now;
        if (kb.aKey.isPressed && tA < 0f) tA = now;
        if (kb.sKey.isPressed && tS < 0f) tS = now;
        if (kb.dKey.isPressed && tD < 0f) tD = now;
    }

    bool AnyMoveKeyPressed()
    {
        var kb = Keyboard.current;
        if (kb == null) return false;
        return kb.wKey.isPressed || kb.aKey.isPressed || kb.sKey.isPressed || kb.dKey.isPressed;
    }

    float GetMoveSignAlongEdge(Vector2 edgeDir)
    {
        var kb = Keyboard.current;
        if (kb == null) return 0f;

        float bestScore = -999f;
        float bestTime = -999f;
        float bestSign = 0f;

        TryEdgeMoveCandidate(kb.wKey.isPressed, Vector2.up, tW, edgeDir, ref bestScore, ref bestTime, ref bestSign);
        TryEdgeMoveCandidate(kb.aKey.isPressed, Vector2.left, tA, edgeDir, ref bestScore, ref bestTime, ref bestSign);
        TryEdgeMoveCandidate(kb.sKey.isPressed, Vector2.down, tS, edgeDir, ref bestScore, ref bestTime, ref bestSign);
        TryEdgeMoveCandidate(kb.dKey.isPressed, Vector2.right, tD, edgeDir, ref bestScore, ref bestTime, ref bestSign);

        return bestSign;
    }

    void TryEdgeMoveCandidate(bool pressed, Vector2 dir, float time, Vector2 edgeDir,
                              ref float bestScore, ref float bestTime, ref float bestSign)
    {
        if (!pressed) return;

        float dot = Vector2.Dot(edgeDir, dir);
        float score = Mathf.Abs(dot);

        if (score < moveAlignThreshold) return;

        if (score > bestScore + 1e-4f || (Mathf.Abs(score - bestScore) <= 1e-4f && time > bestTime))
        {
            bestScore = score;
            bestTime = time;
            bestSign = Mathf.Sign(dot);
        }
    }

    // 与当前边方向“不一致”的输入方向（用于提前拐弯）。返回最新按下且不沿边的方向
    Vector2 GetTurnIntentDir(Vector2 edgeDir)
    {
        var kb = Keyboard.current;
        if (kb == null) return Vector2.zero;

        Vector2 best = Vector2.zero;
        float bestTime = -999f;

        void Consider(bool pressed, Vector2 dir, float time)
        {
            if (!pressed) return;

            float d = Mathf.Abs(Vector2.Dot(edgeDir, dir));
            if (d >= moveAlignThreshold) return; // 沿边，不算拐

            if (time > bestTime)
            {
                bestTime = time;
                best = dir;
            }
        }

        Consider(kb.wKey.isPressed, Vector2.up, tW);
        Consider(kb.aKey.isPressed, Vector2.left, tA);
        Consider(kb.sKey.isPressed, Vector2.down, tS);
        Consider(kb.dKey.isPressed, Vector2.right, tD);

        return best;
    }

    // ---------------- 节点选边：转弯优先于直走 ----------------

    PathNode ChooseNextAtNode(PathNode node, PathNode from)
    {
        if (node.neighbors == null || node.neighbors.Count == 0) return null;

        Vector2 incomingDir = Vector2.zero;
        if (from) incomingDir = (node.Pos2D - from.Pos2D).normalized;

        PathNode straight = null;
        if (from)
        {
            float best = -999f;
            foreach (var nb in node.neighbors)
            {
                if (!nb) continue;
                Vector2 v = (nb.Pos2D - node.Pos2D).normalized;
                float d = Vector2.Dot(v, incomingDir);
                if (d > best) { best = d; straight = nb; }
            }
        }

        PathNode bestTurn = null;
        float bestTurnTime = -999f;
        float bestTurnDot = -999f;

        PathNode bestAny = null;
        float bestAnyTime = -999f;
        float bestAnyDot = -999f;

        var kb = Keyboard.current;
        if (kb == null) return null;

        EvaluateNodeCandidate(kb.wKey.isPressed, Vector2.up, tW, node, straight,
            ref bestTurn, ref bestTurnTime, ref bestTurnDot,
            ref bestAny, ref bestAnyTime, ref bestAnyDot);

        EvaluateNodeCandidate(kb.aKey.isPressed, Vector2.left, tA, node, straight,
            ref bestTurn, ref bestTurnTime, ref bestTurnDot,
            ref bestAny, ref bestAnyTime, ref bestAnyDot);

        EvaluateNodeCandidate(kb.sKey.isPressed, Vector2.down, tS, node, straight,
            ref bestTurn, ref bestTurnTime, ref bestTurnDot,
            ref bestAny, ref bestAnyTime, ref bestAnyDot);

        EvaluateNodeCandidate(kb.dKey.isPressed, Vector2.right, tD, node, straight,
            ref bestTurn, ref bestTurnTime, ref bestTurnDot,
            ref bestAny, ref bestAnyTime, ref bestAnyDot);

        if (bestTurn != null) return bestTurn;
        return bestAny;
    }

    void EvaluateNodeCandidate(bool pressed, Vector2 desiredDir, float keyTime,
                               PathNode node, PathNode straight,
                               ref PathNode bestTurn, ref float bestTurnTime, ref float bestTurnDot,
                               ref PathNode bestAny, ref float bestAnyTime, ref float bestAnyDot)
    {
        if (!pressed) return;

        PathNode best = null;
        float bestDot = -999f;

        foreach (var nb in node.neighbors)
        {
            if (!nb) continue;
            Vector2 v = (nb.Pos2D - node.Pos2D).normalized;
            float d = Vector2.Dot(v, desiredDir);
            if (d > bestDot) { bestDot = d; best = nb; }
        }

        if (best == null || bestDot < acceptDot) return;

        bool isTurn = (straight != null && best != straight);

        if (isTurn)
        {
            if (keyTime > bestTurnTime || (Mathf.Abs(keyTime - bestTurnTime) <= 1e-4f && bestDot > bestTurnDot))
            {
                bestTurn = best;
                bestTurnTime = keyTime;
                bestTurnDot = bestDot;
            }
        }

        if (keyTime > bestAnyTime || (Mathf.Abs(keyTime - bestAnyTime) <= 1e-4f && bestDot > bestAnyDot))
        {
            bestAny = best;
            bestAnyTime = keyTime;
            bestAnyDot = bestDot;
        }
    }
}
