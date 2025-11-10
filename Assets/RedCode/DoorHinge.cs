using UnityEngine;

namespace RedCard {
    public class DoorHinge : MonoBehaviour {
        public enum State {
            Closed,
            Opened,
            Closing,
            Opening
        }

        public float fullyOpenAngle;
        public State state = State.Closed;
        public float t = 0f;
        public float timeToOpen = .5f; 
        public BoxCollider doorCollider;
        public MeshRenderer knobRenderer0;
        public MeshRenderer knobRenderer1;

        private void Awake() {
            transform.localRotation = Quaternion.identity;
            knobRenderer0.materials[0].color = Color.yellow;
            knobRenderer1.materials[0].color = Color.yellow;
        }

        void Update() {
            switch (state) {
                case State.Opening:
                    t += Time.deltaTime * 1f/timeToOpen;
                    if (t > 1f) {
                        t = 1f;
                        state = State.Opened;
                    }
                    break;
                case State.Closing:
                    t -= Time.deltaTime * 1f/timeToOpen;
                    if (t < 0f) {
                        t = 0f;
                        state = State.Closed;
                    }
                    break;

                default:
                    doorCollider.enabled = true;
                    enabled = false;
                    break;
            }

            transform.localRotation = Quaternion.Euler(0, Mathf.Lerp(0f, fullyOpenAngle, t), 0f);
        }
    }
}
