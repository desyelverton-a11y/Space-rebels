using UnityEngine;

/// <summary>
/// Attach this script to a GameObject with a MeshFilter (e.g., a Cube)
/// to invert its normals so the inside is visible.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
public class InvertNormals : MonoBehaviour
{
    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.mesh == null)
        {
            Debug.LogError("No MeshFilter or mesh found on this GameObject.");
            return;
        }

        Mesh mesh = mf.mesh;

        // 1. Invert normals
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -normals[i];
        }
        mesh.normals = normals;

        // 2. Reverse triangle winding order
        for (int subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
        {
            int[] triangles = mesh.GetTriangles(subMesh);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                // Swap order of vertices to flip face direction
                int temp = triangles[i];
                triangles[i] = triangles[i + 1];
                triangles[i + 1] = temp;
            }
            mesh.SetTriangles(triangles, subMesh);
        }

        Debug.Log("Mesh normals inverted successfully!");
    }

    // Update is called once per frame
    void Update()
    {

    }
}