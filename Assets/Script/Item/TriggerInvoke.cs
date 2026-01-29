using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class TriggerInvoke : MonoBehaviour
{
    [Header("Event")]
    [SerializeField] private UnityEvent onTriggered;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnlyOnce = true;

    
    [SerializeField] private GameObject objectToDetect;

    [Header("Runtime")]
    [SerializeField] private bool triggered = false;

    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            // 按你的前提：这个 collider 应该是 trigger
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // 1) 是否是我们要找的对象
        if (objectToDetect != null)
        {
            if (other.gameObject != objectToDetect) return;
        }
        else
        {
            // 如果没指定 objectToDetect，默认不触发（避免误触）
            return;
        }

        // 2) 是否仍需要触发
        if (triggerOnlyOnce && triggered) return;

        // 3) 标记 + invoke
        triggered = true;
        onTriggered?.Invoke();
    }

    // 可选：外部重置
    public void ResetTrigger()
    {
        triggered = false;
    }
}
