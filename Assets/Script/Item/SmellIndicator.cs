using UnityEngine;

public class SmellIndicator : MonoBehaviour
{
    [SerializeField]
    private PlayerMaskModel PlayerMaskModel;
    public GameObject smellEffect;
    SpriteRenderer sr;

    private void Reset()
    {
        if (PlayerMaskModel == null)
            PlayerMaskModel = FindFirstObjectByType<PlayerMaskModel>(FindObjectsInactive.Include);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        sr = GetComponent<SpriteRenderer>();
        Hide();
    }

    void Hide()
    {
        // hide sprite 
        if (sr != null)
        {
            sr.enabled = false;
        }
        // hide smell effect 
        if (smellEffect != null)
        {
            smellEffect.SetActive(false);
        }
    }

    void Show()
    {
        // show sprite 
        if (sr != null)
        {
            sr.enabled = true;
        }
        // show smell effect 
        if (smellEffect != null)
        {
            smellEffect.SetActive(true);
        }
    }



    // Update is called once per frame
    void Update()
    {
        // check mask id 
        if (PlayerMaskModel.checkMaskOn(101))
        {
            Show();
        } else
        {
            Hide();
        }
    }
}
