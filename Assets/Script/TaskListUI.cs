using UnityEngine;
using UnityEngine.UI;

public class TaskListUI : MonoBehaviour
{
    [System.Serializable]
    public struct TaskUI
    {
        public int taskId;
        public Image checkmark; // drag the checkmark Image here
    }

    [SerializeField] private TaskUI[] tasks;

    private void OnEnable()
    {
        RefreshAll();
    }

    public void RefreshAll()
    {
        var tl = TaskList.Instance;
        if (tl == null) return;

        foreach (var t in tasks)
        {
            bool done = tl.IsCompleted(t.taskId);
            t.checkmark.gameObject.SetActive(done);
        }
    }
}
