using UnityEngine;
using UnityEngine.InputSystem;

namespace RedCard {

    public class MainMenu : MonoBehaviour {

        public Rigidbody rbWhistle;
        public float bumpPower = 0.25f;


        private void Update() {
            if (Keyboard.current.spaceKey.wasPressedThisFrame) {
                Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                randomDir.Normalize();
                if (rbWhistle) rbWhistle.AddForce(bumpPower * randomDir, ForceMode.Impulse);
            }
        }
    }
}
