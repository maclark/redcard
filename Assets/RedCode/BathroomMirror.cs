using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;


namespace RedCard {

    public enum Graphic {
        None,
        Dragon,
        Heart,
        Rose,
        Skull,

        Count,
    }

    public enum Category {
        None,
        Skin,
        Hair,
        Muscle,
        Nails,
        Ink,

        Count,
    }

    public class Tattoo {
        public Graphic graphic;
        public float r;
        public float theta;
        public float phi;
        public Color inkColor0;
        public Color inkColor1;
    }

    public enum MirrorMode {
        Inactive,
        Approaching,
        GazingAtArm,
        Manicure,
        SelectingTattoo,
        PlacingTattoo,
        SelectingJewelry,
    }

    public class BathroomMirror : MonoBehaviour {

        [Header("ASSIGNATIONS")]
        public GameObject customizationCanvasPrefab;

        [Header("SETTINGS")]
        public float approachSpeed = 1f;
        public float inFrontOfMirrorOffset = 1f;
        public float arm_fov = 50f;
        public float mirrorCanvasFadeDuration = .25f;
        public AnimationCurve approachCurve;

        [Header("VARS")]
        public CustomizationCanvas customCan;
        public MirrorMode mode = MirrorMode.Inactive;
        public Arm currentArm;
        public Arm otherArm;


        public const int MAX_TATTOOS = 3;
        public const string MIRROR_ACTION_MAP = "GazingInMirror";

        Vector3 approachStartPos;
        Quaternion approachStartGaze;
        float t;
        RefControls arbitro;
        bool equippedNailPolishBrush = false;
        bool usingSponge = false;
        int nailColorIndex;

        public void Awake() {

            enabled = false;
            mode = MirrorMode.Inactive;

            customCan = Instantiate(customizationCanvasPrefab, transform).GetComponent<CustomizationCanvas>();

            customCan.switchArms.onClick.AddListener(SwitchArms);
            customCan.dominanceCheckbox.onClick.AddListener(ToggleDominance);
            customCan.back.onClick.AddListener(Back);

            customCan.hairThicknessSlider.onValueChanged.AddListener(HairThicknessSlid);
            customCan.hairLengthSlider.onValueChanged.AddListener(HairLengthSlid);
            customCan.hairCurlSlider.onValueChanged.AddListener(HairCurlSlid);

            customCan.muscleSlider.onValueChanged.AddListener(MuscleSlid);

            customCan.nailLengthSlider.onValueChanged.AddListener(NailLengthSlid);
            customCan.nailPolishJar.jarButton.onClick.AddListener(ClickedOnNailPolishJar);
            customCan.gameObject.SetActive(false);
            customCan.nailPolishBrush.gameObject.SetActive(false);
            customCan.nailPolishRemoverSponge.gameObject.SetActive(false);
            customCan.nailPolishRemoverMiniSponge.gameObject.SetActive(false);
            customCan.nailColorSelectedIndex = 1;
            customCan.nailPolishJar.liquid.color = customCan.nailColors[customCan.nailColorSelectedIndex];
            Debug.Assert(customCan.colorBoxPrefab);
            Debug.Assert(customCan.colorRowPrefab);

            customCan.pickTattoo.onClick.AddListener(PickTattoo);
            
            for (int i = 0; i < customCan.nails.Length; i++) {
                int fingerIndex = i;
                void PaintThisNail() {
                    PaintNail(fingerIndex);
                }
                customCan.nails[i].onClick.AddListener(PaintThisNail);
            }

            string map = MIRROR_ACTION_MAP;
            var action = PlayerInput.all[0].actions.FindActionMap(map).FindAction("PrimaryAction");
            if (action != null) {
                action.started += PrimaryAction;
                action.canceled += PrimaryAction;
            }
            else Debug.LogWarning("couldn't find PrimaryAction action");

        }

        void HairThicknessSlid(float value) {
            //print("new thickness " + value);
            // from what to what??
            arbitro.leftArm.data.hairThickness = value;
            arbitro.rightArm.data.hairThickness = value;
            arbitro.leftArm.UpdateHairDensity();
            arbitro.rightArm.UpdateHairDensity();
            if (mode != MirrorMode.Approaching) customCan.PlaySliderSound();
            ArmData.SaveArms(arbitro.leftArm.data, arbitro.rightArm.data);
        }
        void HairLengthSlid(float value) {
            //print("new hair length " + value);
            arbitro.leftArm.data.hairLength = value;
            arbitro.rightArm.data.hairLength = value;
            arbitro.leftArm.UpdateHairLength();
            arbitro.rightArm.UpdateHairLength();
            if (mode != MirrorMode.Approaching) customCan.PlaySliderSound();
            ArmData.SaveArms(arbitro.leftArm.data, arbitro.rightArm.data);
        } 

        void HairCurlSlid(float value) {
            //print("new curl: " + value);
            arbitro.leftArm.data.hairCurl = value;
            arbitro.rightArm.data.hairCurl = -value;
            arbitro.leftArm.UpdateHairDensity();
            arbitro.rightArm.UpdateHairDensity();
            if (mode != MirrorMode.Approaching) customCan.PlaySliderSound();
            ArmData.SaveArms(arbitro.leftArm.data, arbitro.rightArm.data);
        } 

        // puny, normal, bulging
        void MuscleSlid(float value) {
            //print("muscle slid " + value);
            int size = Mathf.RoundToInt(value);
            if (size == 0) {
                customCan.respectAndStamina.gameObject.SetActive(true);
                // puny, so respect (which is on top) should have the MINUS icon
                if (customCan.minusIcon.anchoredPosition.y > customCan.plusIcon.anchoredPosition.y) {
                    // we're good
                }
                else {
                    Vector2 cache = customCan.minusIcon.anchoredPosition;
                    customCan.minusIcon.anchoredPosition = customCan.plusIcon.anchoredPosition;
                    customCan.plusIcon.anchoredPosition = cache;
                }
            }
            else if (size == 1) {
                customCan.respectAndStamina.gameObject.SetActive(false);
            }
            else {
                customCan.respectAndStamina.gameObject.SetActive(true);
                // bulging, so respect (which is on top) should have the PLUS icon
                if (customCan.minusIcon.anchoredPosition.y > customCan.plusIcon.anchoredPosition.y) {
                    Vector2 cache = customCan.minusIcon.anchoredPosition;
                    customCan.minusIcon.anchoredPosition = customCan.plusIcon.anchoredPosition;
                    customCan.plusIcon.anchoredPosition = cache;
                }
                else {
                    // we're good
                }
            }

            // we only do this if necessay, so we don't shuffle the hair when approaching
            if (arbitro.leftArm.data.muscleSize != size) {
                arbitro.leftArm.data.muscleSize = size;
                arbitro.rightArm.data.muscleSize = size;
                arbitro.leftArm.UpdateMuscle();
                arbitro.rightArm.UpdateMuscle();
                arbitro.leftArm.UpdateHairDensity();
                arbitro.rightArm.UpdateHairDensity();
            }

            if (mode != MirrorMode.Approaching) customCan.PlaySliderSound();
            ArmData.SaveArms(arbitro.leftArm.data, arbitro.rightArm.data);
        } 

        void NailLengthSlid(float value) {
            // from 375 to 900
            // initialize at 425
            // for pinky... subtract 40
            // width is not touched (use self reference)
            currentArm.data.nailLength = value;
            currentArm.UpdateNails();


            for (int i = 1; i < customCan.nails.Length; i++) {
                if (customCan.nails[i].TryGetComponent(out RectTransform rt)) {
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, value);
                    customCan.nailLines[i].rectTransform.sizeDelta = new Vector2(rt.sizeDelta.x, value);
                }
            }
            if (customCan.nails[0].TryGetComponent(out RectTransform rtPinky)) {
                // pinky is shorter
                rtPinky.sizeDelta = new Vector2(rtPinky.sizeDelta.x, value - 40f);
                customCan.nailLines[0].rectTransform.sizeDelta = new Vector2(rtPinky.sizeDelta.x, value - 40f);
            }

            if (mode != MirrorMode.Approaching) customCan.PlaySliderSound();
            ArmData.SaveArms(arbitro.leftArm.data, arbitro.rightArm.data);
        }

        void PaintNail(int nailIndex) {
            if (equippedNailPolishBrush) {
                Color c = customCan.nailPolishBrush.bristles.color;
                if (c.a < 1f) {
                    AudioManager.am.sfxAso.PlayOneShot(customCan.cleanedNail);
                }
                else {
                    AudioManager.am.sfxAso.PlayOneShot(customCan.paintedNail);
                }
                customCan.nails[nailIndex].image.color = customCan.nailPolishBrush.bristles.color;
                currentArm.data.nailColorIndices[nailIndex] = nailColorIndex;
                // #TODO actually pain the referee's 3d nails
            }
            ArmData.SaveArms(arbitro.leftArm.data, arbitro.rightArm.data);
        }

        void PickTattoo() {
            Debug.LogWarning("pick tatoooo");
            ArmData.SaveArms(arbitro.leftArm.data, arbitro.rightArm.data);
        }

        void CloseColorBox() {
            if (customCan.colorBox) {
                customCan.nailPolishRemoverMiniSponge.SetParent(null);
                customCan.nailPolishRemoverMiniSponge.gameObject.SetActive(false);
                Vector2 ogSizeDelta = new Vector2(customCan.colorBox.rtParent.sizeDelta.x, customCan.colorBox.parentHeightCache);
                customCan.colorBox.rtParent.sizeDelta = ogSizeDelta;
                customCan.colorBox.rtParentShadow.sizeDelta = ogSizeDelta;
                Destroy(customCan.colorBox.gameObject); //#HACK
                customCan.colorBox = null;
            }
        }
        void ClickedOnNailPolishJar() {
            if (equippedNailPolishBrush) {

                equippedNailPolishBrush = false;

                customCan.nailPolishBrush.gameObject.SetActive(false);
                customCan.nailPolishRemoverSponge.gameObject.SetActive(false);
                customCan.nailPolishJar.openJar.gameObject.SetActive(false);
                customCan.nailPolishJar.closedJar.gameObject.SetActive(true);
                CloseColorBox();

                Cursor.visible = true;
                AudioManager.am.sfxAso.PlayOneShot(customCan.grabbedBrush);
            }
            else {
                equippedNailPolishBrush = true;

                customCan.nailPolishJar.closedJar.gameObject.SetActive(false);
                customCan.nailPolishJar.openJar.gameObject.SetActive(true);
                customCan.nailPolishBrush.gameObject.SetActive(!usingSponge);
                customCan.nailPolishRemoverSponge.gameObject.SetActive(usingSponge);

                if (customCan.colorBox) CloseColorBox();

                customCan.colorBox = ColorBox.MakeColorBox(customCan, customCan.nailsRect, customCan.nailsBackground, customCan.nailsBackgroundShadow, customCan.nailColors);
                RectTransform first = customCan.colorBox.rows[0].swatches[0].GetComponent<RectTransform>();
                customCan.nailPolishRemoverMiniSponge.SetParent(first.transform.parent);
                customCan.nailPolishRemoverMiniSponge.anchoredPosition = first.anchoredPosition;
                customCan.nailPolishRemoverMiniSponge.gameObject.SetActive(true);
                int count = customCan.nailColorSelectedIndex;
                for(int i = 0; i < customCan.colorBox.rows.Length; i++) {
                    int l = customCan.colorBox.rows[i].swatches.Length;
                    if (l >= count) {
                        print("nailColorSelectedIndex " + customCan.nailColorSelectedIndex);
                        print("count: " + count);
                        customCan.colorBox.SelectedSwatch(customCan.colorBox.rows[i].swatches[count], customCan.nailColorSelectedIndex);
                        break;
                    }
                    else count -= l;
                }
            }
        }

        public void SelectedColor(Category cat, int index) {

            CustomizationOptions cops = RedMatch.match.customizationOptions;

            switch (cat) {
                case Category.Skin:
                    arbitro.leftArm.SetSkinColor(index);
                    arbitro.rightArm.SetSkinColor(index);
                    for (int i = 0; i < customCan.fingers.Length; i++) {
                        customCan.fingers[i].color = RedMatch.match.customizationOptions.skinSwatchColors[index];
                    }
                    break;

                case Category.Hair:
                    arbitro.leftArm.SetHairColor(index);
                    arbitro.rightArm.SetHairColor(index);
                    break;

                case Category.Nails:
                    nailColorIndex = index;
                    Color c = RedMatch.match.customizationOptions.nailSwatchColors[index];
                    customCan.nailColorSelectedIndex = index;
                    customCan.nailPolishJar.liquid.color = c;
                    customCan.nailPolishBrush.bristles.color = c;

                    if (c.a < 1f) {
                        AudioManager.am.sfxAso.PlayOneShot(customCan.selectedSponge);
                        usingSponge = true;
                        customCan.nailPolishBrush.gameObject.SetActive(!usingSponge);
                        customCan.nailPolishRemoverSponge.gameObject.SetActive(usingSponge);
                    }
                    else {
                        AudioManager.am.sfxAso.PlayOneShot(customCan.selectedNailColor);
                        usingSponge = false;
                        customCan.nailPolishBrush.gameObject.SetActive(true);
                        customCan.nailPolishRemoverSponge.gameObject.SetActive(false);
                    }
                    break;
            }
        }

        void SwitchArms() {
            Arm cachedArm = currentArm;
            currentArm = otherArm;
            otherArm = cachedArm;
            
            if (currentArm.data.isDominant) {
                customCan.makeDominantText.text = Language.current[Words.IsDominantChecked];
            }
            else {
                customCan.makeDominantText.text = Language.current[Words.IsDominantUnchecked];
            }

            otherArm.gameObject.SetActive(false);
            currentArm.gameObject.SetActive(true);
            currentArm.transform.localPosition = currentArm.localLoweredPos;

            OrientFingers();

            arbitro.SlotEquipped((int)RefEquipment.Barehand);
        }

        void OrientFingers() {

            // default position was painting left hand with the right
            float pinkySign = (currentArm.side) == Chirality.Left ? -1f : 1f; 
            if (customCan.fingers.Length >= 5) {

                // pinky is first finger, i know, weird!
                if (customCan.fingers[0].transform.parent is RectTransform rtPinky) { 
                    rtPinky.localPosition = new Vector3(pinkySign *Mathf.Abs(rtPinky.localPosition.x), rtPinky.localPosition.y);
                } 

                // thumb
                if (customCan.fingers[4].transform.parent is RectTransform rtThumb) {
                    rtThumb.localPosition = new Vector3(-pinkySign * Mathf.Abs(rtThumb.localPosition.x), rtThumb.localPosition.y);
                    rtThumb.localRotation = Quaternion.Euler(0f, 0f, pinkySign * 12);
                } 

                // eh, i'm currently rotating the nailpolish brush every frame to match arm
                // it's fine
            }

        }

        void ToggleDominance() {
            if (currentArm.data.isDominant) {
                currentArm.data.isDominant = false;
                otherArm.data.isDominant = true;
                customCan.makeDominantText.text = Language.current[Words.IsDominantUnchecked];
                arbitro.dominantArm = otherArm;
            }
            else {

                currentArm.data.isDominant = true;
                otherArm.data.isDominant = false;
                customCan.makeDominantText.text = Language.current[Words.IsDominantChecked];
                arbitro.dominantArm = currentArm;
            }

            ArmData.SaveArms(arbitro.leftArm.data, arbitro.rightArm.data);
        }

        void Back() {
            switch (mode) {
                case MirrorMode.Approaching:
                case MirrorMode.GazingAtArm:
                case MirrorMode.Inactive:
                    LeaveMirror();
                    break;
                default:
                    print("mode " + mode);
                    throw new System.NotImplementedException();
            }
        }

        private void PrimaryAction(InputAction.CallbackContext ctx) {
            if (mode == MirrorMode.PlacingTattoo) {

            }
        }

        private void Update() {
            // mouse no longer over the NAILS category

            if (equippedNailPolishBrush) {

                Vector2 mousePosition = Vector2.zero;
                if (Mouse.current != null) mousePosition = Mouse.current.position.ReadValue();

                if (RectTransformUtility.RectangleContainsScreenPoint(customCan.nailsBackground, mousePosition, null)) {

                    Cursor.visible = false;

                    if (customCan.nailPolishBrush.TryGetComponent(out RectTransform follower)) {
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            follower.parent as RectTransform,
                            mousePosition,
                            null, // null for Screen Space - Overlay
                            out Vector2 pos
                        );
                        follower.anchoredPosition = pos;
                        customCan.nailPolishRemoverSponge.anchoredPosition = pos;
                        // adding slight tilt to angle of nail brush heh
                        if (currentArm.side == Chirality.Left) {
                            // painting with right hand
                            follower.localRotation = Quaternion.Euler(0f, 0f, 150f);
                        }
                        else {
                            // painting with left hand
                            follower.localRotation = Quaternion.Euler(0f, 0f, 210f);
                        }
                    }
                }
                else {

                    // mouse no longer over the NAILS category
                    Cursor.visible = true;
                }
            }

            switch (mode) {
                case MirrorMode.Approaching:
                    t += Time.deltaTime * approachSpeed;
                    if (t >= 1f) {
                        t = 1f;
                        mode = MirrorMode.GazingAtArm;
                    }
                    float s = approachCurve.Evaluate(t);
                    Vector3 inFrontOfMirror = transform.position + transform.forward * inFrontOfMirrorOffset;
                    Vector3 inFrontOfMirrorFloorLevel = new Vector3(inFrontOfMirror.x, arbitro.transform.position.y, inFrontOfMirror.z);
                    arbitro.transform.position = Vector3.Lerp(approachStartPos, inFrontOfMirrorFloorLevel, s);
                    Quaternion lookingIntoMirror = Quaternion.LookRotation(-transform.forward, Vector3.up);
                    arbitro.cam.transform.rotation = Quaternion.Slerp(approachStartGaze, lookingIntoMirror, s);
                    //arbitro.cam.fieldOfView = Mathf.Lerp(arbitro.normal_vert_fov, arm_fov, s);

                    // in ref controls, in update, we're doing this:
                    // cameraTransform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
                    // so we need pitch and yaw to match as though they're looking straight
                    arbitro.pitch = arbitro.cam.transform.localEulerAngles.x;
                    arbitro.yaw = arbitro.cam.transform.localEulerAngles.y;
                    arbitro.lastLook = Vector2.zero;


                    float tStartFadingCanvasIn = 1f - mirrorCanvasFadeDuration;
                    if (t > tStartFadingCanvasIn) {
                        float tGUI = t - tStartFadingCanvasIn;
                        customCan.group.alpha = Mathf.Lerp(0f, 1f, tGUI / mirrorCanvasFadeDuration);
                    }
                    break;

                case MirrorMode.Inactive:
                    if (customCan.group.alpha > 0f) {
                        float fadeOutSpeed = 1f / (1f - mirrorCanvasFadeDuration);
                        customCan.group.alpha -= fadeOutSpeed * Time.deltaTime;
                    }
                    else {
                        customCan.gameObject.SetActive(false);
                        enabled = false;
                    }
                    break;
            }
        }


        public void ApproachMirror(RefControls approacher) {

            mode = MirrorMode.Approaching;

            customCan.InitSkinAndHairColorButtons(this);
            
            ReadOnlyArray<PlayerInput> allInput = PlayerInput.all;
            foreach (PlayerInput input in allInput) {
                InputActionMap map = input.actions.FindActionMap(MIRROR_ACTION_MAP);
                if (map != null) input.SwitchCurrentActionMap(MIRROR_ACTION_MAP);
                else Debug.LogError("can't find map: " + MIRROR_ACTION_MAP);
            }

            arbitro = approacher;
            arbitro.hud.gameObject.SetActive(false);
            if (arbitro.TryGetComponent(out CharacterController cc)) cc.enabled = false;
            arbitro.lookVelocity = Vector2.zero;
            arbitro.yaw = 0f;
            arbitro.pitch = 0f;
            enabled = true;
            t = 0f;

            approachStartPos = arbitro.transform.position;
            approachStartGaze = arbitro.cam.transform.rotation;

            customCan.gameObject.SetActive(true);
            customCan.swatchHoverHighlight.gameObject.SetActive(false);
            customCan.group.alpha = 0f;
            customCan.makeDominantText.text = Language.current[Words.IsDominantChecked];

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;

            if (arbitro.dominantArm == arbitro.leftArm) {
                currentArm = arbitro.leftArm;
                otherArm = arbitro.rightArm;
            }
            else {
                currentArm = arbitro.rightArm;
                otherArm = arbitro.leftArm;
            }

            // now let's match all the bells and whistles to our current arm!
            ////////////////////////// 
            ////////////////////////// 
            //////////////////////////
            //////////////////////////


            // #TODO
            // what about "is dominant [X]" ?
            // #TODO sounds, but don't play if in mode approaching

            // skin
            int skinIndex = arbitro.leftArm.data.skinColorIndex;
            if (skinIndex >= 0 && skinIndex < customCan.skinColorSwatches.Length) {
                customCan.SelectedSkinSwatch(customCan.skinColorSwatches[skinIndex], skinIndex);
            }
            else Debug.LogError("invalid initial skin index " + skinIndex);

            // skin
            customCan.hairThicknessSlider.SetValueWithoutNotify(currentArm.data.hairThickness);
            customCan.hairLengthSlider.SetValueWithoutNotify(currentArm.data.hairLength);
            customCan.hairCurlSlider.SetValueWithoutNotify(currentArm.data.hairCurl);
            int hairIndex = arbitro.leftArm.data.hairColorIndex;
            if (hairIndex >= 0 && hairIndex < customCan.hairColorSwatches.Length) {
                customCan.SelectedHairSwatch(customCan.hairColorSwatches[hairIndex], hairIndex, true);
            }
            else Debug.LogError("invalid initial hair index " + hairIndex);

            // muscle
            customCan.muscleSlider.SetValueWithoutNotify(currentArm.data.muscleSize);
            MuscleSlid(currentArm.data.muscleSize);

            // nails
            // have to set the nail visuals
            customCan.nailLengthSlider.SetValueWithoutNotify(currentArm.data.nailLength);
            OrientFingers();
            NailLengthSlid(currentArm.data.nailLength);
            CustomizationOptions cops = RedMatch.match.customizationOptions;
            Color skinColor = cops.skinSwatchColors[currentArm.data.skinColorIndex];
            for (int i = 0; i < customCan.fingers.Length; i++) {
                customCan.fingers[i].color = skinColor;
                customCan.nails[i].image.color = cops.nailSwatchColors[currentArm.data.nailColorIndices[i]];
            }

            // tattoos
            // tattoos
            // tattoos
            // tattoos


        }

        public void LeaveMirror() {
            if (!arbitro) {
                Debug.LogError("arbitro not at mirror");
                return; //////earlyreturn///
            }

            if (!currentArm.data.isDominant) SwitchArms();

            ReadOnlyArray<PlayerInput> allInput = PlayerInput.all;
            foreach (PlayerInput input in allInput) {
                InputActionMap map = input.actions.FindActionMap(RedMatch.REFEREEING_ACTION_MAP);
                if (map != null) input.SwitchCurrentActionMap(RedMatch.REFEREEING_ACTION_MAP);
                else Debug.LogError("can't find map: " + RedMatch.REFEREEING_ACTION_MAP);
            }

            arbitro.hud.gameObject.SetActive(true);
            arbitro.cam.fieldOfView = arbitro.normal_vert_fov;
            if (arbitro.TryGetComponent(out CharacterController cc)) cc.enabled = true;
            arbitro = null;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            mode = MirrorMode.Inactive;
        }

        private void OnDestroy() {
            string map = MIRROR_ACTION_MAP;
            if (PlayerInput.all.Count > 0) { 
            var action = PlayerInput.all[0].actions.FindActionMap(map).FindAction("PrimaryAction");
                if (action != null) {
                    action.started -= PrimaryAction;
                    action.canceled -= PrimaryAction;
                }
            }
        }

    }
}