using UnityEngine;
using UnityEngine.UI;

namespace RedCard {

    public class ColorBox : MonoBehaviour {

        [Header("ASSIGNATIONS")]
        public CustomizationCanvas customCan;
        public RectTransform swatchHoverHighlight;
        public RectTransform swatchSelectionHighlight;

        [Header("VARS")]
        public ColorRow[] rows = new ColorRow[0];
        public Button highlighted;

        void HighlightedSwatch(Button b) {
            highlighted = b;
            swatchHoverHighlight.gameObject.SetActive(true);
            swatchHoverHighlight.SetParent(b.transform.parent);
            swatchHoverHighlight.SetAsFirstSibling();
            swatchHoverHighlight.anchoredPosition = b.GetComponent<RectTransform>().anchoredPosition;
        }
        void DehighlightedSwatch(Button b) {
            if (highlighted == b) {
                swatchHoverHighlight.gameObject.SetActive(false);
            }
        }

        public void SelectedSwatch(Button b, int index) {
            if (customCan.transform.parent && customCan.transform.parent.TryGetComponent(out RefereeeCustomizer bathMirror)) {
                bathMirror.SelectedColor(Category.Nails, index);
            }
            swatchSelectionHighlight.gameObject.SetActive(true);
            swatchSelectionHighlight.SetParent(b.transform.parent);
            swatchSelectionHighlight.SetAsFirstSibling();
            swatchSelectionHighlight.anchoredPosition = b.GetComponent<RectTransform>().anchoredPosition;
        }

        public void FillColors(Color[] colors) {
            int colorIndex = 0;
            for(int i = 0; i < rows.Length; i++) {
                ColorRow row = rows[i];

                for (int j = 0; j < row.swatches.Length; j++) {
                    Button b = row.swatches[j];
                    if (colorIndex >= colors.Length) {
                        b.image.color = Color.clear;
                        b.interactable = false;
                    }
                    else {
                        Color c = colors[colorIndex];
                        b.image.color = c;
                        int index = colorIndex;
                        void ClickedSwatch() {
                            SelectedSwatch(b, index);
                        };
                        b.onClick.AddListener(ClickedSwatch);

                        PointerHandler ph = b.gameObject.AddComponent<PointerHandler>();
                        ph.onEnter += (data) => HighlightedSwatch(b);
                        ph.onExit += (data) => DehighlightedSwatch(b);

                        colorIndex++;
                    }
                }
            }
        }
    }
}
