using UnityEngine;
using UnityEditor;
using System.IO;

public class MaterialShaderUpdater : EditorWindow
{
    private Shader oldShader;
    private Shader newShader;
    private bool updateInPlace = false;

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
        updateInPlace = EditorGUILayout.Toggle("Update In Place", updateInPlace);
        EditorGUILayout.HelpBox("If enabled, materials in Assets will be modified directly instead of creating copies.", MessageType.Info);

        if (GUILayout.Button("Update Materials in Active Scene", GUILayout.Height(30)))
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
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.InstanceID);
        int updatedRenderers = 0;
        string newMaterialFolder = "Assets/Materials/UpdatedMaterials";

        if (!updateInPlace && !Directory.Exists(newMaterialFolder))
        {
            Directory.CreateDirectory(newMaterialFolder);
            AssetDatabase.Refresh();
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
                    else if (updateInPlace)
                    {
                        newMat = mat;
                    }
                    else
                    {
                        newMat = new Material(mat);
                        string newPath = Path.Combine(newMaterialFolder, mat.name + ".mat");
                        newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);
                        AssetDatabase.CreateAsset(newMat, newPath);
                    }

                    if (newMat != null)
                    {
                        newMat.shader = newShader;
                        newMat.CopyPropertiesFromMaterial(mat);
                        newMaterials[i] = newMat;
                        EditorUtility.SetDirty(newMat);
                        updated = true;
                    }
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

        EditorUtility.DisplayDialog("Success", $"Updated materials on {updatedRenderers} objects in the scene.", "OK");
    }
}
