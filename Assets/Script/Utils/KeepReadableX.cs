using UnityEngine;

public class KeepReadableX : MonoBehaviour
{
    private Vector3 _base;

    private void Awake()
    {
        _base = transform.localScale;
        _base.x = Mathf.Abs(_base.x);
    }

    private void LateUpdate()
    {
        if (transform.parent == null) return;

        float parentSign = Mathf.Sign(transform.parent.lossyScale.x);
        if (parentSign == 0f) parentSign = 1f;

        // Make world X scale positive by matching parent's sign
        Vector3 s = _base;
        s.x *= parentSign;
        transform.localScale = s;
    }
}
