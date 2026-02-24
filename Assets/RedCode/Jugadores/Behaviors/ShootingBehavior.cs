using UnityEngine;
using System.Linq;
using Shared;
using System;
using static UnityEngine.GraphicsBuffer;

namespace RedCard {
    public class ShootingBehavior : AbstractShootingBehavior {
        private const float ONEONONE_GK_TO_GOALNET_X_DIST = 5;
        private const float TO_GK_DIST = 10;
        private const float ONEONEONE_TOLERANCE_BONUS = 20;

        private (Transform point, float angleFree) shootingTarget;

        private Vector3 shootingDir;

        private readonly float minBallProgress;

        private readonly float toleranceMulti;

        private Jugador opponentGK;

        private float adminMulti = 1;

        private readonly Curve shootingSkillToleranceMultiplierCurve = new Curve(new Curve.Point[] {
            new Curve.Point (0, 0.5f),
            new Curve.Point (0.5f, 0.9f),
            new Curve.Point (0.6f, 1f),
            new Curve.Point (0.7f, 1.3f),
            new Curve.Point (0.75f, 1.5f),
            new Curve.Point (0.8f, 1.6f),
            new Curve.Point (0.85f, 1.8f),
            new Curve.Point (0.9f, 1.9f),
            new Curve.Point (1f, 2f),
        });

        private readonly Curve keepingToleranceCurve = new Curve(new Curve.Point[] {
            new Curve.Point (0, 0f),
            new Curve.Point (0.5f, 0.25f),
            new Curve.Point (0.6f, 0.5f),
            new Curve.Point (0.7f, 0.75f),
            new Curve.Point (0.75f, 1),
            new Curve.Point (0.8f, 1.25f),
            new Curve.Point (0.85f, 1.5f),
            new Curve.Point (0.9f, 1.75f),
            new Curve.Point (1f, 2f),
        });

        private readonly Curve ballHeightToToleranceCurve = new Curve(new Curve.Point[] {
            new Curve.Point (-10, 0f),
            new Curve.Point (0, 0f),
            new Curve.Point (0.4f, 15f),
            new Curve.Point (1f, 40f),
            new Curve.Point (2f, 80f),
            new Curve.Point (20, 100)
        });

        private readonly Curve ballHeightToToleranceByBallProgressCurve = new Curve(new Curve.Point[] {
            new Curve.Point (0, 0f),
            new Curve.Point (0.75f, 0.2f),
            new Curve.Point (0.85f, 0.7f),
            new Curve.Point (0.9f, 1f),
            new Curve.Point (1f, 100f)
        });

        public ShootingBehavior(float minBallProgress, float toleranceMulti = 1) {
            this.minBallProgress = minBallProgress;
            this.toleranceMulti = toleranceMulti;
        }

        public ShootingBehavior() {
            this.minBallProgress = 0;
            this.toleranceMulti = 1;
        }

        private float toleranceMod {
            get {
                if (opponentGK == null) {
                    opponentGK = opponents.FirstOrDefault(x => x.isGK);
                }

                var ballHeight = ball.transform.position.y;

                // calculate by shooting.
                var shooting = jugador.ActualShooting * 0.75f + jugador.ActualShootPower * 0.25f;

                var keeping = opponentGK.ActualPositioning * 0.25f +
                    opponentGK.ActualAcceleration * 0.125f +
                    opponentGK.ActualBallControl * 0.125f +
                    opponentGK.ActualReaction * 0.25f +
                    opponentGK.ActualTopSpeed * 0.25f;

                var gkTol = keepingToleranceCurve.Evaluate(keeping / 100);

                var tol = shootingSkillToleranceMultiplierCurve.Evaluate(shooting / 100f) * toleranceMulti;

                var byBallHeight = ballHeightToToleranceCurve.Evaluate(ballHeight);
                var byBallProgressByHeight = ballHeightToToleranceByBallProgressCurve.Evaluate(jugador.team.BallProgress);

                return Mathf.Max(1, 1 + tol - gkTol + (ballHeightToToleranceCurve.Evaluate(ballHeight) * byBallProgressByHeight));
            }
        }

        public bool IsOneOnOneWithTheGoalKeeper() {
            var opponentGK = opponents.FirstOrDefault(x => x.isGK);
            if (opponentGK == null) {
                return false; // NO GK? ok...
            }

            var gkPos = opponentGK.Position;
            var targetGoalNetPos = targetGoalNet.Position;

            if (Mathf.Abs(targetGoalNetPos.x - gkPos.x) > ONEONONE_GK_TO_GOALNET_X_DIST &&
                Vector3.Distance(gkPos, jugador.Position) < TO_GK_DIST) {
                return true;
            }

            return false;
        }

        public override bool Behave(bool isAlreadyActive) {
            if (ball.holder != jugador) {
                return false;
            }

            if (!isAlreadyActive) {
                if (jugador.team.BallProgress < minBallProgress) {
                    return false;
                }

                if (!CanShoot()) {
                    return false;
                }

                Vector3 forward = jugador.attackingDir; // new Vector3(Player.toGoalXDirection, 0, 0);
                Vector3 toGoal = targetGoalNet.Position - jugador.Position;

                float angleToGoal = AngleToGoal(targetGoalNet);

                bool oneOnOne = IsOneOnOneWithTheGoalKeeper();

                float tolerance = adminMulti * (toleranceMod + (oneOnOne ? ONEONEONE_TOLERANCE_BONUS : 0));

                bool shouldShoot = RedMatch.match.settings.ShootRoll(
                    angleToGoal,
                    toGoal.magnitude,
                    tolerance);

                if (shouldShoot) {
                    adminMulti = 1;

                    shootingTarget = targetGoalNet.GetShootingVector(
                        jugador, opponents);

                    var target = shootingTarget.point.position;

                    shootingDir = target - jugador.Position;

                    Debug.Log($"[SHOULD SHOOT] {shootingTarget}");
                    isAlreadyActive = true;
                }
            }

            if (isAlreadyActive) {
                jugador.team.KeepPlayerBehavioursForAShortTime();

                jugador.CurrentAct = Acts.Shoot;

                Debug.Log($"Shooting => {shootingTarget}");

                jugador.controller.Stop(in dt);

                if (jugador.controller.LookTo(in dt, shootingDir)) {
                    var shootPowerByAngleFree = RedMatch.match.settings.shootPowerModByAngleFree.Evaluate(shootingTarget.angleFree);

                    Vector3 target = targetGoalNet.
                        GetShootingVectorFromPoint(jugador, shootingTarget.point) * shootPowerByAngleFree;

                    jugador.Shoot(target);
                }

                return true;
            }

            return false;
        }
    }
}
