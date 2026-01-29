using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrunkLine : MonoBehaviour
{
    public Transform targetTip;              // 鼻头
    public Transform root;                   // 根部（可不填）

    public float minPointDistance = 0.08f;   // 前进采样间隔
    public float minSegmentLength = 0.02f;   // 渲染时过滤很短的段，避免几何异常

    public float tileWorldLength = 0.5f;     // 纹理重复周期（世界长度）
    public bool invertScroll = false;

    [Header("Merge at junctions")]
    public float snapToExistingRadius = 0.12f; // 回到旧点时吸附半径（建议略大于minPointDistance）
    public bool snapToExisting = true;

    [Header("LineRenderer")]
    public int cornerVerts = 0;              // 绳子纹理建议0，否则拐角会扇形拉伸
    public int capVerts = 0;                 // 建议0，绳头用单独Sprite更好看

    [Header("Retract/Extend detection")]
    public float lengthEps = 0.0005f;        // 防抖阈值：长度变化小于它就当作没变

    LineRenderer lr;
    Material matInstance;
    string texProp = "_MainTex";

    // 给鼻头用：是否在回头（收回）——用长度变化判定，更稳
    public bool IsRetracting { get; private set; } = false;

    // 给鼻头用：末段外向方向（倒数第二点 -> 鼻头）
    public Vector2 OutwardDir { get; private set; } = Vector2.right;

    // 给鼻头用：鼻头本帧真实运动方向（世界坐标，单位向量）
    public Vector2 TipMoveDir { get; private set; } = Vector2.right;

    // 给鼻头用：当前绳长 + 本帧绳长变化量
    public float TrunkLength { get; private set; } = 0f;
    public float TrunkLengthDelta { get; private set; } = 0f;

    Vector3 prevTipPos;

    // pts：最后一个点永远是鼻头位置（实时更新）
    readonly List<Vector3> pts = new List<Vector3>();
    readonly List<Vector3> renderPts = new List<Vector3>();

    float trunkLength = 0f;       // 内部当前绳长
    float prevTrunkLength = 0f;   // 上一帧绳长

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;

        lr.textureMode = LineTextureMode.Tile;
        lr.numCornerVertices = Mathf.Max(0, cornerVerts);
        lr.numCapVertices = Mathf.Max(0, capVerts);

        if (lr.material != null)
        {
            matInstance = new Material(lr.material);
            lr.material = matInstance;

            if (matInstance.HasProperty("_BaseMap")) texProp = "_BaseMap";
            else if (matInstance.HasProperty("_MainTex")) texProp = "_MainTex";
        }

        ApplyTextureDensity();

        Vector3 rootPos = root ? root.position : transform.position;

        pts.Clear();
        pts.Add(rootPos);
        pts.Add(rootPos); // tip点，从0开始

        prevTipPos = targetTip ? targetTip.position : rootPos;

        trunkLength = 0f;
        prevTrunkLength = 0f;
        TrunkLength = 0f;
        TrunkLengthDelta = 0f;

        IsRetracting = false;
        OutwardDir = Vector2.right;
        TipMoveDir = Vector2.right;

        ApplyLine();
        UpdateTextureScroll();
    }

    void OnValidate()
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        ApplyTextureDensity();
    }

    void Update()
    {
        if (!targetTip) return;

        // 先记录真实位移（不依赖pts是否插点）
        Vector3 rawTipPos = targetTip.position;
        Vector2 tipDelta = (Vector2)(rawTipPos - prevTipPos);
        if (tipDelta.sqrMagnitude > 1e-8f)
            TipMoveDir = tipDelta.normalized;

        prevTipPos = rawTipPos;

        // 更新路径
        Vector3 tipPos = rawTipPos;
        pts[pts.Count - 1] = tipPos;

        if (snapToExisting)
        {
            if (TrySnapAndTruncate(ref tipPos))
                pts[pts.Count - 1] = tipPos;
        }

        // 采样点插入（足够远才插）
        Vector3 lastSample = pts[Mathf.Max(0, pts.Count - 2)];
        if ((tipPos - lastSample).sqrMagnitude >= minPointDistance * minPointDistance)
            pts.Insert(pts.Count - 1, tipPos);

        // 直线回头删点（让路径真正缩短）
        TrimBacktrackingColinear();

        // 更新外向方向：倒数第二点 -> 鼻头
        if (pts.Count >= 2)
        {
            Vector2 seg = (Vector2)(pts[pts.Count - 1] - pts[pts.Count - 2]);
            if (seg.sqrMagnitude > 1e-6f)
                OutwardDir = seg.normalized;
        }

        // 计算绳长 + 绳长变化量
        trunkLength = GetLength(pts);
        TrunkLengthDelta = trunkLength - prevTrunkLength;
        prevTrunkLength = trunkLength;

        TrunkLength = trunkLength;

        // 回头/收回：只看绳长是否变短（避免方向法在路口抖）
        if (TrunkLengthDelta < -lengthEps) IsRetracting = true;
        else if (TrunkLengthDelta > lengthEps) IsRetracting = false;
        // 在 (-eps, eps) 范围内保持上一帧状态也可以；如果你希望停住时算“非收回”，用下一行替换：
        // else IsRetracting = false;

        ApplyLine();
        UpdateTextureScroll();
    }

    bool TrySnapAndTruncate(ref Vector3 tipPos)
    {
        int bestIdx = -1;
        float bestDistSqr = snapToExistingRadius * snapToExistingRadius;

        for (int i = 0; i < pts.Count - 1; i++)
        {
            float d2 = (pts[i] - tipPos).sqrMagnitude;
            if (d2 <= bestDistSqr)
            {
                bestDistSqr = d2;
                bestIdx = i;
            }
        }

        if (bestIdx < 0) return false;

        Vector3 snapPoint = pts[bestIdx];

        pts.RemoveRange(bestIdx + 1, pts.Count - (bestIdx + 1));
        pts.Add(snapPoint);

        tipPos = snapPoint;
        return true;
    }

    void TrimBacktrackingColinear()
    {
        const float colinearDot = 0.995f;

        while (pts.Count >= 3)
        {
            Vector3 B = pts[pts.Count - 3];
            Vector3 A = pts[pts.Count - 2];
            Vector3 T = pts[pts.Count - 1];

            Vector3 BA = A - B;
            Vector3 AT = T - A;

            if (BA.sqrMagnitude < 1e-8f || AT.sqrMagnitude < 1e-8f) break;

            Vector3 dir = BA.normalized;
            float col = Mathf.Abs(Vector3.Dot(dir, AT.normalized));
            float ahead = Vector3.Dot(dir, AT);

            if (col >= colinearDot && ahead < 0f)
            {
                pts.RemoveAt(pts.Count - 2);
                continue;
            }
            break;
        }
    }

    void ApplyLine()
    {
        renderPts.Clear();

        for (int i = 0; i < pts.Count; i++)
        {
            Vector3 p = pts[i];

            if (renderPts.Count == 0)
            {
                renderPts.Add(p);
                continue;
            }

            Vector3 prev = renderPts[renderPts.Count - 1];
            if (Vector3.Distance(prev, p) < minSegmentLength) continue;

            renderPts.Add(p);
        }

        if (renderPts.Count < 2)
        {
            Vector3 rp = root ? root.position : transform.position;
            renderPts.Clear();
            renderPts.Add(rp);
            renderPts.Add(rp);
        }

        lr.positionCount = renderPts.Count;
        lr.SetPositions(renderPts.ToArray());
    }

    void ApplyTextureDensity()
    {
        if (!lr) return;

        float repeatPerUnit = (tileWorldLength <= 1e-4f) ? 1f : (1f / tileWorldLength);
        lr.textureScale = new Vector2(repeatPerUnit, 1f);

        if (matInstance)
            matInstance.SetTextureScale(texProp, Vector2.one);
    }

    void UpdateTextureScroll()
    {
        if (!matInstance) return;

        float repeatPerUnit = (tileWorldLength <= 1e-4f) ? 1f : (1f / tileWorldLength);

        float scrollUV = trunkLength * repeatPerUnit;
        if (invertScroll) scrollUV = -scrollUV;

        matInstance.SetTextureOffset(texProp, new Vector2(-scrollUV, 0f));
    }

    static float GetLength(List<Vector3> p)
    {
        float sum = 0f;
        for (int i = 1; i < p.Count; i++)
            sum += Vector3.Distance(p[i - 1], p[i]);
        return sum;
    }

    public void ResetTrunk()
    {
        Vector3 rootPos = root ? root.position : transform.position;

        pts.Clear();
        pts.Add(rootPos);
        pts.Add(rootPos);

        prevTipPos = targetTip ? targetTip.position : rootPos;

        trunkLength = 0f;
        prevTrunkLength = 0f;
        TrunkLength = 0f;
        TrunkLengthDelta = 0f;

        IsRetracting = false;
        OutwardDir = Vector2.right;
        TipMoveDir = Vector2.right;

        ApplyLine();
        UpdateTextureScroll();
    }
}
