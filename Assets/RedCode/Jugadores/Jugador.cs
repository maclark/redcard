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
        public bool IsGK = false;
        public bool isInOffsidePosition = false;
        public bool isRunningBehindDefense = false;
        public bool IsHoldingBall = false;
        public float anger = 0f;
        public float fieldProgress;
        public JugadorController controller;

        public BallHitAnimationEvent ballHitAnimationEvent { protected set; get; }

        [HideInInspector] public Vector3 runForwardBehaviourFinalPosition;
        public Vector3 attackingDir => team.ourGoal.transform.forward;
        public GoalNet opponentGoal => team.opponentGoal;
        public Acts CurrentAct; //#CAPS
        public float NextBehavior;

        // #CAPS
        public Vector3 Position => controller.transform.position;


        #region GET FUNCTIONS with MODIFIERS, for in game match.
        // supposed to be like Actual[Stat] * Engine.[Stat]Modifier;
        public float GetStrength() => 50f;
        public float GetAcceleration() => 50f;
        public float GetTopSpeed() => 50f;
        public float GetDribbleSpeed() => 50f;
        public float GetLongBall() => 50f;
        public float GetAgility() => 50f;
        public float GetShooting() => 50f;
        // #CAPITALIZATION
        public float actualTopSpeed = 50f;
        public float actualDribbleSpeed = 50f;
        public float actualBallKeeping = 50f;
        public float ActualShooting = 50f;
        public float ActualShootPower = 50f;
        public float ActualPositioning = 50f;
        public float ActualAcceleration = 50f;
        public float ActualBallControl = 50f;
        public float ActualReaction = 50f;
        public float ActualTopSpeed = 50f;
        #endregion

        public readonly LimitedCollection<Jugador> markers = new LimitedCollection<Jugador>(5);

        private Vector3 targetBallHitVector;
        private float targetBallHitSpeed;
        private Jugador chaserTarget;

        private bool _caughtInOffside;

        public bool CaughtInOffside {
            get {
                return _caughtInOffside;
            }

            set {
                _caughtInOffside = value;
                controller.SetOffside(value);
            }
        }


        public List<Behavior> behaviors = new List<Behavior>() {
            //new InputTackleBehaviour(),
            //new InputCrossBehaviour(),
            //new InputShortPassBehaviour(),
            //new InputThroughtPassBehaviour(),
            //new InputShootBehaviour(),
            //new InputBlockRestBehaviour(),

            //new ThrowInBehaviour(),
            //new CornerBehaviour(),
            //new KickOffBehaviour(),

            //new OurGKDegageBehaviour (),
            //new OpponentGKDegageBehaviour (),

            //new IsInOffsideBehaviour (),

            //// Try chip shot if GK is away.
            //new ChipShootingBehaviour (),

            //new ShootingBehaviour (0.75f, 0.5f),

            // Relax if there noone around.
            new RunForwardWithBallBehavior(0,
                RunForwardWithBallBehavior.ForwardCurve.Wingman,
                RunForwardWithBallBehavior.BewareMod.SuperCareful,
                false,
                0,
                0.4f,
                 MovementType.Normal), // max 0.55f ball progress
 
            // Stop unnecessary headers.
            new RunForwardWithBallBehavior(0f,
                RunForwardWithBallBehavior.ForwardCurve.Wingman,
                RunForwardWithBallBehavior.BewareMod.Risky,
                false,
                0.5f, // activate on ball height 0.5f
                0.65f), // activate before ball progress 0.65),

   //         new ShootingBehaviour (0, 1f),

   //         new CrossingBehaviour (0.925f, 2),

                new DribblingBehaviour (RunForwardWithBallBehavior.ForwardCurve.MostlyStraight),

   //         // Run to the goal with a normal chasing check.
   //         new RunForwardWithBallBehaviour(0.5f,
   //             RunForwardWithBallBehaviour.ForwardCurve.EarlyToGoal,
   //             RunForwardWithBallBehaviour.BewareMod.Careful,
   //             false),

   //         new ShootingBehaviour (0, 1.25f),
				
			//// Run to the goal with a risky chasing check.
   //         new RunForwardWithBallBehaviour(0.7f,
   //             RunForwardWithBallBehaviour.ForwardCurve.EarlyToGoal,
   //             RunForwardWithBallBehaviour.BewareMod.Normal,
   //             false),

   //         new ShootingBehaviour (0, 1.5f),

   //         new CrossingBehaviour (0.8f),
   //         new PassingBehaviour (0.8f, true, 5),
   //         new PassingBehaviour (0.95f),

   //         new ShootingBehaviour (0, 2.5f),

   //         new CrossingBehaviour (0.925f, 10),
   //         new CrossingBehaviour (0.8f, 1),
   //         new CrossingBehaviour (0.7f, 0.25f),

   //         new CriticalSendBallToSafe (),
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

        public void Shoot(Vector3 targetVelocity) {
            if (!IsHoldingBall) {
                return;
            }

            // #ANIMATION
            //if (!PlayBallHitAnimation(in targetVelocity, PlayerAnimatorVariable.Shoot_R)) {
            //    return;
            //}

            ballHitAnimationEvent = BallHitAnimationEvent.Shoot;

            targetBallHitVector = targetVelocity;

            Debug.Log("[Jugador] Shoot!");
        }

        public void Pass(Vector3 targetPoint, float speedMod = 1) {
            if (!IsHoldingBall) {
                return;
            }

            //#ANIMATION
            //if (!PlayBallHitAnimation(targetPoint - Position, PlayerAnimatorVariable.Pass_R, true)) {
            //    return;
            //}

            Debug.Log($"[Jugador] Pass to {targetPoint}");
            targetBallHitVector = targetPoint;
            targetBallHitSpeed = speedMod;

            ballHitAnimationEvent = BallHitAnimationEvent.Pass;
        }

        public void Cross(Vector3 targetPoint) {
            if (!IsHoldingBall) {
                return;
            }

            //#ANIMATION
            //if (IsGK && IsGKUntouchable) {
            //    if (!PlayBallHitAnimation(targetPoint - Position, PlayerAnimatorVariable.GKDegage_R, true)) {
            //        return;
            //    }
            //}
            //else {
            //    if (!PlayBallHitAnimation(targetPoint - Position, PlayerAnimatorVariable.LongBall_R, true)) {
            //        return;
            //    }
            //}

            Debug.Log("[Jugador] LongBall (Cross)!");
            targetBallHitVector = targetPoint;

            ballHitAnimationEvent = BallHitAnimationEvent.LongBall;
        }

        public bool PassToTarget(
            in float deltaTime,
            Vector3 targetPosition) {

            if (controller.LookTo(in deltaTime, targetPosition - Position)) {
                Pass(targetPosition);
                return true;
            }

            return false;
        }

        /// <summary>
        /// The best player can reach to target player at the shortest time.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="players"></param>
        public static IEnumerable<Jugador> BestOptionsToTargetPlayer(
            Jugador target,
            IEnumerable<Jugador> players,
            in float ballProgress,
            in int howManyPlayersToPick,
            bool considerOffside = true) {

            var maxDistance =
                RedMatch.
                match.
                settings.
                BestOptionToTargetMaxDistanceByBallProgressCurve.
                Evaluate(ballProgress);

            (bool isEligible, float reachTime) ReachTime(Jugador jug) {
                var predicted = Predicter(jug, target);

                float distance = Vector3.Distance(predicted, jug.Position);

                if (jug.IsGK) {
                    distance += RedMatch.match.settings.BestOptionToTargetGKAddition;
                }

                float jugSpeed = jug.GetAcceleration() * jug.GetTopSpeed();

                return (distance < maxDistance, distance / jugSpeed);
            }

            return players.Where(j =>
            j.controller.IsPhysicsEnabled &&
            (!considerOffside || !j.CaughtInOffside)).
            Select(x => (x, ReachTime(x))). // convert to (player, isEligible, reachtime)
            Where(x => x.Item2.isEligible). // eleminate not eligibles
            OrderBy(x => x.Item2.reachTime). // order by reach time
            Take(howManyPlayersToPick). // take required amount
            Select(X => X.x); // convert to player
        }

        /// <summary>
        /// The best player can reach to target position at the shortest time.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="players"></param>
        public static IEnumerable<Jugador> BestOptionsToTargetPosition(
            Vector3 position,
            IEnumerable<Jugador> players,
            int howManyPlayersToPick,
            bool considerOffside = true) {

            float ReachTime(Jugador player) {
                float distance = Vector3.Distance(position, player.Position);

                float playerSpeed = player.GetAcceleration() * player.GetTopSpeed();

                return distance / playerSpeed;
            }

            return players.Where(j =>
            j.controller.IsPhysicsEnabled &&
            (!considerOffside || !j.CaughtInOffside)).
            OrderBy(x => ReachTime(x)).
            Take(howManyPlayersToPick);
        }


        public bool CanMyMarkersChaseMe(float carefulness) {
            var chasers = markers.Members.Where(x => x.controller.IsPhysicsEnabled).Select(marker =>
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

            var meToChaser = chaser.Position - Position;

            var dot = Vector3.Dot(dir, meToChaser.normalized);

            var dot2 = Vector3.Dot((opponentGoal.Position - Position).normalized, meToChaser.normalized);

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
            Vector3 targetPoint = Predicter(sourceJug, targetJug.Position, targetVelocity);

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
            var dir = sourceJug.Position - targetPosition;
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
                controller.LookTo(in deltaTime, ball.transform.position - Position);
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
            var tGoalNetX = targetGoalNet.Position.x;
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
