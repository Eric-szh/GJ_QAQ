using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class FinalPlant : EventInteract
{
    public int plantID;
    public PlayerMaskModel playerMaskModel;
    public List<GameObject> destroyList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMaskModel = GameObject.FindWithTag("Player").GetComponent<PlayerMaskModel>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected new void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == playerCtrl.gameObject)
        {
            TaskList.Instance.CompleteTask(plantID);
            if (TaskList.Instance.AllFinished())
            {
                playerCtrl.SetCurrentInteractable(this);
            }
        }
   
    }

    protected new void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == playerCtrl.gameObject)
        {
            if (TaskList.Instance.AllFinished())
            {
                playerCtrl.SetCurrentInteractable(null);
            }
            TaskList.Instance.UnCompleteTask(plantID);
        }
    }

    protected override bool AcutalInteract()
    {
        if (TaskList.Instance.AllFinished())
        {
            playerMaskModel.loseItem(200);
            playerMaskModel.loseItem(201);
            playerMaskModel.loseItem(202);
            playerCtrl.FreezeAction();
            playerCtrl.Juggle();
            foreach (var obj in destroyList)
            {
                obj.SetActive(false);
            }
            return true;
        }
        return false;

    }
}
