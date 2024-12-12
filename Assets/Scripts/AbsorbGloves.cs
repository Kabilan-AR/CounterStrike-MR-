using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVR;


public class AbsorbGloves : MonoBehaviour
{

    public Portal _portal;
    [SerializeField] private float GravityRadius = 2f;
    [SerializeField] private float GravityForce = 1f;

    [SerializeField] private GameObject Portal;
    //[SerializeField] private Transform PortalPosition;
    



    private OVRInput.Button ActivateAction=OVRInput.Button.PrimaryIndexTrigger;


    private bool isAbsorbing = false;
    private void Awake()
    {
       

    }
    private void ActivateSpatialMove()
    {
        Portal.SetActive(true);
        Debug.Log("Absorbing");
        isAbsorbing = true;
    }
    private void DeactivateSpatialMove()
    {
        Portal.SetActive(false);
        isAbsorbing = false;
    }
    private void Update()
    {
        if (OVRInput.GetDown(ActivateAction, OVRInput.Controller.LTouch))
        {
            ActivateSpatialMove();
        }

        if (OVRInput.GetUp(ActivateAction, OVRInput.Controller.LTouch))
        {
            DeactivateSpatialMove();
        }
        if (isAbsorbing)
        {
            AbsorbAbility();
        }


    }
    public void AbsorbAbility()
    {
        Collider[] colliders = Physics.OverlapSphere(Portal.transform.position, GravityRadius);
        foreach (var obj in colliders)
        {
            Rigidbody objRB = obj.GetComponent<Rigidbody>();
            if (!objRB && obj.gameObject.layer != LayerMask.NameToLayer("Spatial")) continue;
            objRB.useGravity = false;
            objRB.AddForce((Portal.transform.position - obj.transform.position) * GravityForce, ForceMode.Acceleration);
            //objRB.velocity = Vector3.zero;

        }
    }

}
