using UnityEngine;

public class CatCtrller : MonoBehaviour
{
    public Transform moveTarget;
    public float moveSpeed;
    public float arrivalThreshold = 0.1f; // 到达目标的阈值
    public bool isMoving = false; // 是否正在移动

    public void GoToTarget() { 
        isMoving = true;
    }

    public void WhenAtTarget() { 
        isMoving = false;
        GetComponent<AniController>().ChangeAnimationState("Cat_back");
        // flip x 
        GetComponent<SpriteRenderer>().flipX = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving && moveTarget != null)
        {
            // 使用 MoveTowards 实现更平滑的移动
            transform.position = Vector3.MoveTowards(
                transform.position, 
                moveTarget.position, 
                moveSpeed * Time.deltaTime
            );
            
            // 检查是否到达
            if (Vector3.Distance(transform.position, moveTarget.position) < arrivalThreshold)
            {
                WhenAtTarget();
            }
        }
    }
}
