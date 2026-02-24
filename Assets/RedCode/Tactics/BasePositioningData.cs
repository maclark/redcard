using UnityEngine;
using System.Collections.Generic;

namespace RedCard {

    [System.Serializable]
    public abstract class BasePositionsData<T> : SerializedSingletonScriptable<T> where T : Object {

        [SerializeField] private Dictionary<FormationPosition, FieldPosition> Indexed;

        public bool FixedLength = true;

        [SerializeField]
        public FieldPosition[] FieldPositions = new FieldPosition[] {
            new FieldPosition ( FormationPosition.GK),
            new FieldPosition ( FormationPosition.CB),
            new FieldPosition ( FormationPosition.CB_R),
            new FieldPosition ( FormationPosition.CB_L),
            new FieldPosition ( FormationPosition.LB),
            new FieldPosition ( FormationPosition.RB),
            new FieldPosition ( FormationPosition.CM),
            new FieldPosition ( FormationPosition.CM_R),
            new FieldPosition ( FormationPosition.CM_L),
            new FieldPosition ( FormationPosition.RMF),
            new FieldPosition ( FormationPosition.LMF),
            new FieldPosition ( FormationPosition.AMF),
            new FieldPosition ( FormationPosition.AMF_R),
            new FieldPosition ( FormationPosition.AMF_L),
            new FieldPosition ( FormationPosition.ST),
            new FieldPosition ( FormationPosition.ST_R),
            new FieldPosition ( FormationPosition.ST_L)
        };

        public FieldPosition GetPosition(FormationPosition position) {
            if (Indexed == null) {
                Indexed = new Dictionary<FormationPosition, FieldPosition>();
                foreach (var fieldPos in FieldPositions) {
                    Indexed.Add(fieldPos.Position, fieldPos);
                }
            }

            return Indexed[position];
        }
    }
}
