using UnityEngine;
using UnityEngine.UI;


namespace RedCard {
    public class FellThroughFloor : MonoBehaviour {

        public float fadeOutTime = 2f;
        public float floorOOB = -10f;
        public Image fadeOverlay;

        RefControls arbitro;

        float t = 0f;

        private void Awake() {
            fadeOverlay.transform.parent.gameObject.SetActive(false);
        }

        private void Update() {
            if (!arbitro) {
                arbitro = RedMatch.match.arbitro;
            }
            else {
                if (arbitro.transform.position.y < floorOOB) {
                    t += Time.deltaTime;
                    fadeOverlay.transform.parent.gameObject.SetActive(true);
                    fadeOverlay.color = Color.Lerp(Color.clear, Color.black, t / fadeOutTime);
                    if (t > fadeOutTime) {
                        if (t >= fadeOutTime) {
                            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                        }
                    }
                }
            }
        }
    }
}
