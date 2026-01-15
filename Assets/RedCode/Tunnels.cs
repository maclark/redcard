using UnityEngine;

namespace RedCard {

    public class Tunnels : MonoBehaviour {

        public GameObject ceiling;
        public MeshRenderer[] ceilingBulbs = new MeshRenderer[0];

        private void Awake() {
            ceiling.SetActive(true);

            for (int i = 0; i < ceilingBulbs.Length; i++) {
                ceilingBulbs[i].materials[0].SetColor("_EmissionColor", Color.white * 4f);
            }
        }
    }
}
