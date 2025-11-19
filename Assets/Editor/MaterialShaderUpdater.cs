using UnityEngine;
using UnityEditor;
using System.IO;

public class MaterialShaderUpdater : EditorWindow
{
    private Shader oldShader;
    private Shader newShader;

    [MenuItem("Tools/Material Shader Updater")]
    public static void ShowWindow()
    {
        GetWindow<MaterialShaderUpdater>("Material Shader Updater");
    }

    void OnGUI()
    {
        GUILayout.Label("Scene Material Updater", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool finds objects in the current scene using the 'Old Shader' and updates them to use the 'New Shader'. If the material is in a package, it will be copied to 'Assets/Materials/UpdatedMaterials' and then modified.", MessageType.Info);
        
        oldShader = (Shader)EditorGUILayout.ObjectField("Old Shader", oldShader, typeof(Shader), false);
        newShader = (Shader)EditorGUILayout.ObjectField("New Shader", newShader, typeof(Shader), false);

        if (GUILayout.Button("Update Materials in Active Scene"))
        {
            if (oldShader == null || newShader == null)
            {
                EditorUtility.DisplayDialog("Error", "Please select both an old and a new shader.", "OK");
                return;
            }
            
            UpdateSceneMaterials();
        }
    }

    private void UpdateSceneMaterials()
    {
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int updatedRenderers = 0;
        string newMaterialFolder = "Assets/Materials/UpdatedMaterials";

        if (!Directory.Exists(newMaterialFolder))
        {
            Directory.CreateDirectory(newMaterialFolder);
        }

        foreach (Renderer renderer in renderers)
        {
            bool updated = false;
            Material[] sharedMaterials = renderer.sharedMaterials;
            Material[] newMaterials = new Material[sharedMaterials.Length];

            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                Material mat = sharedMaterials[i];
                newMaterials[i] = mat; 

                if (mat != null && mat.shader == oldShader)
                {
                    string path = AssetDatabase.GetAssetPath(mat);
                    Material newMat;

                    if (path.StartsWith("Packages/"))
                    {
                        string newPath = Path.Combine(newMaterialFolder, mat.name + ".mat");
                        newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);
                        
                        AssetDatabase.CopyAsset(path, newPath);
                        newMat = AssetDatabase.LoadAssetAtPath<Material>(newPath);
                    }
                    else
                    {
                        // For materials in Assets, we can create a new instance to be safe,
                        // or modify directly if that's preferred.
                        // Creating a new instance avoids changing a shared material used by other objects unintentionally.
                        newMat = new Material(mat);
                        string newPath = Path.Combine(newMaterialFolder, mat.name + ".mat");
                        newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);
                        AssetDatabase.CreateAsset(newMat, newPath);
                    }

                    newMat.shader = newShader;
                    // Copy properties from the old material to the new one
                    newMat.CopyPropertiesFromMaterial(mat);
                    newMaterials[i] = newMat;
                    EditorUtility.SetDirty(newMat);
                    updated = true;
                }
            }

            if (updated)
            {
                renderer.sharedMaterials = newMaterials;
                EditorUtility.SetDirty(renderer);
                updatedRenderers++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"Updated materials on {updatedRenderers} objects in the scene. Copied package materials to '{newMaterialFolder}'.", "OK");
    }
}
