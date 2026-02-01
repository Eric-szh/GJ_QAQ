
using UnityEngine;

public class CameraFollowClampX : MonoBehaviour
{
    public Transform target;


    public Vector3 offset = new Vector3(0f, 1.5f, -10f);
    public float smooth = 10f;


    public bool clampX = true;

    public float minCameraXLeft = -50f;
    public float maxCameraXRight = 50f;



    void FixedUpdate()
    {
        if (!target) return;

        // 想要的相机位置
        Vector3 desired = target.position + offset;
        desired.y = 0;
        
        if (clampX)
        {


        
            
            desired.x = Mathf.Clamp(desired.x, minCameraXLeft, maxCameraXRight);
            






        }

        transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
    }
}
