using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using System;

namespace RedCard {

    public enum RefEquipment {
        Unused = 0,

        // main gear
        Whistle = 1,
        Barehand = 2,
        Watch = 3,

        // utility, cycled with 4 atm
        Book,
        SprayCan,
        YellowCard,
        RedCard,
        Coin,

        Count,
    }

    public enum Chirality {
        Left,
        Right,
    }

    public enum ItemName {
        Unset,

        Ball,
        RuleBook,
        ProgressAndPoverty,
        MensHealth,
        Banana,
        Crossword,
        Coin,
        Penis,

        Count,
    }

    [RequireComponent(typeof(CharacterController))]
    public class RefControls : MonoBehaviour {

        [Header("ASSIGNATIONS")]
        public Camera cam;
        public Camera armCam;
        public HUD hud;
        public CrossHairs crossHairs;
        public Arm leftArm;
        public Arm rightArm;

        [Header("STAMINA")]
        public AnimationCurve lactateBuildupCurve;
        public float startingLactateConcentration = 1f;
        // glycogen often measured as mmol/kg muscle
        // or as mg/g of tissue
        // above this, lactic acid builds up
        // at 85% hrr

        // hr       |       
        // 70       |   1.0, very slow
        // 102      |   activeLactateRecoveryTriggers
        // 135 70%  |   2.0, medium
        // 155 >85% |   spikes really fast

        // 0.0231049060187 is 1.0 mmol/l at resting recovery
        // 0.0462098120373 is 1.0 mmol/l at active recovery
        // 0.0924196240747 is 2.0 mmol/l at active recovery
        // 0.184839248149  is 4.0 mmol/l at active recovery

        // c = b * ln(2) / h
        // b is blood lactate
        // h is half life
        // c is the constant input that would cause flat blood lactate concentration

        private float oneLactateRRecover = 1 * 0.0231049060187f * PER_MIN; 
        private float oneLactateARecover = 2 * 0.0231049060187f * PER_MIN;
        private float twoLactateARecover = 4 * 0.0231049060187f * PER_MIN;
        private float fourLactateARecover = 8 * 0.0231049060187f * PER_MIN;
        private float activeRecoveryThreshold = 100f;
        private float aerobicThreshold = 135f; // ~70% of max hr 
        private float anaerobicThreshold = 155f; // ~80% of max hr

        private float oblaThreshold = 3.9f; // tons of glycogen burned, lactate builds up in muscles instead of blood!

        private float activeLactateRecoveryHalfLife = 15f * 60f; // mins to reach half amount 
        private float restingLactateRecoveryHalfLife = 30f * 60f; 
        public float standingHeartRate = 70f;

        public float walkingHeartRate = 100f;
        public float joggingHeartRate = 150f;
        public float maxHeartRate = 185f;

        // jogging for "60" mins should leave u exhausted
        public float startingGlycogen = 420f;
        public float exhaustedThreshold = 42f;
        public float weaknessThrehold = 100f;
        public float glycogenBurnStanding = 1f; // per min
        public float glycogenBurnWalking = 2f; // per min
        public float glycogenBurnJogging = 8f; // per min so 420/8 = 52.5 minutes
        public float glycogenExtremeBurn = 40f;
        // this could be a curve. from 155bpm to 185bpm.
        // so 0.33 per bpm over would get abour to 18 glyogen burn per min
        public float glycogenBurnPerHeartRateOverLT = .33f; // this could be a curve

        // from 70 to 150 in 4 mins => 20 per minute
        public float heartRateAcceleration = 20f;
        public float heartRateCreepAcceleration = 10f;
        public float heartRateRecoveryDeceleration = -10f; // half of acceleration

        public float glycogenStores;
        public float lactateConcentration;
        public float heartRate;

        [Header("MOVEMENT")]
        public bool canLookAround; // set false in initialization
        public float dashHeartRateCost = 2f;
        public float dashBurst = 15f;
        public float dashDuration = .1f;
        public float joggingSpeed = 3.5f;
        public float pacingSpeedCap = 2f;
        public float backwardMovementReduction = .67f;
        public float movementEquipmentReduction = .67f;
        public float gravity = -9.81f;
        public float mouseSensitivity = 1f;
        public float arrowLookSensitivity = 5f;
        public float acceleration = 1f;
        public float sprintBoost = 2.5f;

        [Header("ARROW CONTROLS")]
        public bool lookingWithArrows = false;
        public Vector2 arrowsInput = Vector2.zero; 
        public Vector2 lookVelocity = Vector2.zero;
        public float arrowsAcceleration = 15f;
        public float arrowsDrag = 10f;

        [Header("SCOPING IN")]
        public float normal_vert_fov = 74f;
        //[SerializeField] private float normal_horz_fov = complicated formula;
        [SerializeField] private float zoomed_fov = 60f;
        [SerializeField] private float zoom_speed = 20f;

        [Header("INTERACTABLES MAYBE")]
        public Item itemHeld;
        public Item itemTouching;
        public RefereeCustomizer mirror;
        public float itemTossStrength = 1f; // #GAMEPAD
        public Interactable interactableTouching;
        public float interactibility_range = 1.5f;
        public DuffelBag duffel;
        private HashSet<RefEquipment> acquiredEquipment = new HashSet<RefEquipment>();

        [Header("COIN")]
        public Coin coin;
        public float coinFlipStrength = 1f;
        public float minCoinFlipStrength = 1f;

        [Header("WHISTLE")]
        [SerializeField] private GameObject whistle;
        public float minWhistleThreshold = .2f;
        private bool blowingWhistle = false;
        private float tBlowingWhistle = 0f;
        private Vector3 whistleHeldPosition = new Vector3(0.125f, -0.16f, 0.275f);
        private Vector3 whistleHeldRotation = new Vector3(341.7f, 160.7f, 356.4f);
        private Vector3 whistleBlowPosition = new Vector3(0f, -0.05f, 0.125f);
        private Vector3 whistleBlownRotation = new Vector3(0f, 0f, 0f);

        [Header("SPRAY CAN")]
        [SerializeField] private GameObject sprayCan;
        [SerializeField] private Transform sprayNozzle;  
        [SerializeField] private ParticleSystem sprayParticles;
        public AnimationCurve foamBlobGrowthCurve;
        public float foamBlobLife = 60f;
        public float maxBlobScale = .15f;
        public float foamBlobGrowthTime = 1f;
        public float spraySpread = 5f;
        public float tSpray = 0f;
        public float sprayGap = .033f;
        public float sprayDistance = 1f;
        public bool spraying = false;
        public int blobsPerSprayLineVertex = 20;
        public float maxSprayLineGapSqr = .666f;
        public int sprayLineSegments = 20;
        public GameObject foamBlobPrefab;
        private FoamBlob[] foamBlobs;
        private int oldestGrowingBlobIndex;
        private int growingBlobCount;
        private int foamIndex;
        private List<Vector3> sprayLine = new List<Vector3>();
        private float sprayLife = 1.5f;
        private float lastSprayedAt = 0f;
        private float lastBlobSpawnedAt = 0f;
        private int sinceLastSprayVertex = 0;

        [Header("INDICATING ARM")]
        public Arm dominantArm;
        public Vector2 armTilt = new Vector2(1f, .5f);
        public float yawIndicatingTolerance = 10f;
        public float yawSpring = 5f;
        private bool indicating = false;
        private RefTarget target;
        private RefTarget indicatedTarget;
        private Vector3 indicatedPosition;
 
        // game timing
        [Header("STOPWATCH")]
        [SerializeField] private GameObject watch;
        [SerializeField] private TMP_Text watchTimer;
        bool watchRunning = false;
        float hundredths0;
        int seconds0;
        int minutes0;
        float secondsElapsedSinceKickoff = 0f; // the time against which the ref will be judged


        [Header("MISCONDUCT CARDS")]
        [SerializeField] private GameObject yellowCard;
        [SerializeField] private GameObject redCard;
        private Vector3 yellowHeldPosition;
        public Vector3 yellowShownPosition;
        private Vector3 yellowHeldRotation;
        public Vector3 yellowShownRotation;
        private Vector3 redHeldPosition;
        public Vector3 redShownPosition;
        private Vector3 redHeldRotation;
        public Vector3 redShownRotation;
        private bool raisingCard = false;
        public float raisingCardThreshold = .33f;
        private float tRaisingCard = 0f;

        public const float PER_MIN = 1f / 60f; 

        private Transform cameraTransform;
        private CharacterController controller;
        private Vector3 fallVelocity;
        public float pitch = 0f;
        private Vector2 feetInput;
        private Vector3 moveDirection;
        public float yaw;
        private bool movingBackwards = false;
        //private bool exhausted = false;
        private bool sprinting = false;
        private bool scanning = false;
        private bool zooming = false;
        private bool pacingOneself = false;

        public bool debugLooking = false;
        public RefEquipment equipped;

        // can't do this at runtime: LayerMask.NameToLayer(...);
        public static readonly int Default_Layer = 0;
        public static readonly int RefArms_Layer = 6;
        public static readonly int RefBody_Layer = 7;
        public static readonly int BookkUI_Layer = 8;
        public static readonly int Item_Layer = 9; 

        private bool debugStartWithEquipment = false;
        private Dictionary<InputAction, List<Action<InputAction.CallbackContext>>> actionRegistry = new Dictionary<InputAction, List<Action<InputAction.CallbackContext>>>();


        void Start() {

            if (RedMatch.match.arbitro) {
                Debug.LogError("uh oh double arbitros");
            }
            RedMatch.match.arbitro = this;

            if (System.IO.Directory.Exists("C:\\Users\\maxac")) {
                lookingWithArrows = true;
                print("on laptop, lookingWithArrows initialized: " + lookingWithArrows);
            }
            else if (Mouse.current == null) {
                // a trackpad will count as a mouse, so this isn't as valuable as i intended
                lookingWithArrows = true;
                print("no mouse present, lookingWithArrows initialized: " + lookingWithArrows);
            }
            else {
                print("mouse present, not on laptop, lookingWithArrows initialized: " + lookingWithArrows);
            }

            if (Application.isEditor) debugStartWithEquipment = true;
            lookingWithArrows = false;

            RedMatch.match.menu.gameObject.SetActive(false);

            // #OPTIMIZE
            crossHairs = FindAnyObjectByType<CrossHairs>();
            hud = FindAnyObjectByType<HUD>();
            hud.MakeVisible(RefEquipment.Barehand);
            hud.wheel.Init();
            hud.wheel.PopulateBoxes(hud.wheel.nothingToSay);

            float x = hud.cursor.width / 2f;
            float y = hud.cursor.height / 2f;
            Cursor.SetCursor(hud.cursor, new Vector2(x, y), CursorMode.ForceSoftware);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            canLookAround = true;

            acquiredEquipment.Clear();
            acquiredEquipment.Add(RefEquipment.Barehand);

            // if at stadium, equip 'em all
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Stadium1_Small" || debugStartWithEquipment) {
                acquiredEquipment.Add(RefEquipment.Whistle);
                hud.MakeVisible(RefEquipment.Whistle);
                acquiredEquipment.Add(RefEquipment.Watch);
                hud.MakeVisible(RefEquipment.Watch);
                acquiredEquipment.Add(RefEquipment.YellowCard);
                hud.MakeVisible(RefEquipment.YellowCard);
                acquiredEquipment.Add(RefEquipment.RedCard);
                hud.MakeVisible(RefEquipment.RedCard);
                acquiredEquipment.Add(RefEquipment.Book);
                hud.MakeVisible(RefEquipment.Book);
                acquiredEquipment.Add(RefEquipment.SprayCan);
                hud.MakeVisible(RefEquipment.SprayCan);
            }

            leftArm.gameObject.SetActive(false);
            rightArm.gameObject.SetActive(false);
            bool rightHanded = (UnityEngine.Random.value > .1f);
            leftArm.data = ArmData.LoadArms(Chirality.Left, !rightHanded);
            leftArm.Init();
            rightArm.data = ArmData.LoadArms(Chirality.Right, rightHanded);
            rightArm.Init();
            ArmData.SaveArms(leftArm.data, rightArm.data);
            if (leftArm.data.isDominant) dominantArm = leftArm;
            else dominantArm = rightArm;

            if (coin) coin.gameObject.SetActive(false);

            sprayCan.SetActive(false);
            Debug.Assert(sprayNozzle);
            sprayParticles.gameObject.SetActive(true); // just in case it was turned off in editor
            sprayParticles.Stop();
            sprayLife = sprayParticles.main.startLifetime.Evaluate(0f);
            foamBlobs = new FoamBlob[500];
            for (int i = 0; i < foamBlobs.Length; ++i) {
                FoamBlob blob = Instantiate(foamBlobPrefab).GetComponent<FoamBlob>();
                blob.gameObject.name = "FoamBlob" + i;
                blob.gameObject.SetActive(false);
                foamBlobs[i] = blob;
            }

            yellowCard.GetComponent<MeshRenderer>().materials[0].color = Color.yellow;
            yellowCard.SetActive(false);
            yellowHeldPosition = yellowCard.transform.localPosition;
            yellowHeldRotation = yellowCard.transform.localEulerAngles;
            redCard.GetComponent<MeshRenderer>().materials[0].color = Color.red;
            redCard.SetActive(false);
            redHeldPosition = redCard.transform.localPosition;
            redHeldRotation = redCard.transform.localEulerAngles;

            watch.SetActive(false);
            watchTimer.text = "00:00:00";

            glycogenStores = startingGlycogen;
            heartRate = standingHeartRate;
            lactateConcentration = startingLactateConcentration;

            cam.fieldOfView = normal_vert_fov;
            cameraTransform = cam.transform;


            for (int i = 0; i < cam.layerCullDistances.Length; i++) {
                print($"layerCullDistances[{i}] ({LayerMask.LayerToName(i)}): {cam.layerCullDistances[i]}");

            } 
            //float[] distances = new float[0];
            //cam.layerCullDistances = distances;

            controller = GetComponent<CharacterController>();

            SlotEquipped((int)RefEquipment.Barehand);

            if (PlayerInput.all.Count == 0) {
                Debug.LogError("did we start the game from the stadium scene, hm? no player inputs");
                return; ///////////////early return///////////
            }

            // these are editable in the editor
            // find folder Assets/FootballSimulator/Input/engine.inputactions
            // voila
            string mapName = RedMatch.REFEREEING_ACTION_MAP;
            RedMatch.AssignMap(mapName);

            var action = PlayerInput.all[0].actions.FindActionMap(mapName).FindAction("Pause");
            if (action != null) {
                action.started += RedMatch.match.PauseGame;
                RegisterInput(action, RedMatch.match.PauseGame);
            }

            action = PlayerInput.all[0].actions.FindActionMap(mapName).FindAction("MoveWASD");
            if (action != null) {
                action.started += MoveInput;
                action.performed += MoveInput;
                action.canceled += MoveInput;
                RegisterInput(action, MoveInput);
            }
            else Debug.LogWarning("couldn't find MoveWASD action");

            action = PlayerInput.all[0].actions.FindActionMap(mapName).FindAction("Look");
            if (action != null) {
                action.performed += MouseLook;
                RegisterInput(action, MouseLook);
            }
            else Debug.LogWarning("couldn't find Look action");

            action = PlayerInput.all[0].actions.FindActionMap(mapName).FindAction("PrimaryAction");
            if (action != null) {
                action.started += PrimaryAction;
                action.performed += PrimaryAction;
                action.canceled += PrimaryAction;
                RegisterInput(action, PrimaryAction);
            }
            else Debug.LogWarning("couldn't find PrimaryAction action");

            action = PlayerInput.all[0].actions.FindActionMap(mapName).FindAction("SecondaryAction");
            if (action != null) {
                action.started += SecondaryAction;
                action.performed += SecondaryAction;
                action.canceled += SecondaryAction;
                RegisterInput(action, SecondaryAction);
            }
            else Debug.LogWarning("couldn't find SecondaryAction action");

            action = PlayerInput.all[0].actions.FindActionMap(mapName).FindAction("Sprint");
            if (action != null) {
                action.canceled += Sprint;
                action.started += Sprint;
                RegisterInput(action, Sprint);
            }
            else Debug.LogWarning("couldn't find Sprint action");

            action = PlayerInput.all[0].actions.FindActionMap(mapName).FindAction("Interact");
            if (action != null) {
                action.started += Interact;
                RegisterInput(action, Interact);
            }
            else Debug.LogWarning("couldn't find Interact action");

            action = PlayerInput.all[0].actions.FindActionMap(mapName).FindAction("DialogWheel");
            if (action != null) {
                action.started += OpenDialogWheel;
                action.canceled += CloseDialogWheel;
                RegisterInput(action, OpenDialogWheel);
                RegisterInput(action, CloseDialogWheel);
            }
            else Debug.LogWarning("couldn't find Speech action");

            action = PlayerInput.all[0].actions.FindActionMap(mapName).FindAction("Arrows");
            if (action != null) {
                action.performed += ArrowLook;
                RegisterInput(action, ArrowLook);
            }
            else Debug.LogWarning("couldn't find Arrows action");

            // 1, 2, 3 are barehand, whistle, watch
            // 4 is utility
            for (int slotIndex = 1; slotIndex <= 4; ++slotIndex) {
                action = PlayerInput.all[0].actions.FindActionMap(mapName).FindAction("Slot" + slotIndex);

                int callbackIndex = slotIndex;
                void SlotEquippedDel(InputAction.CallbackContext txt) {
                    SlotEquipped(callbackIndex);
                }
                if (action != null) { 
                    action.started += SlotEquippedDel;
                    RegisterInput(action, SlotEquippedDel);
                }
                else Debug.LogWarning("couldn't find Slot" + slotIndex + " action");
            }

        }


        private void MoveInput(InputAction.CallbackContext ctx) {
            if (ctx.canceled) feetInput = Vector2.zero;
            else {
                feetInput = ctx.ReadValue<Vector2>();
                // a bit janky here, bc you can go backwards, release, tap forwards, and then the acceleration jumps up
                // need to track camera co-linear velocity
                if (feetInput == Vector2.zero) {

                }
                else if (feetInput.y < 0) movingBackwards = true;
                else if (feetInput.y >= 0) movingBackwards = false;
            }
        }

        private void MouseLook(InputAction.CallbackContext ctx) {
            if (lookingWithArrows) return;
            if (!canLookAround) return;
            ///////////////////////earlyreturn///////////////////

            var look = ctx.ReadValue<Vector2>();
            if (hud.wheel.on && !hud.wheel.preppedDialog) {
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                Vector2 toMouse = (Vector2)Input.mousePosition - screenCenter;
                hud.wheel.HighlightOption(toMouse, false);
                look = Vector2.zero;
            }
            yaw += look.x * mouseSensitivity;
            pitch -= look.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -90f, 90f); // Prevent flipping
            lookDelta = lastLook - look;
            lastLook = look;
        }

        private void ArrowLook(InputAction.CallbackContext ctx) {
            if (!lookingWithArrows) return; 
            if (!canLookAround) return;
            //////////////earlyreturn//////////////

            arrowsInput = ctx.ReadValue<Vector2>();
            if (hud.wheel.on && !hud.wheel.preppedDialog) {
                hud.wheel.HighlightOption(arrowsInput, true);
                arrowsInput = Vector2.zero;
            }
        }

        private void Sprint(InputAction.CallbackContext ctx) {
            if (ctx.started) {
                //sprinting = true;
                pacingOneself = true;
            }
            else if (ctx.canceled) {
                //sprinting = false;
                pacingOneself = false;
            }
        }

        private void GrabItem() {
            if (!itemTouching) {
                Debug.LogError("can't grabitem, no itemTouching");
                return; /////////////earlyreturn//////////
            }

            print("grabbing " + itemTouching.iName);
            itemHeld = itemTouching;
            itemHeld.gameObject.layer = RefArms_Layer;
            itemHeld.transform.SetParent(cam.transform);
            itemHeld.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            itemHeld.transform.localPosition = new Vector3(0f, 0f, .5f);
            crossHairs.gameObject.SetActive(false);

            if (itemHeld.onGrabbed != null) {
                if (itemHeld.onGrabbed(new InputAction.CallbackContext(), this)) {
                    Debug.LogWarning($"{itemHeld.iName} blocking rest of Grabbed");
                    return; ////// early exit //////
                }
            }

            switch (itemHeld.iName) {

                case ItemName.Coin:

                    if (itemHeld.TryGetComponent(out Coin c)) {
                        if (coin) Debug.LogError("double coins?");

                        itemHeld = null; // Coin is half item half equipment

                        coin = c;
                        acquiredEquipment.Add(RefEquipment.Coin);
                        SlotEquipped((int)RefEquipment.Coin);
                        hud.MakeVisible(RefEquipment.Coin);

                        coin.enabled = true;
                        if (Vector3.Dot(Vector3.up, coin.transform.up) > 0f) coin.state = Coin.State.HeadsUp;
                        else coin.state = Coin.State.TailsUp;

                        coin.transform.localPosition = new Vector3(0f, -.1f, .3f);
                        coin.rb.isKinematic = true;
                        coin.rb.interpolation = RigidbodyInterpolation.None;
                        foreach (Transform t in coin.transform) {
                            t.gameObject.layer = RefArms_Layer;
                        }
                    }
                    else Debug.LogWarning("item named Coin has no coin!");
                    break;

                case ItemName.Ball:

                    if (itemHeld.TryGetComponent(out RedBall ball)) {
                        ball.rb.interpolation = RigidbodyInterpolation.None;
                        ball.rb.isKinematic = true;
                    }
                    else Debug.LogError("no ball on ball");

                        break;

                default:
                    break;
            }

            // may not be holding an item, because maybe we picked up the coin
            if (itemHeld) {
                hud.fToDrop.gameObject.SetActive(true);
                hud.fToDrop.text = HUD.F_To_Drop;
            }

        }

        private void DropItem() {
            if (!itemHeld) {
                Debug.LogWarning("DropItem, but no item held");
                return;
            }

            InputAction.CallbackContext _ctx = new InputAction.CallbackContext();
            if (itemHeld.onDropped != null && itemHeld.onDropped(_ctx, this)) return; //// early return///

            print("dropping item held " + itemHeld.iName);
            switch (itemHeld.iName) {

                case ItemName.Coin:
                    if (coin) { 
                        print("coin is flipping");
                        coin.state = Coin.State.Flipping;
                        coin.accumulatedRotation = 0f;
                        coin.flipCount = 0;
                        coin.enabled = true;
                        coin.justFlipped = true;
                        coin.aso.PlayOneShot(coin.aso.clip);
                        coin = null;
                        hud.coinIcon.gameObject.SetActive(false);
                        acquiredEquipment.Remove(RefEquipment.Coin);
                    }
                    else Debug.LogError("no coin to drop?");

                    foreach (Transform t in itemHeld.transform) {
                        t.gameObject.layer = Item_Layer;
                    }
                    break;

                case ItemName.Ball:
                    if (itemHeld.TryGetComponent(out RedBall ball)) {
                        ball.rb.isKinematic = false;
                        ball.rb.interpolation = RigidbodyInterpolation.Interpolate;
                    }
                    else Debug.LogError("no ball on ball");
                    break;

            }
            itemHeld.gameObject.layer = Item_Layer;
            itemHeld.transform.SetParent(null);
            if (itemHeld.TryGetComponent(out Rigidbody rb)) {
                rb.isKinematic = false;
                // #GAMEPAD 

                Vector2 toss = cam.transform.right * lookDelta.x + cam.transform.up * lookDelta.y;
                if (lookingWithArrows) {
                    toss = cam.transform.right * lookVelocity.x + cam.transform.up * lookVelocity.y;
                }
                else {
                    print("look delta: " + lookDelta);
                    print("cam.transform.up " + cam.transform.up);
                }

                rb.linearVelocity = itemTossStrength * toss;

                if (itemHeld.iName == ItemName.Coin) {
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                    float xTorque = Mathf.Max(minCoinFlipStrength, toss.magnitude * coinFlipStrength);
                    print("toss magnitude " + toss.magnitude);
                    print("toss torque " + xTorque);
                    rb.AddRelativeTorque(xTorque, 0f, 0f, ForceMode.Impulse);

                    if (itemHeld.TryGetComponent(out Coin tossedCoin)) {
                        tossedCoin.tossedFrom = tossedCoin.transform.position;
                        tossedCoin.tossedDir = toss.normalized;
                        tossedCoin.tossedMagnitude = toss.magnitude;
                    }
                }
            }
            itemHeld = null;
            crossHairs.gameObject.SetActive(true);
            hud.fToDrop.gameObject.SetActive(false);
        }

        private void Interact(InputAction.CallbackContext ctx) {
            if (equipped != RefEquipment.Barehand) return; //////earlyreturn////

            // holdable/droppable/throwable: book, ball, coin?
            // how do we teach throwing equipment?
            // oh yeah, sebaztian's gun...
            // pushable: door, people

            Debug.Assert(ctx.started);

            if (itemHeld) DropItem();
            else {
                if (indicating) {
                    StopIndicating();
                }

                if (itemTouching) {
                    GrabItem();
                }
                else if (duffel) {
                    if (CanTakeSomething(duffel, out RefEquipment equip)) {
                        if (equip == RefEquipment.Coin) {
                            if (duffel.coin) {
                                duffel.equipment.Remove(equip);
                                print("acquired " + equip);
                                itemTouching = duffel.coin.GetComponent<Item>();
                                GrabItem(); // this will equip the coin
                            }
                        }
                        else {
                            acquiredEquipment.Add(equip);
                            SlotEquipped((int)equip);
                            hud.MakeVisible(equip);
                            duffel.equipment.Remove(equip);
                            print("acquired " + equip);
                        }
                    }
                    else print("empty duffelt or we have it all");
                }
                else if (interactableTouching) Interactable.InteractWith(interactableTouching);
            }
        }

        // left-click, right trigger, screen tap
        private void PrimaryAction(InputAction.CallbackContext ctx) {

            if (hud.wheel.on) {
                hud.wheel.PrimaryAction(ctx, target);
                return; ////////earlyreturn///////////
            }

            if (itemHeld && itemHeld.onPrimary != null) {
                if (itemHeld.onPrimary(ctx, this)) return; /////////////////earlyreturn//////////////////
            }

            switch (equipped) {

                case RefEquipment.Whistle:
                    if (ctx.started) {
                        blowingWhistle = true;
                        tBlowingWhistle = 0f;
                        whistle.transform.localPosition = whistleBlowPosition;
                        whistle.transform.localRotation = Quaternion.Euler(whistleBlownRotation);
                    }
                    else if (ctx.canceled) {
                        blowingWhistle = false;
                        if (tBlowingWhistle > minWhistleThreshold) RedMatch.WhistleBlown(tBlowingWhistle);
                        tBlowingWhistle = 0f; // it also zeroes when started. idk.
                        NormalWhistlePosition();
                    }
                    break;

                case RefEquipment.Barehand:
                    if (ctx.started) {
                        indicatedTarget = target;
                        indicating = true;
                        RedMatch.IndicateCall(target, cameraTransform.forward);
                        scanning = false;
                        if (target) target.outline.SetActive(false);
                        target = null;
                        // this is needed to set IndicatedPosition before Update is called
                        ContinueIndicating();
                    }
                    else if (ctx.canceled) {
                        StopIndicating();
                        scanning = true;
                    }
                    break;

                case RefEquipment.Watch:
                    if (ctx.started) {
                        // play beep, all these things probably need some audio
                        watchRunning = !watchRunning;
                    }
                    break;

                case RefEquipment.SprayCan:
                    if (ctx.started) {
                        RedMatch.LineSprayed(sprayLine);
                        sprayLine.Clear();
                        spraying = true;
                        sprayParticles.Play();
                    }
                    else if (ctx.canceled) {
                        lastSprayedAt = Time.time;
                        sprayParticles.Stop();
                        spraying = false;
                    }
                    break;

                case RefEquipment.YellowCard:

                    if (ctx.started) {
                        raisingCard = true;
                        tRaisingCard = 0f;
                        yellowCard.transform.localPosition = yellowShownPosition;
                        yellowCard.transform.localRotation = Quaternion.Euler(yellowShownRotation);
                    }
                    else if (ctx.canceled) {
                        raisingCard = false;
                        NormalCardPositions();
                    }
                    break;
                case RefEquipment.RedCard:
                    if (ctx.started) {
                        raisingCard = true;
                        tRaisingCard = 0f;
                        redCard.transform.localPosition = redShownPosition;
                        redCard.transform.localRotation = Quaternion.Euler(redShownRotation);
                    }
                    else if (ctx.canceled) {
                        raisingCard = false;
                        NormalCardPositions();
                    }
                    break;

                case RefEquipment.Coin:

                    // flip coin
                    if (itemHeld) Debug.LogError("flippping coin but also holding item?");
                    itemHeld = coin.GetComponent<Item>();
                    DropItem();
                    SlotEquipped((int)RefEquipment.Barehand);
                    print("lost coin");

                    break;

                default:
                    Debug.LogWarning("unhandled primary action for equipment: " + equipped);
                    break;
            }
        }

        void ContinueIndicating() {
            if (indicatedTarget) {
                dominantArm.transform.localPosition = dominantArm.localRaisedPos;
                Vector3 dir = Vector3.zero;
                switch (indicatedTarget.targetType) {
                    // for corner, point "45 degree" in direction of flag
                    case TargetType.CornerFlag:
                        indicatedPosition = indicatedTarget.transform.position;
                        dir = Vector3.up * armTilt.y;
                        dir += armTilt.x * (indicatedTarget.transform.position - transform.position).normalized;
                        Debug.DrawLine(cameraTransform.position, cameraTransform.position + dir * 20f, Color.magenta);
                        dominantArm.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                        break;
                    // for goal kick, center circle, point directly
                    case TargetType.SixYardBox:
                    case TargetType.CenterCircle:
                    case TargetType.Ball:
                        indicatedPosition = indicatedTarget.transform.position;
                        dir = armTilt.x * (indicatedTarget.transform.position - transform.position).normalized;
                        Debug.DrawLine(cameraTransform.position, cameraTransform.position + dir * 20f, Color.magenta);
                        dominantArm.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                        break;
                    // for free kick, straight in direction of team taking kick
                    case TargetType.Player:
                        indicatedPosition = indicatedTarget.transform.position;
                        dir = RedMatch.OppositeEndDir(indicatedTarget.attackingEnd);
                        indicatedPosition = transform.position + dir;
                        dir *= armTilt.x;
                        Debug.DrawLine(cameraTransform.position, cameraTransform.position + dir * 20f, Color.magenta);
                        dominantArm.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                        break;
                    default:
                        indicatedPosition = indicatedTarget.transform.position;
                        Debug.LogWarning("continueindicating:unhandled targetType: " + indicatedTarget.targetType);
                        break;
                }
            }
            else {
                dominantArm.transform.rotation = Quaternion.LookRotation(cameraTransform.forward * armTilt.x + Vector3.up * armTilt.y, Vector3.up);
            }
        }

        void StopIndicating() {
            indicatedTarget = null;
            dominantArm.transform.localPosition = dominantArm.localLoweredPos;
            dominantArm.transform.localRotation = Quaternion.identity;
            indicating = false;
        }

        public void NormalWhistlePosition() {
            whistle.transform.localPosition = whistleHeldPosition;
            whistle.transform.localRotation = Quaternion.Euler(whistleHeldRotation);
        }

        void NormalCardPositions() {
            yellowCard.transform.localPosition = yellowHeldPosition;
            yellowCard.transform.localRotation = Quaternion.Euler(yellowHeldRotation);
            redCard.transform.localPosition = redHeldPosition;
            redCard.transform.localRotation = Quaternion.Euler(redHeldRotation);
        }

        private float dashing = 0f;
        private void Dash(InputAction.CallbackContext ctx) {
            if (ctx.started) {
                if (lactateConcentration < oblaThreshold && heartRate < maxHeartRate) {
                    heartRate += dashHeartRateCost * lactateConcentration;
                    dashing = dashDuration;
                }
            }
        }

        private void SecondaryAction(InputAction.CallbackContext ctx) {
            if (hud.wheel.on) {
                hud.wheel.SecondaryAction(ctx);
                return; ///////////earlyreturn///////////
            }

            if (itemHeld && itemHeld.onSecondary != null) {
                if (itemHeld.onSecondary(ctx, this)) return; ////////earlyreturn//////////
            }

            switch (equipped) {
                case RefEquipment.Whistle:
                case RefEquipment.Barehand:
                case RefEquipment.RedCard:
                case RefEquipment.YellowCard:
                    // #TODO allow to switch from toggling to hold-to-zoom in #OPTIONS
                    if (ctx.started) {
                        zooming = !zooming;
                    }
                    break;

                case RefEquipment.Watch:
                    break;
                case RefEquipment.SprayCan:
                    break;
                case RefEquipment.Coin:
                    coin.enabled = true;
                    if (coin.state == Coin.State.HeadsUp) coin.state = Coin.State.TurningOverToTailsUp;
                    else if (coin.state == Coin.State.TailsUp) coin.state = Coin.State.TurningOverToHeadsUp;
                    break;

                default:
                    Debug.LogWarning("unhandled secondary action for refequipment: " + equipped);
                    break;
            }
        }

        private void OpenDialogWheel(InputAction.CallbackContext ctx) {
            zooming = false;
            arrowsInput = Vector2.zero;
            hud.wheel.Open();
        }

        private void CloseDialogWheel(InputAction.CallbackContext ctx) {
            if (!hud.wheel.preppedDialog) hud.wheel.StartClosing();
        }

        public void SlotEquipped(int slotIndex) {
            if (hud.wheel.on) return;
            ///////////earlyreturn/////////
            if (slotIndex != 4 && slotIndex == (int)equipped) return;
            ///////////earlyreturn/////////

            if (itemHeld) DropItem();

            RefEquipment equipment = (RefEquipment)slotIndex;

            if (slotIndex == 4) {
                int hudSlotIndex = (int)hud.selected;
                if (hudSlotIndex >= 4) {
                    hudSlotIndex++;
                    print("acquiredEquipped.Count " + acquiredEquipment.Count);
                    if (hudSlotIndex > acquiredEquipment.Count) hudSlotIndex = 4;
                    equipment = (RefEquipment)hudSlotIndex;
                }
                print("utility equipped: " + equipment);
            }
            else print("equipped: " + equipment);

            if (!acquiredEquipment.Contains(equipment)) {
                print($"haven't acquired {equipment}, therefore can't equip it!");
                return;
                ////////////// early return ////////////////////////////
            }

            whistle.SetActive(false);
            watch.SetActive(false);
            sprayCan.SetActive(false);
            yellowCard.SetActive(false);
            redCard.SetActive(false);
            dominantArm.gameObject.SetActive(false);
            if (coin) coin.gameObject.SetActive(false);

            StopIndicating();
            NormalWhistlePosition();
            NormalCardPositions();

            scanning = false;
            switch (equipment) {
                case RefEquipment.Whistle:
                    whistle.SetActive(true);
                    HUD.SelectIcon(hud, RefEquipment.Whistle);
                    break;
                case RefEquipment.Barehand:
                    scanning = true;
                    dominantArm.gameObject.SetActive(true);
                    dominantArm.transform.localPosition = dominantArm.localLoweredPos;
                    HUD.SelectIcon(hud, RefEquipment.Barehand);
                    break;
                case RefEquipment.Watch:
                    watch.SetActive(true);
                    HUD.SelectIcon(hud, RefEquipment.Watch);
                    break;
                case RefEquipment.Book:
                    // book.SetActive(true);
                    HUD.SelectIcon(hud, RefEquipment.Book);
                    break;
                case RefEquipment.SprayCan:
                    sprayCan.SetActive(true);
                    HUD.SelectIcon(hud, RefEquipment.SprayCan);
                    break;
                case RefEquipment.YellowCard:
                    yellowCard.SetActive(true);
                    HUD.SelectIcon(hud, RefEquipment.YellowCard);
                    break;
                case RefEquipment.RedCard:
                    redCard.SetActive(true);
                    HUD.SelectIcon(hud, RefEquipment.RedCard);
                    break;
                case RefEquipment.Coin:
                    if (coin) coin.gameObject.SetActive(true);
                    else Debug.LogError("can't equip coin, no coin");
                    HUD.SelectIcon(hud, RefEquipment.Coin);
                    break;

                default:
                    Debug.LogWarning("unhandled slot equipped: " + slotIndex);
                    return;
            }

            if (!scanning && target) {
                target.outline.SetActive(false);
                target = null;
            }

            zooming = false;
            raisingCard = false;
            blowingWhistle = false;

            if (spraying) lastSprayedAt = Time.time;
            spraying = false;
            sprayParticles.Stop();

            equipped = equipment;
        }

        public void SpawnFoamBlob(Vector3 point, float targetScale, bool onField=true) {
            FoamBlob blob = foamBlobs[foamIndex];
            blob.gameObject.SetActive(true);
            blob.transform.position = point;
            blob.age = 0f;
            blob.targetScale = targetScale;
            blob.transform.localScale = Vector3.one * 0f;
            growingBlobCount++;
            foamIndex = (foamIndex + 1) % foamBlobs.Length;

            lastBlobSpawnedAt = Time.time;

            point += Vector3.up * .25f;
            if (onField) {
                if (sprayLine.Count == 0) sprayLine.Add(point);
                else if (Vector3.SqrMagnitude(point - sprayLine[^1]) > maxSprayLineGapSqr) {
                    RedMatch.LineSprayed(sprayLine);
                    sprayLine.Clear();
                }
                else {
                    sinceLastSprayVertex++;
                    if (sinceLastSprayVertex > blobsPerSprayLineVertex) {
                        sinceLastSprayVertex -= blobsPerSprayLineVertex;
                        sprayLine.Add(point);
                    }
                    else sprayLine[^1] = point;
                }
            }
        }

        public bool CanTakeSomething(DuffelBag duffel, out RefEquipment equipment) {
            equipment = RefEquipment.Unused;
            bool somethingsAvailable = false;
            for (int i = duffel.equipment.Count - 1; i >= 0; i--) {
                equipment = duffel.equipment[i];
                if (!acquiredEquipment.Contains(equipment)) {
                    somethingsAvailable = true;
                    break;
                }
            }

            // i realize we could shortcircuit this if itemHeld...
            return somethingsAvailable && !itemHeld;
        }

        float dt;
        Ray lookRay;
        public Vector2 lastLook;
        Vector2 lookDelta;
        void Update() {

            if (RedMatch.match.DebugInput()) return; ///////////////earlyreturn///////////////

            dt = Time.deltaTime;

            lookRay = new Ray(cameraTransform.position, cameraTransform.forward);

            if (itemHeld) {
                if (itemHeld.onHeld != null) itemHeld.onHeld(new InputAction.CallbackContext(), this);
            }

            if (blowingWhistle) tBlowingWhistle += dt;
            if (raisingCard) {
                tRaisingCard += dt;
                if (tRaisingCard > raisingCardThreshold) {
                    if (equipped == RefEquipment.YellowCard) RedMatch.ShowYellowCard(target);
                    else if (equipped == RefEquipment.RedCard) RedMatch.ShowRedCard(target);
                    tRaisingCard = 0f;
                }
            }

            if (zooming) {
                if (cam.fieldOfView > zoomed_fov) {
                    cam.fieldOfView -= zoom_speed * dt;
                    if (cam.fieldOfView < zoomed_fov) cam.fieldOfView = zoomed_fov;
                }
            }
            else if (cam.fieldOfView < normal_vert_fov) {
                cam.fieldOfView += zoom_speed * dt;
                if (cam.fieldOfView > normal_vert_fov) cam.fieldOfView = normal_vert_fov;
            }

            if (scanning || hud.wheel.preppedDialog) {

                float tempMaxDistance = 200f;
                if (Physics.Raycast(lookRay, out RaycastHit hit, tempMaxDistance)) {

                    if (hit.collider.TryGetComponent(out RefTarget hitTarget)) {
                        if (hitTarget != target) {
                            if (target) {
                                print("ref has new target: " + target.targetType);
                                if (target.outline) target.outline.SetActive(false);
                            }
                            target = hitTarget;
                            if (target.outline) target.outline.SetActive(true);
                        }
                    }
                    else {
                        if (target) {
                            //print("ref no longer targetting " + target.targetType);
                            if (target.outline) target.outline.SetActive(false);
                            target = null;
                        }
                    }
                }
                else {
                    if (target) {
                        print("ref no longer targetting " + target.targetType);
                        if (target.outline) target.outline.SetActive(false);
                        target = null;
                    }
                }
            }


            // growing foam
            for (int i = 0; i < growingBlobCount; ++i) {
                int blobIndex = (oldestGrowingBlobIndex + i) % foamBlobs.Length;
                FoamBlob blob = foamBlobs[blobIndex];
                float age = blob.age + dt;
                if (age > foamBlobLife) { //foamBlobGrowthTime) {
                    oldestGrowingBlobIndex = (oldestGrowingBlobIndex + 1) % foamBlobs.Length;
                    growingBlobCount--;
                }
                blob.transform.localScale = blob.targetScale * Vector3.one * foamBlobGrowthCurve.Evaluate(age / foamBlobGrowthTime);
                blob.age = age;
            }

            if (spraying) {
                if (Time.time - lastBlobSpawnedAt > sprayLife * 2) {
                    RedMatch.LineSprayed(sprayLine, sprayLineSegments);
                    sprayLine.Clear();
                }
            }
            else if (sprayLine.Count > 0) {
                if (Time.time - lastSprayedAt > sprayLife) {
                    RedMatch.LineSprayed(sprayLine, sprayLineSegments);
                    sprayLine.Clear();
                }
            }


            // check for interactibility? duffel bag for now
            bool showGrabbyHand = false;
            duffel = null;
            itemTouching = null;
            interactableTouching = null;
            if (equipped == RefEquipment.Barehand) {
                // extend range if looking up or down, helps with reaching things on floor
                float cos = Mathf.Cos(Mathf.Deg2Rad * cameraTransform.localEulerAngles.x);
                // caps cosrange at double itneractibility range
                float cosRange = interactibility_range / Mathf.Max(cos, interactibility_range / 2f);
                Physics.Raycast(lookRay, out RaycastHit hitInfo, cosRange);
                if (hitInfo.collider) {
                    if (hitInfo.collider.TryGetComponent(out duffel)) {
                        if (CanTakeSomething(duffel, out var _e)) {
                            showGrabbyHand = true;
                            hud.fToDrop.text = HUD.F_To_Grab;
                        }
                    }
                    else if (hitInfo.collider.TryGetComponent(out Item item) && item.isInteractable) {
                        if (!itemHeld) {
                            itemTouching = item;
                            showGrabbyHand = true;
                            hud.fToDrop.text = HUD.F_To_Grab;
                        }
                    }
                    // are we going to same frame issues where something is marked not interactable the moment it's interacted with?
                    else if (hitInfo.collider.TryGetComponent(out Interactable interactable) && interactable.isInteractable) {
                        if (!itemHeld) {
                            interactableTouching = interactable;
                            showGrabbyHand = true;
                            hud.fToDrop.text = HUD.F_To_Interact;
                        }
                    }
                }
            }
            if (showGrabbyHand) {
                crossHairs.gameObject.SetActive(false);
                hud.grabHand.gameObject.SetActive(true);
                hud.fToDrop.gameObject.SetActive(true);
            }
            else {
                if (!itemHeld) {
                    crossHairs.gameObject.SetActive(true);
                    hud.fToDrop.gameObject.SetActive(false);
                }
                hud.grabHand.gameObject.SetActive(false);
            }

            if (!indicatedTarget) {
                if (lookingWithArrows) {

                    // Smoothly accelerate lookVelocity toward arrowsInput
                    lookVelocity = Vector2.MoveTowards(lookVelocity, arrowsInput, acceleration * dt);

                    // Apply drag when input is near zero, to reduce velocity smoothly
                    if (arrowsInput.magnitude < 0.01f) {
                        lookVelocity = Vector2.MoveTowards(lookVelocity, Vector2.zero, arrowsDrag * dt);
                    }

                    yaw += lookVelocity.x * arrowLookSensitivity * dt;
                    pitch -= lookVelocity.y * arrowLookSensitivity * dt;
                    pitch = Mathf.Clamp(pitch, -90f, 90f); // Prevent flipping
                    cameraTransform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
                }
                else if (!debugLooking) cameraTransform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
            }
            else {

                // clamp and lerp towards indication
                // could use a spring effect 
                Vector3 toTarget = indicatedPosition - cameraTransform.position;
                float yawWanted = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
                float yawDiff = yawWanted - yaw;
                if (Mathf.Abs(yawDiff) > yawIndicatingTolerance) {
                    while (yawDiff > 180f) yawDiff -= 360f;
                    while (yawDiff < -180f) yawDiff += 360f;
                    yaw = yaw + yawSpring * yawDiff * dt;
                }
                cameraTransform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
            }
            var camMod = cameraTransform.rotation;
            var camMul = Quaternion.Euler(0, yaw, 0);
            var targetDirection = camMul * new Vector3(feetInput.x, 0, feetInput.y);
            moveDirection = Vector3.MoveTowards(moveDirection, targetDirection, acceleration * dt);

            // this needs to be after the looking, since we're clamping looking based on indicating
            if (indicating) ContinueIndicating();

            // Apply gravity
            if (controller.isGrounded && fallVelocity.y < 0)
                fallVelocity.y = -2f;

            fallVelocity.y += gravity * dt;

            float stepSpeed = joggingSpeed;
            if (dashing > 0f) {
                if (dashing <= dt) {
                    stepSpeed *= dashBurst * dashing;
                    dashing = 0f;
                }
                else {
                    stepSpeed *= dashBurst * dt;
                    dashing -= dt;
                }
            }

            if (pacingOneself) stepSpeed = pacingSpeedCap;
            else {
                if (movingBackwards) stepSpeed *= backwardMovementReduction;
                if (sprinting) stepSpeed *= sprintBoost;
                if ((int)equipped >= (int)RefEquipment.Book) stepSpeed *= movementEquipmentReduction;
            }
            if (controller.enabled) controller.Move((moveDirection * stepSpeed + fallVelocity) * dt);
            
            if (hud.debugSpeed) hud.debugSpeed.text = Vector3.Dot(controller.velocity, cameraTransform.forward).ToString("F2");

            float speed = controller.velocity.magnitude;
            float dHeartRate = 0f;
            float dGlycogen = 0f;
            if (speed <= 0f) {
                // resting
                // flush lactate slowly

                // lower heartrate fastest towards resting
                if (heartRate > standingHeartRate) dHeartRate = heartRateRecoveryDeceleration;
                else if (heartRate < standingHeartRate - 2f) dHeartRate = heartRateAcceleration;
            }
            else if (speed <= pacingSpeedCap) {
                // walking
                // move heartrate towards walking
                if (heartRate > walkingHeartRate) dHeartRate = heartRateRecoveryDeceleration;
                else if (heartRate < walkingHeartRate - 2f) dHeartRate = heartRateAcceleration;
            }
            else {
                // jogging
                // move heartrate towards jogging
                // inch heartrate up
                if (heartRate < joggingHeartRate - 2f) dHeartRate = heartRateAcceleration;
                else dHeartRate = heartRateCreepAcceleration;
            }
            dHeartRate *= dt * PER_MIN;
            heartRate += dHeartRate;
            if (heartRate > maxHeartRate) heartRate = maxHeartRate;

            float lactateDecay = 0f;
            float dLactate = 0f;
            if (heartRate > anaerobicThreshold) {
                hud.heartRate.color = Color.red;
                lactateDecay = Mathf.Pow(.5f, dt / activeLactateRecoveryHalfLife);
                dLactate += dt * fourLactateARecover; 
                //print("anaerobic lacate generated/min: " + (dLactate / dt).ToString("F8"));
            }
            else if (heartRate > aerobicThreshold) {
                hud.heartRate.color = Color.yellow;
                lactateDecay = Mathf.Pow(.5f, dt / activeLactateRecoveryHalfLife);
                dLactate += dt * twoLactateARecover; 
                //print("aerobic lacate generated/min: " + (dLactate / dt).ToString("F8"));
            }
            else if (heartRate > activeRecoveryThreshold) {
                hud.heartRate.color = Color.white;
                lactateDecay = Mathf.Pow(.5f, dt / activeLactateRecoveryHalfLife);
                dLactate += dt * Mathf.Lerp(oneLactateARecover, twoLactateARecover, (heartRate - activeRecoveryThreshold) / (aerobicThreshold - activeRecoveryThreshold));
                //print("active lacate generated/min: " + (dLactate / dt).ToString("F8"));
            }
            else {
                hud.heartRate.color = Colors.lime; // hm matching ui is fine i guesss
                lactateDecay = Mathf.Pow(.5f, (dt / restingLactateRecoveryHalfLife));
                dLactate += dt * Mathf.Lerp(oneLactateRRecover, oneLactateARecover, (heartRate - standingHeartRate) / (activeRecoveryThreshold - standingHeartRate));
                //print("resting lacate generated/min: " + (dLactate / dt).ToString("F8"));
            }
            float flushed = lactateDecay * lactateConcentration - lactateConcentration;
            //print("lactate flushed/min: " + (flushed / dt).ToString("F8"));
            dLactate += flushed;
            lactateConcentration += dLactate;

            if (lactateConcentration >= oblaThreshold) {
                // extremely high glyogen burn
                hud.lactate.color = Color.red;
                dGlycogen = -dt * PER_MIN * glycogenExtremeBurn;
                //print(hud.debugClock.text + ": max lactate");
            }
            else {
                if (lactateConcentration > 3f) hud.lactate.color = Color.yellow;
                else hud.lactate.color = Color.white;
                // print(hud.debugClock.text + ": lactate " + lactateConcentration);
                // medium or low glyogen burn
                dGlycogen = -dt * PER_MIN * Mathf.Lerp(glycogenBurnStanding, glycogenBurnJogging, (heartRate - standingHeartRate) / (joggingHeartRate - standingHeartRate));
            }
            glycogenStores += dGlycogen;

            // might want some kind of warning system
            if (glycogenStores < exhaustedThreshold) {
                // exhausted
                //exhausted = true;
                hud.glycogen.color = Color.red;
            }
            else if (glycogenStores < weaknessThrehold) {
                // indicate something
                hud.glycogen.color = Color.yellow;
            }

            // #OPTIMIZE cache all strings from 0 to 9999 or whatever
            string dLactatePerS = (dLactate/dt).ToString("F4");
            string dHeartRatePerS = (dHeartRate/dt).ToString("F4");
            string dGlycogenPerS = (dGlycogen/dt).ToString("F4");
            hud.heartRate.text = Mathf.RoundToInt(heartRate).ToString() + " bpm (" + dHeartRatePerS + ")";
            hud.lactate.text = lactateConcentration.ToString("F4") + " mmol/L (" + dLactatePerS + ")";
            hud.glycogen.text = Mathf.FloorToInt(glycogenStores).ToString() + " g (" + dGlycogenPerS + ")";

            if (watchRunning) {
                hundredths0 += 100f * dt;
                if (hundredths0 >= 100) {
                    hundredths0 -= 100;
                    seconds0++;
                    if (seconds0 >= 60) {
                        minutes0++;
                        seconds0 -= 60;
                    }
                }
                watchTimer.text = HUD.double_digit_strings[minutes0] + ":" +
                                    HUD.double_digit_strings[seconds0] + ":" +
                                    HUD.double_digit_strings[Mathf.FloorToInt(hundredths0)];
            }

            secondsElapsedSinceKickoff += dt;
        }


        private void RegisterInput(InputAction action, Action<InputAction.CallbackContext> callback) {
            if (actionRegistry.TryGetValue(action, out var list)) {
                list.Add(callback);
            }
            else actionRegistry.Add(action, new List<Action<InputAction.CallbackContext>> { callback });
        }

        private void OnDestroy() {
            // clean up the input shit
            foreach(var kvp in actionRegistry) {
                foreach(var v in kvp.Value) {
                    kvp.Key.started -= v;
                    kvp.Key.performed -= v;
                    kvp.Key.canceled -= v;
                }
            }
            actionRegistry.Clear();
        }
    }
} 