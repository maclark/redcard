using System.Linq;
using UnityEngine;

namespace RedCard {
    public abstract class AbstractTeammatesShouldBeware : Behavior {
        private const int MAX_MARKING = 3;

        public (bool shouldI, Jugador opponent) ShouldIBeware (bool ignoreOffside = false) {
            var goalNetPosition = goalNet.Position;

            var orderedTeammates = teammates.Where(j => !j.IsGK).
                Select(x => (x, x.fieldProgress));

            var opponentsBehinds = opponents.Where(j => !j.IsGK && (ignoreOffside || !j.isInOffsidePosition)).
            OrderByDescending (x=>x.fieldProgress);

            int opponentsBehindCount = Mathf.Min (MAX_MARKING, opponentsBehinds.Count());

            var teammatesShouldBeBeware = orderedTeammates.OrderBy(x => x.Item2).Take(opponentsBehindCount).Select(x => x.x).ToList ();

            if (teammatesShouldBeBeware.Contains(jugador)) {
                return (true, opponentsBehinds.ElementAt(teammatesShouldBeBeware.FindIndex(x => x == jugador)));
            } else {
                return default;
            }
        }
    }
}
