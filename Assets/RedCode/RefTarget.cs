using UnityEngine;

namespace RedCard {

    public enum TargetType {
        SixYardBox, // goal kick
        CornerFlag,
        CenterCircle,
        PenaltySpot,
        Ball,
        TechnicalArea,
        Bench,
        Crowd,
        Sideline,

        Player,
        Coach,
        Fan,
        Linesman,
    }

    public class RefTarget : MonoBehaviour {
        public TargetType targetType;
        public GameObject outline;
        public FieldEnd attackingEnd;

        private void Start() {
            if (outline) outline.SetActive(false);
            else {
                Debug.LogError(name + " has no outline");
                outline = new GameObject();
                outline.transform.SetParent(transform);
                outline.name = "FallbackOutlineObject";
            }

            RedMatch.Match.targets.Add(this);
        }
    }
}