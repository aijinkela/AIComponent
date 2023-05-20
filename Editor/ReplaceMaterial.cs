using UnityEngine;
using UnityEditor;

public class ReplaceMaterial : AssetPostprocessor
{
    void OnPostprocessModel(GameObject g)
    {
        // Only process models imported from .blend files
        if (!assetPath.ToLower().EndsWith(".blend")) return;

        // Get a reference to the ShapeColorMat material
        Material shapeColorMat = (Material)AssetDatabase.LoadAssetAtPath("Assets/AIComponent/AIBase/ShapeColorMat.mat", typeof(Material));

        // Get all the mesh renderers in the imported model
        MeshRenderer[] renderers = g.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer r in renderers)
        {
            // Check if the renderer's material is named "Vert"
            if (r.sharedMaterial != null && r.sharedMaterial.name.Contains("Vert"))
            {
                // Assign the ShapeColorMat material to this renderer
                r.sharedMaterial = shapeColorMat;
            }
        }

        // Save the changes to the imported asset
        // AssetDatabase.SaveAssets();
    }
}