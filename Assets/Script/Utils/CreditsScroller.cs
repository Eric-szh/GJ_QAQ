using UnityEngine;
using UnityEngine.UI;

public class CreditsScroller : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public RectTransform viewport;   // Masked window
    public RectTransform content;    // Holds all credit items (usually child of viewport)

    [Header("Settings")]
    public float speed = 120f;            // Pixels per second
    public bool loop = false;             // Loop when finished
    public bool useUnscaledTime = true;   // Unaffected by Time.timeScale
    public float endPadding = 0f;         // Extra space after fully leaving the viewport

    [Header("Start Behavior")]
    public bool startCentered = true;     // Start with content centered in viewport
    public float startYOffset = 0f;       // Additional offset applied at start (optional)

    private bool _running;

    private readonly Vector3[] _vCorners = new Vector3[4];
    private readonly Vector3[] _cCorners = new Vector3[4];

    private void OnEnable()
    {
        Prepare();
    }

    public void Run()
    {
        _running = true;
    }

    public void Stop()
    {
        _running = false;
    }

    public void Prepare()
    {
        if (viewport == null || content == null)
        {
            Debug.LogError("CreditsScroller: viewport/content not assigned.");
            return;
        }

        // Ensure UI layout is up to date (important for LayoutGroup/TMP/ContentSizeFitter)
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // Optional: if you rely on LayoutGroup preferred size, you can force the height:
        // float preferredH = LayoutUtility.GetPreferredHeight(content);
        // if (preferredH > 0f) content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredH);
        // LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // Place content at start position
        if (startCentered)
        {
            CenterContentInViewport();
        }
        else
        {
            // If not centered, just keep current anchoredPosition as baseline
        }

        // Apply optional extra start offset
        var p = content.anchoredPosition;
        p.y += startYOffset;
        content.anchoredPosition = p;
    }

    private void CenterContentInViewport()
    {
        // Compute centers in world space then convert delta to viewport local space
        viewport.GetWorldCorners(_vCorners);
        content.GetWorldCorners(_cCorners);

        Vector3 viewportCenterW = (_vCorners[0] + _vCorners[2]) * 0.5f;
        Vector3 contentCenterW = (_cCorners[0] + _cCorners[2]) * 0.5f;

        // Convert world delta into viewport local delta (content parent is typically viewport)
        float deltaYLocal = viewport.InverseTransformPoint(viewportCenterW).y
                          - viewport.InverseTransformPoint(contentCenterW).y;

        var p = content.anchoredPosition;
        p.y += deltaYLocal;
        content.anchoredPosition = p;
    }

    private void Update()
    {
        if (!_running || viewport == null || content == null) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        var p = content.anchoredPosition;
        p.y += speed * dt;
        content.anchoredPosition = p;

        if (IsContentFullyAboveViewport())
        {
            if (loop)
            {
                Prepare();   // Re-center (or re-apply your chosen start behavior)
                _running = true;
            }
            else
            {
                _running = false;
            }
        }
    }

    private bool IsContentFullyAboveViewport()
    {
        // Calculate bounds of 'content' including all children, in viewport local space
        Bounds b = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, content);

        // Viewport top edge in its own local space
        float viewportTopY = viewport.rect.yMax;

        // b.min.y is the bottom of the whole content (including children) in viewport local space
        return b.min.y >= (viewportTopY + endPadding);
    }
}
