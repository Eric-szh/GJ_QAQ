using UnityEngine;

public class ItemFly : MonoBehaviour
{
    public Transform direction;

    private void OnEnable()
    {
        // calculate 
        Vector2 dir = direction.position - transform.position;
        dir = dir.normalized;

        // get rightbody
        GetComponent<Rigidbody2D>().AddForce(dir * 300f);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if object have tag ground
        if (collision.CompareTag("Ground"))
        {
            // freeze object
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }
}
