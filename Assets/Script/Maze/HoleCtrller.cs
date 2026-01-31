using UnityEngine;

public class HoleCtrller : MonoBehaviour
{
    public int wrongSoundID;
    public int holeID;
    public BubbleAppear bubble;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Right()
    {
        CameraCharacterSwitcher.Instance.SwitchCamera(holeID);
    }

    public void Wrong()
    {
        SoundManager.Instance.PlaySound(wrongSoundID, false);
        bubble.Appear();
    }
}
