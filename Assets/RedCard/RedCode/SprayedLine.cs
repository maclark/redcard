using UnityEngine;

public class SprayedLine : MonoBehaviour
{
    public float life = 5f;
    float t;
    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        if (t > life) Destroy(gameObject);
        
    }
}
