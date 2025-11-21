using UnityEngine;

namespace RedCard {
    public class PlayerInputSingleton : MonoBehaviour {

        private static PlayerInputSingleton _instance;

        private void Awake() {
            if (_instance) {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }
    }
}
