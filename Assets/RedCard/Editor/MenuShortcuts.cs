using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class MenuShortcuts
{
    private const string PLAYGROUND_PATH = "Assets/RedCard/Playground.unity";
    private const string SIMULATOR_START_PATH = "Assets/FootballSimulator/_StartingScene.unity";

    private static void PlayScene(string scenePath) {

        if (EditorApplication.isPlaying) {
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () => PlayScene(scenePath);
        }
        else {
            var currentScene = EditorSceneManager.GetActiveScene();
            if (currentScene.path != scenePath) {
                Debug.Log("opening " + scenePath + " scene...");
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                    Debug.LogWarning("when would this ever happen? user doesn't want to save, but we're using a shortcut?");
                    return;
                }
                EditorSceneManager.OpenScene(scenePath);
            }
            else Debug.Log(scenePath + " already open! gogo");

            EditorApplication.isPlaying = true;
        }
    }

    [MenuItem("StartScene.../PlayPlayground %#asdf", priority = 10)]
    public static void PlayPlayground() {
        PlayScene(PLAYGROUND_PATH);
    }
    
    [MenuItem("StartScene.../PlaySimulatorStart %#Equals", priority = 11)]
    public static void PlaySimulatorStart() {
        PlayScene(SIMULATOR_START_PATH);
    }
}
