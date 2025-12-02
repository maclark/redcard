using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace RedCard {

    public class RulesOfSoccer : MonoBehaviour, IBookText {

        public Transform contentsButtonBoxes;
        public Transform jumpToContents;
        public Transform[] jumpToLawButtons = new Transform[0];

        // (law#, lawTitle, lawStartPageIndex)
        public List<(int, string, int)> lawSections = new List<(int, string, int)>() {
            {(1, "Field of Play", 4)},
            {(2, "The Ball", 8)},
            {(3, "Number of Players", 10)},
            {(4, "Players' Equipment", 14)},
            {(5, "Referees", 18)},
            {(6, "Field of Play", 22)},
            {(7, "Field of Play", 26)},
            {(8, "Field of Play", 30)},
            {(9, "Field of Play", 34)},
            {(10, "Field of Play", 38)},
            {(11, "Field of Play", 42)},
        };

        // ACTUAL BOOK LAYOUT:
        // i-ii:    blank page  | title
        // iii-iv:  copyright   | user's guide
        // v-vi:    blank page  | contents
        // vii-viii: contents   | notes
        // ix-1:    blank page  | law I
        // 2:       text
        // then each law has a blank page + Law #

        // we will do...
        // note that indices start with 0!
        // i-ii:    copyright   | title
        // iii-iv:  contents    | contents
        // 5-6:     law I       | text

        // hm, will want to do chapter headings specially 
        // hm, translation for books will be expensive....

        const int INDEX_PAGE1 = 4;

        public int GetPageCount() {
            return 46;
        }

        public void ShowPage(BookMaker maker, int leftPageIndex) {

            RulesOfSoccerCanvas rosc = maker.rulesOfSoccerCanvas;

            string leftDisplayPage = "";
            if (leftPageIndex < INDEX_PAGE1) {
                leftDisplayPage = (leftPageIndex + 1).ToRomanLower();
            }
            else {
                // if index equals index of page 1, then we want "1"
                leftDisplayPage = (leftPageIndex - INDEX_PAGE1 + 1).ToString();
            }
            int rightPageIndex = leftPageIndex + 1;
            string rightDisplayPage = "";
            if (rightPageIndex < INDEX_PAGE1) {
                rightDisplayPage = (rightPageIndex + 1).ToRomanLower();
            }
            else {
                // if index equals index of page 1, then we want "1"
                rightDisplayPage = (rightPageIndex - INDEX_PAGE1 + 1).ToString();
            }
            rosc.leftBottomNum.text = leftDisplayPage;
            rosc.rightBottomNum.text = rightDisplayPage;


            // true everywhere except at contents
            jumpToContents.gameObject.SetActive(true);
            contentsButtonBoxes.gameObject.SetActive(false);

            rosc.titleSpread.gameObject.SetActive(false);
            rosc.contentsSpread.gameObject.SetActive(false);
            rosc.lawSpread.gameObject.SetActive(false);
            rosc.normalSpread.gameObject.SetActive(false);

            if (leftPageIndex == 0) {
                rosc.titleSpread.gameObject.SetActive(true);
                rosc.leftTopDetails.gameObject.SetActive(false);
                rosc.rightTopDetails.gameObject.SetActive(false);
                rosc.leftBottomDetails.gameObject.SetActive(false);
                rosc.rightBottomDetails.gameObject.SetActive(false);
            }
            else if (leftPageIndex == 2) {
                jumpToContents.gameObject.SetActive(false);
                contentsButtonBoxes.gameObject.SetActive(true);
                rosc.contentsSpread.gameObject.SetActive(true);
                rosc.leftTopDetails.gameObject.SetActive(false);
                rosc.rightTopDetails.gameObject.SetActive(false);
                rosc.leftBottomDetails.gameObject.SetActive(true);
                rosc.rightBottomDetails.gameObject.SetActive(true);
            }
            else {
                // need to go through law sections to find what we're in
                // if we're at the start, then we need to use law spread, not normal
                (int, string, int) law = new (0, "", 0);
                bool atLawTitle = false;
                for (int i = lawSections.Count - 1; i >= 0; i--) {
                    law = lawSections[i];
                    if (law.Item3 == leftPageIndex) {
                        atLawTitle = true;
                        break;
                    }
                    else if (law.Item3 < leftPageIndex) {
                        break;
                    }
                }

                if (law.Item1 == 0) {
                    Debug.LogError("couldn't find law section for page index " + leftPageIndex);
                }
                else {
                    if (atLawTitle) {
                        rosc.lawSpread.gameObject.SetActive(true);
                        rosc.lawSpreadNum.text = law.Item1.ToRomanUpper();
                        rosc.lawSpreadTitle.text = law.Item2;
                        rosc.leftTopDetails.gameObject.SetActive(false);
                        rosc.rightTopDetails.gameObject.SetActive(false);
                        rosc.leftBottomDetails.gameObject.SetActive(false);
                        rosc.rightBottomDetails.gameObject.SetActive(false);
                    }
                    else {
                        rosc.normalSpread.gameObject.SetActive(true);
                        rosc.leftTopLawNum.text = "LAW " + law.Item1.ToRomanUpper();
                        rosc.rightTopLawTitle.text = law.Item2;
                        rosc.leftTopDetails.gameObject.SetActive(true);
                        rosc.rightTopDetails.gameObject.SetActive(true);
                        rosc.leftBottomDetails.gameObject.SetActive(true);
                        rosc.rightBottomDetails.gameObject.SetActive(true);
                    }
                }
            }
        }
    }
}
