using UnityEngine;
using UnityEngine.Events;

public class ItemInteract : EventInteract
{
    [SerializeField] private PlayerMaskModel maskModel;
    [SerializeField] private int itemID;
    [SerializeField] private UnityEvent wrongEvent;

    protected new void Reset()
    {
        base.Reset();
        maskModel = FindFirstObjectByType<PlayerMaskModel>();
    }

    protected override bool AcutalInteract()
    {
        if (maskModel.checkHaveItem(itemID))
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
