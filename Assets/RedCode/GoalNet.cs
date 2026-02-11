using UnityEngine;
using System.Linq;


namespace RedCard {

    public class GoalNet : MonoBehaviour {

        // #CAPS
        public Vector3 Position => transform.position;
        public Transform leftLimit;
        public Transform rightLimit;
        public Transform[] goalPoints;

        public Vector3 GetShootingVectorFromPoint(Jugador jugador,
            Transform point) {

            Vector3 playerPosition = jugador.Position;

            float skill = jugador.GetShooting();

            Vector3 dir = point.position - playerPosition;

            Vector3 dirErrorApplied = RedBall.ApplyDirectionError(dir, skill);

            // restore by distance
            var dist = Vector3.Distance(point.position, playerPosition);
            dirErrorApplied = Vector3.Lerp(dirErrorApplied, dir,
                RedMatch.match.settings.ShootErrorRemoveByDistance.Evaluate(dist));
            // 

            var errorAppliedAngle = Mathf.Abs(Vector3.SignedAngle(dir, dirErrorApplied, Vector3.up));

            //normalize it.
            dirErrorApplied = dirErrorApplied.normalized;

            var dir2D = dirErrorApplied;
            dir2D.y = 0;

            var dirUp = dirErrorApplied;
            dirUp.x = dirUp.z = 0;

            // add multipliers.
            dirErrorApplied += dir2D * RedMatch.match.settings.ShootingForwardAxisMultiplier;
            //

            dirErrorApplied *= RedMatch.match.settings.ShootPowerByDistanceCurve.Evaluate(dir.magnitude);

            dirErrorApplied *= RedMatch.match.settings.ShootPowerBySkillCurve.
                Evaluate(jugador.GetShooting() / 100f);

            Debug.Log($"[Shootpoint found] {dirErrorApplied}");

            dirErrorApplied += Vector3.up * dir.magnitude * RedMatch.match.settings.ShootingUpAxisDistanceMultiplier;

            Debug.Log($"[Shooting point y fixed] {dirErrorApplied}");

            Debug.DrawRay(playerPosition, dir, Color.yellow, 1);
            Debug.DrawRay(playerPosition, dirErrorApplied, Color.green, 1);

            if (dirErrorApplied.y < 1) {
                dirErrorApplied.y = 1;
            }

            return dirErrorApplied;
        }

        /// <summary>
        /// Checks all goal points, 
        /// and return shooting velocities with direction error applied.
        /// </summary>
        /// <param name="jugador">Shooter</param>
        /// <param name="colliders">Possible colliders</param>
        /// <returns>Velocity, and applied error.</returns>
        public (Transform shootPoint, float angleFree)
            GetShootingVector(Jugador jugador, Jugador[] colliders) {

            if (goalPoints.Length == 0) {
                return default;
            }

            var fieldSizeY = RedMatch.match.fieldSize.y;

            var mPosition = jugador.Position;

            float minAngle(Transform m_point) {
                var pointToPlayer = m_point.position - mPosition;

                float min = colliders.Select(x => Mathf.Min(Mathf.Abs(Vector3.SignedAngle(x.Position - mPosition, pointToPlayer, Vector3.up)), 45)).
                OrderBy(x => x).FirstOrDefault();
                return min;
            }

            var shootingVector =
                goalPoints.Select(x => (x, minAngle(x))).OrderBy(x =>
                 Random.Range(-5, 5) +
                 Mathf.Abs(x.x.position.x - fieldSizeY / 2) +
                 Random.Range(0, x.x.position.y) +
                 (45 - x.Item2) / 2).
                 FirstOrDefault();

            return shootingVector;
        }
    }
}
