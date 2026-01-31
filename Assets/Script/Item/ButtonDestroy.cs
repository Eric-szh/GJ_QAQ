using UnityEngine;

public class ButtonDestroy : MonoBehaviour
{
    [SerializeField] private GameObject target;   // 要销毁的对象
    [SerializeField] private float delay = 0f;    // 可选：延迟销毁

    // 在 Button 的 OnClick() 里拖进来调用这个
    public void DestroyTarget()
    {
        if (target == null) return;
        Destroy(target, delay);
    }

    // 如果你想点按钮把“自己”也销毁
    public void DestroySelf()
    {
        Destroy(gameObject, delay);
    }
}