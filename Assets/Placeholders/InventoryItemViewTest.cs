using UnityEngine;
using UnityEngine.InputSystem;

public class ElephantInvViewTester : MonoBehaviour
{
    [SerializeField] private InventoryItemView view;

    private Keyboard kb;

    void Awake()
    {
        kb = Keyboard.current;
    }

    void Start()
    {
        if (view == null)
        {
            Debug.LogError("ElephantInvViewTester: view is null. Drag ElephantInvView in inspector.");
            return;
        }

        // 初始：挂 200,201
        view.UpdateView(new int[] { 200, 201 });
    }

    void Update()
    {
        kb ??= Keyboard.current;
        if (kb == null || view == null) return;

        // 1: [200, 201]
        if (kb.digit1Key.wasPressedThisFrame)
            view.UpdateView(new int[] { 200, 201 });

        // 2: [201, 202, 203]
        if (kb.digit2Key.wasPressedThisFrame)
            view.UpdateView(new int[] { 201, 202, 203 });

        // 3: 空列表（全部卸下）
        if (kb.digit3Key.wasPressedThisFrame)
            view.UpdateView(ArrayEmpty);

        // 4: null（等价于空：全部卸下）
        if (kb.digit4Key.wasPressedThisFrame)
            view.UpdateView(null);
    }

    private static readonly int[] ArrayEmpty = new int[0];
}
