using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace RedCard {

    public class MainMenu : MonoBehaviour {

        [Header("ASSIGNATIONS")]
        public Camera mainCam;
        public Rigidbody rbWhistle;
        public Texture2D cursor;
        public AudioClip[] bumpedWhistles = new AudioClip[0];
        public RectTransform rtSettings;
        public RectTransform rtTopLevel;
        public RectTransform rtCredits;
        public Button playButton;
        public Button settingsButton;
        public Button creditsButton;
        public Button quitButton;
        public Button wishlistButton; // to leave review or something
        public Button discordButton;
        public Button twitchButton;
        public Button tikTokButton;
        public Button youTubeButton;
        public Button[] backs = new Button[0];


        [Header("SETTINGS")]
        public float initialTorque = 1f;
        public float bumpPower = 10f;
        public float whistleStillThreshold = 1f;


        private float whistleIdleThreshold;
        private float whistleIdleTorqueAmp;
        private float whistleIdleTorqueFrequency;
        private float tWhistleIdle = 0f;
        private Ray lookRay;
        private bool whistleIsIdle = false;


        private void Awake() {

            float x = cursor.width / 2f;
            float y = cursor.height / 2f;
            Cursor.SetCursor(cursor, new Vector2(x, y), CursorMode.Auto);


            playButton.onClick.AddListener(PlayGame);
            settingsButton.onClick.AddListener(OpenSettings);
            wishlistButton.onClick.AddListener(OpenWishlist);
            creditsButton.onClick.AddListener(ShowCredits);
            quitButton.onClick.AddListener(() => Application.Quit());
            foreach (Button b in backs) b.onClick.AddListener(BackToMainMenu);

            ReadOnlyArray<PlayerInput> allInput = PlayerInput.all;
            string mapName = BathroomMirror.MIRROR_ACTION_MAP;
            foreach (PlayerInput input in allInput) {
                InputActionMap map = input.actions.FindActionMap(mapName);
                if (map != null) input.SwitchCurrentActionMap(mapName);
                else Debug.LogError("can't find map: " + mapName);
            }
            var action = PlayerInput.all[0].actions.FindActionMap(mapName).FindAction("PrimaryAction");
            if (action != null) {
                action.started += ClickedOnWhistleMaybe;
            }

            KnockWhistle(.5f * Vector3.right);
            rbWhistle.AddTorque(Vector3.up * initialTorque * (Random.value > .5f ? 1f : -1f), ForceMode.Impulse);
        }

        private void Update() {
            print("rbWhistle.sqrAngularVelocity " + rbWhistle.angularVelocity.sqrMagnitude);
            if (!whistleIsIdle && rbWhistle.angularVelocity.sqrMagnitude < whistleStillThreshold) {
                tWhistleIdle += Time.deltaTime;
                if (tWhistleIdle > whistleIdleThreshold) {
                    whistleIsIdle = true;
                    whistleIdleThreshold = Random.Range(1f, 5f);
                    // one out of 5, triple the stillness!
                    if (Random.value > .8f) whistleIdleThreshold *= 3f;
                    whistleIdleTorqueAmp = Random.Range(5, 30) * 0.0001f;
                    whistleIdleTorqueFrequency = Random.Range(.25f, .67f);
                }
            }
            else tWhistleIdle = 0f;

            if (whistleIsIdle) {
                rbWhistle.AddTorque(Vector3.up * whistleIdleTorqueAmp * Mathf.Cos(Time.time * whistleIdleTorqueFrequency), ForceMode.Force);
            }
        }

        private void KnockWhistle(Vector3 direction) {
            if (rbWhistle) {
                whistleIsIdle = false;
                rbWhistle.AddForce(bumpPower * direction, ForceMode.Impulse);
            }
            else Debug.LogWarning("clicking on a menuwhistle with no rb");
        }

        private void ClickedOnWhistleMaybe(InputAction.CallbackContext ctx) {
            lookRay = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(lookRay, out RaycastHit hit, 99f)) {
                if (hit.collider.TryGetComponent(out MenuWhistle whistle)) {
                    Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                    randomDir += lookRay.direction;
                    randomDir.Normalize();
                    KnockWhistle(randomDir);
                    if (bumpedWhistles.Length > 0) AudioManager.PlaySFX(bumpedWhistles[Random.Range(0, bumpedWhistles.Length)]);
                }
            }
        }


        private void PlayGame() {

        }


        private void OpenSettings() {
            // full screen/full screen borderless/window
            // language
            // vsync
            // foul language/minced oaths/baby talkk
            // lingua
            // back
        }

        private void OpenWishlist() {
            //TBD
            Application.OpenURL("https://almostinfinite.substack.com");
        }

        private void ShowCredits() {

        }

        private void BackToMainMenu() {
            rtTopLevel.gameObject.SetActive(true);
            rtCredits.gameObject.SetActive(false);
            rtSettings.gameObject.SetActive(false);
        }
    }
}
