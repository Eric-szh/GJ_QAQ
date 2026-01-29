using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PickupUIView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Refs")]
    [SerializeField] private GameObject panelRoot;     // 整个 panel
    [SerializeField] private Image iconArea;           // 放 sprite 的 Image
    [SerializeField] private TMP_Text titleArea;       // 标题
    [SerializeField] private TMP_Text descriptionArea; // 描述

    [Header("Behavior")]
    [SerializeField] private bool hideOnStart = true;

    void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject;

        if (hideOnStart)
            panelRoot.SetActive(false);
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

        // reveal self
        panelRoot.SetActive(true);
    }

    // when clicked, hide self
    public void OnPointerClick(PointerEventData eventData)
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}

