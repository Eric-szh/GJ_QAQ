using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RatDirection : MonoBehaviour
{
    public TrunkLine trunkLine;

    // 防抖阈值：长度变化小于这个就不更新朝向
    public float lengthEps = 0.0005f;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (!trunkLine) return;

        float dLen = trunkLine.TrunkLengthDelta;

        // 绳长几乎没变：不更新
        if (Mathf.Abs(dLen) <= lengthEps) return;

        Vector2 moveDir = trunkLine.TipMoveDir;
        if (moveDir.sqrMagnitude < 1e-6f) return;
        moveDir.Normalize();

        // 伸出去：朝向=运动方向；收回：朝向=运动方向反向
        Vector2 faceDir = (dLen > 0f) ? moveDir : -moveDir;

        // 规则：上下 -> 转90度；左右 -> flip
        if (Mathf.Abs(faceDir.y) > Mathf.Abs(faceDir.x))
        {
            sr.flipX = false;
            float z = (faceDir.y > 0f) ? 90f : -90f;
            transform.localRotation = Quaternion.Euler(0f, 0f, z);
        }
        else
        {
            transform.localRotation = Quaternion.identity;
            sr.flipX = (faceDir.x < 0f);
        }
    }
}
