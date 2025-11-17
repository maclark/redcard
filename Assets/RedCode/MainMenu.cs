using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using TMPro;

namespace RedCard {

    public class MainMenu : MonoBehaviour {

        [Header("ASSIGNATIONS")]
        public Camera mainCam;
        public Image fadeOverlay;
        public Texture2D cursor;
        [Header("TOP LEVEL")]
        public RectTransform rtTopLevel;
        public RectTransform rtSettings;
        public RectTransform rtCredits;
        public AudioClip selectedSound;
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
        public Slider sfxVolSlider;
        public Slider voicesSlider;
        public Slider musicSlider;
        public Button fullscreen;
        public TMP_Text fullscreenX;
        public Button vsync;
        public TMP_Text vsyncX;
        public AudioClip sliderSlidSound;
        public float sliderSoundGap = .05f;
        public float lastSliderSlidSoundPlayed;


        [Header("WHISTLE")]
        public Rigidbody rbWhistle;
        public AudioClip[] bumpedWhistles = new AudioClip[0];
        public float initialTorque = 1f;
        public float bumpPower = 10f;
        public float whistleStillThreshold = 1f;
        public float fadeOutDuration = .2f;


        private bool startPlaying = false;
        private bool usingMouse = false;
        private Vector2 lastMousePosition;
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
            Cursor.visible = false;
            usingMouse = false;

            rtTopLevel.gameObject.SetActive(true);
            rtSettings.gameObject.SetActive(false);
            rtCredits.gameObject.SetActive(false);
            fadeOverlay.gameObject.SetActive(false);

            playButton.onClick.AddListener(PlayGame);
            settingsButton.onClick.AddListener(OpenSettings);
            wishlistButton.onClick.AddListener(OpenWishlist);
            creditsButton.onClick.AddListener(ShowCredits);
            quitButton.onClick.AddListener(() => {
                AudioManager.PlaySFXOneShot(selectedSound);
                Application.Quit();
            });
            foreach (Button b in backs) b.onClick.AddListener(BackToMainMenu);

            sfxVolSlider.onValueChanged.AddListener(SlidSFX);
            voicesSlider.onValueChanged.AddListener(SlidVoices);
            musicSlider.onValueChanged.AddListener(SlidMusic);

            // separating sound effect from actually setting value
            sfxVolSlider.onValueChanged.AddListener(SlidSlider);
            voicesSlider.onValueChanged.AddListener(SlidSlider);
            musicSlider.onValueChanged.AddListener(SlidSlider);

            // initializing, if saved values
            if (PlayerPrefs.HasKey("SFXVolume")) SlidSFX(PlayerPrefs.GetFloat("SFXVolume"));
            else SlidSFX(.5f);
            if (PlayerPrefs.HasKey("VoicesVolume")) SlidVoices(PlayerPrefs.GetFloat("VoicesVolume"));
            else SlidVoices(.5f);
            if (PlayerPrefs.HasKey("MusicVolume")) SlidMusic(PlayerPrefs.GetFloat("MusicVolume"));
            else SlidMusic(.5f);

            vsync.onClick.AddListener(ToggleFullscreen);
            // default is full screen, so if key is not present or set to 1, we full screen
            if (!PlayerPrefs.HasKey("Fullscreen") || PlayerPrefs.GetInt("Fullscreen") == 1) {
                Screen.fullScreen = true;
                fullscreenX.text = "[X]";
            }
            else {
                Screen.fullScreen = false;
                fullscreenX.text = "[  ]";
            }

            vsync.onClick.AddListener(ToggleVsync);
            // default is vysnc on, so if no key present or we've set vsync to 1...
            if (!PlayerPrefs.HasKey("VSync") || PlayerPrefs.GetInt("Vsync") == 1) {
                QualitySettings.vSyncCount = 1;
                vsyncX.text = "[X]";
            }
            else {
                QualitySettings.vSyncCount = 0;
                vsyncX.text = "[  ]";
            }


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

            if (usingMouse) {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                // check for gamepad input?
            }
            else {
                if (Mouse.current != null) {
                    Vector2 mousePosition = Mouse.current.position.ReadValue();
                    if (mousePosition != lastMousePosition) {
                        usingMouse = true;
                        Cursor.visible = true;
                    }
                    lastMousePosition = mousePosition;
                }
            }

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

            if (startPlaying) {
                countdownToStart -= Time.deltaTime;
                fadeOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(1f, 0f, countdownToStart / fadeOutDuration));
                if (countdownToStart <= 0f) {
                    // hoping scene 1 is always the tunnels
                    UnityEngine.SceneManagement.SceneManager.LoadScene(1);
                }
            }
        }

        float countdownToStart = 0f;

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
                    if (bumpedWhistles.Length > 0) AudioManager.PlaySFXOneShot(bumpedWhistles[Random.Range(0, bumpedWhistles.Length)]);
                }
            }
        }


        private void PlayGame() {
            AudioManager.PlaySFXOneShot(selectedSound);
            countdownToStart = fadeOutDuration;
            startPlaying = true;
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.color = Color.clear;
        }

        private void OpenSettings() {
            AudioManager.PlaySFXOneShot(selectedSound);
            rtTopLevel.gameObject.SetActive(false);
            rtSettings.gameObject.SetActive(true);
            // full screen/full screen borderless/window
            // language
            // vsync
            // foul language/minced oaths/baby talkk
            // lingua
            // back
        }

        private void OpenWishlist() {
            //TBD
            AudioManager.PlaySFXOneShot(selectedSound);
            Application.OpenURL("https://almostinfinite.substack.com");
        }

        private void ShowCredits() {
            AudioManager.PlaySFXOneShot(selectedSound);
        }

        private void BackToMainMenu() {
            AudioManager.PlaySFXOneShot(selectedSound);
            rtTopLevel.gameObject.SetActive(true);
            rtCredits.gameObject.SetActive(false);
            rtSettings.gameObject.SetActive(false);
        }

        private void OnDestroy() {
            string mapName = BathroomMirror.MIRROR_ACTION_MAP;
            var action = PlayerInput.all[0].actions.FindActionMap(mapName).FindAction("PrimaryAction");
            if (action != null) {
                action.started -= ClickedOnWhistleMaybe;
            }
        }


        public static float DecibelsFrom01(float value) {
            if (value < 0 || value > 1) {
                Debug.LogWarning("out of range volume value " + value);
            }
            return Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        }

        private void SlidSlider(float value) {
            if (Time.time - lastSliderSlidSoundPlayed < sliderSoundGap) {
                AudioManager.PlaySFXOneShot(sliderSlidSound);
                lastSliderSlidSoundPlayed = Time.time;
            }
        }
        private void SlidSFX(float value) {
            PlayerPrefs.SetFloat("SFXVolume", value);
            AudioManager.am.mixer.SetFloat("SFXVolume", DecibelsFrom01(value));
        }
        private void SlidVoices(float value) {
            PlayerPrefs.SetFloat("VoicesVolume", value);
            AudioManager.am.mixer.SetFloat("VoicesVolume", DecibelsFrom01(value));
        }
        private void SlidMusic(float value) {
            PlayerPrefs.SetFloat("MusicVolume", value);
            AudioManager.am.mixer.SetFloat("MusicVolume", DecibelsFrom01(value));
        }

        private void ToggleVsync() {
            if (QualitySettings.vSyncCount == 0) {
                // vsync was off
                // turn it on!
                vsyncX.text = "[X]";
                QualitySettings.vSyncCount = 1;
                PlayerPrefs.SetInt("Vsync", 1);
            }
            else {
                vsyncX.text = "[  ]";
                QualitySettings.vSyncCount = 0;
                PlayerPrefs.SetInt("Vsync", 0);
            }
        }

        private void ToggleFullscreen() {
            if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen) {
                // we ware in full screen
                // go windowed
                Screen.fullScreen = false;
                fullscreenX.text = "[  ]";
                PlayerPrefs.SetInt("Fullscreen", 0);
            }
            else {
                Screen.fullScreen = true;
                fullscreenX.text = "[X]";
                PlayerPrefs.SetInt("Fullscreen", 1);
            }
        }
    }
}
