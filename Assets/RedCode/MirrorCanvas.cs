using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RedCard {

    public class MirrorCanvas : MonoBehaviour {

        public CanvasGroup group;
        public Button switchArms;
        public Button dominanceCheckbox;
        public TMP_Text makeDominantText;
        public Button back;


        [Header("SKIN COLOR")]
        public Button[] skinColorSwatches = new Button[0];

        [Header("HAIR")]
        public Slider hairThicknessSlider;
        public Slider hairLengthSlider;
        public Slider hairColorPicker;
        public Slider hairCurlSlider;
        public Button[] hairSwatches = new Button[0];

        [Header("MUSCLE")]
        public Slider muscleSlider;

        [Header("INK")]
        public Button pickTattoo;

        [Header("NAILS")]
        public RectTransform nailBox;
        public RectTransform nailBoxShadow;
        public NailPolishJar nailPolishJar;
        public RectTransform nailPolishRemoverSponge;
        public RectTransform nailPolishRemoverMiniSponge;
        public NailPolishBrush nailPolishBrush;
        public Color keratinColor = new Color(1f, 1f, 1f, .3f);
        public float maxNailHeight = 60f;
        public float minNailHeight = 20f;
        public float minPinkNailHeight = 15f;
        public Slider nailLengthSlider;
        public Button[] nails = new Button[0]; // pinky to thumb
        public Image[] fingers = new Image[0]; // pinky to thumb
        public int nailColorSelectedIndex = 1;
        public Color[] nailColors = new Color[0];

        [Header("COLOR BOXES")]
        public GameObject colorBoxPrefab;
        public GameObject colorRowPrefab;
        public float minColorBoxHeight = 30f;
        public float colorRowHeight = 60f;
        public ColorBox colorBox;
    }
}
