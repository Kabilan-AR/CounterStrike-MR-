using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVR;

public class HomingCounter : MonoBehaviour
{


    public Portal _portal;


    [SerializeField] private GameObject CounterPortal;
    [SerializeField] private Transform CounterReleasePosition;

    private AudioSource _source;
    [SerializeField] private AudioClip _launchSfx;
    private OVRInput.Button ActivateAction = OVRInput.Button.PrimaryIndexTrigger;


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
        _source = GetComponent<AudioSource>();        lineRenderer = GetComponent<LineRenderer>();


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
            Debug.Log("Counter gloves pressed action");
            ActivateSpatialMove();
        }

        if (OVRInput.GetUp(ActivateAction, OVRInput.Controller.RTouch))
        {
            Debug.Log("Counter gloves released action");
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
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Cannon"))
            {
                GameObject obj = _portal.TeleportedObjects[0];
                Vector3 targetPosition = hit.point;

                // Start the coroutine to move the object along a curved path
                StartCoroutine(MoveAlongCurve(obj, CounterReleasePosition.position, targetPosition));

                _portal.TeleportedObjects.Remove(obj);

                // Stop rendering the trajectory after firing
                showTrajectory = false;
            }
        }
    }

    // Coroutine to move the object along the curve
    private IEnumerator MoveAlongCurve(GameObject obj, Vector3 startPoint, Vector3 targetPoint)
    {
        Rigidbody objRigidbody = obj.GetComponent<Rigidbody>();
        objRigidbody.useGravity = false; // Disable gravity for controlled movement
        obj.SetActive(true);

        // Calculate the control point for the curve
        Vector3 midPoint = (startPoint + targetPoint) / 2f;
        Vector3 controlPoint = midPoint + Vector3.up * 5f; // Adjust the height of the curve by modifying the '5f'

        float time = 0;
        float duration = 2f; // Time to reach the target, adjust for speed

        // Animate the object along the curved path over time
        while (time < 1f)
        {
            time += Time.deltaTime / duration;

            // Quadratic Bézier interpolation: B(t) = (1 - t)² * P0 + 2(1 - t)t * P1 + t² * P2
            Vector3 position = Mathf.Pow(1 - time, 2) * startPoint +
                               2 * (1 - time) * time * controlPoint +
                               Mathf.Pow(time, 2) * targetPoint;

            obj.transform.position = position;

            yield return null; // Wait for the next frame
        }

        // Enable gravity once the object has reached the target
        objRigidbody.useGravity = true;

        Debug.Log("Object has reached the target.");
        _source.clip = _launchSfx;
        _source.Play();
    }
    private void RenderTrajectory()
    {
        if (_portal.TeleportedObjects.Count == 0)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;
        lineRenderer.positionCount = Mathf.CeilToInt(LinePoints / TimeBetweenPoints) + 1;

        // Get the positions
        Vector3 startPosition = CounterReleasePosition.position;
        Vector3 targetPosition = GetPredictedHitPoint(); // Function that gets the target position (hit.point)

        // Calculate the control point for the curve
        Vector3 midPoint = (startPosition + targetPosition) / 2f;
        Vector3 controlPoint = midPoint + Vector3.up * 5f; // Adjust the curve height (5f)

        int i = 0;
        lineRenderer.SetPosition(i, startPosition);

        // Simulate the trajectory along the Bézier curve
        for (float t = 0; t <= 1; t += TimeBetweenPoints / LinePoints)
        {
            i++;

            // Quadratic Bézier interpolation
            Vector3 point = Mathf.Pow(1 - t, 2) * startPosition +
                            2 * (1 - t) * t * controlPoint +
                            Mathf.Pow(t, 2) * targetPosition;

            lineRenderer.SetPosition(i, point);

            // Optional: Perform a raycast to detect collisions along the curve
            if (i > 1)
            {
                Vector3 lastPosition = lineRenderer.GetPosition(i - 1);
                if (Physics.Raycast(lastPosition, (point - lastPosition).normalized, out RaycastHit hit, (point - lastPosition).magnitude))
                {
                    lineRenderer.SetPosition(i, hit.point);
                    lineRenderer.positionCount = i + 1; // Stop drawing after the hit
                    return;
                }
            }
        }
    }

    // Helper function to get the predicted hit point
    private Vector3 GetPredictedHitPoint()
    {
        RaycastHit hit;
        Vector3 castDirection = CounterReleasePosition.forward;

        if (Physics.SphereCast(CounterReleasePosition.position, 5f, castDirection, out hit, maxRayDistance))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Cannon"))
            {
                return hit.point;
            }
        }

        // If no cannon is hit, return a default far point or handle differently
        return CounterReleasePosition.position + castDirection * maxRayDistance;
    }

}
