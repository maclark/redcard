using UnityEngine;
using System.Collections.Generic;

namespace RedCard {

    [System.Serializable]
    public abstract class BasePositionsData<T> : SerializedSingletonScriptable<T> where T : Object {

        [SerializeField] private Dictionary<Positions, FieldPosition> Indexed;

        public bool FixedLength = true;

        [SerializeField]
        public FieldPosition[] FieldPositions = new FieldPosition[] {
            new FieldPosition ( Positions.GK),
            new FieldPosition ( Positions.CB),
            new FieldPosition ( Positions.CB_R),
            new FieldPosition ( Positions.CB_L),
            new FieldPosition ( Positions.LB),
            new FieldPosition ( Positions.RB),
            new FieldPosition ( Positions.CM),
            new FieldPosition ( Positions.CM_R),
            new FieldPosition ( Positions.CM_L),
            new FieldPosition ( Positions.RMF),
            new FieldPosition ( Positions.LMF),
            new FieldPosition ( Positions.AMF),
            new FieldPosition ( Positions.AMF_R),
            new FieldPosition ( Positions.AMF_L),
            new FieldPosition ( Positions.ST),
            new FieldPosition ( Positions.ST_R),
            new FieldPosition ( Positions.ST_L)
        };

        public FieldPosition GetPosition(Positions position) {
            if (Indexed == null) {
                Indexed = new Dictionary<Positions, FieldPosition>();
                foreach (var fieldPos in FieldPositions) {
                    Indexed.Add(fieldPos.Position, fieldPos);
                }
            }

            return Indexed[position];
        }
    }
}
