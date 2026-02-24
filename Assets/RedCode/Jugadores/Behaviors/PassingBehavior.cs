using UnityEngine;
using System.Linq;

namespace RedCard {
    public class PassingBehavior : Behavior {

        private PassTarget target;

        private readonly bool onlyIfFrontOfUs;
        private readonly float minBallProgress;
        private readonly float maxBallProgress;
        private readonly float frontXThreshold;
        private readonly bool onlyIfCloserToGoalNet;

        public PassingBehavior(float maxBallProgress = 1) {
            this.maxBallProgress = maxBallProgress;
        }

        /// <summary>
        /// Pick the targets only if they are closer to target goal net than us.
        /// </summary>
        /// <param name="minBallProgress"></param>
        /// <param name="onlyIfCloserToGoalNet"></param>
        public PassingBehavior(
            float minBallProgress = 0,
            bool onlyIfCloserToGoalNet = false) {

            this.minBallProgress = minBallProgress;
            this.onlyIfCloserToGoalNet = onlyIfCloserToGoalNet;
        }

        /// <summary>
        /// Construct a passing behaviour with 'front of us' checker.
        /// When you checked 'onlyIfFrontOfUs' the player will pass only if the target is front of us in X Axis (to forward without considering horizontal position). So beware, centre forward can pass to the corner side :-)
        /// </summary>
        /// <param name="minBallProgress">Minimum ball progress to activate. Between 0-1</param>
        /// <param name="onlyIfFrontOfUs">Select if passing point is front of us.</param>
        /// <param name="frontXThreshold">If onlyIfFrontOfUs true, optionally add more X threshold to consider it is 'Front'</param>
        public PassingBehavior(
            float minBallProgress,
            bool onlyIfFrontOfUs,
            float frontXThreshold) {
            this.onlyIfFrontOfUs = onlyIfFrontOfUs;
            this.minBallProgress = minBallProgress;
            this.frontXThreshold = frontXThreshold;
        }

        public override bool Behave(bool isAlreadyActive) {
            if (ball.holder != jugador) {
                return false;
            }

            if (!isAlreadyActive) {
                if (jugador.team.BallProgress < minBallProgress) {
                    return false;
                }

                if (jugador.team.BallProgress > maxBallProgress) {
                    return false;
                }

                var targetGoalNetPosition = targetGoalNet.Position;

                var distanceToTargetGoalNet = Vector3.Distance(jugador.Position, targetGoalNetPosition);

                var targets = teammates.Where(x =>
                    (!onlyIfCloserToGoalNet || Vector3.Distance(x.Position, targetGoalNetPosition) < distanceToTargetGoalNet) &&
                    (!onlyIfFrontOfUs || jugador.IsFrontOfMe(x.Position, frontXThreshold))).ToArray();

                target = jugador.FindPassTarget(in targets, in targetGoalNetPosition);

                if (target.IsValid) {
                    Debug.Log($"[PassingBehaviour] OptionName: {target._OptionName}");
                    isAlreadyActive = true;
                }
            }

            if (isAlreadyActive) {
                jugador.CurrentAct = Acts.PassingToBetterOpportunity;

                jugador.team.KeepPlayerBehavioursForAShortTime();

                jugador.controller.Stop(in dt);

                if (jugador.controller.LookTo(in dt, target._Position - jugador.Position)) {
                    // set pass target. after pass target player will behave with BallChasingBehaviour.
                    jugador.passingTarget = target._ActualTarget;

                    float speedMod = jugador.SpeedModForPassing();

                    if (target._PassType == PassType.LongPass) {
                        var dir = target._Position - jugador.Position;
                        var add = RedMatch.match.settings.CrossTargetAdditionNormalByDistance.Evaluate(dir.magnitude);

                        var crossAddition = dir.normalized * add * target._PassPower;

                        jugador.Cross(target._Position + crossAddition);
                    }
                    else {
                        jugador.Pass(target._Position, speedMod * target._PassPower);
                    }
                }

                return true;
            }

            return false;
        }
    }
}
