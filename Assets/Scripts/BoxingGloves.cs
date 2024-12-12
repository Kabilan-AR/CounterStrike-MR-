using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxingGloves : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hand touched something" + collision.gameObject.name);
        if(collision.gameObject.layer == LayerMask.NameToLayer("Bomb"))
        {
            
            collision.gameObject.GetComponent<Rigidbody>().AddForce(transform.forward*400f, ForceMode.Force);
            Debug.Log("Bomb punched");
        }
    }
}
