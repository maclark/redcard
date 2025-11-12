using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

// #TODO rings, bracelets?
// amputation
// scars

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
        public GameObject mirrorCanvasPrefab;

        [Header("SETTINGS")]
        public float approachSpeed = 1f;
        public float inFrontOfMirrorOffset = 1f;
        public float arm_fov = 50f;
        public float mirrorCanvasFadeDuration = .25f;
        public AnimationCurve approachCurve;

        [Header("VARS")]
        public MirrorCanvas mirror;
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
        Category colorBoxCat;

        void SaveArms() {
            // #TODO
            Debug.LogWarning("save arms not implemented");
        }

        public void Awake() {

            enabled = false;
            mode = MirrorMode.Inactive;

            mirror = Instantiate(mirrorCanvasPrefab, transform).GetComponent<MirrorCanvas>();

            mirror.switchArms.onClick.AddListener(SwitchArms);
            mirror.dominanceCheckbox.onClick.AddListener(ToggleDominance);
            mirror.back.onClick.AddListener(Back);

            //mirror.skinColorSwatches

            mirror.hairThicknessSlider.onValueChanged.AddListener(HairThicknessSlid);
            mirror.hairLengthSlider.onValueChanged.AddListener(HairLengthSlid);
            //mirror.hairColorPicker.onValueChanged.AddListener(HairColorPicker);
            mirror.hairCurlSlider.onValueChanged.AddListener(HairCurlSlid);

            mirror.muscleSlider.onValueChanged.AddListener(MuscleSlid);

            mirror.nailLengthSlider.onValueChanged.AddListener(NailLengthSlid);
            mirror.nailPolishJar.liquid.onClick.AddListener(ClickedOnNailPolishJar);
            mirror.nailPolishJar.brushHandle.onClick.AddListener(ClickedOnNailPolishBrushInJar);
            mirror.gameObject.SetActive(false);
            mirror.nailPolishBrush.gameObject.SetActive(false);
            Debug.Assert(mirror.colorBoxPrefab);
            Debug.Assert(mirror.colorRowPrefab);

            mirror.pickTattoo.onClick.AddListener(PickTattoo);
            
            for (int i = 0; i < mirror.nails.Length; i++) {
                int fingerIndex = i;
                void PaintThisNail() {
                    PaintNail(fingerIndex);
                }
                mirror.nails[i].onClick.AddListener(PaintThisNail);
            }

            string map = MIRROR_ACTION_MAP;
            var action = PlayerInput.all[0].actions.FindActionMap(map).FindAction("MoveWASD");
            if (action != null) {
                action.started += MoveInput;
            }
            else Debug.LogWarning("couldn't find MoveWASD action");

            action = PlayerInput.all[0].actions.FindActionMap(map).FindAction("Look");
            if (action != null) action.performed += LookInput;
            else Debug.LogWarning("couldn't find Look action");

            action = PlayerInput.all[0].actions.FindActionMap(map).FindAction("PrimaryAction");
            if (action != null) {
                action.started += PrimaryAction;
                action.canceled += PrimaryAction;
            }
            else Debug.LogWarning("couldn't find PrimaryAction action");

            action = PlayerInput.all[0].actions.FindActionMap(map).FindAction("SecondaryAction");
        }

        void HairThicknessSlid(float value) {
            arbitro.leftArm.hairDensity = value;
            arbitro.rightArm.hairDensity = value;
            arbitro.leftArm.UpdateHairDensity();
            arbitro.rightArm.UpdateHairDensity();
            SaveArms();
        }
        void HairLengthSlid(float value) {
            arbitro.leftArm.hairLength = value * .00667f;
            arbitro.rightArm.hairLength = value * .00667f;
            arbitro.leftArm.UpdateHairLength();
            arbitro.rightArm.UpdateHairLength();
            SaveArms();
        } 
        void HairColorPicker(float value) {
            Color c = Random.ColorHSV();
            arbitro.leftArm.SetHairColor(c);
            arbitro.rightArm.SetHairColor(c);
            SaveArms();
        }
        void HairCurlSlid(float value) {
            arbitro.leftArm.hairCurlDegrees = value;
            arbitro.rightArm.hairCurlDegrees = -value;
            arbitro.leftArm.UpdateHairDensity();
            arbitro.rightArm.UpdateHairDensity();
            SaveArms();
        } 
        void MuscleSlid(float value) {
            // default arm size right now is .25f
            // default slider value is 1 (goes from 0 to 2)
            float armDiameter = .15f + .1f * value;
            arbitro.leftArm.radius = .5f * armDiameter;
            arbitro.rightArm.radius = .5f * armDiameter;
            arbitro.leftArm.limb.localScale = new Vector3(armDiameter, 1f, armDiameter);
            arbitro.rightArm.limb.localScale = new Vector3(armDiameter, 1f, armDiameter);
            arbitro.leftArm.UpdateHairDensity();
            arbitro.rightArm.UpdateHairDensity();
            SaveArms();
        } 
        void StartManicure() {
            mode = MirrorMode.Manicure;
            currentArm.RandomNailColor();
            // #TODO zoom to nails
        }
        void SetSkinColor(Color skinColor) {
            for (int i = 0; i < arbitro.leftArm.colorer.skin.Length; i++) {
                arbitro.leftArm.colorer.skin[i].materials[0].color = skinColor;
                arbitro.rightArm.colorer.skin[i].materials[0].color = skinColor;
            }
            for (int i = 0; i < mirror.fingers.Length; i++) {
                mirror.fingers[i].color = skinColor;
            }
        }
        void NailLengthSlid(float value) {
            float nailExtension = Mathf.Lerp(0f, mirror.maxNailHeight, value);
            for (int i = 1; i < mirror.nails.Length; i++) {
                if (mirror.nails[i].TryGetComponent(out RectTransform rt)) {
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, mirror.minNailHeight + nailExtension);
                }
            }
            if (mirror.nails[0].TryGetComponent(out RectTransform rtPinky)) {
                rtPinky.sizeDelta = new Vector2(rtPinky.sizeDelta.x, mirror.minPinkNailHeight + nailExtension);
            }
        }
        void PaintNail(int index) {
            if (equippedNailPolishBrush) mirror.nails[index].image.color = mirror.nailPolishBrush.bristles.color;
        }
        void ClearNailColor() {
            for (int i = 0; i < mirror.nails.Length; i++) {
                mirror.nails[i].image.color = mirror.keratinColor;
            }
        }
        void PickTattoo() {
            Debug.LogWarning("pick tatoooo");
        }

        void ClearColorBox() {
            if (mirror.colorBox) {
                Vector2 ogSizeDelta = new Vector2(mirror.colorBox.rtParent.sizeDelta.x, mirror.colorBox.parentHeightCache);
                mirror.colorBox.rtParent.sizeDelta = ogSizeDelta;
                mirror.colorBox.rtParentShadow.sizeDelta = ogSizeDelta;
                Destroy(mirror.colorBox.gameObject); //#HACK
                mirror.colorBox = null;
                colorBoxCat = Category.None;
            }
        }
        void ClickedOnNailPolishJar() {
            if (mirror.nailPolishBrush.gameObject.activeSelf) {

                equippedNailPolishBrush = false;

                mirror.nailPolishBrush.gameObject.SetActive(false);
                mirror.nailPolishJar.openJar.gameObject.SetActive(false);
                mirror.nailPolishJar.closedJar.gameObject.SetActive(true);
                ClearColorBox();

                Cursor.visible = true;
            }
            else {
                TakeOutNailBrush();
            }
        }
        void ClickedOnNailPolishBrushInJar() {
            TakeOutNailBrush();
        }
        void TakeOutNailBrush() {
                equippedNailPolishBrush = true;

                mirror.nailPolishJar.closedJar.gameObject.SetActive(false);
                mirror.nailPolishJar.openJar.gameObject.SetActive(true);
                mirror.nailPolishBrush.gameObject.SetActive(true);

                if (mirror.colorBox) ClearColorBox();

                colorBoxCat = Category.Nails;
                mirror.colorBox = ColorBox.MakeColorBox(mirror, mirror.nailBox, mirror.nailBoxShadow, mirror.nailColors);
        }
        public void SelectedColor(Color c) {
            switch (colorBoxCat) {
                case Category.Nails:
                    mirror.nailPolishJar.liquid.image.color = c;
                    mirror.nailPolishBrush.bristles.color = c;
                    break;
            }
        }
        public void ClearColor() {

            switch (colorBoxCat) {
                case Category.Nails:
                    mirror.nailPolishJar.liquid.image.color = mirror.keratinColor;
                    mirror.nailPolishBrush.bristles.color = mirror.keratinColor;
                    break;
            }
        }

        public void ApplyTattoo(Tattoo tat, float r, float theta) {
            currentArm.tattoos.Add(tat);
            // #TODO
            SaveArms();
        }
        void SwitchArms() {
            Arm cachedArm = currentArm;
            currentArm = otherArm;
            otherArm = cachedArm;
            
            if (currentArm.isDominant) {
                mirror.makeDominantText.text = Language.current[Words.IsDominantChecked];
            }
            else {
                mirror.makeDominantText.text = Language.current[Words.IsDominantUnchecked];
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
            if (mirror.fingers.Length >= 5) {

                // pinky is first finger, i know, weird!
                if (mirror.fingers[0].transform.parent is RectTransform rtPinky) { 
                    rtPinky.localPosition = new Vector3(pinkySign *Mathf.Abs(rtPinky.localPosition.x), rtPinky.localPosition.y);
                } 

                // thumb
                if (mirror.fingers[4].transform.parent is RectTransform rtThumb) {
                    rtThumb.localPosition = new Vector3(-pinkySign * Mathf.Abs(rtThumb.localPosition.x), rtThumb.localPosition.y);
                    rtThumb.localRotation = Quaternion.Euler(0f, 0f, pinkySign * 12);
                } 

                // eh, i'm currently rotating the nailpolish brush every frame to match arm
                // it's fine
            }

        }

        void ToggleDominance() {
            if (currentArm.isDominant) {
                currentArm.isDominant = false;
                otherArm.isDominant = true;
                mirror.makeDominantText.text = Language.current[Words.IsDominantUnchecked];
                arbitro.dominantArm = otherArm;
            }
            else {

                currentArm.isDominant = true;
                otherArm.isDominant = false;
                mirror.makeDominantText.text = Language.current[Words.IsDominantChecked];
                arbitro.dominantArm = currentArm;
            }

            SaveArms();
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

        private void MoveInput(InputAction.CallbackContext ctx) {

        }
        private void LookInput(InputAction.CallbackContext ctx) {

        }
        private void PrimaryAction(InputAction.CallbackContext ctx) {
            if (mode == MirrorMode.PlacingTattoo) {

            }
        }
        private void SecondaryAction(InputAction.CallbackContext ctx) {

        }

        private void Update() {
            // mouse no longer over the NAILS category

            if (equippedNailPolishBrush) {

                Vector2 mousePosition = Vector2.zero;
                if (Mouse.current != null) mousePosition = Mouse.current.position.ReadValue();

                if (RectTransformUtility.RectangleContainsScreenPoint(mirror.nailBox, mousePosition, null)) {

                    Cursor.visible = false;

                    if (mirror.nailPolishBrush.TryGetComponent(out RectTransform follower)) {
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            follower.parent as RectTransform,
                            mousePosition,
                            null, // null for Screen Space - Overlay
                            out Vector2 pos
                        );
                        follower.anchoredPosition = pos;
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


            //#TEMP
            if (false && mirror.colorBox) {

                Vector2 mousePosition = Vector2.zero;
                if (Mouse.current != null) mousePosition = Mouse.current.position.ReadValue();

                if (!RectTransformUtility.RectangleContainsScreenPoint(mirror.colorBox.rtParent, mousePosition, null)) {
                    ClearColorBox();
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
                        mirror.group.alpha = Mathf.Lerp(0f, 1f, tGUI / mirrorCanvasFadeDuration);
                    }
                    break;

                case MirrorMode.Inactive:
                    if (mirror.group.alpha > 0f) {
                        float fadeOutSpeed = 1f / (1f - mirrorCanvasFadeDuration);
                        mirror.group.alpha -= fadeOutSpeed * Time.deltaTime;
                    }
                    else {
                        mirror.gameObject.SetActive(false);
                        enabled = false;
                    }
                    break;
            }
        }


        public void ApproachMirror(RefControls approacher) {
            
            ReadOnlyArray<PlayerInput> allInput = PlayerInput.all;
            foreach (PlayerInput input in allInput) {
                InputActionMap map = input.actions.FindActionMap(MIRROR_ACTION_MAP);
                if (map != null) input.SwitchCurrentActionMap(MIRROR_ACTION_MAP);
                else Debug.LogError("can't find map: " + MIRROR_ACTION_MAP);
            }

            arbitro = approacher;
            if (arbitro.TryGetComponent(out CharacterController cc)) cc.enabled = false;
            arbitro.lookVelocity = Vector2.zero;
            arbitro.yaw = 0f;
            arbitro.pitch = 0f;
            enabled = true;
            t = 0f;

            arbitro.hud.gameObject.SetActive(false);
            mirror.gameObject.SetActive(true);
            mirror.group.alpha = 0f;
            approachStartPos = arbitro.transform.position;
            approachStartGaze = arbitro.cam.transform.rotation;

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
            mirror.makeDominantText.text = Language.current[Words.IsDominantChecked];

            OrientFingers();

            mode = MirrorMode.Approaching;
        }

        public void LeaveMirror() {
            if (!arbitro) {
                Debug.LogError("arbitro not at mirror");
                return; //////earlyreturn///
            }

            if (!currentArm.isDominant) SwitchArms();

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

    }
}