// RopeSkinnedMeshGenerator.cs
// Generates a skinned tubular rope mesh with N bones and smooth weight blending.
// Usage: Attach to an empty GameObject in the scene, set parameters, and click "Generate" in inspector (calls at runtime).

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[ExecuteInEditMode]
public class RopeSkinnedMeshGenerator : MonoBehaviour {
    public int boneCount = 6;                    // matches your physics chain
    public float totalLength = 0.125f;          // user provided
    public float radius = 0.01f;                // fat nylon cord -> diameter 0.02
    public int radialSegments = 12;             // circle detail
    public int lengthSubdivisions = 40;         // mesh subdivisions along length (smoothness)
    public string meshName = "Rope_Skinned";

    [ContextMenu("Generate Rope Skinned Mesh")]
    public void Generate() {
#if UNITY_EDITOR
        // clear children
        for (int i = transform.childCount - 1; i >= 0; i--) {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Create bone transforms
        Transform[] bones = new Transform[boneCount];
        for (int i = 0; i < boneCount; i++) {
            GameObject b = new GameObject($"bone_{i}");
            b.transform.parent = transform;
            float t = (float)i / (boneCount - 1);
            b.transform.localPosition = new Vector3(0f, -t * totalLength, 0f);
            b.transform.localRotation = Quaternion.identity;
            bones[i] = b.transform;
        }

        // Create mesh object
        GameObject meshGO = new GameObject(meshName);
        meshGO.transform.parent = transform;
        meshGO.transform.localPosition = Vector3.zero;
        meshGO.transform.localRotation = Quaternion.identity;

        Mesh mesh = BuildTubeMesh(totalLength, radius, radialSegments, lengthSubdivisions);

        // Setup bindposes and bone weights
        BoneWeight[] weights = new BoneWeight[mesh.vertexCount];
        Matrix4x4[] bindposes = new Matrix4x4[boneCount];

        Vector3[] verts = mesh.vertices;

        // compute bone positions in mesh local space (bones are in object space under transform)
        for (int i = 0; i < boneCount; i++) {
            // bone local position relative to meshGO (both children of this transform)
            Vector3 localPos = bones[i].localPosition - meshGO.transform.localPosition;
            bindposes[i] = meshGO.transform.worldToLocalMatrix * bones[i].localToWorldMatrix;
        }

        // Assign weights: smooth linear blend along Y axis from top (0) to bottom (totalLength)
        for (int v = 0; v < verts.Length; v++) {
            float y = -verts[v].y; // positive from top to bottom
            float t = Mathf.Clamp01(y / totalLength) * (boneCount - 1);
            int b0 = Mathf.FloorToInt(t);
            int b1 = Mathf.Clamp(b0 + 1, 0, boneCount - 1);
            float frac = t - b0;

            BoneWeight bw = new BoneWeight();
            if (b0 == b1) {
                bw.boneIndex0 = b0;
                bw.weight0 = 1f;
            }
            else {
                // distribute across two nearest bones for smooth blending
                bw.boneIndex0 = b0;
                bw.weight0 = 1f - frac;
                bw.boneIndex1 = b1;
                bw.weight1 = frac;
            }
            weights[v] = bw;
        }

        mesh.boneWeights = weights;
        mesh.bindposes = bindposes;

        // Create SkinnedMeshRenderer
        SkinnedMeshRenderer smr = meshGO.AddComponent<SkinnedMeshRenderer>();
        smr.sharedMesh = mesh;
        smr.bones = bones;
        smr.rootBone = bones[0];

        // Default material
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = "RopeMaterial_Default";
        smr.sharedMaterial = mat;

        // Save mesh as asset for reuse (Editor only)
        string assetPath = "Assets/GeneratedMeshes/" + meshName + ".asset";
        System.IO.Directory.CreateDirectory(Application.dataPath + "/GeneratedMeshes");
        AssetDatabase.CreateAsset(mesh, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Generated skinned rope mesh '{meshName}' with {mesh.vertexCount} verts and {boneCount} bones.\nBindposes saved to mesh asset at {assetPath} (Editor only).");
#endif
    }

    Mesh BuildTubeMesh(float length, float radius, int radialSeg, int lengthSeg) {
        Mesh m = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        for (int i = 0; i <= lengthSeg; i++) {
            float t = (float)i / lengthSeg;
            float y = -t * length; // extend downwards
            for (int j = 0; j < radialSeg; j++) {
                float ang = (float)j / radialSeg * Mathf.PI * 2f;
                Vector3 p = new Vector3(Mathf.Cos(ang) * radius, y, Mathf.Sin(ang) * radius);
                verts.Add(p);
                norms.Add(new Vector3(p.x, 0f, p.z).normalized);
                uvs.Add(new Vector2((float)j / radialSeg, t));
            }
            // duplicate first vertex of ring for UV seam if desired - not necessary with indexed mesh
        }

        for (int i = 0; i < lengthSeg; i++) {
            int ringStart = i * radialSeg;
            int nextRingStart = (i + 1) * radialSeg;
            for (int j = 0; j < radialSeg; j++) {
                int jNext = (j + 1) % radialSeg;
                // tri 1
                tris.Add(ringStart + j);
                tris.Add(nextRingStart + j);
                tris.Add(nextRingStart + jNext);
                // tri2
                tris.Add(ringStart + j);
                tris.Add(nextRingStart + jNext);
                tris.Add(ringStart + jNext);
            }
        }

        m.SetVertices(verts);
        m.SetNormals(norms);
        m.SetUVs(0, uvs);
        m.SetTriangles(tris, 0);
        m.RecalculateBounds();
        m.RecalculateTangents();
        return m;
    }
}


