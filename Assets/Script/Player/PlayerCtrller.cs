using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCtrller : MonoBehaviour
{
    // new input system
    public InputActionAsset playerInputAction;
    private InputActionMap _playerMap;
    private InputAction _moveAction;
    private InputAction _interactAction;
    private InputAction _hotbar1;
    private InputAction _hotbar2;
    private InputAction _hotbar3;
    private InputAction _hotbar4;
    private InputAction _hotbar5;

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

    public PlayerMaskModel _playerMaskModel;

    private bool showe;

    private void Reset()
    {
        _playerMaskModel = FindFirstObjectByType<PlayerMaskModel>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // get input action map
        _playerMap = playerInputAction.FindActionMap("Player");
        _moveAction = _playerMap.FindAction("Move");
        _interactAction = _playerMap.FindAction("Interact");
        _hotbar1 = _playerMap.FindAction("Hotbar1");
        _hotbar2 = _playerMap.FindAction("Hotbar2");
        _hotbar3 = _playerMap.FindAction("Hotbar3");
        _hotbar4 = _playerMap.FindAction("Hotbar4");
        _hotbar5 = _playerMap.FindAction("Hotbar5");

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
        if (actionFreeze) return;

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

        if (_hotbar1.WasPressedThisFrame())
        {
            _playerMaskModel.useMask(0);
        } else if (_hotbar2.WasPressedThisFrame())
        {
            _playerMaskModel.useMask(1);
        } else if (_hotbar3.WasPressedThisFrame())
        {
            _playerMaskModel.useMask(2);
        } else if (_hotbar4.WasPressedThisFrame())
        {
            _playerMaskModel.useMask(3);
        } else if (_hotbar5.WasPressedThisFrame())
        {
            _playerMaskModel.useMask(4);
        }


        // Show or hide the E prompt based on whether there is a current interactable
        if (currentInteractable != null && showe)
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

    public void SetCurrentInteractable(IInteractable interactable, bool showE = true)
    {
        currentInteractable = interactable;
        showe = showE;
        Debug.Log("Set current interactable to " + (interactable != null ? interactable.ToString() : "null"));

    }

    public void LogText(string text)
    {
        Debug.Log(text);
    }

    void OnInteract()
    {
        currentInteractable?.Interact(gameObject);
    }

    public void Juggle()
    {
        GetComponent<AniController>().ChangeAnimationState("Player_juggle");
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
