// 11/20/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;

public class Resettable : MonoBehaviour
{
    private Vector3 startPosition;

    private void Start()
    {
        // Store the starting position of the object
        startPosition = transform.position;
    }

    public void ResetPosition()
    {
        // Reset the object to its starting position
        transform.position = startPosition;
    }
}
