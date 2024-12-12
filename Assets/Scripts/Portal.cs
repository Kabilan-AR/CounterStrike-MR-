using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class Portal : MonoBehaviour
{
    public List<GameObject> TeleportedObjects;
    private AudioSource _source;
    [SerializeField] private AudioClip _absorbSfx;
    private void Start()
    {
        _source = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other)
    {
     
        if (other.gameObject.layer == LayerMask.NameToLayer("Bomb"))
        {
            other.gameObject.layer = LayerMask.NameToLayer("PlayerBomb");
            other.gameObject.SetActive(false);
            other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            _source.clip = _absorbSfx; _source.Play();
            TeleportedObjects.Add(other.gameObject);
        }
    }
}
