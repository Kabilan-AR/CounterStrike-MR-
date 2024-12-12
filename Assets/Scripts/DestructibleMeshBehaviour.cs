using Meta.WitAi;
using Meta.XR.MRUtilityKit;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class DestructibleMeshBehaviour : MonoBehaviour
{
    [SerializeField] private DestructibleGlobalMeshSpawner destructibleGlobalMeshSpawner;
    [SerializeField] private GameObject debries;
    [SerializeField] private GameObject particleEffect;

    private DestructibleMeshComponent destructibleMeshComponent;
    private List<GameObject> globalMeshSegments = new();
    private OVRCameraRig cameraRig;

    private void Awake()
    {
        cameraRig = FindObjectOfType<OVRCameraRig>();
    }

    private void OnEnable()
    {
        destructibleGlobalMeshSpawner.OnSegmentationCompleted += AddBarycentricCoordinates;
        destructibleGlobalMeshSpawner.OnDestructibleMeshCreated.AddListener(OnDestructibleMeshCreated);
    }

    private void OnDisable()
    {
        destructibleGlobalMeshSpawner.OnSegmentationCompleted -= AddBarycentricCoordinates;
        destructibleGlobalMeshSpawner.OnDestructibleMeshCreated.RemoveListener(OnDestructibleMeshCreated);
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
            OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            TryDestroyMeshSegment();
        }
    }

    private void TryDestroyMeshSegment()
    {
        var ray = GetControllerRay();
        if (Physics.Raycast(ray, out var hit))
        {
            var hitObject = hit.collider.gameObject;
            if (globalMeshSegments.Contains(hitObject) && hitObject != destructibleMeshComponent.ReservedSegment)
            {
                PlayVisualEffects(hit.point);
                // The DestroySegment function is preferred when destroying mesh segments
                // as it takes care of destroying the assets instantiated.
                destructibleMeshComponent.DestroySegment(hitObject);
            }
        }
    }

    private void PlayVisualEffects(Vector3 hitLocation)
    {
        GameObject tempDebries = Instantiate(debries);
        GameObject effect = Instantiate(particleEffect);
        effect.transform.position = hitLocation;
        tempDebries.transform.position = hitLocation;
        tempDebries.GetComponent<Debries>().ScatterDebries();
    }

    private Ray GetControllerRay()
    {
        Vector3 rayOrigin;
        Vector3 rayDirection;
        switch (OVRInput.activeControllerType)
        {
            case OVRInput.Controller.Touch:
            case OVRInput.Controller.RTouch:
                rayOrigin = cameraRig.rightHandOnControllerAnchor.position;
                rayDirection = cameraRig.rightHandOnControllerAnchor.forward;
                break;

            case OVRInput.Controller.LTouch:
                rayOrigin = cameraRig.leftHandOnControllerAnchor.position;
                rayDirection = cameraRig.leftHandOnControllerAnchor.forward;
                break;
            // hands
            default:
                {
                    var rightHand = cameraRig.rightHandAnchor.GetComponentInChildren<OVRHand>();
                    // can be null if running in Editor with Meta Linq app and the headset is put off
                    if (rightHand != null)
                    {
                        rayOrigin = rightHand.PointerPose.position;
                        rayDirection = rightHand.PointerPose.forward;
                    }
                    else
                    {
                        rayOrigin = cameraRig.centerEyeAnchor.position;
                        rayDirection = cameraRig.centerEyeAnchor.forward;
                    }

                    break;
                }
        }

        return new Ray(rayOrigin, rayDirection);
    }

    private void OnDestructibleMeshCreated(DestructibleMeshComponent destructibleMeshComponentRef)
    {
        destructibleMeshComponent = destructibleMeshComponentRef;
        destructibleMeshComponent.GetDestructibleMeshSegments(globalMeshSegments);
        foreach (var globalMeshSegment in globalMeshSegments)
        {
            globalMeshSegment.AddComponent<MeshCollider>();
        }
    }

    private DestructibleMeshComponent.MeshSegmentationResult AddBarycentricCoordinates(
    DestructibleMeshComponent.MeshSegmentationResult meshSegmentationResult)
    {
        var newSegments = new List<DestructibleMeshComponent.MeshSegment>();

        foreach (var segment in meshSegmentationResult.segments)
        {
            var newSegment = addBarycentricCoordinaatesToMeshSegment(segment);
            newSegments.Add(newSegment);
        }

        var newReservedSegment = addBarycentricCoordinaatesToMeshSegment(meshSegmentationResult.reservedSegment);

        return new DestructibleMeshComponent.MeshSegmentationResult()
        {
            segments = newSegments,
            reservedSegment = newReservedSegment
        };
    }

    private static DestructibleMeshComponent.MeshSegment addBarycentricCoordinaatesToMeshSegment(
        DestructibleMeshComponent.MeshSegment segment)
    {
        using var vs = new NativeArray<Vector3>(segment.positions, Allocator.Temp);
        using var ts = new NativeArray<int>(segment.indices, Allocator.Temp);
        var vertices = new Vector3[ts.Length];
        var idx = new int[ts.Length];
        var barCoord = new Vector4[ts.Length];

        for (var i = 0; i < ts.Length; i += 3)
        {
            vertices[i + 0] = vs[ts[i + 0]];
            vertices[i + 1] = vs[ts[i + 1]];
            vertices[i + 2] = vs[ts[i + 2]];
            barCoord[i + 0] = new Vector4(1, 0, 0, 0); // Barycentric coordinates for vertex 1
            barCoord[i + 1] = new Vector4(0, 1, 0, 0); // Barycentric coordinates for vertex 2
            barCoord[i + 2] = new Vector4(0, 0, 1, 0); // Barycentric coordinates for vertex 3
        }

        for (var i = 0; i < ts.Length; i++) idx[i] = i;

        var newSegment = new DestructibleMeshComponent.MeshSegment()
        {
            indices = idx,
            positions = vertices,
            tangents = barCoord
        };
        return newSegment;
    }
}