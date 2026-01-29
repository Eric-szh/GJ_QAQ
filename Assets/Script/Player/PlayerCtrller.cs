using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCtrller : MonoBehaviour
{
    // new input system
    public InputActionAsset playerInputAction;
    private InputActionMap _playerMap;
    private InputAction _moveAction;
    private InputAction _interactAction;

    private bool actionFreeze = false;

    [Header("Player Settings")]
    public float moveSpeed = 5f;
    private Vector2 moveInput;
    public IInteractable currentInteractable = null;
    public bool startFacingRight;
    public GameObject? E;

    [Header("Animation Settings")]
    public string idleAnimationName = "None";
    public string walkAnimationName = "None";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get input action map
        _playerMap = playerInputAction.FindActionMap("Player");
        _moveAction = _playerMap.FindAction("Move");
        _interactAction = _playerMap.FindAction("Interact");

        E?.SetActive(false);

        // if not start facing right, flip the player
        if (!startFacingRight)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            // change directionn of E too
            if (E != null)
            {
                E.transform.localScale = new Vector3(-1, 1, 1);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = _moveAction.ReadValue<Vector2>();
        moveInput = new Vector2(moveInput.x, 0f);

        // if move input is not zero, change face direction
        if (moveInput != Vector2.zero)
        {
            ChangeFaceDirection(moveInput);
            GetComponent<AniController>().ChangeAnimationState(walkAnimationName);
        }
        else
        {
            GetComponent<AniController>().ChangeAnimationState(idleAnimationName);
        }

        if (_interactAction.WasPressedThisFrame())
            OnInteract();

        // Show or hide the E prompt based on whether there is a current interactable
        if (currentInteractable != null)
        {
            E?.SetActive(true);
        }
        else
        {
            E?.SetActive(false);

        }
    }

    void FixedUpdate()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 targetPos = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);
    }

    public void SetCurrentInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;
        Debug.Log("Set current interactable to " + (interactable != null ? interactable.ToString() : "null"));

    }

    void OnInteract()
    {
        currentInteractable?.Interact(gameObject);
    }

    void ChangeFaceDirection(Vector2 direction)
    {
        // Do nothing if no horizontal intent.
        if (Mathf.Approximately(direction.x, 0f)) return;

        // Determine whether we want to face right based on movement direction.
        bool wantFaceRight = direction.x > 0f;

        // If the sprite was authored facing right, right = +1; otherwise right = -1.
        float rightScaleX = startFacingRight ? 1f : -1f;

        // Final scale: face right -> rightScaleX, face left -> -rightScaleX.
        float scaleX = wantFaceRight ? rightScaleX : -rightScaleX;

        ApplyScaleX(transform, scaleX);
        if (E != null) ApplyScaleX(E.transform, scaleX);
    }

    static void ApplyScaleX(Transform t, float scaleX)
    {
        Vector3 s = t.localScale;
        s.x = scaleX;
        t.localScale = s;
    }


    public void FreezeAction()
    {
        actionFreeze = true;
    }   

    public void UnfreezeAction()
    {
        actionFreeze = false;
    }

}
