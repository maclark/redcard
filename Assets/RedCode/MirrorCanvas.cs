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
        public float nailWidth = 30f;
        public float pinkyNailWidth = 28f;
        public Slider nailLengthSlider;
        public Button[] nails = new Button[0];
        public Button pinkyFinger;
        public Button ringFinger;
        public Button middleFinger;
        public Button indexFinger;
        public Button thumb;
        public Image[] fingers = new Image[0];

    }
}
