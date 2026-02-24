using System.Collections.Generic;
using System.Linq;

namespace RedCard {
    public class PositionRules {

        private static Dictionary<FormationPosition, FormationPosition> rules = new Dictionary<FormationPosition, FormationPosition>() {
            { FormationPosition.GK, FormationPosition.GK },
            { FormationPosition.LB, FormationPosition.LB },
            { FormationPosition.RB, FormationPosition.RB },
            { FormationPosition.LMF, FormationPosition.LMF },
            { FormationPosition.RMF, FormationPosition.RMF },
            { FormationPosition.LW, FormationPosition.LW },
            { FormationPosition.RW, FormationPosition.RW },
            { FormationPosition.CB, FormationPosition.CB | FormationPosition.CB_L | FormationPosition.CB_R },
            { FormationPosition.CM, FormationPosition.CM | FormationPosition.CM_L | FormationPosition.CM_R },
            { FormationPosition.DMF, FormationPosition.DMF | FormationPosition.DMF_L | FormationPosition.DMF_R },
            { FormationPosition.AMF, FormationPosition.AMF | FormationPosition.AMF_L | FormationPosition.AMF_R },
            { FormationPosition.ST, FormationPosition.ST | FormationPosition.ST_L | FormationPosition.ST_R }
        };

        public static FormationPosition GetRandomPosition() {
            var randomPick = rules.Keys.OrderBy(x => System.Guid.NewGuid()).FirstOrDefault();
            return randomPick;
        }

        public static FormationPosition GetBasePosition(FormationPosition position) {
            var result = rules.Where(x => x.Value.HasFlag(position)).FirstOrDefault();
            if (result.Equals(default(KeyValuePair<FormationPosition, FormationPosition>))) {
                // position doesnt have a rule.
                return position;
            }

            return result.Key;
        }

        /// <summary>
        /// Get all playable positions for the given base position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static FormationPosition GetPositions(FormationPosition position) {
            return rules.Where(x => x.Key.HasFlag(position)).FirstOrDefault().Value;
        }

        public static IEnumerable<FormationPosition> GetAllPositions() {
            return rules.Keys;
        }
    }
}