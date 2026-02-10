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

        // #TODO #DIVING
        // when a player dives, is "no call" good enough?
        // half time/full time
    }
}
