using System.Collections.Generic;
using UnityEngine;

public class PlayerMaskView : MonoBehaviour
{
    public Dictionary<int, Sprite> maskDict;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    public void updateMask(int maskid)
    {
        // find the sprite in the dict
        Sprite maskSprite = maskDict[maskid];
        // update the sprite renderer
        this.GetComponent<SpriteRenderer>().sprite = maskSprite;
    }
}
