using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RedCard {

    public class PageImage {
        public Sprite spr;
        public Vector2 size;
        public Vector2 anchoredPos;
    }

    public class PrintingPress : MonoBehaviour {

        public RulesOfSoccerCanvas rulesOfSoccerCanvas;
        public AudioClip bookOpening;
        public AudioClip bookClosing;
        public AudioClip pageFlip;
        public AudioClip pageFlipping;

        private static PrintingPress _instance;

        public static PrintingPress press {
            get {
                if (!_instance) {
                    _instance = FindAnyObjectByType<PrintingPress>();
                    if (!_instance) Debug.LogError("couldn't find an instance of the bookmaker");
                }
                return _instance;
            }
        }

        private void Awake() {
            if (_instance) {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }
    }
}
