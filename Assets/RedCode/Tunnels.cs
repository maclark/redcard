using UnityEngine;

namespace RedCard {

    public class Tunnels : MonoBehaviour {

        public GameObject ceiling;

        private void Awake() {
            ceiling.SetActive(true);
        }
    }
}
