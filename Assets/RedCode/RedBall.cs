using UnityEngine;
using System;

namespace RedCard {

    public class RedBall : MonoBehaviour {


        [Header("ASSIGNATIONS")]
        public Rigidbody rb;

        [Header("VARS")]
        public Jugador holder;
        public Jugador thrower;

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
    }
}
