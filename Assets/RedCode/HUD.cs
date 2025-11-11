using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RedCard {
    public class HUD : MonoBehaviour {
        public TMP_Text debugSpeed;
        public TMP_Text debugClock;
        public GameObject thumbsUpPrefab;
        public GameObject thumbsDownPrefab;
        public Image grabHand;
        public TMP_Text fToDrop;
        public DialogWheel wheel;
        public Texture2D cursor;


        [Header("BIOMETRICS")]
        public TMP_Text heartRate;
        public Slider heartRateSlider;
        public TMP_Text lactate;
        public Slider lactateSlider;
        public TMP_Text glycogen;
        public Slider glycogenSlider;

        [Header("EQUIPMENT")]
        [SerializeField] private Image barehandIcon;
        [SerializeField] private Image whistleIcon;
        [SerializeField] private Image watchIcon;
        [SerializeField] private Image bookIcon;
        [SerializeField] private Image sprayCanIcon;
        [SerializeField] private Image yellowCardIcon;
        [SerializeField] private Image redCardIcon;
        public Image coinIcon;
        [SerializeField] private RectTransform utilityBar;
        [SerializeField] private TMP_Text slotNum1;
        [SerializeField] private TMP_Text slotNum2;
        [SerializeField] private TMP_Text slotNum3;
        [SerializeField] private TMP_Text slotNum4;
        [SerializeField] private CanvasGroup equipmentGroup;
        
        public RefEquipment selected;
        public Image highlighted = null;
        public TMP_Text highlightedNum = null;

        [SerializeField] private float HOLD_FULL_ALPHA = 2f;
        [SerializeField] private float FADE_OUT_TIME = 1f;


        public static string[] double_digit_strings;

        private Image[] icons;
        private TMP_Text[] slotsNums;
        private float alpha = 1f;


        public static readonly string F_To_Grab = "'F' grab";
        public static readonly string F_To_Drop = "'F' drop";
        public static readonly string F_To_Interact = "'F' interact";
        private static readonly Color semi_transparent = new Color(0f, 0f, 0f, .333f);

        private void Awake() {
            Debug.Assert(debugClock);
            Debug.Assert(thumbsUpPrefab);
            Debug.Assert(thumbsDownPrefab);

            wheel.gameObject.SetActive(false);
            grabHand.gameObject.SetActive(false);
            fToDrop.gameObject.SetActive(false);
            
            icons = new Image[] {
                barehandIcon,
                whistleIcon,
                watchIcon,
                bookIcon,
                sprayCanIcon,
                yellowCardIcon,
                redCardIcon,
                coinIcon,
            };

            foreach (var ic in icons) {
                ic.color = semi_transparent;
                ic.gameObject.SetActive(false);
            }
            slotNum4.gameObject.SetActive(false);

            slotsNums = new TMP_Text[] {
                slotNum1,
                slotNum2,
                slotNum3,
                slotNum4,
            };

            foreach (var n in slotsNums) n.color = Color.black;

            double_digit_strings = new string[100];
            for (int i = 0; i < 100; ++i) {
                if (i < 10) double_digit_strings[i] = "0" + i.ToString();
                else double_digit_strings[i] = i.ToString();
            }
        }

        private void Update() {
            alpha -= Time.deltaTime;
            //equipmentGroup.alpha = Mathf.Lerp(0f, 1f, alpha / FADE_OUT_TIME);
            if (debugClock) debugClock.text = RedSim.matchMinutes.ToString();
        }

        public void MakeVisible(RefEquipment equip) {
            switch (equip) {
                case RefEquipment.Whistle:
                    whistleIcon.gameObject.SetActive(true);
                    break;
                case RefEquipment.Watch:
                    watchIcon.gameObject.SetActive(true);
                    break;
                case RefEquipment.Barehand:
                    barehandIcon.gameObject.SetActive(true);
                    break;
                case RefEquipment.SprayCan:
                    slotNum4.gameObject.SetActive(true);
                    sprayCanIcon.gameObject.SetActive(true);
                    break;
                case RefEquipment.Book:
                    slotNum4.gameObject.SetActive(true);
                    bookIcon.gameObject.SetActive(true);
                    break;
                case RefEquipment.YellowCard:
                    slotNum4.gameObject.SetActive(true);
                    yellowCardIcon.gameObject.SetActive(true);
                    break;
                case RefEquipment.RedCard:
                    slotNum4.gameObject.SetActive(true);
                    redCardIcon.gameObject.SetActive(true);
                    break;
                case RefEquipment.Coin:
                    slotNum4.gameObject.SetActive(true);
                    coinIcon.gameObject.SetActive(true);
                    break;

                default:
                    Debug.LogWarning("unhandled ref equipment " + equip);
                    break;
            }
        }

        public static void SelectIcon(HUD hud, RefEquipment newSelected) {

            hud.alpha = hud.HOLD_FULL_ALPHA + hud.FADE_OUT_TIME;
            hud.equipmentGroup.alpha = 1f;
            if (hud.selected == newSelected) return;

            bool validSelection = true;
            bool utilitySelected = false;
            Image newHighlighted = null;
            TMP_Text newHighlightedNum = hud.slotNum4;
            Color highlightedColor = Color.black;
            switch(newSelected) {
                case RefEquipment.Whistle:
                    newHighlighted = hud.whistleIcon;
                    newHighlightedNum = hud.slotNum1;
                    break;
                case RefEquipment.Barehand:
                    newHighlighted = hud.barehandIcon;
                    newHighlightedNum = hud.slotNum2;
                    break;
                case RefEquipment.Watch:
                    newHighlighted = hud.watchIcon;
                    newHighlightedNum = hud.slotNum3;
                    break;


                // utilities
                case RefEquipment.Book:
                    utilitySelected = true;
                    newHighlighted = hud.bookIcon;
                    break;
                case RefEquipment.SprayCan:
                    utilitySelected = true;
                    newHighlighted = hud.sprayCanIcon;
                    break;
                case RefEquipment.YellowCard:
                    utilitySelected = true;
                    newHighlighted = hud.yellowCardIcon;
                    highlightedColor = Color.white;
                    break;
                case RefEquipment.RedCard:
                    utilitySelected = true;
                    newHighlighted = hud.redCardIcon;
                    highlightedColor = Color.white;
                    break;
                case RefEquipment.Coin:
                    utilitySelected = true;
                    newHighlighted = hud.coinIcon;
                    break;

                default:
                    validSelection = false;
                    break;
            }


            if (validSelection) {
                hud.selected = newSelected;
                hud.highlighted = newHighlighted;
                hud.highlightedNum = newHighlightedNum;
                foreach (Image i in hud.icons) i.color = semi_transparent;
                //foreach (TMP_Text t in hud.slotsNums) t.color = new Color(0f, 0f, 0f, .5f);
                hud.highlighted.color = highlightedColor;
                hud.highlightedNum.color = Color.black;

                if (utilitySelected) hud.utilityBar.localScale = Vector3.one;
                else hud.utilityBar.localScale = Vector3.one * .5f;
            }
            else {
                Debug.LogWarning("unhandled hud icon selected: " + newSelected);
            }
        }


        public void ShowCorrectCall() {
            GameObject thumbsUp = Instantiate(thumbsUpPrefab, transform);
            thumbsUp.transform.localPosition = Vector3.zero;
            Destroy(thumbsUp, 1f);
        }

        public void ShowBadCall() {
            GameObject thumbsDown = Instantiate(thumbsDownPrefab, transform);
            thumbsDown.transform.localPosition = Vector3.zero;
            Destroy(thumbsDown, 1f);
        }
    }
}
