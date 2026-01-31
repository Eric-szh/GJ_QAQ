using UnityEngine;
using UnityEngine.Events;

public class EventInteract : MonoBehaviour, IInteractable
{
    [SerializeField] private UnityEvent interactEvent;
    [SerializeField] private bool invokeOnlyOnce = false;
    private bool hasBeenInvoked = false;
    [SerializeField] protected PlayerCtrller playerCtrl;

    public Sprite hightlight;
    private Sprite orignalSprite;

    protected void Reset()
    {
        playerCtrl = GameObject.FindWithTag("Player").GetComponent<PlayerCtrller>();
    }   
    public virtual void Interact(GameObject interactor)
    {
        if (invokeOnlyOnce && hasBeenInvoked)
        {
            return;
        }
        hasBeenInvoked = AcutalInteract();
        if (hasBeenInvoked)
        {
            // change interactable in playerCtrl to be null
            playerCtrl.SetCurrentInteractable(null);
        }

    }

    protected virtual bool AcutalInteract()
    // return true if the function passes all checks
    {
        interactEvent.Invoke();
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == playerCtrl.gameObject && !hasBeenInvoked)
        {
            playerCtrl.SetCurrentInteractable(this);
            // if it have higlight, change sprite to hightlight
            if (hightlight != null)
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    orignalSprite = sr.sprite;
                    sr.sprite = hightlight;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == playerCtrl.gameObject)
        {
            // check if this is the interactable
            if ((object)playerCtrl.currentInteractable == (object)this)
            {
                playerCtrl.SetCurrentInteractable(null);
                // if it have higlight, change sprite back to orignal
                if (hightlight != null)
                {
                    SpriteRenderer sr = GetComponent<SpriteRenderer>();
                    if (sr != null && orignalSprite != null)
                    {
                        sr.sprite = orignalSprite;
                    }
                }
        }
        }
    }
}
