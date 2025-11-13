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
        public MirrorCanvas mirrorCan;
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

        void SaveArms() {
            // #TODO
            Debug.LogWarning("save arms not implemented");
        }

        public void Awake() {

            enabled = false;
            mode = MirrorMode.Inactive;

            mirrorCan = Instantiate(mirrorCanvasPrefab, transform).GetComponent<MirrorCanvas>();

            mirrorCan.switchArms.onClick.AddListener(SwitchArms);
            mirrorCan.dominanceCheckbox.onClick.AddListener(ToggleDominance);
            mirrorCan.back.onClick.AddListener(Back);

            //mirror.skinColorSwatches

            mirrorCan.hairThicknessSlider.onValueChanged.AddListener(HairThicknessSlid);
            mirrorCan.hairLengthSlider.onValueChanged.AddListener(HairLengthSlid);
            mirrorCan.hairCurlSlider.onValueChanged.AddListener(HairCurlSlid);

            mirrorCan.muscleSlider.onValueChanged.AddListener(MuscleSlid);

            mirrorCan.nailLengthSlider.onValueChanged.AddListener(NailLengthSlid);
            mirrorCan.nailPolishJar.jarButton.onClick.AddListener(ClickedOnNailPolishJar);
            mirrorCan.gameObject.SetActive(false);
            mirrorCan.nailPolishBrush.gameObject.SetActive(false);
            mirrorCan.nailPolishRemoverSponge.gameObject.SetActive(false);
            mirrorCan.nailPolishRemoverMiniSponge.gameObject.SetActive(false);
            mirrorCan.nailColorSelectedIndex = 1;
            mirrorCan.nailPolishJar.liquid.color = mirrorCan.nailColors[mirrorCan.nailColorSelectedIndex];
            Debug.Assert(mirrorCan.colorBoxPrefab);
            Debug.Assert(mirrorCan.colorRowPrefab);

            mirrorCan.pickTattoo.onClick.AddListener(PickTattoo);
            
            for (int i = 0; i < mirrorCan.nails.Length; i++) {
                int fingerIndex = i;
                void PaintThisNail() {
                    PaintNail(fingerIndex);
                }
                mirrorCan.nails[i].onClick.AddListener(PaintThisNail);
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
        void NailLengthSlid(float value) {
            float nailExtension = Mathf.Lerp(0f, mirrorCan.maxNailHeight, value);
            for (int i = 1; i < mirrorCan.nails.Length; i++) {
                if (mirrorCan.nails[i].TryGetComponent(out RectTransform rt)) {
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, mirrorCan.minNailHeight + nailExtension);
                }
            }
            if (mirrorCan.nails[0].TryGetComponent(out RectTransform rtPinky)) {
                rtPinky.sizeDelta = new Vector2(rtPinky.sizeDelta.x, mirrorCan.minPinkNailHeight + nailExtension);
            }
        }
        void PaintNail(int index) {
            if (equippedNailPolishBrush) {
                if (mirrorCan.nailPolishRemoverSponge.gameObject.activeSelf) {
                    mirrorCan.nails[index].image.color = mirrorCan.keratinColor;
                }
                else mirrorCan.nails[index].image.color = mirrorCan.nailPolishBrush.bristles.color;
            }
        }
        void ClearNailColor() {
        }
        void PickTattoo() {
            Debug.LogWarning("pick tatoooo");
        }

        void CloseColorBox() {
            if (mirrorCan.colorBox) {
                mirrorCan.nailPolishRemoverMiniSponge.SetParent(null);
                mirrorCan.nailPolishRemoverMiniSponge.gameObject.SetActive(false);
                Vector2 ogSizeDelta = new Vector2(mirrorCan.colorBox.rtParent.sizeDelta.x, mirrorCan.colorBox.parentHeightCache);
                mirrorCan.colorBox.rtParent.sizeDelta = ogSizeDelta;
                mirrorCan.colorBox.rtParentShadow.sizeDelta = ogSizeDelta;
                Destroy(mirrorCan.colorBox.gameObject); //#HACK
                mirrorCan.colorBox = null;
            }
        }
        void ClickedOnNailPolishJar() {
            if (equippedNailPolishBrush) {

                equippedNailPolishBrush = false;

                mirrorCan.nailPolishBrush.gameObject.SetActive(false);
                mirrorCan.nailPolishRemoverSponge.gameObject.SetActive(false);
                mirrorCan.nailPolishJar.openJar.gameObject.SetActive(false);
                mirrorCan.nailPolishJar.closedJar.gameObject.SetActive(true);
                CloseColorBox();

                Cursor.visible = true;
            }
            else {
                equippedNailPolishBrush = true;

                mirrorCan.nailPolishJar.closedJar.gameObject.SetActive(false);
                mirrorCan.nailPolishJar.openJar.gameObject.SetActive(true);
                mirrorCan.nailPolishBrush.gameObject.SetActive(!usingSponge);
                mirrorCan.nailPolishRemoverSponge.gameObject.SetActive(usingSponge);

                if (mirrorCan.colorBox) CloseColorBox();

                mirrorCan.colorBox = ColorBox.MakeColorBox(mirrorCan, mirrorCan.nailBox, mirrorCan.nailBoxShadow, mirrorCan.nailColors);
                RectTransform first = mirrorCan.colorBox.rows[0].swatches[0].GetComponent<RectTransform>();
                mirrorCan.nailPolishRemoverMiniSponge.SetParent(first.transform.parent);
                mirrorCan.nailPolishRemoverMiniSponge.anchoredPosition = first.anchoredPosition;
                mirrorCan.nailPolishRemoverMiniSponge.gameObject.SetActive(true);
                int count = mirrorCan.nailColorSelectedIndex;
                for(int i = 0; i < mirrorCan.colorBox.rows.Length; i++) {
                    int l = mirrorCan.colorBox.rows[i].swatches.Length;
                    if (l >= count) {
                        mirrorCan.colorBox.SelectedSwatch(mirrorCan.colorBox.rows[i].swatches[count]);
                        break;
                    }
                    else count -= l;
                }
            }
        }
        public void SelectedColor(Category cat, Color c) {
            switch (cat) {
                case Category.Skin:
                    for (int i = 0; i < arbitro.leftArm.colorer.skin.Length; i++) {
                        arbitro.leftArm.colorer.skin[i].materials[0].color = c;
                        arbitro.rightArm.colorer.skin[i].materials[0].color = c;
                    }
                    for (int i = 0; i < mirrorCan.fingers.Length; i++) {
                        mirrorCan.fingers[i].color = c;
                    }
                    break;
                case Category.Hair:
                    arbitro.leftArm.SetHairColor(c);
                    arbitro.rightArm.SetHairColor(c);
                    break;
                case Category.Nails:
                    usingSponge = false;
                    mirrorCan.nailPolishJar.liquid.color = c;
                    mirrorCan.nailPolishBrush.bristles.color = c;
                    mirrorCan.nailPolishBrush.gameObject.SetActive(!usingSponge);
                    mirrorCan.nailPolishRemoverSponge.gameObject.SetActive(usingSponge);
                    break;
            }
        }
        public void ClearColor(Category cat) {

            switch (cat) {
                case Category.Nails:
                    usingSponge = true;
                    mirrorCan.nailPolishJar.liquid.color = mirrorCan.keratinColor;
                    mirrorCan.nailPolishBrush.gameObject.SetActive(!usingSponge);
                    mirrorCan.nailPolishRemoverSponge.gameObject.SetActive(usingSponge);
                    break;

                default:
                    Debug.LogError("when do we clear color for anything but nails?");
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
                mirrorCan.makeDominantText.text = Language.current[Words.IsDominantChecked];
            }
            else {
                mirrorCan.makeDominantText.text = Language.current[Words.IsDominantUnchecked];
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
            if (mirrorCan.fingers.Length >= 5) {

                // pinky is first finger, i know, weird!
                if (mirrorCan.fingers[0].transform.parent is RectTransform rtPinky) { 
                    rtPinky.localPosition = new Vector3(pinkySign *Mathf.Abs(rtPinky.localPosition.x), rtPinky.localPosition.y);
                } 

                // thumb
                if (mirrorCan.fingers[4].transform.parent is RectTransform rtThumb) {
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
                mirrorCan.makeDominantText.text = Language.current[Words.IsDominantUnchecked];
                arbitro.dominantArm = otherArm;
            }
            else {

                currentArm.isDominant = true;
                otherArm.isDominant = false;
                mirrorCan.makeDominantText.text = Language.current[Words.IsDominantChecked];
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

                if (RectTransformUtility.RectangleContainsScreenPoint(mirrorCan.nailBox, mousePosition, null)) {

                    Cursor.visible = false;

                    if (mirrorCan.nailPolishBrush.TryGetComponent(out RectTransform follower)) {
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            follower.parent as RectTransform,
                            mousePosition,
                            null, // null for Screen Space - Overlay
                            out Vector2 pos
                        );
                        follower.anchoredPosition = pos;
                        mirrorCan.nailPolishRemoverSponge.anchoredPosition = pos;
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
                        mirrorCan.group.alpha = Mathf.Lerp(0f, 1f, tGUI / mirrorCanvasFadeDuration);
                    }
                    break;

                case MirrorMode.Inactive:
                    if (mirrorCan.group.alpha > 0f) {
                        float fadeOutSpeed = 1f / (1f - mirrorCanvasFadeDuration);
                        mirrorCan.group.alpha -= fadeOutSpeed * Time.deltaTime;
                    }
                    else {
                        mirrorCan.gameObject.SetActive(false);
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
            arbitro.hud.gameObject.SetActive(false);
            if (arbitro.TryGetComponent(out CharacterController cc)) cc.enabled = false;
            arbitro.lookVelocity = Vector2.zero;
            arbitro.yaw = 0f;
            arbitro.pitch = 0f;
            enabled = true;
            t = 0f;

            mirrorCan.gameObject.SetActive(true);
            mirrorCan.group.alpha = 0f;
            mirrorCan.makeDominantText.text = Language.current[Words.IsDominantChecked];

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

            approachStartPos = arbitro.transform.position;
            approachStartGaze = arbitro.cam.transform.rotation;
            mode = MirrorMode.Approaching;

            // hm. maybe new convention where i avoid calling out until end of function...
            // bc turns out OrientFingers relied on currentArm being assigned
            // and InitSkin.. relied on approacher being assigned
            // it's obvious now, but maybe this convention will help
            // ofc, if OrientFingers relied on something in InitSkin then oh well
            OrientFingers();
            mirrorCan.InitSkinAndHairColorButtons(this, approacher.skinIndex, approacher.hairIndex);
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