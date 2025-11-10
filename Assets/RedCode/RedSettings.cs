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

        // #TODO #DIVING
        // when a player dives, is "no call" good enough?
        // half time/full time
    }
}
