using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Debries : MonoBehaviour
{
    [SerializeField] private float explosionForce = 1f; // Force of the explosion
    [SerializeField] private float lifeTime = 2f; // life of the debries

    private Rigidbody[] rbs;

    private void Awake()
    {
        rbs = GetComponentsInChildren<Rigidbody>();
    }

    public void ScatterDebries()
    {
        foreach (var rb in rbs)
        {
            Vector3 explosionDirection = (rb.position - transform.position).normalized;
            // Apply force to the Rigidbody
            rb.AddForce(explosionDirection * explosionForce, ForceMode.Impulse);
            rb.useGravity = true;
            Destroy(rb.gameObject, lifeTime);
        }
    }
}