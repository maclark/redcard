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
        public Image background;
        public TMP_Text leftPage;
        public TMP_Text rightPage;
        public TMP_Text leftPageNo;
        public TMP_Text rightPageNo;
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

        public void WritePages(int pageNo, string leftWords, string rightWords, PageImage[] illustrations) {
            for (int i = 0; i < pageImages.Length; i++) {
                Image im = pageImages[i];
                if (illustrations != null && i < illustrations.Length) {
                    im.gameObject.SetActive(true);
                    PageImage pi = illustrations[i];
                    im.sprite = pi.spr;
                    RectTransform rt = im.GetComponent<RectTransform>();
                    rt.sizeDelta = pi.size;
                    rt.anchoredPosition = pi.anchoredPos;
                }
                else im.gameObject.SetActive(false);
            }

            leftPage.text = leftWords;
            rightPage.text = rightWords;
            leftPageNo.text = Common.int_strings[pageNo];
            rightPageNo.text = Common.int_strings[pageNo + 1];
        }
    }
}
