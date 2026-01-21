using UnityEngine;


namespace RedCode {
    public class AmbientLightSetter : MonoBehaviour {

        public Color ambientColor;
        public UnityEngine.Rendering.AmbientMode ambientMode;

        void Update() {
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientLight = ambientColor;
        }
    }
}
