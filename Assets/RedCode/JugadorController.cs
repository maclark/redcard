using UnityEngine;
using System;


namespace RedCard {
    public class JugadorController : MonoBehaviour {

        public RefTarget target;
        public CapsuleCollider capsule;
        public Rigidbody rb;
        public Action<Collision> CollisionEnterEvent { get; set; }
        public Vector3 direction;
        public bool isPhysicsEnabled;
        public float moveSpeed;
        public float targetMoveSpeed;
        //public FootballerAnimator anim;
        //public FootballerGraphic graphic;
        //public FootballerUI ui;
        // supposed to be a "new CapsuleCollider collider", but idk why

        private void OnValidate() {
            rb = GetComponent<Rigidbody>();
            target = GetComponent<RefTarget>();
            capsule = GetComponent<CapsuleCollider>();


            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.interpolation = RigidbodyInterpolation.Extrapolate;
            rb.linearDamping = 1;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }

        public void Stop(in float dt) {
            // const float DIRECTION_RECOVERY_WHEN_STOP = 5f;
            // const float STOPPING_SPEED = 5;
            // also, this is had use of lerp, right?
            // but it's thru LateUpdate, which is a constant frame speed
            targetMoveSpeed = Mathf.Lerp(targetMoveSpeed, 0, dt * 5);
            direction = Vector3.Lerp(direction, transform.forward, dt * 5f);
        }
    }
}
