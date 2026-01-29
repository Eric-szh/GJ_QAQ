using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryMaskViewTest : MonoBehaviour
{
    [SerializeField] private InventoryMaskView view;

    private Keyboard kb;

    void Awake()
    {
        kb = Keyboard.current;
    }

    void Start()
    {
        if (view == null)
        {
            Debug.LogError("MaskRowViewTester: view is null. Drag your MaskRowView into the inspector.");
            return;
        }

        // 初始显示
        view.UpdateView(new int[] { 100, 101, 102 });
        view.UpdateSelection(-1);
    }

    void Update()
    {
        kb ??= Keyboard.current;
        if (kb == null || view == null) return;

        // 1: 显示 [100, 101, 102]
        if (kb.digit1Key.wasPressedThisFrame)
            view.UpdateView(new int[] { 100, 101, 102 });

        // 2: 显示 [102, 101, 100]
        if (kb.digit2Key.wasPressedThisFrame)
            view.UpdateView(new int[] { 102, 101, 100 });

        // 3: 显示更短的列表（测试空槽）
        if (kb.digit3Key.wasPressedThisFrame)
            view.UpdateView(new int[] { 101 });

        // 4: 显示更长的列表（超过 slot 数会自动截断）
        if (kb.digit4Key.wasPressedThisFrame)
            view.UpdateView(new int[] { 100, 101, 102, 103, 104, 105 });

        // 5: 啥也没有
        if (kb.digit5Key.wasPressedThisFrame)
            view.UpdateView(new int[] { });

        // Q/W/E/R: 选中 slot 0/1/2/3（如果你只有3格，R 选不中也不会报错，只是全不亮）
        if (kb.qKey.wasPressedThisFrame)
            view.UpdateSelection(0);

        if (kb.wKey.wasPressedThisFrame)
            view.UpdateSelection(1);

        if (kb.eKey.wasPressedThisFrame)
            view.UpdateSelection(2);

        if (kb.rKey.wasPressedThisFrame)
            view.UpdateSelection(3);

        // ESC: 取消选中
        if (kb.escapeKey.wasPressedThisFrame)
            view.UpdateSelection(-1);
    }
}
