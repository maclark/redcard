using Shared;
using UnityEngine;

namespace RedCard {
    public class OpponentGKDegageBehavior : Behavior {

        private readonly Curve movementCurve = new Curve(new Curve.Point[] {
            new Curve.Point(5, (float)MovementType.Relax),
            new Curve.Point(15, (float)MovementType.Normal),
            new Curve.Point(25, (float)MovementType.BestHeCanDo),
        });

        public override bool Behave(bool isAlreadyActive) {
            if (!IsOpponentGoalKeeperHasTheBallWithProtection()) {
                return false;
            }

            TeamPosture posture = TeamPosture.WaitingForOpponentGK;

            Vector3 tacticalPosition = jugador.GetFieldPosition(
                false,
                posture,
                in xFieldEnd,
                in yFieldEnd,
                ball.transform.position,
                null,
                in xOffsideLine,
                goalNet,
                targetGoalNet
                );

            float distanceToTarget = (tacticalPosition - jugador.Position).magnitude;

            MovementType movement = (MovementType)Mathf.RoundToInt(movementCurve.Evaluate(distanceToTarget));

            jugador.controller.MoveTo(in dt, tacticalPosition, false, movement);
            jugador.FocusToBall(in dt, ball);

            return true;
        }
    }
}
