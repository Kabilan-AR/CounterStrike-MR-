using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody rb;
    [HideInInspector] public Vector3 velocity;
    private AudioSource audioSource;
    [SerializeField] private AudioClip _boomClip;
    [SerializeField] private AudioClip _PlayerHitClip;
    private float timer = 0f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        

    }
    private void Start()
    {
        velocity = rb.velocity;
    }
    private void Update()
    {
      //Debug.Log("Velocity of:"+gameObject.name+"is:"+velocity);
      if (timer > 4f) 
        { 
            timer = 0f;
            Destroy(gameObject);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Map"))
        {
 
            Destroy(gameObject);
        }
       if(collision.gameObject.layer == LayerMask.NameToLayer("Bomb"))
        {
            audioSource.clip = _boomClip;
            //audioSource.spread.CompareTo(velocity);
            audioSource.Play();
            Destroy(gameObject);
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Cannon"))
        {
            Destroy(gameObject,1f);
        }
        else if(collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
             
            audioSource.clip = _PlayerHitClip;
            audioSource.Play();
        }
    }
   
}
