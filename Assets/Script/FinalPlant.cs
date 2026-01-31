using UnityEngine;

public class FinalPlant : EventInteract
{
    public int plantID;
    public PlayerMaskModel playerMaskModel;

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
        base.OnTriggerEnter2D(collision);
        if (collision.CompareTag("Player"))
        {
            TaskList.Instance.CompleteTask(plantID);
        }
    }

    protected new void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
        if (collision.CompareTag("Player"))
        {
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
            Debug.Log("All tasks finished! You win!");
            return true;
        }
        Debug.Log("FinalPlant interacted");
        return false;

    }
}
