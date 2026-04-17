// 11/20/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEngine;

public class ResetArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object has a "Resettable" component
        Resettable resettable = other.GetComponent<Resettable>();
        if (resettable != null)
        {
            // Reset the object's position
            resettable.ResetPosition();
        }
    }
}
