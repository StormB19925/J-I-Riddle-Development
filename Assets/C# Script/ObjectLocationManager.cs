using UnityEngine;
using System.Collections.Generic;

public class ObjectLocationManager : MonoBehaviour
{
    [Header("References")]
    public GameObject tableObject;
    public List<GameObject> objectsOnTable = new List<GameObject>();
    public Transform playerCamera;

    [Header("Placement Settings")]
    public float spawnDistance = 1.0f;
    public float tableRootYOffset = 0f;
    public float objectSpacing = 0.3f;
    
    [Header("Surface Selection")]
    public string preferredSurfaceName = "polySurface7"; // The main desk surface
    public float preferredSurfaceHeight = 0.75f;

    [Header("Object Orientation")]
    public float objectRotationYOffset = 0f;
    public bool facePlayer = true;

    private void Start()
    {
        // Position objects on start
        PositionObjects();
    }

    /// <summary>
    /// Positions the table in front of the player and places objects on top of it.
    /// </summary>
    [ContextMenu("Position Objects")]
    public void PositionObjects()
    {
        // 1. Identify Camera
        if (playerCamera == null)
        {
            playerCamera = Camera.main != null ? Camera.main.transform : null;
            if (playerCamera == null)
            {
                GameObject eyeAnchor = GameObject.Find("CenterEyeAnchor");
                if (eyeAnchor != null) playerCamera = eyeAnchor.transform;
            }

            if (playerCamera == null)
            {
                Debug.LogError("[ObjectLocationManager] Player Camera not found!");
                return;
            }
        }

        // 2. Identify Table
        if (tableObject == null)
        {
            tableObject = GameObject.Find("Table");
            if (tableObject == null)
            {
                Debug.LogError("[ObjectLocationManager] Table object not found in scene!");
                return;
            }
        }

        // 3. Collect Objects if list is empty
        if (objectsOnTable.Count == 0)
        {
            AddObjectToList("Teddy Bear With Rig");
            AddObjectToList("BigRedButton");
            AddObjectToList("[BuildingBlock] TouchHandGrab Cube");
            AddObjectToList("Cube");
        }

        // 4. Calculate Table Position and Orientation
        Vector3 cameraPos = playerCamera.position;
        Vector3 cameraForward = playerCamera.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        // Place table in front of player
        Vector3 tableWorldPos = cameraPos + cameraForward * spawnDistance;
        tableWorldPos.y = tableRootYOffset;
        tableObject.transform.position = tableWorldPos;
        
        // Face the player
        Vector3 tableLookTarget = cameraPos;
        tableLookTarget.y = tableWorldPos.y;
        tableObject.transform.LookAt(tableLookTarget);
        tableObject.transform.Rotate(0, 180, 0); // Rotate 180 so the front of the desk faces the player

        // 5. Find the best surface
        Transform surfaceTransform = tableObject.transform.Find(preferredSurfaceName);
        Renderer tabletopRenderer = null;
        
        if (surfaceTransform != null)
        {
            tabletopRenderer = surfaceTransform.GetComponent<Renderer>();
        }
        
        if (tabletopRenderer == null)
        {
            // Fallback: Find surface closest to preferred height
            Renderer[] allRenderers = tableObject.GetComponentsInChildren<Renderer>();
            float bestHeightDiff = float.MaxValue;
            foreach (var r in allRenderers)
            {
                if (r.bounds.size.x < 0.15f || r.bounds.size.z < 0.15f) continue;
                float diff = Mathf.Abs(r.bounds.max.y - preferredSurfaceHeight);
                if (diff < bestHeightDiff)
                {
                    bestHeightDiff = diff;
                    tabletopRenderer = r;
                }
            }
        }

        if (tabletopRenderer != null)
        {
            float surfaceY = tabletopRenderer.bounds.max.y;
            Vector3 surfaceCenter = tabletopRenderer.bounds.center;

            // 6. Position Objects on the selected surface
            List<GameObject> activeObjects = new List<GameObject>();
            foreach (var obj in objectsOnTable)
            {
                if (obj != null && obj.activeInHierarchy) activeObjects.Add(obj);
            }

            int count = activeObjects.Count;
            if (count > 0)
            {
                // Align objects in a row relative to the table's right axis
                float rowWidth = (count - 1) * objectSpacing;
                Vector3 rowStart = surfaceCenter - tableObject.transform.right * (rowWidth / 2f);

                for (int i = 0; i < count; i++)
                {
                    GameObject obj = activeObjects[i];
                    Vector3 objTargetPos = rowStart + tableObject.transform.right * (i * objectSpacing);
                    
                    // Determine height offset based on collider or renderer
                    float bottomToRootOffset = 0;
                    Collider col = obj.GetComponentInChildren<Collider>();
                    Renderer ren = obj.GetComponentInChildren<Renderer>();
                    if (col != null) bottomToRootOffset = obj.transform.position.y - col.bounds.min.y;
                    else if (ren != null) bottomToRootOffset = obj.transform.position.y - ren.bounds.min.y;
                    
                    objTargetPos.y = surfaceY + bottomToRootOffset;
                    obj.transform.position = objTargetPos;
                    
                    if (facePlayer)
                    {
                        Vector3 lookTarget = cameraPos;
                        lookTarget.y = objTargetPos.y;
                        obj.transform.LookAt(lookTarget);
                        // Standard assets face +Z. Let's try 0 offset first (facing player).
                        obj.transform.Rotate(0, objectRotationYOffset, 0);
                    }
                }
                
                Debug.Log($"[ObjectLocationManager] Table moved to {tableWorldPos}. {count} objects placed on {tabletopRenderer.name} at height {surfaceY}.");
            }
        }
        else
        {
            Debug.LogError("[ObjectLocationManager] No suitable tabletop surface found.");
        }
    }

    private void AddObjectToList(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null && !objectsOnTable.Contains(obj))
        {
            objectsOnTable.Add(obj);
        }
    }
}
