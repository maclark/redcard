using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TypeItOut : MonoBehaviour
{
    public string goal;
    public TMP_Text txt;
    public bool go = false;

    public float[] delays = new float[] { .125f };
    float t;
    int index;



    void Start()
    {
        Debug.Assert(goal.Length == delays.Length);
    }

    // Update is called once per frame
    void Update()
    {

        if (Keyboard.current.gKey.wasPressedThisFrame) {
            go = true;
            index = 0;
            t = 0f;
        }

        if (go && index < goal.Length) {
            t += Time.deltaTime;
            if (t > delays[index]) {
                t -= delays[index];
                index++;
            }
            txt.text = goal.Substring(0, index);
        }

    }
}
