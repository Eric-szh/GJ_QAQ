using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMaskView : MonoBehaviour
{
    [Serializable]
    public class MaskEntry
    {
        public int id;
        public Sprite sprite;
    }

    [SerializeField] private List<MaskEntry> masks = new();
    private Dictionary<int, Sprite> maskDict;

    private void Awake()
    {
        maskDict = new Dictionary<int, Sprite>(masks.Count);
        foreach (var e in masks)
        {
            // 防重复 key，避免运行时异常/覆盖不自知
            if (maskDict.ContainsKey(e.id))
            {
                Debug.LogWarning($"Duplicate mask id: {e.id}", this);
                continue;
            }
            maskDict.Add(e.id, e.sprite);
        }
    }

    public void UpdateMask(int maskId)
    {
        if (maskDict != null && maskDict.TryGetValue(maskId, out var s))
            GetComponent<SpriteRenderer>().sprite = s;
        else
            Debug.LogWarning($"Mask id not found: {maskId}", this);
    }
}
