using UnityEngine;
using System.IO;

public class RenderTextureSaver : MonoBehaviour {

    public bool saveTexture = false;
    public string filePath;
    public RenderTexture rt;

    private void Update() {
        if (saveTexture) {
            SaveRenderTextureToPNG(rt, filePath);
            saveTexture = false;
        }
    }

    public static void SaveRenderTextureToPNG(RenderTexture rt, string filePath) {
        // Backup the currently active RenderTexture
        RenderTexture currentActiveRT = RenderTexture.active;

        // Make the RenderTexture the active one
        RenderTexture.active = rt;

        // Create a Texture2D with same dimensions
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        // Encode to PNG
        byte[] bytes = tex.EncodeToPNG();

        // Write to file
        File.WriteAllBytes(filePath, bytes);

        // Restore active RenderTexture
        RenderTexture.active = currentActiveRT;

        // Cleanup
        Object.Destroy(tex);

#if UNITY_EDITOR
        Debug.Log("Saved RenderTexture to: " + filePath);
#endif
    }
}
