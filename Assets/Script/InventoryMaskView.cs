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

    // 每个 slot 的高亮图层（建议是 slot 的子物体 Image，平时隐藏）
    // 数量最好和 slotImages 一样；如果你不想做多张，也可以改成 Outline/单个移动框
    [SerializeField] private List<GameObject> slotHighlights = new List<GameObject>();

    [Header("Behavior")]
    [SerializeField] private bool hideEmptySlots = false; // true: 没有 mask 的 slot 直接隐藏 Image

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
        // Editor 里改了列表后，重新建字典（运行中不做）
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

    private void ClearAllSlots()
    {
        for (int i = 0; i < slotImages.Count; i++)
        {
            var img = slotImages[i];
            if (img == null) continue;

            img.sprite = null;
            img.enabled = !hideEmptySlots; // hideEmptySlots=true => enabled=false
            img.color = Color.white;
        }
    }

    // Public updateView(int[] masklist)
    // maskList: 例如 [100, 101, 102]
    public void UpdateView(int[] maskList)
    {
        if (slotImages == null || slotImages.Count == 0)
            return;

        // 先清空
        ClearAllSlots();

        if (maskList == null) return;

        int count = Mathf.Min(maskList.Length, slotImages.Count);
        for (int i = 0; i < count; i++)
        {
            int id = maskList[i];
            var img = slotImages[i];
            if (img == null) continue;

            if (spriteDict.TryGetValue(id, out var sp) && sp != null)
            {
                img.sprite = sp;
                img.enabled = true; // 有内容就显示
            }
            else
            {
                // 找不到 sprite：保持空
                img.sprite = null;
                img.enabled = !hideEmptySlots;
            }
        }
    }

    // Public updateSelection(int slot)
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

        // slot = -1 时，上面循环不会命中任何 i==slot，等价于全部 SetActive(false)
        if (slot < 0)
        {
            for (int i = 0; i < slotHighlights.Count; i++)
            {
                var hl = slotHighlights[i];
                if (hl == null) continue;
                hl.SetActive(false);
            }
        }
    }
}
