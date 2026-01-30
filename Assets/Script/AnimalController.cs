using UnityEngine;

public class AnimalController : MonoBehaviour
{
    [Header("Animation")]
    public string idleAniName = "Idle";
    public string correctAniName = "Correct";

    [Header("References")]
    public BubbleAppear bubble;   // reference to the BubbleAppear script on the bubble GameObject
    public GameObject pickup;    

    [Header("Sound")]
    public int wrongSoundId = 0; // configure in Inspector; 0 = no sound

    private AniController _ani;

    void Start()
    {
        _ani = GetComponent<AniController>();
        if (_ani != null)
        {
            _ani.ChangeAnimationState(idleAniName);
        }
        else
        {
            Debug.LogWarning("AnimalController: AniController component not found on this GameObject.");
        }

        if (pickup != null)
            pickup.SetActive(false);  
    }

    // Called by ItemInteract / MaskInteract when the player is incorrect
    public void Wrong()
    {
        if (wrongSoundId != 0)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySound(wrongSoundId, false);
            else
                Debug.LogWarning("AnimalController.Wrong(): SoundManager.Instance is null.");
        }

        bubble?.Appear();
    }

    // Called by ItemInteract / MaskInteract when the player is correct
    public void Right()
    {
        if (_ani != null)
        {   
            //if now animation directly droppickup
            DropPickUp();
            // or play animation then drop pickup
            //_ani.ChangeAnimationState(correctAniName, false);
            // The correct animation should include an Animation Event that calls DropPickUp()
        }
        else
        {
            Debug.LogWarning("AnimalController.Right(): AniController component not found.");
        }
    }

    // Triggered from an animation event on the correct animation to drop the pickup
    public void DropPickUp()
    {
        if (pickup != null)
            pickup.SetActive(true);
    }
}
