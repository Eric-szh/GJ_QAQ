using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class RatController : MonoBehaviour
{
    public float speed = 4f;

    public Vector2 MoveDirWorld { get; private set; } = Vector2.right; // 当前运动方向（世界坐标）
    public bool IsBackingUp { get; private set; } = false;             // 是否在后退

    [Header("Graph")]
    public PathNode startNode;
    public float nodeArriveRadius = 0.06f;

    [Header("Direction matching")]
    [Range(-1f, 1f)] public float acceptDot = 0.7f;          // 节点选边时：方向匹配阈值（直角路口可用 0.7~0.9）
    [Range(-1f, 1f)] public float moveAlignThreshold = 0.7f; // 边上推进时：输入必须沿线段方向（直角路口可用 0.7~0.9）

    Rigidbody2D rb;

    PathNode prevNode;
    PathNode currNode;
    PathNode nextNode;
    float edgeT; // 0..1 在 curr->next 上的位置

    // 记录按键最近按下时间（用于“新按下优先”）
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

    void Update()
    {
        UpdateKeyTimes();
    }

    void FixedUpdate()
    {
        if (!currNode) return;

        // 没有目标边：在节点处等待输入并选边
        if (!nextNode)
        {
            if (AnyMoveKeyPressed())
            {
                nextNode = ChooseNextAtNode(currNode, prevNode);
                edgeT = 0f;
            }
            return;
        }

        // 边中间可停：没按键就不推进
        if (!AnyMoveKeyPressed()) return;

        Vector2 a = currNode.Pos2D;
        Vector2 b = nextNode.Pos2D;
        float len = Vector2.Distance(a, b);
        if (len < 1e-5f) { nextNode = null; return; }

        Vector2 edgeDir = (b - a).normalized;

        // 只用“沿当前边方向/反方向”的按键来推进（垂直方向键可提前按着用于到节点转弯）
        float moveSign = GetMoveSignAlongEdge(edgeDir);
        if (Mathf.Abs(moveSign) < 0.5f) return;

        float deltaT = (speed * Time.fixedDeltaTime) / len;
        edgeT = Mathf.Clamp01(edgeT + Mathf.Sign(moveSign) * deltaT);

        Vector2 pos = Vector2.Lerp(a, b, edgeT);





        rb.MovePosition(pos);

        // 回到 currNode 端：清空 nextNode，允许立刻重新选（解决你说的“回头不能重新选”）
        if (edgeT <= 0f + 1e-6f)
        {
            rb.position = currNode.Pos2D;
            nextNode = null;
            edgeT = 0f;
            return;
        }

        // 到达 nextNode 端：更新节点，清空 nextNode，然后立刻按当前按键选下一条（转弯优先逻辑在 ChooseNextAtNode）
        if (edgeT >= 1f - 1e-6f || Vector2.Distance(pos, b) <= nodeArriveRadius)
        {
            prevNode = currNode;
            currNode = nextNode;
            rb.position = currNode.Pos2D;

            nextNode = null;
            edgeT = 0f;

            // 如果仍按着键，立即选下一条边（不会在节点“顿一下”）
            if (AnyMoveKeyPressed())
                nextNode = ChooseNextAtNode(currNode, prevNode);

            return;
        }
    }

    // -------- 输入：记录“是否按住 + 最近按下时间” --------

    void UpdateKeyTimes()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float now = Time.unscaledTime;

        if (kb.wKey.wasPressedThisFrame) tW = now;
        if (kb.aKey.wasPressedThisFrame) tA = now;
        if (kb.sKey.wasPressedThisFrame) tS = now;
        if (kb.dKey.wasPressedThisFrame) tD = now;

        // 如果按住进入 Play 或焦点切换导致没触发 wasPressedThisFrame，补一次
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

    // 返回 +1 表示沿 edgeDir 正向走，-1 表示反向走，0 表示不走
    float GetMoveSignAlongEdge(Vector2 edgeDir)
    {
        var kb = Keyboard.current;
        if (kb == null) return 0f;

        // 候选方向：只看四向按键
        // 在“能沿边走”的候选中，选最适合的（同适合度时选最近按下的）
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

        float dot = Vector2.Dot(edgeDir, dir); // 正向>0，反向<0
        float score = Mathf.Abs(dot);

        if (score < moveAlignThreshold) return; // 不够沿边，就不用于推进

        // 先比“沿边程度”，再比“最近按下”
        if (score > bestScore + 1e-4f || (Mathf.Abs(score - bestScore) <= 1e-4f && time > bestTime))
        {
            bestScore = score;
            bestTime = time;
            bestSign = Mathf.Sign(dot);
        }
    }

    // -------- 节点选边：转弯优先于直走 --------

    PathNode ChooseNextAtNode(PathNode node, PathNode from)
    {
        if (node.neighbors == null || node.neighbors.Count == 0) return null;

        Vector2 incomingDir = Vector2.zero;
        if (from) incomingDir = (node.Pos2D - from.Pos2D).normalized;

        // 找“直走边”（最接近 incomingDir 的那条）。起点没有 from 时，直走概念不成立。
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

        // 根据“正在按住”的方向键，找可走的候选边
        // 规则：如果存在 非直走 候选，则只在这些候选里选（转弯优先）；否则才选直走/其它
        PathNode bestTurn = null;
        float bestTurnTime = -999f;
        float bestTurnDot = -999f;

        PathNode bestAny = null;
        float bestAnyTime = -999f;
        float bestAnyDot = -999f;

        var kb = Keyboard.current;
        if (kb == null) return null;

        EvaluateNodeCandidate(kb.wKey.isPressed, Vector2.up, tW, node, straight, ref bestTurn, ref bestTurnTime, ref bestTurnDot, ref bestAny, ref bestAnyTime, ref bestAnyDot);
        EvaluateNodeCandidate(kb.aKey.isPressed, Vector2.left, tA, node, straight, ref bestTurn, ref bestTurnTime, ref bestTurnDot, ref bestAny, ref bestAnyTime, ref bestAnyDot);
        EvaluateNodeCandidate(kb.sKey.isPressed, Vector2.down, tS, node, straight, ref bestTurn, ref bestTurnTime, ref bestTurnDot, ref bestAny, ref bestAnyTime, ref bestAnyDot);
        EvaluateNodeCandidate(kb.dKey.isPressed, Vector2.right, tD, node, straight, ref bestTurn, ref bestTurnTime, ref bestTurnDot, ref bestAny, ref bestAnyTime, ref bestAnyDot);

        if (bestTurn != null) return bestTurn;
        return bestAny;
    }

    void EvaluateNodeCandidate(bool pressed, Vector2 desiredDir, float keyTime, PathNode node, PathNode straight,
                               ref PathNode bestTurn, ref float bestTurnTime, ref float bestTurnDot,
                               ref PathNode bestAny, ref float bestAnyTime, ref float bestAnyDot)
    {
        if (!pressed) return;

        // 找“最符合 desiredDir 的邻居”
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

        bool isTurn = (straight != null && best != straight); // 有 straight 的情况下，非 straight 就算“转弯”

        // 选择标准：优先“最近按下”，同时间戳再比 bestDot
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
