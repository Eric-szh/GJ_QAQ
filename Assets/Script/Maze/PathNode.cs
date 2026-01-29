using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    public List<PathNode> neighbors = new List<PathNode>();

    public Vector2 Pos2D => (Vector2)transform.position;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.12f);
        Gizmos.color = Color.white;
        foreach (var nb in neighbors)
        {
            if (!nb) continue;
            Gizmos.DrawLine(transform.position, nb.transform.position);
        }
    }
#endif
}
