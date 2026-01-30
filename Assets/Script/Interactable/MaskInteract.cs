using UnityEngine;
using UnityEngine.Events;

public class MaskInteract : EventInteract
{
    [SerializeField] private PlayerMaskModel maskModel;
    [SerializeField] private int maskID;
    [SerializeField] private UnityEvent wrongEvent;

    protected new void Reset()
    {
        base.Reset();
        maskModel = FindFirstObjectByType<PlayerMaskModel>();  
    }

    protected override bool AcutalInteract()
    {
        if (maskModel.checkMaskOn(maskID))
        {
            return base.AcutalInteract();
        }
        else
        {
            wrongEvent.Invoke();
            return false;
        }

    }
}
