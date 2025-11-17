// BoneDriver.cs
// Attach this to the same GameObject (or another manager). Assign physics link transforms (the objects that your configurable joints use)
// and the generated skinned mesh root. It will copy positions/rotations from your physics links to the bone transforms in LateUpdate.

using UnityEngine;

public class BoneDriver : MonoBehaviour {
    public Transform skinnedRoot;       // the root GameObject containing bone_0..bone_5 children created by the generator
    public Transform[] physicsLinks;    // assign your 6 physics link transforms here (ordered top->bottom)

    void LateUpdate() {
        if (skinnedRoot == null || physicsLinks == null) return;
        int boneCount = physicsLinks.Length;
        for (int i = 0; i < boneCount; i++) {
            Transform bone = skinnedRoot.Find($"bone_{i}");
            if (bone == null) continue;
            bone.position = physicsLinks[i].position;
            bone.rotation = physicsLinks[i].rotation;
        }
    }
}


/* README / Instructions (brief)

1) In Unity Editor, create an empty GameObject in your scene. Attach RopeSkinnedMeshGenerator.
2) Set boneCount = 6 and totalLength = 0.125 (already set in inspector by default here). Radius = 0.01 (fat nylon cord).
3) Press the context-menu Generate (right-click the component title in inspector and choose "Generate Rope Skinned Mesh") OR run the scene and call Generate() from code.
4) The script will create a child GameObject named "Rope_Skinned" with a SkinnedMeshRenderer and child bone transforms named bone_0..bone_5.
5) Attach BoneDriver to a manager GameObject. Assign the skinnedRoot to the parent that contains the bones (the GameObject the generator created), and assign physicsLinks[] with your 6 configurable-joint link transforms (top->bottom order).
6) In Play mode, BoneDriver will copy the physics link transforms onto the bones so the skinned mesh follows the physics chain.

Notes:
- You can replace the default Standard material on the SkinnedMeshRenderer with a rope material (normal map + detail) to get a nylon look.
- If you want a slightly flattened strap: change vertex positions in BuildTubeMesh to use an ellipse (multiply x by 1.8) or scale the mesh transform on X.
- If you prefer runtime-only mesh (not saving an asset), remove the AssetDatabase lines and the code will still create a mesh at runtime.
*/
