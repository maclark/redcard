using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RedCard {

    public partial class Jugador {

        public int id = -1;
        public string givenName;
        public string surname;
        public RedTeam team;
        public RefTarget target;
        public AngerBar angerBar;
        public bool isGoalie = false;
        public bool isInOffsidePosition = false;
        public bool isRunningBehindDefense = false;
        public float anger = 0f;
        public float fieldProgress;
        public JugadorController controller;
        


        [HideInInspector] public Vector3 runForwardBehaviourFinalPosition;
        public Vector3 attackingDir => team.ourGoal.transform.forward;
        public GoalNet opponentGoal => team.opponentGoal;
        public Acts currentAct;

        #region GET FUNCTIONS with MODIFIERS, for in game match.
        // supposed to be like Actual[Stat] * Engine.[Stat]Modifier;
        public float GetStrength() => 100f;
        public float GetAcceleration() => 100f;
        public float GetTopSpeed() => 100f;
        public float GetDribbleSpeed() => 100f;
        public float GetLongBall() => 100f;
        public float GetAgility() => 100f;
        public float GetShooting() => 100f;
        #endregion



        public readonly LimitedCollection<Jugador> markers = new LimitedCollection<Jugador>(5);

        private Jugador chaserTarget;

        public List<Behavior> behaviors = new List<Behavior>() {

            //new KickkOffBehavior(),
            //new IsInOffsideBehavior(),
            // Relax if there noone around.
            new DribbleBehavior(0,
                DribbleBehavior.ForwardCurve.Wingman,
                DribbleBehavior.BewareMod.SuperCareful,
                false,
                0,
                0.4f,
                 MovementType.Normal), // max 0.55f ball progress
 
            // Stop unnecessary headers.
            new DribbleBehavior(0f,
                DribbleBehavior.ForwardCurve.Wingman,
                DribbleBehavior.BewareMod.Risky,
                false,
                0.5f, // activate on ball height 0.5f
                0.65f), // activate before ball progress 0.65),

        }; 

        public void Behave(
                in float time,
                in float dt,
                in float xFieldEnd,
                in float yFieldEnd,
                in MatchStatus matchStatus,
                in TeamPosture teamPosture,
                in float xOpponentOffsideLine,
                in float xOurOffsideLine,
                RedBall matchBall,
                GoalNet opponentGoalNet,
                in Jugador[] teammates,
                in Jugador[] opponents
            ) {

            // ...

        }

        public Vector3 pos => controller.transform.position;

        public void ProcessBehaviors(in float time) {

            // if (controller.IsAnimationABlocker( 

            // skip rate is an AI difficulty level thing
            // skipRate =
            // if skip rate > ?, return


            //IEnumerable<Behavior> forcedBehaviors = behaviors.Where(x => x.forceBehavior);
            // foreach (Behavior b in forcedBehaviors) ... return

            // bool reset = 

            // ....
        }

        public bool CanMyMarkersChaseMe(float carefulness) {
            var chasers = markers.Members.Where(x => x.controller.isPhysicsEnabled).Select(marker =>
            (marker, CanChaseMe(marker, in carefulness)));

            chaserTarget = chasers.OrderBy(x => x.Item2.distance).FirstOrDefault().marker;

            return chasers.Where(x => x.Item2.result).Any();
        }

        /// <summary>
        /// Can the target player chase me in min distance.
        /// </summary>
        /// <param name="chaser"></param>
        /// <param name="carefulness">It should be between 0 and 1</param>
        /// <returns></returns>
        public (bool result, float distance) CanChaseMe(Jugador chaser, in float carefulness) {
            var dir = chaser.attackingDir;

            var meToChaser = chaser.pos - pos;

            var dot = Vector3.Dot(dir, meToChaser.normalized);

            var dot2 = Vector3.Dot((opponentGoal.pos - pos).normalized, meToChaser.normalized);

            float reachTime(Jugador jug) {
                float speedMod = 0.04f;
                float distMod = 0.3f;

                var accLate = jug.controller.targetMoveSpeed * speedMod;
                var distLate = distMod * meToChaser.magnitude * (2.5f - dot - (dot2 * 0.25f));

                return distLate / (accLate + 1);
            }

            // 

            var time = reachTime(chaser);

            return (time < carefulness, time);
        }

        /// <summary>
        /// When a jugador wants to reach another one, it should predict his next position.
        /// </summary>
        /// <param name="sourceJug"></param>
        /// <param name="targetJug"></param>
        /// <returns></returns>
        public static Vector3 Predicter(Jugador sourceJug, Jugador targetJug) {
            Vector3 targetVelocity = targetJug.controller.direction * targetJug.controller.moveSpeed;
            Vector3 targetPoint = Predicter(sourceJug, targetJug.pos, targetVelocity);

            return targetPoint;
        }

        /// <summary>
        /// Predict position of a moving target from a player.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="targetVelocity"></param>
        /// <returns></returns>
        public static Vector3 Predicter(Jugador source, Vector3 target, Vector3 targetVelocity) {
            float angle = AngleBetweenJugador(source, target, targetVelocity);
            float angleMod = AngleDifferenceMod(angle, source);
            return angleMod * targetVelocity.normalized + target;
        }

        private static float AngleBetweenJugador(Jugador sourceJug, Vector3 targetPosition, Vector3 targetVelocity) {
            var dir = sourceJug.pos - targetPosition;
            var targetDir = targetVelocity;

            var angleWithDistance = Mathf.Pow(dir.magnitude, RedMatch.match.settings.Angle_Distance_Power);

            return Mathf.Abs(Vector3.SignedAngle(dir, targetDir, Vector3.up)) * targetVelocity.magnitude * angleWithDistance;
        }

        /// <summary>
        /// When a player runs forward, what should I do with my angle to that player to catch him?
        /// Usually we power the angle, and mod with something.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private static float AngleDifferenceMod(float angle, Jugador source) {
            RedSettings matchSettings = RedMatch.match.settings;
            return Mathf.Pow(angle, matchSettings.Angle_Power) * matchSettings.Angle_Multi * matchSettings.Angle_PlayerProgress.Evaluate(source.fieldProgress);
        }

        public virtual void FocusToBall(in float deltaTime, RedBall ball) {
            if (ball.holder != this && !isRunningBehindDefense)
                controller.LookTo(in deltaTime, ball.transform.position - pos);
        }

        /// <summary>
        /// Returns isoffside & position for onside.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="targetGoalNet"></param>
        /// <param name="offsideLine"></param>
        /// <returns></returns>
        public static (bool, float) IsPositionOffside(Vector3 position, GoalNet targetGoalNet, in float offsideLine) {
            var posX = position.x;
            var tGoalNetX = targetGoalNet.pos.x;
            var distanceToGoal = Mathf.Abs(tGoalNetX - posX);
            var offsideLineToGoal = Mathf.Abs(tGoalNetX - offsideLine);

            //

            return (distanceToGoal < offsideLineToGoal, offsideLine + targetGoalNet.transform.forward.x);
        }

        public bool BoundCheck(in float fieldBoundCheck, in Vector3 position, in Vector2 fieldSize) {
            var up = position.z + fieldBoundCheck;
            var down = position.z - fieldBoundCheck;
            var left = position.x - fieldBoundCheck;
            var right = position.x + fieldBoundCheck;

            if (up > fieldSize.y ||
                down < 0 ||
                left < 0 ||
                right > fieldSize.x) {
                return false;
            }

            return true;
        }
    }
}
