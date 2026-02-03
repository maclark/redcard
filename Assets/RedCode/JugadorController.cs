using UnityEngine;
using System;


namespace RedCard {
    public class JugadorController : MonoBehaviour {

        public RefTarget target;
        public CapsuleCollider capsule;
        public Action<Collision> CollisionEnterEvent { get; set; }
        public Vector3 direction;
        public bool isPhysicsEnabled;
        public float moveSpeed;
        public float targetMoveSpeed;
        //public FootballerAnimator anim;
        //public FootballerGraphic graphic;
        //public FootballerUI ui;


        
        private Rigidbody rb;
        // supposed to be a "new CapsuleCollider collider", but idk why

        private void OnValidate() {
            rb = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();


            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.interpolation = RigidbodyInterpolation.Extrapolate;
            rb.linearDamping = 1;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }

    }
}
