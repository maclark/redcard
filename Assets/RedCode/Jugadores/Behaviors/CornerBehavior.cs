using UnityEngine;
using System.Linq;

namespace RedCard {
    public class CornerBehaviour : Behavior {
        private readonly Vector2 cornerBox = new Vector2(8, 16);

        private const float TO_PENALTY_BOX = 9.15f;

        private const float TO_TARGET = 0.8f;

        private const float DIRECTION_MULTIPLIER = 7;

        private Vector3 target;

        /// <summary>
        /// Player will try to find a target, closer to goal net and not marked.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="ball"></param>
        /// <param name="targetGoalNet"></param>
        /// <param name="teammates"></param>
        /// <returns></returns>
        public override bool Behave(bool isAlreadyActive) {
            if (!jugador.isCornerHolder) {
                return false;
            }

            if (!isAlreadyActive) {
                var penaltyPoint = targetGoalNet.Position;
                penaltyPoint -= targetGoalNet.transform.forward * TO_PENALTY_BOX;

                target = penaltyPoint + new Vector3(
                    Random.Range(-cornerBox.x, cornerBox.x),
                    0,
                    Random.Range(-cornerBox.y, cornerBox.y)
                    );

                var closest = teammates.OrderBy(x => Vector3.Distance(x.Position, target)).FirstOrDefault();

                target = Vector3.Lerp(target, closest.Position, TO_TARGET) + DIRECTION_MULTIPLIER * (closest.Position - jugador.Position).normalized;

                Debug.Log($"Corner target is {target}");

                isAlreadyActive = true;
            }

            if (isAlreadyActive) {
                // interesting, is a corner a thrown in?
                jugador.CurrentAct = Acts.ThrowIn;

                jugador.Cross(target);

                // teammates chase the ball directly.
                foreach (Jugador e in jugador.team.jugadores) {
                    if (e.fieldProgress > 0.8f) {
                        // hm, by string eh? a bit lazy, FS, perhaps
                        e.ActivateBehavior("BallChasingWithoutCondition");
                    }
                }
            }

            return true;
        }
    }
}
