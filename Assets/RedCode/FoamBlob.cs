using UnityEngine;
using System.Collections.Generic;

namespace RedCard {
    public class FoamBlob : MonoBehaviour {
        public float age = 0f;
        public float targetScale = 1f;

        public static List<Vector3> SmoothLine(List<Vector3> points, int subdivisions) {
            List<Vector3> smoothedPoints = new List<Vector3>();

            if (points.Count < 2)
                return points;

            for (int i = 0; i < points.Count - 1; i++) {
                Vector3 p0 = i == 0 ? points[i] : points[i - 1];
                Vector3 p1 = points[i];
                Vector3 p2 = points[i + 1];
                Vector3 p3 = (i + 2 < points.Count) ? points[i + 2] : p2;

                for (int j = 0; j <= subdivisions; j++) {
                    float t = j / (float)subdivisions;
                    Vector3 point = CatmullRom(p0, p1, p2, p3, t);
                    smoothedPoints.Add(point);
                }
            }

            return smoothedPoints;
        }

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
            // Catmull-Rom spline formula
            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
            );
        }
    }
}
