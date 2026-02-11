using UnityEngine;
using System;


namespace RedCard {
    public class JugadorController : MonoBehaviour {

        public RefTarget target;
        public CapsuleCollider capsule;
        public Rigidbody rb;
        public Jugador jugador;

        public Action<Collision> CollisionEnterEvent { get; set; }
        public Vector3 dir;
        public bool IsPhysicsEnabled;
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

        public void SetOffside(bool isInOffide) {
            //#UI
            //playerUI.SetBool(PlayerUI.UIAnimatorVariable.ShowOffside, isInOffide);
        }

        public bool MoveTo(
            in float dT,
            Vector3 to,
            bool faceTowards = true,
            MovementType movementType = MovementType.BestHeCanDo) {
            return MoveTo(in dT, to, faceTowards, movementType);
        }

        public void Stop(in float dt) {
            // const float DIRECTION_RECOVERY_WHEN_STOP = 5f;
            // const float STOPPING_SPEED = 5;
            // also, this is had use of lerp, right?
            // but it's thru LateUpdate, which is a constant frame speed
            targetMoveSpeed = Mathf.Lerp(targetMoveSpeed, 0, dt * 5);
            dir = Vector3.Lerp(dir, transform.forward, dt * 5f);
        }

        /// <summary>
        /// LookTo direction, returns true if we are already looking there.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool LookTo(in float dt, Vector3 lookDirection) {
            const float ANGLE_TO_APPROVE_LOOKAT = 60f;
            const float ANGLE_TO_APPROVE_ADD_BY_PER_BALL_HEIGHT_WHEN_HOLDING_BALL = 60f;

            if (lookDirection == Vector3.zero || transform.forward == lookDirection) {
                return true;
            }

            lookDirection.y = 0;

            float agileSpeed = AgileToDirection(lookDirection).turnResult;
            RedBall matchBall = RedMatch.match.matchBall;
            bool holdingBall = matchBall.holder == jugador;
            bool throwingBall = matchBall.thrower == jugador;


            // inlined LerpRotation()
            float agility = agileSpeed * (holdingBall ? RedMatch.match.settings.AgileToDirectionWhenHoldingBallModifier : 1);
            rb.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookDirection),
                dt * agility);

            transform.rotation = rb.rotation;

            float angle = Vector3.SignedAngle(transform.forward, lookDirection, Vector3.up);

            float ballHeightMod = 0;

            if (holdingBall && !throwingBall) {
                ballHeightMod = matchBall.transform.position.y * ANGLE_TO_APPROVE_ADD_BY_PER_BALL_HEIGHT_WHEN_HOLDING_BALL;
            }

            if (Mathf.Abs(angle) <= (throwingBall ? 10 : ANGLE_TO_APPROVE_LOOKAT + ballHeightMod)) {
                return true;
            }

            return false;
        }
        private (float turnResult, float angleDifferency) AgileToDirection(Vector3 targetDirection) {
            if (dir == targetDirection) {
                return (1, 0);
            }

            float angleDifferency = Mathf.Abs(Vector3.SignedAngle(dir, targetDirection, Vector3.up));

            float agility = jugador.GetAgility();

            float turnDifficulty =
                RedMatch.match.settings.AgileToDirectionMoveSpeedHardness.Evaluate(moveSpeed) *
                RedMatch.match.settings.AgileToDirectionAngleDifferencyHardness.Evaluate(angleDifferency);

            float turnResult = agility / (turnDifficulty + 1);

            return (turnResult, angleDifferency);
        }

    }
}
