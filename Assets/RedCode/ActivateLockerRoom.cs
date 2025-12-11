using UnityEngine;


namespace RedCard {
    public class ActivateLockerRoom : MonoBehaviour {

        public Transform stadiumLights;
        public LockerRoom lockerRoom;


        private void Awake() {
            DeactivateStadium();
        }

        private void OnTriggerEnter(Collider other) {
            if (other.GetComponentInParent<RefControls>()) {
                DeactivateStadium();
            }
        }

        private void DeactivateStadium() { 

            print("activating locker room (if needed)");
            stadiumLights.gameObject.SetActive(false);

            foreach (var cl in lockerRoom.ceilingLights) {
                cl.gameObject.SetActive(true);
            }
        }

    }
}
