using Shared;
using UnityEngine;

namespace RedCard {
    public class OurGKDegageBehaviour : Behavior {
        private readonly Curve movementCurve = new Curve(new Curve.Point[] {
            new Curve.Point (5, (float)MovementType.Relax),
            new Curve.Point (10, (float)MovementType.Normal),
            new Curve.Point (15, (float)MovementType.BestHeCanDo),
        });

        public override bool Behave(bool isAlreadyActive) {
            if (!IsOurGoalKeeperHasTheBallWithProtection()) {
                return false;
            }

            TeamPosture posture = TeamPosture.WaitingForGK;

            var tacticalPosition = jugador.GetFieldPosition(
                false,
                posture,
                in xFieldEnd,
                in yFieldEnd,
                ball.transform.position,
                null,
                in xOffsideLine,
                goalNet,
                targetGoalNet);

            float distanceToTarget = (tacticalPosition - jugador.Position).magnitude;

            var movement = (MovementType)Mathf.RoundToInt(movementCurve.Evaluate(distanceToTarget));

            jugador.controller.MoveTo(in dt, tacticalPosition, false, movement);

            jugador.FocusToBall(in dt, ball);

            return true;
        }
    }
}
