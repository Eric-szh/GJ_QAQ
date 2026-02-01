using UnityEngine;
using UnityEngine.Events;

public class MaskInteract : EventInteract
{
    [SerializeField] private PlayerMaskModel maskModel;
    [SerializeField] private int maskID;
    [SerializeField] private UnityEvent wrongEvent;
    [SerializeField] private int catHiss = 306;

    protected new void Reset()
    {
        base.Reset();
        maskModel = FindFirstObjectByType<PlayerMaskModel>();  
    }

    protected override bool AcutalInteract()
    {
        if (maskModel.checkMaskOn(maskID))
        {
            // additionaly check if maskID is 100, if so, play hiss sound
            if (maskID == 100)
            {
                Debug.Log("Playing cat hiss sound");
                SoundManager.Instance.PlaySound(catHiss, false);
            }
            return base.AcutalInteract();
        }
        else
        {
            wrongEvent.Invoke();
            return false;
        }

    }
}
