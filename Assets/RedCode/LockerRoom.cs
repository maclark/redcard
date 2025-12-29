using UnityEngine;

namespace RedCard {

    public class LockerRoom : MonoBehaviour {

        public GameObject ceiling;
        public Light[] ceilingLights = new Light[0];
        public MeshRenderer[] ceilingBulbs = new MeshRenderer[0];

        private void Awake() {

            if (ceiling) ceiling.SetActive(true);
            else Debug.LogError("missing locker room ceiling");

            // idk about this, in Interactble, when flipping switch
            // i assume these two are equal
            Debug.Assert(ceilingBulbs.Length == ceilingLights.Length);

            for (int i = 0; i < ceilingBulbs.Length; i++) {
                ceilingBulbs[i].materials[0].SetColor("_EmissionColor", Color.white * 4f);
            }
        }
    }
}
