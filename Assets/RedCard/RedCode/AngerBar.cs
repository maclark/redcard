using UnityEngine;

namespace RedCard {
    public class AngerBar : MonoBehaviour {
        [SerializeField] SpriteRenderer fill;
        [SerializeField] SpriteRenderer background;

        const float yPos_fill = 0f;
        const float xPos_full = 0f;
        const float xPos_empty = -0.95f; 
        const float fill_width = 7.8f;
        const float fill_height = 0.8f;

        public void Awake() {
            SetFill(0f);
        }

        public void SetFill(float value) {
            fill.size = new Vector2(Mathf.Clamp01(value) * fill_width, fill_height);
            fill.gameObject.SetActive(value > 0f);
            background.gameObject.SetActive(value > 0f);
            fill.transform.localPosition = new Vector3(Mathf.Lerp(xPos_empty, xPos_full, value), yPos_fill);
        }
    }
}
