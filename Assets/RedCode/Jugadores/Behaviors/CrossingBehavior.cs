
using UnityEngine;

using System.Linq;
using Shared;

namespace RedCard {
    /// <summary>
    /// Cross to the penalty area.
    /// </summary>
    public class CrossingBehaviour : Behavior {
        private Jugador target;

        private const float MIN_DISTANCE_TO_TARGET = 16;
        private const float MAX_X_DIFF = 20;
        private const float MIN_Z_DIFF = 14;

        private readonly float minBallProgress;
        private readonly float chanceMultiplier;

        public CrossingBehaviour(float minBallProgress = 0, float chanceMultiplier = 1) {
            this.minBallProgress = minBallProgress;
            this.chanceMultiplier = chanceMultiplier;
        }

        private static readonly Curve MAX_BACKWARDS_DIFF = new Curve(new Curve.Point[] {
            new Curve.Point(0, 0),
            new Curve.Point(0.85f, 0),
            new Curve.Point(0.9f, 5),
            new Curve.Point(0.93f, 12),
            new Curve.Point(0.96f, 15),
            new Curve.Point(1, 20)
        });

        private static AnimationCurve CROSS_CHANCE_BY_BALL_PROGRESS = new AnimationCurve(new Keyframe[] {
            new Keyframe (0, 0),
            new Keyframe (0.5f, 0.5f),
            new Keyframe (0.7f, 1f),
            new Keyframe (0.8f, 10),
            new Keyframe (0.9f, 30),
            new Keyframe (0.95f, 60),
            new Keyframe (1, 90)
        });

        private float ZPower(in Vector3 position) {
            return Mathf.Abs(position.z - yFieldEnd / 2);
        }

        public override bool Behave(bool isAlreadyActive) {
            if (ball.holder != jugador) {
                return false;
            }

            if (!isAlreadyActive) {
                if (jugador.team.BallProgress < minBallProgress) {
                    return false;
                }

                var chance =
                    CROSS_CHANCE_BY_BALL_PROGRESS.Evaluate(jugador.fieldProgress) *
                    chanceMultiplier;

                var chanceRoll = Random.Range(0, 100) < chance;

                if (!chanceRoll) {
                    return false;
                }

                var myPos = jugador.Position;

                float myZPower = ZPower(myPos) - 1; // -1 addition.

                float myX = myPos.x;
                float myZ = myPos.z;
                float myDir = jugador.attackingDir.x;

                var max_backwards = MAX_BACKWARDS_DIFF.Evaluate(jugador.fieldProgress);

                var targetPlayer = teammates.
                    Where(j =>
                    j != jugador &&
                    !j.isInOffsidePosition &&
                    ZPower(j.Position) < myZPower &&
                    Mathf.Abs(myX - j.Position.x) < MAX_X_DIFF &&
                    Mathf.Abs(myZ - j.Position.z) > MIN_Z_DIFF &&
                    (myX - j.Position.x) * myDir < max_backwards &&
                    Vector3.Distance(myPos, j.Position) >= MIN_DISTANCE_TO_TARGET).
                    OrderByDescending(j => j.fieldProgress * 10 + (j.CanMyMarkersChaseMe(1) ? 0 : 10)).
                    FirstOrDefault();

                if (targetPlayer != null) {
                    jugador.passingTarget = targetPlayer;

                    var playerPos = jugador.Position;

                    isAlreadyActive = true;

                    //
                    target = targetPlayer;
                }
            }

            if (isAlreadyActive) {
                jugador.team.KeepPlayerBehavioursForAShortTime();

                jugador.CurrentAct = Acts.Crossing;

                if (jugador.controller.LookTo(in dt, target.Position - jugador.Position)) {
                    jugador.Cross(target.Position);
                    return false;
                }

                return true;
            }

            return false;
        }
    }
}
