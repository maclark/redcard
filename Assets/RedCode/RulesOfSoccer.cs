using UnityEngine;
using UnityEngine.AddressableAssets;
using System.IO;
using System.Collections.Generic;


namespace RedCard {

    public class RulesOfSoccer : MonoBehaviour, IBookText {

        public AssetReferenceT<TextAsset> textRef;
        public Transform contentsButtonBoxes;
        public BookPageButton jumpToContents;
        public BookPageButton[] jumpToLawButtons = new BookPageButton[0];
        public BookDocument document;

        // (law#, lawTitle, lawStartPageIndex)
        public List<(int, string, int)> lawSections = new List<(int, string, int)>() {
            {(1, "Field of Play", 4)},
            {(2, "The Ball", 8)},
            {(3, "Number of Players", 10)},
            {(4, "Players' Equipment", 14)},
            {(5, "Referees", 18)},
            {(6, "Linesmen", 22)},
            {(7, "Duration of Game", 26)},
            {(8, "The Start of Play", 30)},
            {(9, "Ball In and Out of Play", 34)},
            {(10, "Method of Scoring", 38)},
            {(11, "Off-Side", 42)},
            {(12, "Fouls and Misconduct", 46)},
            {(13, "Free-kick", 50)},
            {(14, "Penalty-kick", 54)},
            {(15, "Throw-in", 58)},
            {(16, "Goal-Kick", 62)},
            {(17, "CornerKick", 64)},
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
        const int INDEX_CONTENTS = 2;
        const string INDENT_SPACES = "    "; // 4 spaces


        public void Awake() {

            jumpToContents.number = INDEX_CONTENTS;

            for (int i = 0; i < jumpToLawButtons.Length; i++) {
                BookPageButton jump = jumpToLawButtons[i];
                if (i < lawSections.Count) {
                    jump.number = lawSections[i].Item3;
                }
                else Debug.LogWarning("more jump to law buttons than law sections");
            }

            LoadROSC();
        }

#if UNITY_EDITOR

        private void Update() {

            if (UnityEngine.InputSystem.Keyboard.current.f5Key.wasPressedThisFrame) {
                LoadROSC();
            }
        }

#endif


        private void LoadROSC() {
            if (textRef.Asset is TextAsset ta) {
                document = BookParser.Parse(ta.text);
            }
            else Debug.LogError("cannot find text for rules of soccer ros: " + textRef.Asset);
        }

        public int GetPageCount() {
            return Mathf.Max(70, lawSections[^1].Item3 + 2);
        }

        public void ShowPage(PrintingPress press, int leftPageIndex) {
            print("showing page " + leftPageIndex);

            RulesOfSoccerCanvas rosc = press.rulesOfSoccerCanvas;

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
                print("title!");
                rosc.titleSpread.gameObject.SetActive(true);
                rosc.leftTopDetails.gameObject.SetActive(false);
                rosc.rightTopDetails.gameObject.SetActive(false);
                rosc.leftBottomDetails.gameObject.SetActive(false);
                rosc.rightBottomDetails.gameObject.SetActive(false);
            }
            else if (leftPageIndex == INDEX_CONTENTS) {
                print("contents!");
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

                    BookPage leftPage = document.Pages[leftPageIndex];
                    BookPage rightPage = document.Pages[leftPageIndex + 1];

                    if (atLawTitle) {
                        print("law title!");
                        rosc.lawSpread.gameObject.SetActive(true);
                        rosc.lawSpreadNum.text = "Law " + law.Item1.ToRomanUpper();
                        rosc.lawSpreadTitle.text = law.Item2;
                        rosc.lawPageNum.text = "LAW " + law.Item1.ToRomanUpper();
                        rosc.lawPageTitle.text = law.Item2;
                        rosc.lawPageText.text = "This is the wording of this law";

                        rosc.leftTopDetails.gameObject.SetActive(false);
                        rosc.rightTopDetails.gameObject.SetActive(true);
                        rosc.leftBottomDetails.gameObject.SetActive(false);
                        rosc.rightBottomDetails.gameObject.SetActive(true);
                    }
                    else {
                        print("normal!");
                        rosc.normalSpread.gameObject.SetActive(true);
                        rosc.leftTopLawNum.text = "LAW " + law.Item1.ToRomanUpper();
                        rosc.rightTopLawTitle.text = law.Item2;
                        rosc.leftTopDetails.gameObject.SetActive(true);
                        rosc.rightTopDetails.gameObject.SetActive(true);
                        rosc.leftBottomDetails.gameObject.SetActive(true);
                        rosc.rightBottomDetails.gameObject.SetActive(true);

                        string col0 = "";
                        string col1 = "";
                        string currentCol;
                        for (int i = 0; i < leftPage.Blocks.Count; i++) {
                            BookBlock block = leftPage.Blocks[i];
                            if (block.Column == 0) currentCol = col0;
                            else currentCol = col1;
                            for (int j = 0; j < block.Elements.Count; j++) {
                                BookElement element = block.Elements[j];
                                if (element.Type == BookElementType.Space) {
                                    for (int k = 0; k < element.SpaceLines; k++) {
                                        currentCol += "\n";
                                    }
                                }
                                else if (element.Type == BookElementType.Paragraph) {
                                    if (block.Indent == 1) currentCol += INDENT_SPACES;
                                    currentCol += $"{element.Text}\n";
                                }
                                else Debug.LogError("what book type is this? " + element.Type);
                            }
                        }

                        rosc.leftPageCol0.text = col0;
                        rosc.leftPageCol1.text = col1;

                        for (int i = 0; i < rightPage.Blocks.Count; i++) {
                            BookBlock block = rightPage.Blocks[i];
                            if (block.Column == 0) currentCol = col0;
                            else currentCol = col1;
                            for (int j = 0; j < block.Elements.Count; j++) {
                                BookElement element = block.Elements[j];
                                if (element.Type == BookElementType.Space) {
                                    for (int k = 0; k < element.SpaceLines; k++) {
                                        currentCol += "\n";
                                    }
                                }
                                else if (element.Type == BookElementType.Paragraph) {
                                    if (block.Indent == 1) currentCol += INDENT_SPACES;
                                    currentCol += $"{element.Text}\n";
                                }
                                else Debug.LogError("what book type is this? " + element.Type);
                            }
                        }

                        rosc.rightPageCol0.text = col0;
                        rosc.rightPageCol1.text = col1;
                    }
                }
            }
        }
    }
}
