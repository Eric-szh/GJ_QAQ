using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    public Sprite openSprite;
    public UnityEvent onOpen;
    public int wrongSoundID;
    public BubbleAppear bubble;

    [SerializeField] private SpriteRenderer selfRenderer;      
    [SerializeField] private SpriteRenderer otherRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Wrong() {
        // Play wrong sound using wrongSoundID
        SoundManager.Instance.PlaySound(wrongSoundID, false);
        bubble.Appear();
    }

    public void Right()
    {
        if (selfRenderer == null) selfRenderer = GetComponent<SpriteRenderer>();

        if (selfRenderer != null) selfRenderer.enabled = false;  // 关自己

        if (otherRenderer != null) otherRenderer.enabled = true; // 开另一个

        onOpen?.Invoke();
    }
}
