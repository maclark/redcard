using UnityEngine;
using System.Collections.Generic;
using System.Linq;


namespace RedCard {

    // Jugador.Passing 
    public partial class Jugador {

        /// <summary>
        /// Throught pass points. Length is fixed.
        /// </summary>
        private readonly ThroughPassPoint[] passPoints = new ThroughPassPoint[15];

        /// <summary>
        /// If this is not null, we will trigger him with ball chasing behaviour after a pass.
        /// This will be set by behaviours.
        /// </summary>
        public Jugador passingTarget;

        public float SpeedModForPassing() {
            var skill = ActualAcceleration * 0.2f + ActualTopSpeed * 0.8f;
            return RedMatch.match.settings.PassPowerReceiverSpeedCurve.Evaluate(skill);
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
        /// Find a pass box around of the target player.
        /// </summary>
        /// <param name="targetPlayer"></param>
        /// <returns></returns>
        public IEnumerable<(
            string optionName,
            Jugador actualTarget,
            Vector3 position,
            int priority,
            bool enableUserInput,
            List<PassType> passTypes,
            Color debugColor)>
            FindPassPositions(Jugador targetPlayer) {

            float predicter = RedMatch.match.settings.CanSeePredictPositionVelocityMod;
            var predictPosition = PredictPositionWithVelocityMod(predicter);
            var predictTargetPosition = targetPlayer.PredictPositionWithVelocityMod(predicter);

            float dist = Vector3.Distance(predictPosition, predictTargetPosition);

            var toPasser = (Position - predictTargetPosition).normalized;

            var passAngle =
                Mathf.Abs(Vector3.SignedAngle(toPasser, targetPlayer.controller.dir, Vector3.up));

            var passAngleCurved = RedMatch.match.settings.
                PassingAngleCurve.Evaluate(
                passAngle / 180);

            var passAngleMultiplied = passAngleCurved;

            var distanceModifier = RedMatch.match.settings.PassingDistancePlayerVelocityModifier * dist;

            var forwardDir =
                targetPlayer.Velocity *
                passAngleMultiplied *
                RedMatch.match.settings.PassingPlayerVelocityModifier +
                targetPlayer.Velocity *
                passAngleMultiplied *
                distanceModifier;

            var passOptions = RedMatch.match.settings.ThroughPassOptions.Where(x => x.IsEnabled).ToArray();

            if (passOptions.Length >= passPoints.Length) {
                Debug.LogError($"Pass points has {passOptions.Length} on engine settings, but constantly has {passPoints.Length} length on PlayerBase script.");
                return default;
            }

            int passOptionsLength = passOptions.Length;

            for (int i = 0; i < passOptionsLength; i++) {
                passPoints[i] = new ThroughPassPoint(
                    passOptions[i],
                    predictTargetPosition,
                    forwardDir,
                    team.opponentGoal.Position);
            }

            var fieldSize = RedMatch.match.fieldSize;
            var fieldBoundCheck = RedMatch.match.settings.PassingFieldBoundCheck;

            (Vector3 dir, bool isPassed) throughtPointCheck(ThroughPassPoint point) {
                if (point.OnlyWhenRunningForward) {
                    if (
                        (targetPlayer.attackingDir.x > 0 && targetPlayer.controller.dir.x < 0) ||
                        (targetPlayer.attackingDir.x < 0 && targetPlayer.controller.dir.x > 0)) {

                        return default;
                    }
                }

                var playerToPoint = point.Position - predictPosition;

                var canCross = !point.DisableCrossing;

                if (!BoundCheck(in fieldBoundCheck, in point.Position, fieldSize)) {
                    return default;
                }

                return (point.Position, true);
            }

            var targets = passPoints.Take(passOptionsLength).
                Select(x => (x, (throughtPointCheck(x), x.Priority))).
                Where(x => x.Item2.Item1.isPassed).
                Select(x => (x.x.OptionName, targetPlayer, x.Item2.Item1.dir, x.Item2.Priority, x.x.EnableUserInput, x.x.PassTypes, x.x.DebugColor));

            return targets;

        }


        /// Returns the pass targets, ordered by priority.
        public ((PassType passType, string optionName, Vector3 position, int priority, Jugador actualTarget), float calculatedPriority)[]
        AvailablePassTargets(IEnumerable<Jugador> possibleTeammates, Vector3 targetGoalNetPosition) {
            var minPassDistance = RedMatch.match.settings.MinimumPassDistance;

            var passOptions = possibleTeammates.Where(x => x.controller.IsPhysicsEnabled && !x.isInOffsidePosition).
                // no diving GK.
                Where(x => !x.IsGK || !x.behaviors.Where(x => x is GKShieldBehavior).Cast<GKShieldBehavior>().FirstOrDefault().IsOnJump).
                Select(x => FindPassPositions(x)).
                SelectMany(x => x).
                Where(x => Vector3.Distance(x.position, Position) > minPassDistance).
                ToArray();

            if (passOptions.Length == 0) {
                return default;
            }

            var blockers = team.id == 0 ?
                RedMatch.match.losAl.jugadores.AsEnumerable() :
                RedMatch.match.somerville.jugadores.AsEnumerable();

            // check can see.
            var passOptionsInVision = passOptions.Select(x => (
                x.optionName, // 0
                x.position, // 1
                x.priority, // 2
                CanSeeTarget( // 3
                    x.passTypes,
                    x.actualTarget,
                    x.position,
                    blockers),
                x.actualTarget // 4

                )).ToArray();

            //
            var shortPasses = passOptionsInVision.Where(x => x.Item4.shortPassAvailable).
                Select(x => (PassType.ShortPass, x.optionName, x.position, x.priority, x.actualTarget));

            var longPasses = passOptionsInVision.Where(x => x.Item4.longPassAvailable).
                Select(x => (PassType.LongPass, x.optionName, x.position, x.priority, x.actualTarget));

            var finalOptions = shortPasses.Concat(longPasses);

            // only short passes for GKs.
            finalOptions = finalOptions.Where(x =>
                !x.actualTarget.IsGK ||
                x.Item1 == PassType.ShortPass);
            //

            // remove if i am closer to pass point than actual target.
            var myPosition = Position;
            finalOptions = finalOptions.Where(
                x =>
                    Vector3.Distance(x.position, myPosition) >
                    Vector3.Distance(x.position, x.actualTarget.Position));
            //

            if (!finalOptions.Any()) {
                return default;
            }

            var fieldSize = RedMatch.match.fieldSize;

            var engineSettigs_middlePriority = RedMatch.match.settings.PassingMiddlePriority;
            var engineSettigs_distancePriority = RedMatch.match.settings.PassingOptionDistanceToPriority;
            var engineSettings_crossPriority = RedMatch.match.settings.PassingCrossPriority;

            var myPos = Position;

            float distancePriority(Vector3 pos) {
                var distance = Vector3.Distance(myPos, pos);
                return distance * engineSettigs_distancePriority;
            }

            float middlePriority(Vector3 pos) {
                var distanceToMiddle = Mathf.Abs((fieldSize.y / 2) - pos.z);
                return distanceToMiddle * team.BallProgress * engineSettigs_middlePriority;
            }

            float priority(PassType passType, Vector3 position, int priority) {
                return (passType == PassType.LongPass ? engineSettings_crossPriority : 0) + // cross priority
                Mathf.Pow(XPower(in fieldSize.x, position), RedMatch.match.settings.XPowerPow) +
                middlePriority(position) +
                distancePriority(position) +
                priority * RedMatch.match.settings.PassingOptionPriorityPower;
            }

            var orderedOptions = finalOptions.
                Select(x => (x, priority(x.Item1, x.position, x.priority))).OrderByDescending(x => x.Item2).ToArray();

            return orderedOptions;
        }

        private PassTarget BestPassTarget(
            IEnumerable<Jugador> possibleTeammates, Vector3 targetGoalNetPosition
            ) {

            var availables = AvailablePassTargets(possibleTeammates, targetGoalNetPosition);

            if (availables == null || !availables.Any()) {
                return default;
            }

            var first = availables.Select(x => x.Item1).First();

            var actualTargetPosition = first.actualTarget.Position;
            var ourPosition = Position;
            var usToActualTarget = actualTargetPosition - ourPosition;
            var usToPoint = first.position - ourPosition;
            var angle = Mathf.Abs(Vector3.SignedAngle(usToActualTarget, usToPoint, Vector3.up));

            var m_angle = RedMatch.match.settings.PassPowerByPassAngleCurve.Evaluate(angle);
            var m_distance = RedMatch.match.settings.PassPowerByAngledPassDistanceCurve.Evaluate(usToPoint.magnitude);
            var passPowerMod = m_angle * m_distance;

            return new PassTarget(first.Item1,
                first.optionName,
                first.position,
                first.actualTarget,
                passPowerMod);
        }

        private (bool shortPassAvailable, bool longPassAvailable) CanSeeTarget(
            List<PassType> passTypes,
            Jugador actualTarget,
            Vector3 target,
            IEnumerable<Jugador> colliders) {

            float predictPositionDistance = RedMatch.match.settings.CanSeePredictPositionVelocityMod;

            var passerPosition = PredictPositionWithVelocityMod(predictPositionDistance);

            var actualTargetPosition = actualTarget.PredictPositionWithVelocityMod(predictPositionDistance);

            var passerToPoint = target - passerPosition;

            var actualTargetToPoint = target - actualTargetPosition;

            var actualTargetToPointDistance = actualTargetToPoint.magnitude;

            var actualTargetToPasser = passerPosition - actualTargetPosition;

            var passerToActualTarget = actualTargetPosition - passerPosition;

            var actualTargetToPasserDistance = actualTargetToPasser.magnitude;

            var passerToPointDistance = passerToPoint.magnitude;

            var ballProgressMod = 
                RedMatch.match.settings.
                CanSeeSecureAngleModifierByBallProgressCurve.
                Evaluate(actualTarget.fieldProgress);

            float secureAngleBetweenPasserAndThread =
                RedMatch.match.settings.
                CanSeeSecureAngleBetweenPasserAndThread * ballProgressMod;

            var angleModByDistanceCurve = 
                RedMatch.match.settings.
                CanSeeAngleModByDistanceCurve;

            float threadDistanceAdd = RedMatch.match.settings.CanSeeThreadDistanceAdditionByBallProgress.Evaluate(actualTarget.fieldProgress);

            float crossingSecureDistanceFromPasser =
                RedMatch.match.settings.
                PassingCrossBlockApproveDistanceToPasserByDistance.Evaluate(passerToPointDistance);

            float crossingSecureDistanceToTarget =
                RedMatch.match.settings.
                PassingCrossBlockApproveDistanceToTargetByDistance.Evaluate(passerToPointDistance);

            float crossingBehindLimit = 
                RedMatch.match.settings.
                CrossingBehindDistanceByFieldProgress.Evaluate(actualTarget.fieldProgress);

            (bool shortPassBlocked, bool longPassBlocked) isInCone(Jugador @thread) {
                var threadPosition = thread.PredictPositionWithVelocityMod(predictPositionDistance);

                var passerToThread = threadPosition - passerPosition;
                var actualTargetToThread = threadPosition - actualTargetPosition;

                var passerToPoint_passerToThreadAngle = Mathf.Abs(Vector3.SignedAngle(passerToPoint, passerToThread, Vector3.up));

                var passerToPoint_passerToActualTargetAngle = Mathf.Abs(Vector3.SignedAngle(passerToActualTarget, passerToPoint, Vector3.up));

                var threadDistanceToPasser = Vector3.Distance(threadPosition, passerPosition) + threadDistanceAdd;
                var threadDistanceToPoint = Vector3.Distance(threadPosition, target) + threadDistanceAdd;

                var passerAngleMod = angleModByDistanceCurve.Evaluate(threadDistanceToPasser);

                bool threadForPasser =
                    passerToPoint_passerToActualTargetAngle > passerToPoint_passerToThreadAngle || (
                    threadDistanceToPasser < passerToPointDistance &&
                    passerToPoint_passerToThreadAngle < secureAngleBetweenPasserAndThread * passerAngleMod);

                bool threadForActualTarget = threadDistanceToPoint < actualTargetToPointDistance;

                bool shortPassBlocked = threadForPasser || threadForActualTarget;
                bool longPassBlocked = true;

                #region
                var backwards = 0f;

                if (attackingDir.x > 0) {
                    // home team.
                    backwards = -passerToPoint.z;
                }
                else {
                    // away team.
                    backwards = passerToPoint.z;
                }

                var outOfAngle = passerToPoint_passerToThreadAngle > secureAngleBetweenPasserAndThread * 2;

                if (
                    backwards < crossingBehindLimit &&
                    actualTargetToPointDistance < threadDistanceToPoint &&
                    (outOfAngle || threadDistanceToPasser > crossingSecureDistanceFromPasser) &&
                    (outOfAngle || threadDistanceToPoint > crossingSecureDistanceToTarget)
                    ) {
                    longPassBlocked = false;
                }
                #endregion

                return (
                    !passTypes.Contains(PassType.ShortPass) || shortPassBlocked,
                    !passTypes.Contains(PassType.LongPass) || longPassBlocked);
            }

            var results = colliders.Select(x => isInCone(x)).ToArray();

            return (!results.Any(x => x.shortPassBlocked), !results.Any(x => x.longPassBlocked));
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


        /// <summary>
        /// When we want to find a teammate, we will be sure he is front of us, or not that far back in X axis.
        /// </summary>
        /// <param name="teammate"></param>
        /// <returns></returns>
        private bool DoesItMakeSenseToFindThisTeammate(Jugador teammate) {
            return
                IsFrontOfMe(teammate.Position) || // is at front of me
                Mathf.Abs(teammate.Position.x - Position.x) < RedMatch.match.settings.PassingBackwardMaxDistance; // or backward, but not too far.
        }

        public PassTarget FindPassTarget(
            in Jugador[] teammates,
            in Vector3 targetGoalNetPosition,
            bool ignoreChecks = false) {
            var targets = teammates.Where(x =>
            x != this &&
            (ignoreChecks || DoesItMakeSenseToFindThisTeammate(x)));

            return BestPassTarget(targets, targetGoalNetPosition);
        }





    }



}