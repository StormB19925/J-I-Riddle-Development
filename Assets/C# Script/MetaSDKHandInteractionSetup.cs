// 11/25/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class MetaSDKHandInteractionSetup : MonoBehaviour
{
    public GameObject targetPrefab; // Assign the prefab in the Inspector

    public void SetupHandInteraction()
{
        if (targetPrefab == null)
        {
            Debug.LogError("Target prefab is not assigned!");
            return;
        }

        // Add Rigidbody
        Rigidbody rb = targetPrefab.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = targetPrefab.AddComponent<Rigidbody>();
        }
        rb.mass = 1f;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.isKinematic = false;

        // Add Grabbable Component
        Grabbable grabbable = targetPrefab.GetComponent<Grabbable>();
        if (grabbable == null)
        {
            grabbable = targetPrefab.AddComponent<Grabbable>();
        }
        grabbable.InjectOptionalTargetTransform(targetPrefab.transform);
        grabbable.InjectOptionalOneGrabTransformer(grabbable.GetComponent<GrabFreeTransformer>());

        // Add HandGrabInteractable
        HandGrabInteractable handGrabInteractable = targetPrefab.GetComponent<HandGrabInteractable>();
        if (handGrabInteractable == null)
        {
            handGrabInteractable = targetPrefab.AddComponent<HandGrabInteractable>();
        }
        handGrabInteractable.InjectRigidbody(rb);
        handGrabInteractable.InjectOptionalPointableElement(grabbable);

        // Add TouchHandGrabInteractable Component
        TouchHandGrabInteractable touchHandGrabInteractable = targetPrefab.GetComponent<TouchHandGrabInteractable>();
        if (touchHandGrabInteractable == null)
        {
            touchHandGrabInteractable = targetPrefab.AddComponent<TouchHandGrabInteractable>();
        }
        var colliders = new List<Collider>(targetPrefab.GetComponentsInChildren<Collider>());
        touchHandGrabInteractable.InjectAllTouchHandGrabInteractable(targetPrefab.GetComponent<Collider>(), colliders);
        touchHandGrabInteractable.InjectOptionalPointableElement(grabbable);

        Debug.Log("Hand interaction components successfully added to " + targetPrefab.name);
    }
}
