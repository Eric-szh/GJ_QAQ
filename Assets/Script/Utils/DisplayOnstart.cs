using UnityEngine;

public class DisplayOnstart : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject objectToDisplay;
    void Start()
    {
        if (objectToDisplay != null)
        {
            objectToDisplay.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
