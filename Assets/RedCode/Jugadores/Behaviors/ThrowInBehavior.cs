using UnityEngine;
using System.Linq;

namespace RedCard {
    public class ThrowInBehavior : Behavior {
        private Jugador target;

        /// <summary>
        /// Player will try to find a target, closer to goal net and not marked.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="ball"></param>
        /// <param name="targetGoalNet"></param>
        /// <param name="teammates"></param>
        /// <returns></returns>
        public override bool Behave(bool isAlreadyActive) {
            if (!jugador.isThrowHolder) {
                return false;
            }

            if (!isAlreadyActive) {
                Vector3 playerPos = jugador.Position;

                var targetTeammate = teammates.
                    Where(x => x != jugador && !x.isGK).
                    OrderBy(x =>
                    Vector3.Distance(x.Position, playerPos)).
                    Take(5).
                    OrderBy(x => System.Guid.NewGuid()).
                    FirstOrDefault();

                if (targetTeammate != null) {
                    target = targetTeammate;
                    isAlreadyActive = true;
                }
            }

            if (isAlreadyActive) {
                jugador.CurrentAct = Acts.ThrowIn;

                var targetPos = target.Position;

                if (jugador.controller.LookTo(in dt, targetPos - jugador.Position)) {
                    jugador.Cross(targetPos);

                    jugador.passingTarget = target;
                    EventManager.Trigger(new PlayerThrowInEvent(jugador));
                }
            }

            return true;
        }
    }
}
