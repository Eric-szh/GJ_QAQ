using UnityEngine;
using UnityEngine.Events;

public class EventInteract : MonoBehaviour, IInteractable
{
    [SerializeField] private UnityEvent interactEvent;
    [SerializeField] private bool invokeOnlyOnce = false;
    private bool hasBeenInvoked = false;
    [SerializeField] protected PlayerCtrller playerCtrl;
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
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == playerCtrl.gameObject)
        {
            // check if this is the interactable
            if (playerCtrl.currentInteractable == this)
            {
                playerCtrl.SetCurrentInteractable(null);
            }
        }
    }
}
