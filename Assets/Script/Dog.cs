using UnityEngine;

public class Dog : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The solid wall child object that blocks the player")]
    public GameObject childWall;

    [Tooltip("Reference to the player's mask model to check current mask")]
    public PlayerMaskModel playerMask;

    [Header("Sound Settings")]
    [Tooltip("The ID for the looping bark sound (starts with 30x)")]
    public int barkSoundId = 300; // Example ID

    private void Reset()
    {
        // Fix: Using FindFirstObjectByType with Inactive.Include is safer
        // because the playerMaskModel might be on a UI panel that is hidden (inactive)
        if (playerMask == null)
        {
            playerMask = Object.FindFirstObjectByType<PlayerMaskModel>(FindObjectsInactive.Include);
            if (playerMask == null) Debug.LogError("Dog Debug: Could not find PlayerMaskModel in scene!");
        }

        // Fix: Specifically look for the first child to ensure it's the 'solid wall'
        if (childWall == null && transform.childCount > 0)
        {
            childWall = transform.GetChild(0).gameObject;
            Debug.Log($"Dog Debug: Found child wall: {childWall.name}");
        }
    }

    private void Start()
    {
        // Ensure the wall is off initially unless the player is already nearby
        if (childWall != null)
            childWall.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))
        {
            
            if (playerMask != null && playerMask.checkMaskOn(000))
            {

            }
            
            else 
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySound(barkSoundId, true);
                }

               
                if (childWall != null)
                {
                    childWall.SetActive(true);
                }

            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        
        
          
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopSound(barkSoundId);
            }

            // Optionally disable the wall again so they can try again with a mask
            if (childWall != null)
            {
                childWall.SetActive(false);
            }
        
    }
}
