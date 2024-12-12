using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVR;

public class OVRSnapTurn : MonoBehaviour
{
    public Transform playerTransform; // The object you want to rotate, typically the OVRCameraRig or parent of it
    public float turnAngle = 45f;     // Angle to snap turn (e.g., 45 degrees)
    public float thumbstickThreshold = 0.7f; // Threshold to detect thumbstick tilt
    public OVRInput.Controller controller = OVRInput.Controller.LTouch; // Which controller to detect input from
    public float cooldownTime = 0.25f; // Cooldown between turns to prevent rapid turns

    private bool isCooldown = false;   // To manage cooldown

    void Update()
    {
        HandleSnapTurn();
    }

    private void HandleSnapTurn()
    {
        if (isCooldown) return; // Prevent turning while on cooldown

        // Get the thumbstick input from the controller
        Vector2 thumbstickInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controller);

        // Check if the thumbstick is moved left or right beyond the threshold
        if (thumbstickInput.x > thumbstickThreshold)
        {
            // Snap turn to the right
            playerTransform.Rotate(Vector3.up, turnAngle);
            StartCoroutine(SnapTurnCooldown());
        }
        else if (thumbstickInput.x < -thumbstickThreshold)
        {
            // Snap turn to the left
            playerTransform.Rotate(Vector3.up, -turnAngle);
            StartCoroutine(SnapTurnCooldown());
        }
    }

    private IEnumerator SnapTurnCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldown = false;
    }
}
