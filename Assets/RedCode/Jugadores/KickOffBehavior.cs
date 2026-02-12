
using UnityEngine;

using System.Linq;

namespace RedCard {
    public class KickOffBehaviour : Behavior {
        private Jugador teammateToPass;

        public override bool Behave(bool isAlreadyActive) {
            if (matchStatus != MatchStatus.WaitingForKickOff) {
                return false;
            }

            if (ball.holder != jugador) {
                return false;
            }

            if (!isAlreadyActive) {
                // find a player and pass.
                teammateToPass =
                    teammates.Where(j => j != jugador). // from all teammates
                    OrderBy(j => Vector3.Distance(j.Position, jugador.Position)). // order by position
                    Take(4). // take first 4
                    OrderBy(x => System.Guid.NewGuid()).FirstOrDefault(); // pick randomly.

                if (teammateToPass != null) {
                    isAlreadyActive = true;
                }
            }

            if (isAlreadyActive) {
                if (jugador.PassToTarget(in dt, teammateToPass.Position)) {
                    // set pass target. after pass target player will behave with BallChasingBehaviour.
                    jugador.passingTarget = teammateToPass;
                }
            }

            return true;
        }
    }
}
