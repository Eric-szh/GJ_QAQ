using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMaskModel : MonoBehaviour
{

    private List<int> itemGot;
    private List<int> maskGot;
    private int maskEquiped;

    private List<int> itemPickuped;



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
            // TODO: update maskview
        }
        else if (pickupID / 100 == 2)
        {
            itemGot.Add(pickupID);
            // TODO: update itemview
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
            // TODO: update itemview
        }
    }
}
