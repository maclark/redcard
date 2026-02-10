
using UnityEngine;

namespace RedCard {
    public abstract class AbstractShootingBehaviour : Behavior {
        private const float MIN_Z_DISTANCE_SQR_TO_ANGLE_CHECK = 4;
        private const float MAX_X_DISTANCE_SQR_TO_ANGLE_CHECK = 6;
        private const float MAX_ANGLE = 80;

        protected bool CanShoot () {
            var goalToMe = jugador.pos - opponentGoalNet.pos;

            if (Mathf.Abs (goalToMe.z) > MIN_Z_DISTANCE_SQR_TO_ANGLE_CHECK && 
                Mathf.Abs (goalToMe.x) <= MAX_X_DISTANCE_SQR_TO_ANGLE_CHECK) {

                var angleBetweenGoal = AngleToGoal(opponentGoalNet);

                if (angleBetweenGoal > MAX_ANGLE) {
                    Debug.Log("Cannot shoot.");
                    return false;
                }
            }

            return true;
        }

        protected float AngleToGoal (GoalNet goalNet) {
            Vector3 playerPosition = jugador.pos;

            Vector3 dirToLeft = goalNet.leftLimit.position - playerPosition;
            Vector3 dirToRight = goalNet.rightLimit.position - playerPosition;

            // was using jugador.GoalDirection instead of goalnet.backward
            float leftAngle = Mathf.Abs (Vector3.SignedAngle(-goalNet.transform.forward, dirToLeft, Vector3.up));
            float rightAngle = Mathf.Abs(Vector3.SignedAngle(-goalNet.transform.forward, dirToRight, Vector3.up));

            float final = Mathf.Min (leftAngle, rightAngle);

            return final;
        }
    }
}
