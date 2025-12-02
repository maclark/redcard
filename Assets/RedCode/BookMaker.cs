using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RedCard {

    public class PageImage {
        public Sprite spr;
        public Vector2 size;
        public Vector2 anchoredPos;
    }

    public class BookMaker : MonoBehaviour {

        public Canvas canvas;
        public RulesOfSoccerCanvas rulesOfSoccerCanvas;
        public Image background;
        public TMP_Text leftPage;
        public TMP_Text rightPage;
        public TMP_Text leftPageNo;
        public TMP_Text rightPageNo;
        public Image leftBackground;
        public Image rightBackground;
        public Image[] pageImages = new Image[0];


        private static BookMaker _instance;

        public static BookMaker maker {
            get {
                if (!_instance) {
                    _instance = FindAnyObjectByType<BookMaker>();
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
