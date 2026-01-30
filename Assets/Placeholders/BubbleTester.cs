using UnityEngine;
using UnityEngine.InputSystem; // You need this at the top!

public class BubbleTester : MonoBehaviour
{
    public BubbleAppear bubbleScript;

    void Update()
    {
        // This is the "New Input System" way to check for a key press
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("Space pressed: Triggering Appear()");
            bubbleScript.Appear();
        }
    }
}