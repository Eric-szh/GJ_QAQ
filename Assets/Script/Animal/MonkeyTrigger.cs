using UnityEngine;

public class MonkeyTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";

    [Header("Open On Enter")]
    [SerializeField] private GameObject openAlways;     // 玩家进入就打开
    [SerializeField] private GameObject openIfHasItem;  // 有道具再打开
    [SerializeField] private int requiredItemId = 203;

    [Header("Optional")]
    [SerializeField] private bool closeOnExit = false;  // 出去是否关闭
    [SerializeField] private bool openOnce = false;     // 只触发一次

    private bool _used;
    private void Start()
    {

        if (openAlways != null) openAlways.SetActive(false);
        if (openIfHasItem != null) openIfHasItem.SetActive(false);

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_used && openOnce) return;
        if (!other.CompareTag(playerTag)) return;

        if (openAlways != null) openAlways.SetActive(true);

        // 从玩家身上拿 PlayerMaskModel
        var model = other.GetComponent<PlayerMaskModel>();
        if (model == null)
        {
            // 有时 collider 在子物体上（脚/身体），就从父物体找
            model = other.GetComponentInParent<PlayerMaskModel>();
        }

        if (model != null && model.checkHaveItem(requiredItemId))
        {
            if (openIfHasItem != null) openIfHasItem.SetActive(true);
        }

        if (openOnce) _used = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!closeOnExit) return;
        if (!other.CompareTag(playerTag)) return;

        if (openAlways != null) openAlways.SetActive(false);
        if (openIfHasItem != null) openIfHasItem.SetActive(false);
    }
}
