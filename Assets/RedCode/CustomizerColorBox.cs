using UnityEngine;
using UnityEngine.UI;

namespace RedCard {

    public class ColorBox : MonoBehaviour {
        public MirrorCanvas mirror;
        public float parentHeightCache;
        public RectTransform rtParent;
        public RectTransform rtParentShadow;
        public RectTransform botLine;
        public RectTransform rightLine;
        public RectTransform leftLine;
        public RectTransform swatchHighlight;
        public Sprite noColorIcon;

        public const float vert_line_gap = 15f;
        public const float horz_line_gap = 12f;
        public const float line_width = 4f;
        public const float box_width = 400f;
        public const float first_row_offset = 10f;

        void SelectedSwatch(Button b) {
            swatchHighlight.anchoredPosition = b.GetComponent<RectTransform>().anchoredPosition;
            if (mirror.transform.parent && mirror.transform.parent.TryGetComponent(out BathroomMirror bathMirror)) {
                bathMirror.SelectedColor(b.image.color);
            }
        }

        void ClearColor(Button b) {
            swatchHighlight.anchoredPosition = b.GetComponent<RectTransform>().anchoredPosition;
            if (mirror.transform.parent && mirror.transform.parent.TryGetComponent(out BathroomMirror bathMirror)) {
                bathMirror.ClearColor();
            }
        }

        public static ColorBox MakeColorBox(MirrorCanvas mirror, RectTransform rt, RectTransform rtShadow, Color[] colors) {

            ColorBox box = Instantiate(mirror.colorBoxPrefab, rt).GetComponent<ColorBox>();

            rt.parent.SetAsLastSibling();

            box.parentHeightCache = rt.sizeDelta.y;
            box.mirror = mirror;
            box.rtParent = rt;
            box.rtParentShadow = rtShadow;

            int rowsNeeded = Mathf.CeilToInt(colors.Length / 6f);
            Debug.LogWarning("rows needed " + rowsNeeded);
            float extension = mirror.minColorBoxHeight + rowsNeeded * mirror.colorRowHeight;
            Vector2 newSize = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y + extension);
            box.rtParent.sizeDelta = newSize;
            box.rtParentShadow.sizeDelta = newSize;
            box.botLine.anchoredPosition = new Vector2(0f, -vert_line_gap - mirror.colorRowHeight * rowsNeeded);
            box.leftLine.sizeDelta = new Vector2(line_width, extension - vert_line_gap);
            box.leftLine.anchoredPosition = new Vector2(-box_width / 2f + horz_line_gap, 0f);
            box.rightLine.sizeDelta = new Vector2(line_width, extension - vert_line_gap);
            box.rightLine.anchoredPosition = new Vector2(box_width / 2f - horz_line_gap, 0f);

            int colorIndex = 0;
            for(int i = 0; i < rowsNeeded; i++) {
                ColorRow row = Instantiate(mirror.colorRowPrefab, box.transform).GetComponent<ColorRow>();
                if (row.TryGetComponent(out RectTransform rtRow)) {
                    rtRow.anchoredPosition = new Vector2(0f, first_row_offset -i * mirror.colorRowHeight);
                }
                else Debug.LogWarning("no rt on color row");

                print("row.swatches.length " + row.swatches.Length);
                for (int j = 0; j < row.swatches.Length; j++) {

                    Button b = row.swatches[j];
                    if (colorIndex >= colors.Length) {
                        b.image.color = Color.clear;
                        b.interactable = false;
                    }
                    else {
                        Color c = colors[colorIndex];
                        if (c.a == 0f) {
                            b.image.sprite = box.noColorIcon;
                            void ClickedClearSwatch() {
                                box.ClearColor(b);
                            };
                            b.onClick.AddListener(ClickedClearSwatch);
                        }
                        else {
                            b.image.color = c;
                            void ClickedSwatch() {
                                box.SelectedSwatch(b);
                            };
                            b.onClick.AddListener(ClickedSwatch);
                        }
                        colorIndex++;
                    }
                }
            }

            return box;
        }
    }
}
