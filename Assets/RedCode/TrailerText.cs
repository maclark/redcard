using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace RedCard {

    public class TrailerText : MonoBehaviour {
        public bool go = false;
        public TMP_Text[] texts = new TMP_Text[0];

        //public string[] texts = new string[] {
        //    "In the time before V.A.R.",
        //    "One man stood alone",
        //    "Between the beautiful game...",
        //    "and chaosBetween the beautiful game..."
        //};

        public float[] delays = new float[] { 3f };
        public float[] fades = new float[] { .3f };

        public float t = 0f;
        public float fadeOut = 5f;
        public int index = 0;

        void Start() {
            foreach (var txt in texts) txt.color = Color.clear;
        }

        // Update is called once per frame
        void Update() {

            if (Keyboard.current.iKey.wasPressedThisFrame) {
                go = true;
                index = 0;
                t = 0f;
                Debug.Assert(delays.Length == texts.Length);
                Debug.Assert(fades.Length == texts.Length);
                foreach (var txt in texts) txt.color = Color.clear;
            }


            if (go && index < texts.Length) {
                t += Time.deltaTime;
                texts[index].color = Colors.redcard.SetAlpha(t / fades[index]);

                if (t > delays[index]) {
                    t -= delays[index];
                    index++;
                }
            }
            else if (go) {
                print("texts.length " + texts.Length);
                for (int i = 0; i < texts.Length; i++) {
                    print("texts[i].color.a " + texts[i].color.a);
                    texts[i].color = Colors.redcard.SetAlpha(texts[i].color.a - Time.deltaTime * (1f / fadeOut));
                }
            }

        }
    }
} 
