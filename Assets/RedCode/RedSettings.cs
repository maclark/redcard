using UnityEngine;

namespace RedCard {
    [CreateAssetMenu(fileName = "RedSettings", menuName = "RedSettings")]
    public class RedSettings : ScriptableObject {
        public float fastCallThreshold = 1f;
        public float lateCallThreshold = 5f;
        public float maxCallLateness = 10f; // maybe only applies for fouls, not out of bounds

        public float maxAnger = 100f;


        [Header("NORMAL FOULS")]
        public float angerFouled = 10f;
        public float soothedGotCall = -8f;
        public float teamAngerFouled = 3f;
        public float teamSoothedGotCall = -2f;
        public float angerCalledForFoul = 5f;
        public float teamAngerCalledForFoul = 2f;
        public float angerWronglyCalledForFoul = 20f;
        public float teamAngerWronglyCalledForFoul = 10f;
        public float respectLostWrongFoul = 0.1f;

        [Header("THROW INS")]
        public float teamAngerWrongThrowIn = 3f;
        public float repectLostWrongThrowIn = .05f;

        [Header("CORNERS & GOAL KICKS")]
        public float teamAngerWrongCornerKick = 6f;
        public float respectLostWrongCorner = .15f;

        [Header("GOALS & PENALTIES")]
        public float teamAngerWrongPenalty = 30f;
        public float respectLostWrongPenalty = .25f;
        public float teamAngerWrongGoal = 50f;
        public float respectLostWrongGoal = .4f;

        // / / / / / / / / / simulator
        // / / / / / / / / / simulator
        // / / / / / / / / / simulator

        [Header("RUN CURVES")]
        public AnimationCurve[] runningForwardCurves;
        public AnimationCurve avoidanceCurve;

        [Header("PLAYER RUN ANGLE MOD")]
        public float Angle_Power = 1.25f;
        public float Angle_Multi = 0.002f;
        public float Angle_Distance_Power = 0.8f;

        [Header("BallChasingBehaviour")]
        [Tooltip("When a AI player wants to chase the ball, we will make it harder by distance")]
        public AnimationCurve BallChasingChaserToBallDistanceAdditionCurve;

        [Header("Slight movement by angle difference")]
        public float BallMagnetRadius = 2f;
        public float BallMagnetPower = 1f;

        public float AgileToDirectionWhenHoldingBallModifier = 2;

        [Header("Agile to direction")]
        public AnimationCurve AgileToDirectionAngleDifferencyHardness;
        public AnimationCurve AgileToDirectionMoveSpeedHardness;
        public AnimationCurve Angle_PlayerProgress;

        [Header("Direction Error settings")]
        public bool IsDirectionErrorEnabled;
        public AnimationCurve DirectionErrorModByVelocityCurve;
        public AnimationCurve DirectionErrorSkillModCurve;

        [Header("Shooting velocity")]
        public float ShootingForwardAxisMultiplier = 2f;
        public float ShootingUpAxisDistanceMultiplier = 4f;
        public AnimationCurve ShootPowerByDistanceCurve;
        public AnimationCurve ShootPowerBySkillCurve;
        public float ShootingBlockAngle = 10;
        public AnimationCurve ShootErrorRemoveByDistance;

        [Header("AI Shoot Tolerance")]
        [Range(0f, 100f)]
        [Tooltip("AI will roll numbers between 0 and 100. If AI_ShootTolerance is bigger than it, it will decide to shoot.")]
        [SerializeField] private float AI_ShootTolerance = 25f;
        [SerializeField] private AnimationCurve AI_ShootToleranceDistanceCurveMod;
        [SerializeField] private AnimationCurve AI_ShootToleranceDividerAngleCurveMod;
        [SerializeField] private AnimationCurve AI_DistanceToAngleCurveMod;

        // in FS, this was in its own file
        // EngineSettings_ShootingOption or something, see ShootingBehaviour.cs
        public AnimationCurve shootPowerModByAngleFree;

        [Header("BestOptionToTargetPoint (When someone have the ball)")]
        public AnimationCurve BestOptionToTargetMaxDistanceByBallProgressCurve;
        public float BestOptionToTargetGKAddition = -10;

        [Header("BallChasing GK addition")]
        public float BallChasingDistanceGKAddition = 15;

        [Header("JoinTheAttackCurves")]
        public AnimationCurve[] JoinTheAttackCurves;


        // #TODO #DIVING
        // when a player dives, is "no call" good enough?
        // half time/full time


        /// <summary>
        /// Roll values to get if shoot is preferred.
        /// </summary>
        /// <param name="angle">Angle with the goal net</param>
        /// <param name="distance">Distance to the goal net</param>
        /// <returns></returns>
        public bool ShootRoll(in float angle, in float distance, in float toleranceMod = 1) {
            float distanceMod = AI_ShootToleranceDistanceCurveMod.Evaluate(distance);

            float angleModdedByDistance = angle * AI_DistanceToAngleCurveMod.Evaluate(distance);

            float angleDivider = AI_ShootToleranceDividerAngleCurveMod.Evaluate(angleModdedByDistance);

            float roller = AI_ShootTolerance * toleranceMod * distanceMod / angleDivider;

            return UnityEngine.Random.Range(0f, 100f) < roller;
        }
    }
}
