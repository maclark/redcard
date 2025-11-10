using UnityEngine;

namespace RedCard {

    public class RedBall : MonoBehaviour {


        public Rigidbody rb;

        private void Awake() {
            if (TryGetComponent(out MeshRenderer mr)) mr.materials[0].color = Color.red;
        }

        private void OnCollisionEnter(Collision collision) {
            if (collision.collider.TryGetComponent(out CaptainBody cap)) {
                if (TryGetComponent(out Item it)) {
                    if (it.isInteractable) {
                        //it.isInteractable = false;
                        // actually, just let you take the ball out of the hands of a captain
                        cap.hasBall = true;
                        transform.SetParent(cap.transform, true);
                        transform.localRotation = Quaternion.identity;
                        transform.localScale = .25f * new Vector3(1f / cap.transform.localScale.x, 1f / cap.transform.localScale.y, 1f / cap.transform.localScale.z);
                        rb.isKinematic = true;
                    }
                }
            } 
        }

        private void OnTriggerEnter(Collider other) {
            print("ball trigger with " + other);
        }
    }
}
