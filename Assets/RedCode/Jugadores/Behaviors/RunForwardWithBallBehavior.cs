
using System.Linq;
using UnityEngine;

namespace RedCard {
    public class RunForwardWithBallBehavior : Behavior {
        private const float BEWARE_SUPER_CAREFUL = 2.3f;
        private const float BEWARE_CAREFUL = 1.8f;
        private const float BEWARE_NORMAL = 1.3f;
        private const float BEWARE_RISKY = 0.75f;

        private readonly AnimationCurve carefulbyBallprogress = new AnimationCurve(new Keyframe[] {
            new Keyframe (0, 1f),
            new Keyframe (0.25f, 0.9f),
            new Keyframe (0.5f, 0.6f),
            new Keyframe (0.75f, 0.3f),
            new Keyframe (0.9f, 0.2f),
            new Keyframe (1, 0.1f)
        });

        private readonly AnimationCurve carefulByAngleToGoal = new AnimationCurve(new Keyframe[] {
            new Keyframe (0, 0),
            new Keyframe (20f, 0.01f),
            new Keyframe (30f, 0.05f),
            new Keyframe (50f, 0.1f),
            new Keyframe (90f, 0.4f),
        });

        private const float ALREADY_ACTIVE_LERP_SPEED = 3;

        private readonly float bewareVelocityMod;
        private readonly ForwardCurve curve;
        private readonly float maxBallProgress;
        private readonly bool ignoreChasing;
        private readonly float minBallProgress;
        private readonly float minBallHeight;
        private readonly MovementType movementType;

        public enum BewareMod {
            SuperCareful,
            Careful,
            Normal,
            Risky
        }

        public enum ForwardCurve {
            EarlyToGoal,
            Wingman,
            MostlyStraight
        }

        /// <summary>
        /// Construct a running behaviour with ignoring chasing.
        /// </summary>
        /// <param name="forwardCurve"></param>
        public RunForwardWithBallBehavior(
            float minBallProgress,
            ForwardCurve forwardCurve,
            BewareMod bewareMod = BewareMod.Normal,
            bool ignoreChasing = true,
            float minBallHeight = 0,
            float maxBallProgress = 1,
            MovementType movementType = MovementType.BestHeCanDo) {

            this.ignoreChasing = ignoreChasing;

            this.minBallHeight = minBallHeight;

            this.minBallProgress = minBallProgress;
            this.curve = forwardCurve;
            this.maxBallProgress = maxBallProgress;

            this.movementType = movementType;

            switch (bewareMod) {
                case BewareMod.SuperCareful: this.bewareVelocityMod = BEWARE_SUPER_CAREFUL; break;
                case BewareMod.Careful: this.bewareVelocityMod = BEWARE_CAREFUL; break;
                case BewareMod.Normal: this.bewareVelocityMod = BEWARE_NORMAL; break;
                case BewareMod.Risky: this.bewareVelocityMod = BEWARE_RISKY; break;
            }
        }

        public RunForwardWithBallBehavior(
            float maxBallProgress,
            BewareMod bewareMod,
            ForwardCurve curve,
            float minBallProgress = 0) {

            this.curve = curve;
            this.maxBallProgress = maxBallProgress;
            this.minBallProgress = minBallProgress;
            this.movementType = MovementType.BestHeCanDo;

            switch (bewareMod) {
                case BewareMod.Careful: this.bewareVelocityMod = BEWARE_CAREFUL; break;
                case BewareMod.Normal: this.bewareVelocityMod = BEWARE_NORMAL; break;
                case BewareMod.Risky: this.bewareVelocityMod = BEWARE_RISKY; break;
            }
        }

        public override bool Behave(bool isAlreadyActive) {
            if (ball.holder != jugador) {
                return false;
            }

            if (jugador.team.BallProgress > maxBallProgress) {
                return false;
            }

            if (jugador.team.BallProgress < minBallProgress) {
                return false;
            }

            if (ball.transform.position.y < minBallHeight) {
                return false;
            }

            Vector3 playerPosition = jugador.Position;

            Vector3 toGoal = targetGoalNet.Position - playerPosition;
            Vector3 toForward = goalNet.transform.forward;

            // to goal rotation by ball progress;
            float ballProgress = jugador.team.BallProgress;

            float lerper = RedMatch.match.settings.runningForwardCurves[(int)curve].Evaluate(ballProgress);

            Vector3 runningDir = Vector3.Lerp(toForward, toGoal, lerper);

            if (!ignoreChasing && jugador.CanMyMarkersChaseMe(
                bewareVelocityMod +
                carefulbyBallprogress.Evaluate(jugador.fieldProgress) +
                carefulByAngleToGoal.Evaluate(Mathf.Abs(Vector3.SignedAngle(jugador.attackingDir, targetGoalNet.Position - jugador.Position, Vector3.up)))
                )) {

                return false; // check original dir.
            }

            Vector3 targetPosition = jugador.Position + runningDir * 5;

            Vector3 avoided = targetPosition;
            jugador.AvoidMarkers(opponents, ref avoided);

            if (jugador.BoundCheck(0, avoided, new Vector2(xFieldEnd, yFieldEnd))) {
                targetPosition = avoided; // approve.
            }

            jugador.CurrentAct = Acts.RunningForward;

            if (!isAlreadyActive) {
                jugador.runForwardBehaviourFinalPosition = jugador.Position + jugador.controller.direction * 2;
            }

            jugador.runForwardBehaviourFinalPosition = Vector3.Lerp(jugador.runForwardBehaviourFinalPosition, targetPosition, dt * ALREADY_ACTIVE_LERP_SPEED);

            // available to run forward.
            if (jugador.controller.MoveTo(in dt, jugador.runForwardBehaviourFinalPosition, true, movementType)) {
                return false;
            }

            return true;
        }
    }
}
