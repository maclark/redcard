using UnityEngine;
using System.Collections.Generic;

public static class RedExtensions
{
    public static void Shuffle<T>(this IList<T> list) {
        for (int i = 0; i < list.Count; i++) {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    public static T ClosestTo<T>(this IEnumerable<T> data, Vector3 from) where T : MonoBehaviour {

        T closest = default(T);
        float minSqrDistance = float.MaxValue;
        foreach (T t in data) {
            if (t == null) continue;
            float sqrDistance = (t.transform.position - from).sqrMagnitude; 

            if (sqrDistance < minSqrDistance) {
                minSqrDistance = sqrDistance;
                closest = t;
            }
        }

        return closest;
    }

    public static T GetRandom<T>(this IList<T> list) {

        if (list == null || list.Count == 0) {
            Debug.LogError("empty list");
            return default(T);
        }

        return list[Random.Range(0, list.Count)];
    }

    public static Color SetAlpha(this Color c, float alpha) {
        return new Color(c.r, c.g, c.b, Mathf.Clamp01(alpha));
    }
}
