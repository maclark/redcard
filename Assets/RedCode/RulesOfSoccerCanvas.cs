using UnityEngine;
using TMPro;

namespace RedCard {
    public class RulesOfSoccerCanvas : MonoBehaviour {
        public RectTransform titleSpread;
        public RectTransform contentsSpread;

        public RectTransform lawSpread;
        public TMP_Text lawSpreadTitle;
        public TMP_Text lawSpreadNum;
        //public Image logo; // logo doesn't ever change?
        public TMP_Text lawPageTitle;
        public TMP_Text lawPageNum;
        public TMP_Text lawPageText;

        public RectTransform normalSpread;
        public TMP_Text leftPageCol0;
        public TMP_Text leftPageCol1;
        public TMP_Text rightPageCol0;
        public TMP_Text rightPageCol1;

        public TMP_Text leftTopLawNum;
        public RectTransform leftTopDetails;

        public TMP_Text rightTopLawTitle;
        public RectTransform rightTopDetails;

        public TMP_Text leftBottomNum;
        public TMP_Text leftBottomBookTitle;
        public RectTransform leftBottomDetails;

        public TMP_Text rightBottomNum;
        public TMP_Text rightBottomBookTitle;
        public RectTransform rightBottomDetails;

    }
}
