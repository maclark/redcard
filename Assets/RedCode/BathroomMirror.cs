using UnityEngine;
using UnityEngine.UI;
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
        public AnimationCurve approachCurve;

        [Header("VARS")]
        public MirrorCanvas mirror;
        public MirrorMode mode = MirrorMode.Inactive;
        public Arm currentArm;


        public const int MAX_TATTOOS = 3;
        public const string MIRROR_ACTION_MAP = "GazingInMirror";

        Vector3 approachStartPos;
        Quaternion approachStartGaze;
        float t;
        RefControls arbitro;


        void SaveArms() {
            // #TODO
            Debug.LogWarning("save arms not implemented");
        }

        public void Awake() {

            enabled = false;
            mode = MirrorMode.Inactive;

            mirror = Instantiate(mirrorCanvasPrefab, transform).GetComponent<MirrorCanvas>();
            mirror.hairThicknessSlider.onValueChanged.AddListener(HairThicknessSlid);
            mirror.hairLengthSlider.onValueChanged.AddListener(HairLengthSlid);
            mirror.hairColorPicker.onValueChanged.AddListener(HairColorPicker);
            mirror.hairCurlSlider.onValueChanged.AddListener(HairCurlSlid);
            mirror.muscleSlider.onValueChanged.AddListener(MuscleSlid);
            mirror.switchArms.onClick.AddListener(SwitchArms);
            mirror.makeDominant.onClick.AddListener(MakeDominant);
            mirror.startManicure.onClick.AddListener(StartManicure);
            mirror.pickTattoo.onClick.AddListener(PickTattoo);
            mirror.back.onClick.AddListener(Back);
            mirror.gameObject.SetActive(false);

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
        void PickTattoo() {
            throw new System.NotImplementedException();
        }
        public void ApplyTattoo(Tattoo tat, float r, float theta) {
            currentArm.tattoos.Add(tat);
            // #TODO
            SaveArms();
        }
        void SwitchArms() {
           currentArm.gameObject.SetActive(false);
            if (currentArm.side == Chirality.Left) {
                currentArm = arbitro.rightArm;
                currentArm.transform.localPosition = arbitro.rightArmLoweredPos;
            }
            else {
                currentArm = arbitro.leftArm;
                currentArm.transform.localPosition = arbitro.leftArmLoweredPos;
            }
            currentArm.gameObject.SetActive(true);
            arbitro.SlotEquipped((int)RefEquipment.Barehand);
        }

        void MakeDominant() {
            currentArm.isDominant = true;
            if (currentArm.side == Chirality.Right) {
                arbitro.leftArm.isDominant = false;
            }
            else arbitro.rightArm.isDominant = false;
            arbitro.dominantArm = currentArm;
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
            if (!arbitro) {
                Debug.LogError("mirror active but has no arbitro");
                enabled = false;
                return;
            }

            switch (mode) {
                case MirrorMode.GazingAtArm:
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
            approachStartPos = arbitro.transform.position;
            approachStartGaze = arbitro.cam.transform.rotation;

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;

            if (arbitro.dominantArm == arbitro.leftArm) currentArm = arbitro.leftArm;
            else currentArm = arbitro.rightArm;

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

            mirror.gameObject.SetActive(false);
            enabled = false;
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