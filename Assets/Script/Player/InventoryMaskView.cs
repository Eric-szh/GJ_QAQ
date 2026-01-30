using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMaskView : MonoBehaviour
{
    [Serializable]
    public class MaskSpriteDef
    {
        public int id;
        public Sprite sprite;
    }

    [Header("Library: (id -> sprite)")]
    [SerializeField] private List<MaskSpriteDef> maskSprites = new List<MaskSpriteDef>();

    [Header("UI Slots (one row)")]
    [SerializeField] private List<Image> slotImages = new List<Image>();

    [Header("Slot Highlights (same count as slots)")]
    [SerializeField] private List<GameObject> slotHighlights = new List<GameObject>();

    [Header("Empty Slot Fallback")]
    [SerializeField] private Sprite emptySlotSprite;

    [Header("Behavior")]
    [SerializeField] private bool hideEmptySlots = false; // true: empty slot hides Image component

    private readonly Dictionary<int, Sprite> spriteDict = new Dictionary<int, Sprite>();

    private void Awake()
    {
        BuildDict();
        ClearAllSlots();
        UpdateSelection(-1);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            BuildDict();
    }
#endif

    private void BuildDict()
    {
        spriteDict.Clear();
        for (int i = 0; i < maskSprites.Count; i++)
        {
            var def = maskSprites[i];
            if (def == null || def.sprite == null) continue;
            spriteDict[def.id] = def.sprite;
        }
    }

    public void SetEmptySlotSprite(Sprite sprite)
    {
        emptySlotSprite = sprite;
        // Optional: refresh current visuals if you want immediate effect
        // ClearAllSlots();
    }

    private void ApplyEmptySlot(Image img)
    {
        if (img == null) return;

        if (hideEmptySlots)
        {
            img.sprite = null;
            img.enabled = false;
            return;
        }

        img.sprite = emptySlotSprite; // can be null, that's fine
        img.enabled = true;           // show Image even if sprite is null
        img.color = Color.white;
    }

    private void ClearAllSlots()
    {
        for (int i = 0; i < slotImages.Count; i++)
        {
            ApplyEmptySlot(slotImages[i]);
        }
    }

    // maskList: e.g. [100, 101, 102]
    public void UpdateView(int[] maskList)
    {
        if (slotImages == null || slotImages.Count == 0)
            return;

        // Reset all slots to empty state first
        ClearAllSlots();

        if (maskList == null) return;

        int count = Mathf.Min(maskList.Length, slotImages.Count);
        for (int i = 0; i < count; i++)
        {
            var img = slotImages[i];
            if (img == null) continue;

            int id = maskList[i];
            if (spriteDict.TryGetValue(id, out var sp) && sp != null)
            {
                img.sprite = sp;
                img.enabled = true;
                img.color = Color.white;
            }
            else
            {
                // Missing sprite or unknown id -> treat as empty slot
                ApplyEmptySlot(img);
            }
        }
    }

    // slot: -1 => highlight nothing
    public void UpdateSelection(int slot)
    {
        if (slotHighlights == null || slotHighlights.Count == 0)
            return;

        for (int i = 0; i < slotHighlights.Count; i++)
        {
            var hl = slotHighlights[i];
            if (hl == null) continue;
            hl.SetActive(i == slot);
        }
    }
}
