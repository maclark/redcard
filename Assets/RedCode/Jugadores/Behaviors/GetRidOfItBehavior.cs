
using UnityEngine;
using System.Linq;

namespace RedCard {
    /// <summary>
    /// When we are close to our goal net, if someone is around us.
    /// </summary>
    public class GetRidOfItBehavior : Behavior { //BallChasingBehaviour {
        private readonly float maxBallProgress = 0.2f;

        private const float RISK_AREA = 3f;

        private readonly float sendAwayPowerMin = 26;
        private readonly float sendAwayPowerMax = 38;

        private Vector3 targetSendAwayPosition;

        public override bool Behave(bool isAlreadyActive) {
            if (!jugador.isHoldingBall) {
                return false;
            }

            if (jugador.team.BallProgress > maxBallProgress) {
                return false;
            }

            if (!isAlreadyActive) {
                // check around.
                Vector3 myPos = jugador.Position;
                if (
                    opponents.Where(j => j.controller.IsPhysicsEnabled &&
                    Vector3.Distance(j.Position, myPos) < RISK_AREA).Any()) {
                    var forward = jugador.attackingDir;

                    var forwardLook = Quaternion.LookRotation(forward);
                    var myLook = Quaternion.LookRotation(jugador.controller.dir);

                    forward = Quaternion.Slerp(myLook, forwardLook, 0.5f) * Vector3.forward;

                    targetSendAwayPosition = jugador.Position + forward * Random.Range(sendAwayPowerMin, sendAwayPowerMax);

                    isAlreadyActive = true;
                }
            }

            if (isAlreadyActive) {
                if (jugador.controller.LookTo(in dt, targetSendAwayPosition - jugador.Position)) {
                    jugador.Cross(targetSendAwayPosition);
                }

                return true;
            }

            return false;
        }
    }
}
