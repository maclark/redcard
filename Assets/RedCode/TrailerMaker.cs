using UnityEngine;
using UnityEngine.InputSystem;


namespace RedCard {

    public class TrailerMaker : MonoBehaviour {

        public bool go = false;
        public float t = 0f;
        public float duration = 1f;
        public AnimationCurve curve;
        public RectTransform canvas;
        public float delay = 0f;


        public Vector3 offScreenPos;
        public Vector3 offScreenRotation;
        public Vector3 endCanvasPos;

        public static float delayEverything = 2f;

        private void Awake() {
            endCanvasPos = transform.position;
        }

        private void Update() {

            if (Keyboard.current.gKey.wasPressedThisFrame) {
                go = true;
                t = -(delay + delayEverything);
            }

            if (go) {
                t += Time.deltaTime;
                float s = curve.Evaluate(t / duration);
                canvas.transform.position = Vector3.Lerp(offScreenPos, endCanvasPos, s);
                canvas.transform.rotation = Quaternion.Slerp(Quaternion.Euler(offScreenRotation), Quaternion.identity, s);
                if (t >= duration) {
                    go = false;
                }
            }
        }
    }
}
