using System.Linq;
using UnityEngine;

namespace RedCard {
    public partial class Jugador {
        private const float AVOIDANCE_MIN_ANGLE = -50;
        private const float AVOIDANCE_MAX_ANGLE = 50;
        private AnimationCurve avoidanceCurve;

        public void AvoidMarkers(
            Jugador[] targets,
            ref Vector3 targetPos,
            float avoidanceDistance = 7) {

            if (avoidanceCurve == null) {
                avoidanceCurve = RedMatch.match.settings.avoidanceCurve;
            }

            Vector3 jugPos = pos;

            var dir = targetPos - jugPos;

            Debug.DrawLine(jugPos + Vector3.up, jugPos + Vector3.up + dir, Color.white, 0.05f);

            var distToTargetPos = dir.magnitude;

            (float avoidancePow, Vector3 avoidDir) avoid(Jugador marker) {
                var markerPos = marker.pos;

                var dirToMe = jugPos - markerPos;

                var avoidancePow = Mathf.Max(0, (avoidanceDistance - dirToMe.magnitude) / avoidanceDistance);

                float angleMod = avoidanceCurve.Evaluate(avoidancePow);

                var debugColor = Color.Lerp(Color.white, Color.red, angleMod);
                debugColor.a = angleMod;

                Debug.DrawLine(jugPos + Vector3.up, markerPos + Vector3.up, debugColor, 0.05f);

                return (angleMod, dirToMe.normalized);
            }

            var avoidanceData = targets.Where(x =>
            !x.isGoalie &&
            x.controller.isPhysicsEnabled).
            Select(x => avoid(x)).
            Where(x => x.avoidancePow > 0);

            if (avoidanceData.Count() == 0) {
                return;
            }

            Vector3 finalDir = Vector3.zero;

            foreach (var avoidTarget in avoidanceData) {
                finalDir += avoidTarget.avoidDir * avoidTarget.avoidancePow;
            }

            Debug.DrawLine(jugPos + Vector3.up, jugPos + Vector3.up + finalDir, Color.blue, 0.05f);

            var angle = Vector3.SignedAngle(dir, finalDir, Vector3.up);

            angle = Mathf.Clamp(angle, AVOIDANCE_MIN_ANGLE, AVOIDANCE_MAX_ANGLE);

            finalDir = Quaternion.Euler(0, angle, 0) * dir.normalized;

            targetPos = jugPos + finalDir * 2;
        }
    }
}
