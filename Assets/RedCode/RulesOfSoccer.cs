using UnityEngine;

namespace RedCard {

    public class RulesOfSoccer : MonoBehaviour {
        public GameObject openBook;
        public GameObject closedBook;
        public Rigidbody rb;

        private void Awake() {
            openBook.SetActive(false);
            closedBook.SetActive(true);
            rb.isKinematic = false;

            gameObject.layer = RefControls.Item_Layer;
        }
    }

}
