using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PickupUIView : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private GameObject panelRoot;     // 整个 panel
    [SerializeField] private Image iconArea;           // 放 sprite 的 Image
    [SerializeField] private TMP_Text titleArea;       // 标题
    [SerializeField] private TMP_Text descriptionArea; // 描述

    [Header("Behavior")]
    [SerializeField] private bool hideOnStart = true;

    // 防止弹出同一帧被刚才那次按键立刻关掉（可按需调小/设为0）
    [SerializeField] private float ignoreInputSeconds = 0.10f;

    private float _shownTime = -999f;

    void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject;

        if (hideOnStart)
            panelRoot.SetActive(false);
    }

    void Update()
    {
        if (panelRoot == null || !panelRoot.activeSelf) return;

        // 等待一小段时间再允许关闭（避免刚显示就被触发键关闭）
        if (Time.unscaledTime - _shownTime < ignoreInputSeconds) return;

        if (AnyInputPressedThisFrame())
        {
            panelRoot.SetActive(false);
        }
    }

    // updateView(sprite img, str title, str description)
    public void UpdateView(Sprite img, string title, string description)
    {
        if (iconArea != null)
        {
            iconArea.sprite = img;
            iconArea.enabled = (img != null);
        }

        if (titleArea != null)
            titleArea.text = title ?? "";

        if (descriptionArea != null)
            descriptionArea.text = description ?? "";

        panelRoot.SetActive(true);
        _shownTime = Time.unscaledTime;
    }

    private bool AnyInputPressedThisFrame()
    {
        // Keyboard
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        // Mouse buttons
        if (Mouse.current != null &&
            (Mouse.current.leftButton.wasPressedThisFrame ||
             Mouse.current.rightButton.wasPressedThisFrame ||
             Mouse.current.middleButton.wasPressedThisFrame ||
             Mouse.current.backButton.wasPressedThisFrame ||
             Mouse.current.forwardButton.wasPressedThisFrame))
            return true;



        return false;
    }
}
