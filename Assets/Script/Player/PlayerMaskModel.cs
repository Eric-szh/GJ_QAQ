using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMaskModel : MonoBehaviour
{

    private List<int> itemGot;
    private List<int> maskGot;
    private int maskEquiped;

    private List<int> itemPickuped;

    public InventoryMaskView invMaskView;
    public InventoryItemView itemView;
    public PlayerMaskView playerMaskView;

    [Header("Debug Pickup (Inspector)")]
    [SerializeField] private int debugPickupId = 100; // 比如 1xx 是 mask，2xx 是 item
    [SerializeField] private bool debugIgnoreAlreadyPicked = true;

    private void Reset()
    {
        invMaskView = FindFirstObjectByType<InventoryMaskView>();
        itemView = FindFirstObjectByType<InventoryItemView>();
        playerMaskView = FindFirstObjectByType<PlayerMaskView>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maskEquiped = 000;

        // init the lists
        itemGot = new List<int>();
        maskGot = new List<int>();
        itemPickuped = new List<int>();

    }

    public void getPickup(int pickupID)
    {
        // check if the pickupID is already in the list
        if (!itemPickuped.Contains(pickupID))
        {
            itemPickuped.Add(pickupID);
        }
        else
        {
            return;
        }

        // id starts with 1 is mask, id starts with 2 is item
        if (pickupID / 100 == 1)
        {
            maskGot.Add(pickupID);
            invMaskView.UpdateView(maskGot.ToArray());
        }
        else if (pickupID / 100 == 2)
        {
            itemGot.Add(pickupID);
            itemView.UpdateView(itemGot.ToArray());
        }
    }
        
    public void loseItem(int itemID)
    {
        // you can only lose items you have, not masks
        // check if the itemID is in the list
        if (maskGot.Contains(itemID))
        {
            return;
        }

        if (itemGot.Contains(itemID))
        {
            maskGot.Remove(itemID);
            itemView.UpdateView(itemGot.ToArray());
        }
    }

    public bool useMask(int slot)
        // if mask is not wearing successful return false
    {
        // use the mask at the slot position
        if (slot < 0 || slot >= maskGot.Count)
        {
            return false;
        }

        int maskGoingToEquip = maskGot[slot];
        // if the mask is already equiped, took it off
        if (maskEquiped == maskGoingToEquip)
        {
            maskEquiped = 000;
        }
        else
        {
            maskEquiped = maskGoingToEquip;
        }

        // TODO: update invmaskview selection
        invMaskView.UpdateSelection(maskEquiped == 000 ? -1 : slot);
        // TODO: update actual mask in player maskview
        playerMaskView.UpdateMask(maskEquiped);

        return true;
    }

    public bool checkMaskOn(int maskid)
    {
        return maskEquiped == maskid;
    }

    public bool checkHaveItem(int itemid)
    {
        return itemGot.Contains(itemid);
    }

    // Inspector 右上角三点菜单里会出现按钮
    [ContextMenu("DEBUG/Get Pickup (Use debugPickupId)")]
    private void DebugGetPickup()
    {
        if (debugIgnoreAlreadyPicked)
        {
            // 直接绕过 itemPickuped 防重，方便你疯狂点
            if (debugPickupId / 100 == 1)
            {
                if (!maskGot.Contains(debugPickupId))
                    maskGot.Add(debugPickupId);

                invMaskView.UpdateView(maskGot.ToArray());
            }
            else if (debugPickupId / 100 == 2)
            {
                if (!itemGot.Contains(debugPickupId))
                    itemGot.Add(debugPickupId);

                itemView.UpdateView(itemGot.ToArray());
            }
            return;
        }

        // 走原逻辑（会被 itemPickuped 防重挡住）
        getPickup(debugPickupId);
    }

    [ContextMenu("DEBUG/Clear Inventory Lists")]
    private void DebugClear()
    {
        itemGot?.Clear();
        maskGot?.Clear();
        itemPickuped?.Clear();
        maskEquiped = 0;

        invMaskView?.UpdateView(maskGot.ToArray());
        itemView?.UpdateView(itemGot.ToArray());
    }
}
