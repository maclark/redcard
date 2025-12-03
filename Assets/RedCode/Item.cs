using UnityEngine;

namespace RedCard {

    public class Item : MonoBehaviour {

        public ItemName iName;
        public float mass = 1f;
        public bool isInteractable = true;


        public delegate bool ItemAction(UnityEngine.InputSystem.InputAction.CallbackContext ctx, RefControls arbitro);
        public ItemAction onHeld;
        public ItemAction onPrimary;
        public ItemAction onSecondary;
        public ItemAction onGrabbed;
        public ItemAction onDropped;

        private void Awake() {
            if (iName == ItemName.Unset) Debug.LogWarning("unset item name on " + name);
        }
    }
}
