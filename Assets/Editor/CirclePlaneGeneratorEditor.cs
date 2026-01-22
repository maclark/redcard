using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CirclePlaneGenerator))]
public class CirclePlaneGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        CirclePlaneGenerator generator =
            (CirclePlaneGenerator)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Circle")) {
            Undo.RegisterFullObjectHierarchyUndo(
                generator.gameObject,
                "Generate Circle Planes"
            );

            generator.Generate();
        }

        if (GUILayout.Button("Clear")) {
            Undo.RegisterFullObjectHierarchyUndo(
                generator.gameObject,
                "Clear Circle Planes"
            );

            generator.Clear();
        }
    }
}
