using System.Collections;
using UnityEngine;

public class AppearWhenTargetNull : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Collider[] colliders;
    [SerializeField] private Collider2D[] colliders2D;

    private void Awake()
    {
        SetVisible(false);
    }

    private void Start()
    {
        StartCoroutine(WaitThenAppear());
    }

    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
        StopAllCoroutines();
        SetVisible(false);
        StartCoroutine(WaitThenAppear());
    }

    private IEnumerator WaitThenAppear()
    {
        while (target != null) yield return null;
        SetVisible(true);
    }

    private void SetVisible(bool on)
    {
        if (renderers != null)
            foreach (var r in renderers) if (r) r.enabled = on;

        if (colliders != null)
            foreach (var c in colliders) if (c) c.enabled = on;

        if (colliders2D != null)
            foreach (var c in colliders2D) if (c) c.enabled = on;
    }
}
