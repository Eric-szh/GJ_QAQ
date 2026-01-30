using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItemView : MonoBehaviour
{
    [Serializable]
    public class ItemDef
    {
        public int id;
        public GameObject go; // 场景里现成的对象（挂在大象层级下或别处都行）
    }

    [Header("Library: (id -> GameObject)")]
    [SerializeField] private List<ItemDef> itemObjects = new List<ItemDef>();

    // dict of all item hanged (active now): id set
    private readonly Dictionary<int, GameObject> libDict = new Dictionary<int, GameObject>();
    private readonly HashSet<int> activeSet = new HashSet<int>();

    private void Awake()
    {
        BuildDict();
        // 可选：启动时把库里全部先关掉，避免初始状态不一致
        SetAllOff();
        activeSet.Clear();
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
        libDict.Clear();
        for (int i = 0; i < itemObjects.Count; i++)
        {
            var def = itemObjects[i];
            if (def == null || def.go == null) continue;
            libDict[def.id] = def.go;
        }
    }

    private void SetAllOff()
    {
        foreach (var kv in libDict)
        {
            if (kv.Value != null) kv.Value.SetActive(false);
        }
    }

    // public updateView(int[] itemlist)
    // itemList = 外部给的“应该挂在大象上的物品 id”
    public void UpdateView(int[] itemList)
    {
        // 目标集合
        targetSet.Clear();

        if (itemList != null)
        {
            for (int i = 0; i < itemList.Length; i++)
                targetSet.Add(itemList[i]);
        }

        // 1) 对于被移除的：activeSet - targetSet => 关掉并从 activeSet 删除
        tempRemove.Clear();
        foreach (var id in activeSet)
        {
            if (!targetSet.Contains(id))
                tempRemove.Add(id);
        }

        for (int i = 0; i < tempRemove.Count; i++)
        {
            int id = tempRemove[i];
            if (libDict.TryGetValue(id, out var go) && go != null)
                go.SetActive(false);

            activeSet.Remove(id);
        }

        // 2) 对于新增的：targetSet - activeSet => 打开并加入 activeSet
        foreach (var id in targetSet)
        {
            if (activeSet.Contains(id)) continue;

            if (libDict.TryGetValue(id, out var go) && go != null)
            {
                go.SetActive(true);
                activeSet.Add(id);
            }
            else
            {
                // 库里没有这个 id 就跳过（不报错也行；需要的话可以 Debug.LogWarning）
            }
        }
    }

    // 可选：给外部查状态
    public bool IsActive(int id) => activeSet.Contains(id);

    // 临时容器，避免 UpdateView 里反复分配
    private readonly HashSet<int> targetSet = new HashSet<int>();
    private readonly List<int> tempRemove = new List<int>();
}
