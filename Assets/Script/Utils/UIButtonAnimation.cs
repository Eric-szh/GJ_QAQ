using UnityEngine;

public class UIButtonPlayAnim : MonoBehaviour
{
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private string triggerName = "Play";

    public void Play()
    {
        if (targetAnimator == null) return;
        targetAnimator.SetBool(triggerName,true);
    }
}