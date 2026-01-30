using UnityEngine;

public class TaskList : MonoBehaviour
{
    public static TaskList Instance { get; private set; }
    [SerializeField] private bool dontDestroyOnLoad = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

    }
    
    public void CompleteTask(int taskID)
    {
        Debug.Log("Task " + taskID + " completed.");
    }

    public void UnCompleteTask(int taskID)
    {
        Debug.Log("Task " + taskID + " uncompleted.");
    }
}
