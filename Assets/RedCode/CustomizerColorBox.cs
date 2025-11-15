using UnityEngine;
using UnityEngine.UI;

namespace RedCard {

    public class ColorBox : MonoBehaviour {
        [Header("ASSIGNATIONS")]
        public RectTransform rtParent;
        public RectTransform rtParentShadow;
        public RectTransform botLine;
        public RectTransform rightLine;
        public RectTransform leftLine;
        public RectTransform swatchHoverHighlight;
        public RectTransform swatchSelectionHighlight;

        [Header("VARS")]
        public CustomizationCanvas mirror;
        public float parentHeightCache;
        public ColorRow[] rows = new ColorRow[0];
        public Button highlighted;

        public const float horz_line_gap = 12f;
        public const float line_width = 40f;

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
            if (mirror.transform.parent && mirror.transform.parent.TryGetComponent(out BathroomMirror bathMirror)) {
                bathMirror.SelectedColor(Category.Nails, index);
            }
            swatchSelectionHighlight.gameObject.SetActive(true);
            swatchSelectionHighlight.SetParent(b.transform.parent);
            swatchSelectionHighlight.SetAsFirstSibling();
            swatchSelectionHighlight.anchoredPosition = b.GetComponent<RectTransform>().anchoredPosition;
        }

        public static ColorBox MakeColorBox(CustomizationCanvas mirror, RectTransform parent, RectTransform rt, RectTransform rtShadow, Color[] colors) {

            ColorBox box = Instantiate(mirror.colorBoxPrefab, parent).GetComponent<ColorBox>();
            mirror.nailPolishBrush.transform.SetAsLastSibling();
            mirror.nailPolishRemoverSponge.transform.SetAsLastSibling();
            parent.SetAsLastSibling();

            box.parentHeightCache = rt.sizeDelta.y;
            box.mirror = mirror;
            box.rtParent = rt;
            box.rtParentShadow = rtShadow;
            box.swatchHoverHighlight.gameObject.SetActive(false);
            box.swatchSelectionHighlight.gameObject.SetActive(false);

            int rowsNeeded = Mathf.CeilToInt(colors.Length / 6f);
            float extension = mirror.minColorBoxHeight + rowsNeeded * mirror.colorRowHeight;

            // have to scale sizes of boxes
            Vector2 newSize = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y + 10f * extension);
            box.rtParent.sizeDelta = newSize;
            box.rtParentShadow.sizeDelta = newSize;

            // but positions aren't scaled 
            float yBotLine = -extension + horz_line_gap;
            box.botLine.anchoredPosition = new Vector2(0f, yBotLine);
            box.leftLine.sizeDelta = new Vector2(line_width, 10f * Mathf.Abs(yBotLine) + line_width);
            box.rightLine.sizeDelta = new Vector2(line_width, 10f * Mathf.Abs(yBotLine)+ line_width);


            int colorIndex = 0;
            box.rows = new ColorRow[rowsNeeded];
            for(int i = 0; i < rowsNeeded; i++) {
                ColorRow row = Instantiate(mirror.colorRowPrefab, box.transform).GetComponent<ColorRow>();
                box.rows[i] = row;
                if (row.TryGetComponent(out RectTransform rtRow)) {
                    rtRow.anchoredPosition = new Vector2(0f, -i * mirror.colorRowHeight + 7f);
                }
                else Debug.LogWarning("no rt on color row");

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
                            box.SelectedSwatch(b, index);
                        };
                        b.onClick.AddListener(ClickedSwatch);

                        PointerHandler ph = b.gameObject.AddComponent<PointerHandler>();
                        ph.onEnter += (data) => box.HighlightedSwatch(b);
                        ph.onExit += (data) => box.DehighlightedSwatch(b);

                        colorIndex++;
                    }
                }
            }

            return box;
        }
    }
}
