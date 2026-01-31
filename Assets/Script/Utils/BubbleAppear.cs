using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BubbleAppear : MonoBehaviour
{
    // Seconds until the bubble disappears after appearing
    public int timeToDisappear = 3;

    // Cached SpriteRenderer on this GameObject
    private SpriteRenderer _spriteRenderer;
    private Vector3 _baseLocalScale;
    private List<BubbleAppear> otherBubbles;

    private void Awake()
    {
        _baseLocalScale = transform.localScale;
        _baseLocalScale.x = Mathf.Abs(_baseLocalScale.x);

        // find all other BubbleAppear in the scene
        otherBubbles = new List<BubbleAppear>(FindObjectsByType<BubbleAppear>(FindObjectsSortMode.None));
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            Debug.LogWarning("BubbleAppear: no SpriteRenderer found on the GameObject.");
            return;
        }

        // Ensure the bubble is hidden at start by disabling the SpriteRenderer
        _spriteRenderer.enabled = false;

        // disable all child
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    void Update()
    {
    }

    // Show the bubble for timeToDisappear seconds
    public void Appear()
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                Debug.LogWarning("BubbleAppear.Appear(): no SpriteRenderer found.");
                return;
            }
        }

        _spriteRenderer.enabled = true;



        // enable all child
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        // Restart any existing disappearance timer and schedule a new one
        CancelInvoke(nameof(Disappear));
        Invoke(nameof(Disappear), (float)timeToDisappear);

        // disable all other bubble
        foreach (var bubble in otherBubbles)
        {
            if (bubble != this)
            {
                bubble.Disappear();
            }
        }
    }

    private void Disappear()
    {
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;

        // disable all child
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

}
