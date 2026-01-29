using UnityEngine;

public class Pickup : MonoBehaviour
{
    [Header("Refs")]
    public PickupUIView uiview;
    public PlayerMaskModel maskmodel;

    [Header("Pickup Data")]
    [SerializeField] private int itemId;
    [SerializeField] private string title;
    [SerializeField, TextArea(2, 6)] private string description;

    
    [SerializeField] private Sprite fallbackIcon;

    private SpriteRenderer sr;

    void Reset()
    {
        if (uiview == null)
            uiview = FindFirstObjectByType<PickupUIView>(FindObjectsInactive.Include);

        if (maskmodel == null)
            maskmodel = FindFirstObjectByType<PlayerMaskModel>(FindObjectsInactive.Include);

        // 自动抓自己身上的 SpriteRenderer
        sr = GetComponent<SpriteRenderer>();
    }

    void Awake()
    {
        
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (uiview == null) uiview = FindFirstObjectByType<PickupUIView>(FindObjectsInactive.Include);
        if (maskmodel == null) maskmodel = FindFirstObjectByType<PlayerMaskModel>(FindObjectsInactive.Include);
    }

    public void Collect()
    {
        Sprite icon = null;
        if (sr != null) icon = sr.sprite;
        if (icon == null) icon = fallbackIcon;

        if (uiview != null)
            uiview.UpdateView(icon, title, description);

        //Debug.Log($"PlayerMaskModel got pickup id={itemId}");
        if (maskmodel != null)
        maskmodel.getPickup(itemId);

        Destroy(gameObject);
    }
}
