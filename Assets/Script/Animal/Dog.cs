using System.Collections;
using UnityEngine;

public class Dog : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The solid wall child object that blocks the player")]
    public GameObject childWall;

    [Tooltip("Reference to the player's mask model to check current mask")]
    public PlayerMaskModel playerMask;

    [SerializeField] private BubbleAppear bubble;
    [SerializeField] private BubbleAppear bubble2;

    [Header("Sound Settings")]
    [Tooltip("The ID for the looping bark sound (starts with 30x)")]
    public int barkSoundId = 300;

    [Header("Push Settings")]
    [Tooltip("How far to push the player away from the dog")]
    [SerializeField] private float pushDistance = 1.2f;

    [Tooltip("How long the push takes (seconds)")]
    [SerializeField] private float pushDuration = 0.12f;

    [Tooltip("Cooldown to prevent continuous retrigger while staying inside")]
    [SerializeField] private float pushCooldown = 0.35f;

    [Tooltip("The mask ID that makes the dog ignore the player")]
    [SerializeField] private int safeMaskId = 0; // "000" is just 0 in int

    private bool playerInside = false;
    private bool lastSafeState = true;

    private Transform playerTf;
    private Rigidbody2D playerRb;
    private PlayerCtrller playerController;

    private Coroutine pushRoutine;
    private float nextAllowedPushTime = 0f;
    private bool barking = false;

    private void Reset()
    {
        // English comments per preference.
        if (playerMask == null)
        {
            playerMask = Object.FindFirstObjectByType<PlayerMaskModel>(FindObjectsInactive.Include);
            if (playerMask == null) Debug.LogError("Dog Debug: Could not find PlayerMaskModel in scene!");
        }

        if (childWall == null && transform.childCount > 0)
        {
            childWall = transform.GetChild(0).gameObject;
            Debug.Log($"Dog Debug: Found child wall: {childWall.name}");
        }
    }

    private void Start()
    {
        if (childWall != null)
            childWall.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        CachePlayerRefs(collision);

        playerInside = true;
        lastSafeState = IsPlayerSafe();

        // If the player enters already unsafe, react immediately.
        if (!lastSafeState)
        {
            ReactToUnsafePlayer();
        }
        else
        {
            // If safe, ensure we are not blocking.
            StopBarkAndOpenPath();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // Make sure references are valid even if enter missed for some reason.
        if (playerTf == null) CachePlayerRefs(collision);

        bool safeNow = IsPlayerSafe();

        // Detect mask changes while staying inside.
        if (safeNow != lastSafeState)
        {
            lastSafeState = safeNow;

            if (!safeNow)
            {
                ReactToUnsafePlayer();
            }
            else
            {
                // Switched back to safe mask inside the trigger.
                StopBarkAndOpenPath();
            }
        }
        else
        {
            // Still unsafe: optionally you can keep the wall up, but avoid repushing every frame.
            if (!safeNow)
            {
                EnsureBarkAndWallOn();
                TryPushPlayerBack();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerInside = false;

        // Stop any push routine when leaving.
        if (pushRoutine != null)
        {
            StopCoroutine(pushRoutine);
            pushRoutine = null;
        }

        StopBarkAndOpenPath();
        if (playerController != null)
            playerController.UnfreezeAction();

        // Clear cached references.
        playerTf = null;
        playerRb = null;
        playerController = null;
    }

    private void CachePlayerRefs(Collider2D collision)
    {
        playerTf = collision.transform;
        playerRb = collision.attachedRigidbody != null ? collision.attachedRigidbody : collision.GetComponent<Rigidbody2D>();

        // The class name user provided: PlayerCtrller.
        playerController = collision.GetComponent<PlayerCtrller>();
    }

    private bool IsPlayerSafe()
    {
        if (playerMask == null) return false;

        // Treat "000" as 0. If your mask IDs are different, adjust safeMaskId.
        return playerMask.checkMaskOn(safeMaskId);
    }

    private void ReactToUnsafePlayer()
    {
        EnsureBarkAndWallOn();
        TryPushPlayerBack();
    }

    private void EnsureBarkAndWallOn()
    {
        if (!barking && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(barkSoundId, true);
            barking = true;
        }

        if (bubble != null)
            bubble.Appear();

        if (bubble2 != null)
            bubble2.Appear();

        if (childWall != null)
            childWall.SetActive(true);
    }

    private void StopBarkAndOpenPath()
    {
        if (barking && SoundManager.Instance != null)
        {
            SoundManager.Instance.StopSound(barkSoundId);
            barking = false;
        }

        if (childWall != null)
            childWall.SetActive(false);
    }

    private void TryPushPlayerBack()
    {
        if (!playerInside) return;
        if (playerTf == null) return;

        if (Time.time < nextAllowedPushTime) return;
        nextAllowedPushTime = Time.time + pushCooldown;

        if (pushRoutine != null)
        {
            StopCoroutine(pushRoutine);
            pushRoutine = null;
        }

        pushRoutine = StartCoroutine(PushPlayerBackRoutine());
    }

    private IEnumerator PushPlayerBackRoutine()
    {
        // Freeze player movement before pushing.
        if (playerController != null)
            playerController.FreezeAction();

        if (playerRb != null)
            playerRb.linearVelocity = Vector2.zero;

        Vector2 startPos = playerTf.position;

        // Push direction: away from dog.
        Vector2 dir = ((Vector2)playerTf.position - (Vector2)transform.position);
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right; // Fallback direction

        dir.Normalize();
        Vector2 targetPos = startPos + dir * pushDistance;

        float t = 0f;
        float dur = Mathf.Max(0.01f, pushDuration);

        while (t < dur)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / dur);

            Vector2 newPos = Vector2.Lerp(startPos, targetPos, alpha);

            if (playerRb != null)
            {
                playerRb.MovePosition(newPos);
            }
            else
            {
                playerTf.position = newPos;
            }

            yield return null;
        }

        // Unfreeze after push finishes.
        if (playerController != null)
            playerController.UnfreezeAction();

        pushRoutine = null;
    }
}
