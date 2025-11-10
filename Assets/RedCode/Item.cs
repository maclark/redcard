using UnityEngine;

namespace RedCard {
    public class Item : MonoBehaviour {
        public ItemName iName;
        public float mass = 1f;
        public bool isInteractable = true;

        private void Awake() {
            if (iName == ItemName.Unset) Debug.LogWarning("unset item name on " + name);
        }
    }
}
