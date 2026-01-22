using UnityEngine;


public class CirclePlaneGenerator : MonoBehaviour {

    public Transform[] segments = new Transform[0];
    public float r = 5f;
    public float offset = 0f;
    public int count = 16;

    public GameObject prefab;


    public void Generate() {

        Clear();
        segments = new Transform[count];
        for (int i = 0; i < count; i++) {
            GameObject go = Instantiate(prefab);
            segments[i] = go.transform;
            go.transform.SetParent(transform);
        }

        float dTheta = Mathf.PI * 2f / segments.Length;
        float theta = 0f;

        for (int i = 0; i < segments.Length; i++) {

            Transform t = segments[i];

            t.position = transform.position + new Vector3(r * Mathf.Cos(theta), 0f, r * Mathf.Sin(theta));

            //t.rotation = Quaternion.Euler(0f, offset + Mathf.Rad2Deg * (theta + Mathf.PI / 2f), 0f);
            t.rotation = Quaternion.LookRotation(transform.position - t.position, Vector3.up);

            theta += dTheta;
        }
    }
    public void Clear() {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
