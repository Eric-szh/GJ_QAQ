using UnityEngine;

public class AniController : MonoBehaviour
{
    public Animator animator;
    private string currentState;
    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ChangeAnimationState(string newState, bool forced = false)
    {
        if (newState == "None") return; 

        if (currentState == newState && !forced) return;

        if (forced)
        {
            animator.Play(newState, -1, 0f);
        }
        else
        {
            animator.Play(newState);
        }


        currentState = newState;
    }
}
