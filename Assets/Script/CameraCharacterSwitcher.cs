using UnityEngine;

public class CameraCharacterSwitcher : MonoBehaviour
{
    public static CameraCharacterSwitcher Instance { get; private set; }

    [Header("Assign 3 cameras in order: 0,1,2")]
    [SerializeField] private Camera[] cameras;

    [Header("Assign 3 characters in order: 0,1,2")]
    [SerializeField] private GameObject[] characters;

    [Header("Singleton Options")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    [Header("Inv Ui")]
    [SerializeField] private GameObject inventoryUI;

    private void Awake()
    {
        // Singleton guard
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        SwitchCamera(0); // Start with the first camera and character
    }

    public void SwitchCamera(int cameraIndex)
    {
        if (cameras == null || cameras.Length == 0)
        {
            Debug.LogError("Cameras array is not assigned.");
            return;
        }

        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("Characters array is not assigned.");
            return;
        }

        if (cameraIndex < 0 || cameraIndex >= cameras.Length)
        {
            Debug.LogError($"Invalid cameraIndex: {cameraIndex}. Valid range: 0..{cameras.Length - 1}");
            return;
        }

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null) cameras[i].enabled = (i == cameraIndex);
        }

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null) characters[i].SetActive(i == cameraIndex);
        }

        // if switch to carema index 2 and 1, hide inventory UI
        if (inventoryUI != null)
        {
            if (cameraIndex == 2 || cameraIndex == 1)
            {
                inventoryUI.SetActive(false);
            }
            else
            {
                inventoryUI.SetActive(true);
            }
        }
    }
}
