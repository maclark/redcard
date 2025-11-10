using UnityEngine;
using UnityEditor;
using RedCard;

public class AddOutlineHighlightEditor : EditorWindow {

    public const string OUTLINE_MAT_PATH = "Assets/RedCard/SimpleOutlineMaterial.mat";

    [MenuItem("GameObject/Add Outline Highlight", false, 11)]
    public static void AddOutline() {
        GameObject selected = Selection.activeGameObject;

        if (selected == null) {
            Debug.LogError("No GameObject selected.");
            return;
        }

        MeshFilter mf = selected.GetComponent<MeshFilter>();
        MeshRenderer mr = selected.GetComponent<MeshRenderer>();

        if (mf == null || mr == null) {
            Debug.LogError("Selected GameObject must have MeshFilter and MeshRenderer.");
            return;
        }

        // Try loading from a fixed path, or create a default one
        Material outlineMaterial = AssetDatabase.LoadAssetAtPath<Material>(OUTLINE_MAT_PATH);
        if (outlineMaterial == null) {
            Debug.LogError("Outline material not found at " + OUTLINE_MAT_PATH);
            return;
        }

        if (selected.TryGetComponent(out RefTarget target)) {
            if (target.outline) {
                Debug.LogWarning(selected.name + " already has an outline assigned");
                return;
            }
        }
        else {
            Debug.LogWarning("Trying to put a highlight outline on a non ref target: " + selected.name);
            return;
        }

        // Create outline object
        GameObject outlineObj = new GameObject("Outline");
        outlineObj.transform.SetParent(selected.transform, false);
        outlineObj.transform.localPosition = Vector3.zero;
        outlineObj.transform.localRotation = Quaternion.identity;
        outlineObj.transform.localScale = Vector3.one * 1.05f;
        target.outline = outlineObj;
        target.outline.SetActive(false);

        // Copy mesh filter and renderer
        MeshFilter outlineFilter = outlineObj.AddComponent<MeshFilter>();
        outlineFilter.sharedMesh = mf.sharedMesh;

        MeshRenderer outlineRenderer = outlineObj.AddComponent<MeshRenderer>();
        outlineRenderer.sharedMaterial = outlineMaterial;
        outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        outlineRenderer.receiveShadows = false;

        Debug.Log("Outline added to " + selected.name);
    }
}


