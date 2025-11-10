using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


namespace RedCard {

    public enum Semantics {
        None,
        DotDotDot,
        ThisIsHeads,
        ThisIsTails,
        CoinLandedHeads,
        CoinLandedTails,
        CoinFlipWinnerQuestion,
        CoinFlipLoserQuestion,
        Advantage,
        PlayOn,
        CarefulPlayer,

        Count,
    }

    [System.Serializable]
    public class DialogData {
        public Semantics semantics;
        public string line;
        public float backgroundWidth = 10f;
        public float r = 550f;
        public float angle = 0f;
        public float halfSelectionWidth = 15f;
        public float endAngle;

        public DialogData() {

        }

        public DialogData(Semantics s, string line, float backgroundWidth, float r, float angle, float endAngle) {
            this.semantics = s;
            this.line = line;
            this.backgroundWidth = backgroundWidth;
            this.r = r;
            this.angle = angle;
            this.endAngle = endAngle;
        }
    }


    public class DialogWheel : MonoBehaviour {

        [Header("ASSIGNATIONS")]
        public UnityEngine.UI.Image backgroundCircle; 

        [Header("SETTINGS")]
        public float openSpeed = 3f;
        public float highlightPulseSpeed = 5f;
        public float highlightAmplitude = .2f;
        public float minimumHighlightRangePx = 100f;
        public float minimumHighlightRangeArrows = .1f;

        [Header("VARS")]
        public bool preppedDialog = false;
        public bool on = false;
        public DialogBox highlighted;
        public DialogBox[] allOps = new DialogBox[0];
        public RefTarget spokeTo;

        [Header("DIALOG GROUPS")]
        public DialogData[] nothingToSay = new DialogData[0];
        public DialogData[] coinFlipExplanation = new DialogData[0];
        public DialogData[] coinFlipResults = new DialogData[0];
        public DialogData[] coinFlipWinnerQuestion = new DialogData[0];
        public DialogData[] coinFlipLoserQuestion = new DialogData[0];
        public DialogData[] duringPlay = new DialogData[0];

        public static readonly Color default_background = new Color(0f, 0f, 0f, .5f);

        float t;

        public void Init() {
            t = 0f;
            backgroundCircle.enabled = true; // reference all assignations in inits
            StartClosing();
            UseEnglish();
            PopulateBoxes(nothingToSay);
        }

        public void UseEnglish() { 
            nothingToSay = new DialogData[] {
                new DialogData(Semantics.DotDotDot, "...", 600f, 250f, 90f, 360f),
            };
            coinFlipExplanation = new DialogData[] { 
                new DialogData(Semantics.ThisIsHeads, "This is \"Heads\"", 500f, 500f, 0f, 90f),
                new DialogData(Semantics.ThisIsTails, "This is \"Tails\"", 500f, 500f, 180f, 270f),
            };
            coinFlipResults = new DialogData[] { 
                new DialogData(Semantics.CoinLandedHeads, "It's \"Heads\"", 500f, 500f, 0f, 90f),
                new DialogData(Semantics.CoinLandedTails, "It's \"Tails\"", 500f, 500f, 180f, 270f),
            };
            coinFlipWinnerQuestion = new DialogData[] {
                new DialogData(Semantics.CoinFlipWinnerQuestion, "Kickoff or switch sides?", 600f, 250f, 90f, 360f),
            };
            coinFlipLoserQuestion = new DialogData[] {
                new DialogData(Semantics.CoinFlipLoserQuestion, "Switch sides?", 400f, 250f, 90f, 360f),
            };
            duringPlay = new DialogData[] { 
                new DialogData(Semantics.Advantage, "Advantage!", 500f, 250f, 90f, 180f),
                new DialogData(Semantics.PlayOn, "Nothing in it!", 500f, 400f, 210f, 270f),
                new DialogData(Semantics.CarefulPlayer, "Watch yourself, player.", 700f, 400f, 330f, 360f),
            };
            PopulateBoxes(coinFlipExplanation);
        }


        public void Open() {
            if (forceReopen) {
                Debug.LogWarning("how is Open possible while repopulating? you'd have to close, which would cancel reopening...");
                return;
            }

            on = true;
            enabled = true;
            gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            RedMatch.Match.arbitro.crossHairs.gameObject.SetActive(false);
        }

        public void StartClosing() {

            if (forceReopen) forceReopen = false;

            on = false;
            enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (highlighted) {
                highlighted.background.color = Color.green;
                Debug.Log("referee said " + highlighted.datum.semantics);
                RedMatch.Match.arbitro.NormalWhistlePosition();
                RedMatch.OnRefSpoke?.Invoke(highlighted.datum.semantics, null);
            }
        }

        public void PrimaryAction(InputAction.CallbackContext ctx, RefTarget target) {
            if (ctx.started) {
                if (preppedDialog) {
                    // fire it at target!
                    StartClosing();
                    spokeTo = target;
                    RedMatch.OnRefSpoke?.Invoke(highlighted.datum.semantics, target);
                }
                else if (highlighted) {
                    preppedDialog = true;
                    foreach (var db in activeBoxes) db.gameObject.SetActive(false);
                    highlighted.gameObject.SetActive(true);
                    highlighted.transform.localPosition = Vector3.zero;
                    highlighted.background.color = new Color(0f, 1f, 0f, .5f);
                    backgroundCircle.enabled = false;
                }
            }
        }

        public void SecondaryAction(InputAction.CallbackContext ctx) { 
            if (ctx.started) {
                if (preppedDialog) {
                    preppedDialog = false;
                    ClearHighlighted();
                }

                StartClosing();
            }
        }

        private void Update() {

            if (on) {
                t += Time.deltaTime * openSpeed;
                if (t >= 1f) {
                    t = 1f;
                    enabled = false; 
                }

                if (highlighted) {
                    highlighted.transform.localScale =  Vector3.one * (1f + highlightAmplitude * Mathf.Cos(highlightPulseSpeed * Time.time));
                }

            }
            else {
                t -= Time.deltaTime * openSpeed;
                if (t < 0f) {
                    ClearHighlighted();
                    if (repopulateData != null) PopulateBoxes(repopulateData);
                    repopulateData = null;

                    if (forceReopen) {
                        print("now forcing reopen");
                        forceReopen = false;
                        Open();
                    }
                    else {
                        // Close
                        t = 0f;
                        enabled = false;
                        gameObject.SetActive(false);
                        backgroundCircle.enabled = true;
                        RedMatch.Match.arbitro.crossHairs.gameObject.SetActive(true);
                        if (preppedDialog) {
                            preppedDialog = false;
                            if (currentData != null) PopulateBoxes(currentData);
                        }
                    }
                }
            }
            transform.localScale = Vector3.one * t;
        }


        List<DialogBox> activeBoxes = new List<DialogBox>();
        bool forceReopen = false;
        DialogData[] repopulateData = null;
        DialogData[] currentData = null;
        public void PopulateBoxes(params DialogData[] data) {
            if (on && !forceReopen) {
                print("forceReopen");
                repopulateData = data;
                StartClosing();
                forceReopen = true;
            }
            else {
                currentData = data;
                activeBoxes.Clear();
                for (int i = 0; i < allOps.Length; i++) {
                    if (i < data.Length) {
                        DialogData datum = data[i];
                        DialogBox box = allOps[i];
                        box.gameObject.SetActive(true);
                        float theta = datum.angle * Mathf.Deg2Rad;
                        box.transform.localPosition = datum.r * new Vector3(Mathf.Cos(theta), Mathf.Sin(theta));
                        box.text.text = datum.line;
                        box.text.color = Color.white;
                        box.background.color = default_background;
                        box.background.rectTransform.sizeDelta = new Vector2(datum.backgroundWidth, 100f);
                        box.datum = datum;
                        activeBoxes.Add(box);
                    }
                    else allOps[i].gameObject.SetActive(false);
                }
            }
        }

        public void ClearHighlighted() {
            if (highlighted) {
                highlighted.transform.localScale = Vector3.one;
                highlighted.text.color = Color.white;
                highlighted.background.color = default_background;
                highlighted = null;
            }
        }

        public void HighlightOption(Vector2 toMouse, bool usingArrows) {

            if (usingArrows) {
                if (toMouse.magnitude < minimumHighlightRangeArrows) {
                    ClearHighlighted();
                    return; ///////////////////earlyreturn////////////////
                }
            }
            else if (toMouse.magnitude < minimumHighlightRangePx) {
                ClearHighlighted();
                return; //////////////////early return/////////////////////
            }

            float theta = Mathf.Atan2(toMouse.y, toMouse.x);
            if (activeBoxes.Count == 0) {
                ClearHighlighted();
                Debug.LogWarning("highlighting with no options!?");
                return; /////////////earlyreturn//////////////////////////
            }


            float angle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg;
            while (angle < 0f) angle += 360f;
            DialogBox box = null;
            for (int i = 0; i < activeBoxes.Count; i++) {
                DialogData datum = activeBoxes[i].datum;
                if (angle < datum.endAngle) { 
                    box = activeBoxes[i];
                    break;
                }
            }

            if (!box) {
                // can we be sure here?
                box = activeBoxes[0];
            }

            ClearHighlighted();
            highlighted = box;
            highlighted.text.color = Color.black;
            highlighted.background.color = Color.white;
        }
    }
}
