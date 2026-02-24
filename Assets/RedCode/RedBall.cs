using UnityEngine;
using System;

namespace RedCard {

    public class RedBall : MonoBehaviour {

        [SerializeField] private AnimationCurve followSpeedCurve;
        [SerializeField] private float holdedBallFollowSpeed = 1f;

        [Header("ASSIGNATIONS")]
        public Rigidbody rb;

        [Header("VARS")]
        public Jugador holder;
        public Jugador thrower;

        private Vector3 holdingPosition;
        private float followSpeedProgress;
        private float followSpeed;


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

        /// <summary>
        /// Get direction error. Target dir should not be normalized since we gonna use its magnitude on calculation.
        /// </summary>
        /// <returns>Returns a direction vector (not normalized). It has same velocity magnitude with the given vector</returns>
        public static Vector3 ApplyDirectionError(Vector3 targetDir, float skill, in float maxErrorAngle = -1) {
            Quaternion dirError = GetDirectionError(targetDir.magnitude, skill, in maxErrorAngle);
            return dirError * targetDir;
        }

        public static Quaternion GetDirectionError(float velocityMagnitude, float skill, in float maxAngleError = -1) {
            if (!RedMatch.match.settings.IsDirectionErrorEnabled) {
                return Quaternion.identity;
            }

            float maxDirError = RedMatch.match.settings.DirectionErrorModByVelocityCurve.Evaluate(velocityMagnitude) *
                RedMatch.match.settings.DirectionErrorSkillModCurve.Evaluate(skill / 100f);

            float @error = UnityEngine.Random.Range(maxDirError / 4f, maxDirError);

            float @sideForward = UnityEngine.Random.Range(0, 10) > 4 ? -1 : 1;
            float @sideUp = UnityEngine.Random.Range(0, 10) > 4 ? -1 : 1;

            if (maxAngleError > 0) {
                // clamp error.
                error = Mathf.Min(maxAngleError, error);
            }

            return Quaternion.Euler(error * sideUp, error * sideForward, 0);
        }

        // "Returns true if progress is completed.", but its result is unused in FS?
        public bool HolderBehave(Vector3 position, Quaternion rotation, in float dt, float speedMod) {

            if (holder == null) return false; ///////// early ret ////////

            if (RedMatch.match.matchStatus == MatchStatus.WaitingForKickOff ||
                RedMatch.match.matchStatus == MatchStatus.NotPlaying) {
                return false; ///////// earl e. re turn
            }

            followSpeedProgress = Mathf.Min(1, followSpeedProgress+ dt + holdedBallFollowSpeed * speedMod);
            followSpeed = followSpeedCurve.Evaluate(followSpeedProgress) * 1; // why * 1?

            // supposed to zero out velocity
            // idk, if there's a holder, isn't rigidbody non kinematic?
            if (!rb.isKinematic) {
                Debug.LogWarning("ball is non-kinematic with holder!");
                rb.linearVelocity = Vector3.zero;
            }

            Vector3 targetPosition = Vector3.Lerp(
                holdingPosition,
                position,
                followSpeed);

            if (RedMatch.match.matchStatus.HasFlag(MatchStatus.Playing)) {
                // ball should be in the field when held by jugador ... maybe
                Vector2 fieldSize = RedMatch.match.fieldSize;
                targetPosition.x = Mathf.Clamp(targetPosition.x, 0, fieldSize.x);
                targetPosition.z = Mathf.Clamp(targetPosition.x, 0, fieldSize.y); // y is z!
            }

            transform.position = targetPosition;
            transform.rotation = rotation;

            return followSpeed >= 1; // what's this about?
        }
    }
}
