using UnityEngine;

namespace RedCard {

    public class CloseLockerRoom : MonoBehaviour {

        public Transform stadiumLights;

        public Interactable doorInteractable;
        public DoorHinge doorHinge;
        public Interactable lockerLightsInteractable;
        public LightSwitch lightSwitch;

        public LockerRoom lockerRoom;

        private void OnTriggerEnter(Collider other) {
            if (other.GetComponentInParent<RefControls>()) {

                print("deactivating locker room (if needed)");

                foreach (var ceilingLight in lockerRoom.ceilingLights) {
                    ceilingLight.gameObject.SetActive(false);
                }

                doorHinge.enabled = true;
                doorHinge.state = DoorHinge.State.Closing;
                doorHinge.doorCollider.enabled = false;
            }


            // turn on stadium lights!!!!
            stadiumLights.gameObject.SetActive(true);
        }
    }
}
