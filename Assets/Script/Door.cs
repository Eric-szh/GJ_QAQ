using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    public Sprite openSprite;
    public UnityEvent onOpen;
    public int wrongSoundID;
    public BubbleAppear bubble;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Wrong() {
        // Play wrong sound using wrongSoundID
        SoundManager.Instance.PlaySound(wrongSoundID, false);
        bubble.Appear();
    }

    void Right()
    {
        GetComponent<SpriteRenderer>().sprite = openSprite;
        onOpen.Invoke();
    }
}
