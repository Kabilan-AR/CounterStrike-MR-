using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVR;

public class CounterGloves : MonoBehaviour
{
 

    public Portal _portal;


    [SerializeField] private GameObject CounterPortal;
    [SerializeField] private Transform CounterReleasePosition;

    private AudioSource _source;
    [SerializeField] private AudioClip _launchSfx;
    private OVRInput.Button ActivateAction = OVRInput.Button.One;


    private LineRenderer lineRenderer;
    private Bomb _bomb;

    private bool isCountering = false;
    private bool showTrajectory = false;

    private float gravity = -9.81f;
    [Range(10, 100)]
    private int LinePoints = 25;

    [Range(0.01f, 0.25f)]
    private float TimeBetweenPoints = 0.1f;
    [SerializeField] private float maxRayDistance = 300f; 
    [SerializeField] private float launchSpeed = 20f; 
    private void Awake()
    {
        _source = GetComponent<AudioSource>();
    
        lineRenderer = GetComponent<LineRenderer>();
        

    }
    private void ActivateSpatialMove()
    {
        lineRenderer.enabled = true;
        CounterPortal.SetActive(true);
        Debug.Log("Countering");
        showTrajectory = true;

        
    }
    private void DeactivateSpatialMove()
    {
        lineRenderer.enabled = false;
        CounterPortal.SetActive(false);
        showTrajectory = false;
        CounterAbility();


    }
    private void Update()
    {
        if (OVRInput.GetDown(ActivateAction, OVRInput.Controller.RTouch))
        {
            ActivateSpatialMove();
        }

        // Check if the Oculus controller button (A button) is released
        if (OVRInput.GetUp(ActivateAction, OVRInput.Controller.RTouch))
        {
            DeactivateSpatialMove();
        }
        if (showTrajectory)
        {
            RenderTrajectory();
        }

    }

    public void CounterAbility()
    {
        if (_portal.TeleportedObjects.Count == 0) return;
        Debug.Log("CounterStrike");
        RaycastHit hit;
        Vector3 castDirection = CounterReleasePosition.forward;

        if (Physics.SphereCast(CounterReleasePosition.position, 5f, castDirection, out hit, maxRayDistance))
        {
            if (hit.collider.gameObject.layer==LayerMask.NameToLayer("Cannon"))
            {

                GameObject obj = _portal.TeleportedObjects[0];
                Rigidbody objRigidbody = obj.GetComponent<Rigidbody>();
                Vector3 targetPosition = hit.point;
                Vector3 directionToTarget = (targetPosition - CounterReleasePosition.position).normalized;
                Vector3 calculatedVelocity = directionToTarget * launchSpeed;
                obj.GetComponent<Rigidbody>().useGravity = false; // Disable gravity for straight line launch
                obj.transform.position = CounterReleasePosition.position;
                objRigidbody.velocity = calculatedVelocity;

                obj.GetComponent<Rigidbody>().AddForce(CounterReleasePosition.forward * launchSpeed, ForceMode.Impulse);
                obj.transform.position = CounterReleasePosition.position;


                Debug.Log("Object is launched");
                obj.SetActive(true);
                _source.clip = _launchSfx;
                _source.Play();
                _portal.TeleportedObjects.Remove(obj);

                showTrajectory = false; // Stop rendering the trajectory after firing
            }
        }
        //GameObject obj = _portal.TeleportedObjects[0];
        //obj.GetComponent<Rigidbody>().useGravity = true;
        //obj.transform.position = CounterReleasePosition.position;
        //obj.SetActive(true);
        //obj.GetComponent<Rigidbody>().AddForce(obj.GetComponent<Rigidbody>().velocity, ForceMode.Force);
        //_source.clip = _launchSfx; _source.Play();
        //_portal.TeleportedObjects.Remove(obj);
    }

    private void RenderTrajectory()
    {
        if (_portal.TeleportedObjects.Count == 0) { lineRenderer.enabled = false; return; }
        lineRenderer.enabled = true;

        lineRenderer.positionCount = Mathf.CeilToInt(LinePoints / TimeBetweenPoints) + 1;
        Vector3 startPosition = CounterReleasePosition.position;
        Vector3 startVelocity = 20f * CounterReleasePosition.transform.forward / _portal.TeleportedObjects[0].GetComponent<Rigidbody>().mass;
        int i = 0;
        lineRenderer.SetPosition(i, startPosition);
        for (float time = 0; time < LinePoints; time += TimeBetweenPoints)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            lineRenderer.SetPosition(i, point);

            Vector3 lastPosition = lineRenderer.GetPosition(i - 1);

            if (Physics.Raycast(lastPosition,
                (point - lastPosition).normalized,
                out RaycastHit hit,
                (point - lastPosition).magnitude
                ))
            {
                lineRenderer.SetPosition(i, hit.point);
                lineRenderer.positionCount = i + 1;
                return;
            }
        }
        //Vector3 initialVelocity = _portal.TeleportedObjects[0].GetComponent<Rigidbody>().velocity;
        //lineRenderer.enabled = true;
        //lineRenderer.positionCount = Mathf.CeilToInt(LinePoints / TimeBetweenPoints) + 1;

        //Vector3 startPosition = CounterReleasePosition.position;

        //int i = 0;
        //lineRenderer.SetPosition(i, startPosition);

        ////Simulate the trajectory
        //for (float time = 0; time < LinePoints; time += TimeBetweenPoints)
        //{
        //    i++;

        //    // Calculate the position of the projectile at the given time
        //    Vector3 point = startPosition + initialVelocity * time;
        //    point.y = startPosition.y + initialVelocity.y * time + (0.5f * gravity * time * time);

        //    lineRenderer.SetPosition(i, point);

        //    // Check for collision using a raycast
        //    Vector3 lastPosition = lineRenderer.GetPosition(i - 1);
        //    if (Physics.Raycast(lastPosition, (point - lastPosition).normalized, out RaycastHit hit, (point - lastPosition).magnitude))
        //    {
        //        lineRenderer.SetPosition(i, hit.point);
        //        lineRenderer.positionCount = i + 1;
        //        return;
        //    }
        //}

    }
}
