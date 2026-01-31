using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskList : MonoBehaviour
{
    public static TaskList Instance { get; private set; }
    private Dictionary<int, bool> _completedById;
    public event Action<int, bool> OnTaskCompletionChanged;
    [SerializeField] private bool dontDestroyOnLoad = true;
    [SerializeField] public TaskListUI taskListChecks;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        _completedById = new Dictionary<int, bool>
        {
            { 100, false },
            { 101, false  },
            { 102, false },
            { 103, false  },
            { 104, false }
        };
    }

    public bool IsCompleted(int taskID)
    {
        return _completedById[taskID];
    }
    
    public void CompleteTask(int taskID)
    {
        _completedById[taskID] = true;
        Debug.Log("Task " + taskID + " completed.");
        taskListChecks.RefreshAll();
    }

    public void UnCompleteTask(int taskID)
    {
        _completedById[taskID] = false;
        Debug.Log("Task " + taskID + " uncompleted.");
    }

    public bool AllFinished()
{
    foreach (var kv in _completedById)
    {
        if (!kv.Value) return false;
    }
    return true;
}
}
