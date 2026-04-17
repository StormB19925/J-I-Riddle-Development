// 12/2/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Collections.Generic; // Required for List

public class FixTeddyBearInteractionSetup : MonoBehaviour
{
    public GameObject teddyBearPrefab; // Assign the "Teddy Bear With Rig" prefab in the Inspector

    /// <summary>
    /// Instantiates the prefab and applies interaction components to the instance.
    /// </summary>
    public void FixSetup()
    {
        if (teddyBearPrefab == null)
        {
            Debug.LogError("Teddy Bear prefab is not assigned!");
            return;
        }

        // Instantiate the prefab to avoid modifying the asset directly at runtime
        GameObject teddyBearInstance = Instantiate(teddyBearPrefab, this.transform.position, this.transform.rotation);
        teddyBearInstance.name = "InteractiveTeddyBear";

        // Ensure Rigidbody exists
        Rigidbody rb = teddyBearInstance.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = teddyBearInstance.AddComponent<Rigidbody>();
        }
        rb.mass = 1f;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.isKinematic = false;

        // Ensure BoxCollider exists
        BoxCollider boxCollider = teddyBearInstance.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = teddyBearInstance.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(0.29f, 0.50f, 0.50f); // Adjust size as needed
            boxCollider.center = Vector3.zero;
            boxCollider.isTrigger = false;
        }

        // Ensure GrabFreeTransformer exists for the Grabbable
        GrabFreeTransformer grabFreeTransformer = teddyBearInstance.GetComponent<GrabFreeTransformer>();
        if (grabFreeTransformer == null)
        {
            grabFreeTransformer = teddyBearInstance.AddComponent<GrabFreeTransformer>();
        }

        // Ensure Grabbable Component exists
        Grabbable grabbable = teddyBearInstance.GetComponent<Grabbable>();
        if (grabbable == null)
        {
            grabbable = teddyBearInstance.AddComponent<Grabbable>();
        }
        grabbable.InjectOptionalTargetTransform(teddyBearInstance.transform);
        grabbable.InjectOptionalOneGrabTransformer(grabFreeTransformer);

        // Add HandGrabInteractable for direct grabbing
        HandGrabInteractable handGrabInteractable = teddyBearInstance.GetComponent<HandGrabInteractable>();
        if (handGrabInteractable == null)
        {
            handGrabInteractable = teddyBearInstance.AddComponent<HandGrabInteractable>();
        }
        handGrabInteractable.InjectRigidbody(rb);
        handGrabInteractable.InjectOptionalPointableElement(grabbable);

        // Ensure TouchHandGrabInteractable is correctly set up
        TouchHandGrabInteractable touchHandGrabInteractable = teddyBearInstance.GetComponent<TouchHandGrabInteractable>();
        if (touchHandGrabInteractable == null)
        {
            touchHandGrabInteractable = teddyBearInstance.AddComponent<TouchHandGrabInteractable>();
        }
        var colliders = new List<Collider>(teddyBearInstance.GetComponentsInChildren<Collider>());
        touchHandGrabInteractable.InjectAllTouchHandGrabInteractable(boxCollider, colliders);
        touchHandGrabInteractable.InjectOptionalPointableElement(grabbable);

        Debug.Log("Teddy Bear setup fixed successfully on instance: " + teddyBearInstance.name);
    }
}
