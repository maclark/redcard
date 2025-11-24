using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RedCard {

    public class CustomizationCanvas : MonoBehaviour {

        public CanvasGroup group;
        public Button switchArms;
        public Button dominanceCheckbox;
        public TMP_Text makeDominantText;
        public Button back;

        [Header("AUDIO")]
        public float sliderSoundGap = .05f; // 1/20th
        public AudioClip selectedSound;
        public AudioClip sliderSound;
        public AudioClip grabbedBrush; // also for put away brush?
        public AudioClip paintedNail;
        public AudioClip cleanedNail;
        public AudioClip selectedNailColor;
        public AudioClip selectedSponge;
        public float lastSlideSoundPlayed;


        [Header("SKIN COLOR")]
        public Button[] skinColorSwatches = new Button[0];

        [Header("HAIR")]
        public Slider hairThicknessSlider;
        public Slider hairLengthSlider;
        public Slider hairCurlSlider;
        public Button[] hairColorSwatches = new Button[0];

        [Header("MUSCLE")]
        public Slider muscleSlider;
        public RectTransform respectAndStamina;
        public RectTransform plusIcon;
        public RectTransform minusIcon;

        [Header("INK")]
        public Button pickTattoo;

        [Header("NAILS")]
        public RectTransform nailsRect;
        public RectTransform nailsBackground;
        public RectTransform nailsBackgroundShadow;
        public NailPolishJar nailPolishJar;
        public RectTransform nailPolishRemoverSponge;
        public RectTransform nailPolishRemoverMiniSponge;
        public NailPolishBrush nailPolishBrush;
        public Slider nailLengthSlider;
        public Button[] nails = new Button[0]; // pinky to thumb
        public Image[] nailLines = new Image[0]; // pinky to thumb
        public Image[] fingers = new Image[0]; // pinky to thumb
        public int nailColorSelectedIndex = 1;
        public Color[] nailColors = new Color[0];

        [Header("COLOR BOXES")]
        public ColorBox colorBox;
        public RectTransform swatchSkinSelectionHighlight;
        public RectTransform swatchHairSelectionHighlight;
        public RectTransform swatchHoverHighlight;
        public Button highlighted;

        private RefereeCustomizer bathMirror;
        private bool initialized = false;


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
        public void SelectedSkinSwatch(Button b, int index, bool silence = false) {
            bathMirror.SelectedColor(Category.Skin, index);
            swatchSkinSelectionHighlight.gameObject.SetActive(true);
            swatchSkinSelectionHighlight.SetParent(b.transform.parent);
            swatchSkinSelectionHighlight.SetAsFirstSibling();
            swatchSkinSelectionHighlight.anchoredPosition = b.GetComponent<RectTransform>().anchoredPosition;
            if (!silence & bathMirror.mode != MirrorMode.Approaching) AudioManager.am.sfxAso.PlayOneShot(selectedSound);
        }

        public void PlaySliderSound() {
            if (Time.time - lastSlideSoundPlayed > sliderSoundGap) {
                AudioManager.am.sfxAso.PlayOneShot(selectedSound);
                lastSlideSoundPlayed = Time.time;
            }
        }

        public void SelectedHairSwatch(Button b, int index, bool silence=false) {
            bathMirror.SelectedColor(Category.Hair, index);
            swatchHairSelectionHighlight.gameObject.SetActive(true);
            swatchHairSelectionHighlight.SetParent(b.transform.parent);
            swatchHairSelectionHighlight.SetAsFirstSibling();
            swatchHairSelectionHighlight.anchoredPosition = b.GetComponent<RectTransform>().anchoredPosition;
            if (!silence && bathMirror.mode != MirrorMode.Approaching) AudioManager.am.sfxAso.PlayOneShot(selectedSound); 
        }

        // particular values will be set when el arbitro approaches the mirror
        public void InitSkinAndHairColorButtons(RefereeCustomizer motherMirror) {
            if (initialized) return;
            initialized = true;

            bathMirror = motherMirror;
            CustomizationOptions cops = RedMatch.match.customizationOptions;

            for (int i = 0; i < skinColorSwatches.Length; i++) {
                Button b = skinColorSwatches[i];
                if (i < cops.skinSwatchColors.Length) b.image.color = cops.skinSwatchColors[i];
                int index = i;
                void Clicked() {
                    SelectedSkinSwatch(b, index);
                }
                b.onClick.AddListener(Clicked);
                PointerHandler ph = b.gameObject.AddComponent<PointerHandler>();
                ph.onEnter += (pointerData) => HighlightedSwatch(b);
                ph.onExit += (pointerData) => DehighlightedSwatch(b);
            }

            for (int i = 0; i < hairColorSwatches.Length; i++) {
                Button b = hairColorSwatches[i];
                if (i < cops.hairSwatchColors.Length) b.image.color = cops.hairSwatchColors[i];
                int index = i;
                void Clicked() {
                    SelectedHairSwatch(b, index);
                }
                b.onClick.AddListener(Clicked);
                PointerHandler ph = b.gameObject.AddComponent<PointerHandler>();
                ph.onEnter += (data) => HighlightedSwatch(b);
                ph.onExit += (data) => DehighlightedSwatch(b);
            }

            nailColors = (Color[])cops.nailSwatchColors.Clone();
            colorBox.FillColors(nailColors);
            colorBox.gameObject.SetActive(false);
        }
    }
}
