using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using TMPro;

namespace RedCard {

    public class Menu : MonoBehaviour {

        [Header("ASSIGNATIONS")]
        public Image fadeOverlay;
        public AudioClip selectedSound;
        public Texture2D cursor;

        [Header("TITLE MENU")]
        public RectTransform rtTitle;
        public RectTransform rtCredits;
        public Button settingsFromTitleButton;
        public Button creditsButton;
        public Button quitButton;
        public Button wishlistButton; // to leave review or something
        public TMP_Text wishlistTxt;
        public Image wishlistGlow;
        public Button discordButton;

        [Header("PAUSE MENU")]
        public RectTransform pauseUnderlay;
        public RectTransform rtPaused;
        public Button resume;
        public Button settingsFromPauseButton;

        [Header("PLAY, CONTINUE, NEW GAME")]
        public Button playButton;
        public Button yesContinueGame;
        public Button newGame;
        public TMP_Text playButtonTxt;

        [Header("SAVE & QUIT")]
        public RectTransform rtConfirmAndQuit;
        public Button saveAndQuit;
        public Button yesSaveAndQuit;
        public Button doNotSaveAndQuit;

        [Header("SETTINGS MENU")]
        public RectTransform rtSettings;
        public RectTransform rtGeneralSettings;
        public RectTransform rtControlsSettings;
        public Button[] backs = new Button[0];
        public RectTransform subcategoryHighlight;
        public Button generalSettingsButton;
        public TMP_Text generalSettingsTxt;
        public Button controlsSettingsButton;
        public TMP_Text controlsSettingsTxt;
        public TMP_Dropdown languageDropdown;
        public Image languageDropdownArrow;
        public float sliderSoundGap = .05f;
        public Slider sfxVolSlider;
        public Slider voicesSlider;
        public Slider musicSlider;
        public Button fullscreen;
        public TMP_Text fullscreenX;
        public Button vsync;
        public TMP_Text vsyncX;
        public RectTransform vulgarityHighlight;
        public RectTransform vulgaritySelectedBackground;
        public Button explicitLanguage;
        public TMP_Text explicitLangaugeTxt;
        public Button mincedOaths;
        public TMP_Text mincedOathsTxt;
        public Button momIsWatching;
        public TMP_Text momIsWatchingTxt;
        public AudioClip sliderSlidSound;
        public float lastSliderSlidSoundPlayed;
        Button vulgaritySelected;
        TMP_Text vulgaritySelectedTxt;
        Button vulgarityHighlighted;


        [Header("VARS")]
        public TitleScreen title;

        public const string Prefs_MusicVol = "MusicVolume";
        public const string Prefs_SFXVol = "SFXVolume";
        public const string Prefs_VoicesVol = "VoicesVolume";
        public const string Prefs_Fullscreen = "Fullscreen";
        public const string Prefs_Vsync = "Vsync";
        public const string Prefs_Vulgarity = "Vulgarity";


        // private variable declarations
        bool quittingToMain;
        float tQuitting = 0f;
        float quitFadeDuration = 1f;
        bool usingMouse = false;
        Vector2 lastMousePosition;
        bool cachedVisibleCursor;
        CursorLockMode cachedCursorLockMode;

        private void Awake() {

            // TITLE stuff
            settingsFromTitleButton.onClick.AddListener(OpenSettings);
            wishlistButton.onClick.AddListener(OpenWishlist);
            creditsButton.onClick.AddListener(ShowCredits);
            quitButton.onClick.AddListener(() => {
                AudioManager.PlaySFXOneShot(selectedSound);
                Application.Quit();
            });
            foreach (Button b in backs) {
                b.onClick.AddListener(() => AudioManager.PlaySFXOneShot(selectedSound));
                b.onClick.AddListener(OpenTopLevelMenu);
            }
            discordButton.onClick.AddListener(OpenDiscord);

            bool hasSaveGame = false;
            if (hasSaveGame) {
                playButtonTxt.text = "CONTINUE OR NEW GAME...";
            }
            else playButtonTxt.text = "PLAY";
            playButton.onClick.AddListener(CheckForSaveOrPlay);
            yesContinueGame.onClick.AddListener(ContinueGame);
            yesContinueGame.gameObject.SetActive(false);
            newGame.onClick.AddListener(PlayNewGame);
            newGame.gameObject.SetActive(false);
            rtConfirmAndQuit.gameObject.SetActive(false);

            // PAUSE menu
            resume.onClick.AddListener(ResumeGame);
            resume.onClick.AddListener(() => AudioManager.PlaySFXOneShot(selectedSound));
            settingsFromPauseButton.onClick.AddListener(OpenSettings);
            saveAndQuit.onClick.AddListener(() => {
                AudioManager.PlaySFXOneShot(selectedSound);
                rtConfirmAndQuit.gameObject.SetActive(true);
            });
            yesSaveAndQuit.onClick.AddListener(SaveAndQuitToMain);
            doNotSaveAndQuit.onClick.AddListener(() => {
                AudioManager.PlaySFXOneShot(selectedSound);
                rtConfirmAndQuit.gameObject.SetActive(false);
            });



            // SETTINGS MENU
            // needs to be made independent of the main menu
            // because we want to just reuse this same settings menu in-game, duh!
            // this isn't Flock of Dogs!!!

            rtGeneralSettings.gameObject.SetActive(true);
            rtControlsSettings.gameObject.SetActive(false);

            generalSettingsButton.onClick.AddListener(SelectedGeneralSettings);
            generalSettingsButton.onClick.AddListener(() => AudioManager.PlaySFXOneShot(selectedSound));
            controlsSettingsButton.onClick.AddListener(SelectedControlsSettings);
            controlsSettingsButton.onClick.AddListener(() => AudioManager.PlaySFXOneShot(selectedSound));
            SelectedGeneralSettings();

            languageDropdown.onValueChanged.AddListener((_value) => AudioManager.PlaySFXOneShot(selectedSound));
            PointerHandler phLanguage = languageDropdown.gameObject.AddComponent<PointerHandler>();
            phLanguage.onClick += (_data) => AudioManager.PlaySFXOneShot(selectedSound);
            phLanguage.onEnter += (_data) => {
                if (languageDropdown.IsInteractable()) { 
                    languageDropdown.captionText.color = Colors.blackish_green;
                    languageDropdownArrow.color = Colors.blackish_green;
                }
            };
            phLanguage.onExit += (_data) => {
                if (languageDropdown.IsInteractable()) {
                    languageDropdown.captionText.color = Colors.white;
                    languageDropdownArrow.color = Colors.white;
                }
            };

            sfxVolSlider.onValueChanged.AddListener(SlidSFX);
            voicesSlider.onValueChanged.AddListener(SlidVoices);
            musicSlider.onValueChanged.AddListener(SlidMusic);

            // separating sound effect from actually setting value
            sfxVolSlider.onValueChanged.AddListener(SlidSlider);
            voicesSlider.onValueChanged.AddListener(SlidSlider);
            musicSlider.onValueChanged.AddListener(SlidSlider);

            // not decibels
            float sfxVolSetting = .5f;
            if (PlayerPrefs.HasKey(Prefs_SFXVol)) sfxVolSetting = PlayerPrefs.GetFloat(Prefs_SFXVol);
            sfxVolSlider.SetValueWithoutNotify(sfxVolSetting);

            float voicesVolSetting = .5f;
            if (PlayerPrefs.HasKey(Prefs_VoicesVol)) voicesVolSetting = PlayerPrefs.GetFloat(Prefs_VoicesVol);
            voicesSlider.SetValueWithoutNotify(voicesVolSetting);

            float musicVolSetting = .5f;
            if (PlayerPrefs.HasKey(Prefs_MusicVol)) musicVolSetting = PlayerPrefs.GetFloat(Prefs_MusicVol);
            musicSlider.SetValueWithoutNotify(musicVolSetting);

            fullscreen.onClick.AddListener(ToggleFullscreen);
            // default is full screen, so if key is not present or set to 1, we full screen
            if (!PlayerPrefs.HasKey(Prefs_Fullscreen) || PlayerPrefs.GetInt(Prefs_Fullscreen) == 1) {
                Screen.fullScreen = true;
                fullscreenX.text = "x";
            }
            else {
                Screen.fullScreen = false;
                fullscreenX.text = "";
            }

            vsync.onClick.AddListener(ToggleVsync);
            // default is vysnc on, so if no key present or we've set vsync to 1...
            if (!PlayerPrefs.HasKey(Prefs_Vsync) || PlayerPrefs.GetInt(Prefs_Vsync) == 1) {
                QualitySettings.vSyncCount = 1;
                vsyncX.text = "x";
            }
            else {
                QualitySettings.vSyncCount = 0;
                vsyncX.text = "";
            }

            PointerHandler ph = explicitLanguage.gameObject.AddComponent<PointerHandler>();
            ph.onEnter += (data) => VulgarityHighlighted(Vulgarity.Explicit);
            ph.onExit += (data) => VulgarityDehighlighted(Vulgarity.Explicit);
            ph = mincedOaths.gameObject.AddComponent<PointerHandler>();
            ph.onEnter += (data) => VulgarityHighlighted(Vulgarity.MincedOaths);
            ph.onExit += (data) => VulgarityDehighlighted(Vulgarity.MincedOaths);
            ph = momIsWatching.gameObject.AddComponent<PointerHandler>();
            ph.onEnter += (data) => VulgarityHighlighted(Vulgarity.MomIsWatching);
            ph.onExit += (data) => VulgarityDehighlighted(Vulgarity.MomIsWatching);
            
            explicitLanguage.onClick.AddListener(ClickedExplicitLanguage);
            mincedOaths.onClick.AddListener(ClickedMincedOaths);
            momIsWatching.onClick.AddListener(ClickedMomIsWatching);
            Vulgarity v = Vulgarity.Explicit;
            if (PlayerPrefs.HasKey(Prefs_Vulgarity)) {
                v = (Vulgarity)PlayerPrefs.GetInt(Prefs_Vulgarity);
            }
            SelectVulgarity(v);
            vulgarityHighlight.gameObject.SetActive(false);
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
            if (quittingToMain) {
                tQuitting += Time.unscaledDeltaTime;
                fadeOverlay.color = Color.Lerp(Color.clear, Color.black, tQuitting / quitFadeDuration);
                if (tQuitting >= quitFadeDuration) {
                    Time.timeScale = 1f;
                    UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                }
            }
        }

        private void ClickedOnWhistleMaybe(InputAction.CallbackContext ctx) {
            if (title) title.ClickedOnWhistleMaybe();
        }

        private void Unpaused(InputAction.CallbackContext ctx) {
            if (!title) ResumeGame(); // silently?
        }

        private void CheckForSaveOrPlay() {
            if (title) {
                AudioManager.PlaySFXOneShot(selectedSound);
                bool hasSaveGame = false;
                if (hasSaveGame) {
                    playButton.gameObject.SetActive(false);
                    yesContinueGame.gameObject.SetActive(true);
                    newGame.gameObject.SetActive(true);
                }
                else title.PlayGame();
            }
            else {
                Debug.LogError("clicking on Play but we're not at title screen!");
            }
        }

        private void PlayNewGame() {
            if (title) {
                AudioManager.PlaySFXOneShot(selectedSound);
                bool hasSaveGame = false;
                if (hasSaveGame) {
                    // delete it!
                    // #TODO
                    Debug.LogWarning("delete save game");
                }

                title.PlayGame();
            }
            else Debug.LogError("clicking on Play but we're not at title screen!");
        }

        private void ContinueGame() {
            if (title) {
                AudioManager.PlaySFXOneShot(selectedSound);
                Debug.LogWarning("continue save game");
                title.PlayGame();
            }
            else Debug.LogError("clicking on Play but we're not at title screen!");
        }

        private void SaveAndQuitToMain() {
            Debug.LogWarning("fade out and load title scene");
            AudioManager.PlaySFXOneShot(selectedSound);
            RedMatch.match.Save();
            quittingToMain = true;
            tQuitting = 0f;
            fadeOverlay.color = Color.clear;
            fadeOverlay.gameObject.SetActive(true);
        }

        private void ResumeGame() {
            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }

        private void OpenSettings() {
            AudioManager.PlaySFXOneShot(selectedSound);
            rtTitle.gameObject.SetActive(false);
            rtPaused.gameObject.SetActive(false);
            rtSettings.gameObject.SetActive(true);
            rtCredits.gameObject.SetActive(false);
        }


        private void SelectedGeneralSettings() {
            subcategoryHighlight.anchoredPosition = new Vector2(-Mathf.Abs(subcategoryHighlight.anchoredPosition.x), subcategoryHighlight.anchoredPosition.y);
            rtGeneralSettings.gameObject.SetActive(true);
            rtControlsSettings.gameObject.SetActive(false);
        }

        private void SelectedControlsSettings() {
            subcategoryHighlight.anchoredPosition = new Vector2(Mathf.Abs(subcategoryHighlight.anchoredPosition.x), subcategoryHighlight.anchoredPosition.y);
            rtGeneralSettings.gameObject.SetActive(false);
            rtControlsSettings.gameObject.SetActive(true);
        }

        private void OpenWishlist() {
            AudioManager.PlaySFXOneShot(selectedSound);
            Application.OpenURL("https://almostinfinite.substack.com");
        }

        private void ShowCredits() {
            AudioManager.PlaySFXOneShot(selectedSound);
        }

        private void OpenDiscord() {
            AudioManager.PlaySFXOneShot(selectedSound);
            Application.OpenURL("https://discord.gg/YFJNMnVXMN");
        }

        public void OpenTopLevelMenu() {
            if (title) {
                rtTitle.gameObject.SetActive(true);
                rtPaused.gameObject.SetActive(false);
                rtSettings.gameObject.SetActive(false);
                rtCredits.gameObject.SetActive(false);
                fadeOverlay.gameObject.SetActive(false);
                pauseUnderlay.gameObject.SetActive(false);
                discordButton.transform.SetParent(transform);
                discordButton.transform.SetAsFirstSibling();
            }
            else {
                rtTitle.gameObject.SetActive(false);
                rtPaused.gameObject.SetActive(true);
                rtSettings.gameObject.SetActive(false);
                rtCredits.gameObject.SetActive(false);
                fadeOverlay.gameObject.SetActive(false);
                pauseUnderlay.gameObject.SetActive(true);
            }
        }



        public static float DecibelsFrom01(float value) {
            if (value < 0 || value > 1) {
                Debug.LogWarning("out of range volume value " + value);
            }
            return Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        }

        private void SlidSlider(float value) {
            print("slid slider");
            if (Time.time - lastSliderSlidSoundPlayed > sliderSoundGap) {
                AudioManager.PlaySFXOneShot(sliderSlidSound);
                lastSliderSlidSoundPlayed = Time.time;
            }
        }

        private void SlidSFX(float value) {
            Debug.Log("sliding sfx");
            PlayerPrefs.SetFloat(Prefs_SFXVol, value);
            AudioManager.am.mixer.SetFloat(Prefs_SFXVol, DecibelsFrom01(value));
        }

        private void SlidVoices(float value) {
            print("sliding voices");
            PlayerPrefs.SetFloat(Prefs_VoicesVol, value);
            AudioManager.am.mixer.SetFloat(Prefs_VoicesVol, DecibelsFrom01(value));
        }

        private void SlidMusic(float value) {
            print("sliding music");
            PlayerPrefs.SetFloat(Prefs_MusicVol, value);
            AudioManager.am.mixer.SetFloat(Prefs_MusicVol, DecibelsFrom01(value));
        }

        public static void ResetPrefs() {
            print("reset settings preferences");
            PlayerPrefs.DeleteKey(Prefs_MusicVol);
            PlayerPrefs.DeleteKey(Prefs_SFXVol);
            PlayerPrefs.DeleteKey(Prefs_VoicesVol);
            PlayerPrefs.DeleteKey(Prefs_Fullscreen);
            PlayerPrefs.DeleteKey(Prefs_Vsync);
            PlayerPrefs.DeleteKey("Vsycn"); // temporary
            PlayerPrefs.DeleteKey(Prefs_Vulgarity);

            Debug.LogWarning("#TODO reset ref arm preferences");
        }

        private void ToggleVsync() {
            AudioManager.am.sfxAso.PlayOneShot(selectedSound);
            if (QualitySettings.vSyncCount == 0) {
                // vsync was off
                // turn it on!
                vsyncX.text = "x";
                QualitySettings.vSyncCount = 1;
                PlayerPrefs.SetInt(Prefs_Vsync, 1);
            }
            else {
                vsyncX.text = "";
                QualitySettings.vSyncCount = 0;
                PlayerPrefs.SetInt(Prefs_Vsync, 0);
            }
        }

        private void ToggleFullscreen() {
            AudioManager.am.sfxAso.PlayOneShot(selectedSound);
            if (Screen.fullScreen) {
                // we are in full screen
                // go windowed
                Screen.fullScreen = false;
                fullscreenX.text = "";
                PlayerPrefs.SetInt(Prefs_Fullscreen, 0);
            }
            else {
                Screen.fullScreen = true;
                fullscreenX.text = "x";
                PlayerPrefs.SetInt(Prefs_Fullscreen, 1);
            }
        }

        private void VulgarityHighlighted(Vulgarity v) {
            Button b = explicitLanguage;
            TMP_Text txt = explicitLangaugeTxt;
            switch (v) {
                case Vulgarity.Explicit:
                    break;
                case Vulgarity.MincedOaths:
                    b = mincedOaths;
                    txt = mincedOathsTxt;
                    break;
                case Vulgarity.MomIsWatching:
                    b = momIsWatching;
                    txt = momIsWatchingTxt;
                    break;
                default:
                    Debug.LogError("unhandled vulgarity " + v);
                    break;
            }

            if (b.TryGetComponent(out RectTransform rt)) {
                vulgarityHighlighted = b;
                vulgarityHighlight.gameObject.SetActive(true);
                vulgarityHighlight.anchoredPosition = new Vector2(vulgarityHighlight.anchoredPosition.x, rt.anchoredPosition.y);

                if (vulgarityHighlighted == vulgaritySelected) txt.color = Colors.lime;
                else txt.color = Colors.blackish_green;
                if (!txt.text.StartsWith("> ")) txt.text = "> " + txt.text;
            }
        }

        private void VulgarityDehighlighted(Vulgarity v) {
            Button b = explicitLanguage;
            TMP_Text txt = explicitLangaugeTxt;
            switch (v) {
                case Vulgarity.Explicit:
                    break;
                case Vulgarity.MincedOaths:
                    b = mincedOaths;
                    txt = mincedOathsTxt;
                    break;
                case Vulgarity.MomIsWatching:
                    b = momIsWatching;
                    txt = momIsWatchingTxt;
                    break;
                default:
                    Debug.LogError("unhandled vulgarity " + v);
                    break;
            }

            // if we're dehighlighting the highlighted vulgarity, we hide vulgarity highlight
            if (b.TryGetComponent(out RectTransform rt)) {
                if (b == vulgarityHighlighted) {
                    vulgarityHighlight.gameObject.SetActive(false);
                }

                // could we be unhighlighting something that wasn't highlighted? doesn't matter?
                if (txt.text.StartsWith("> ")) txt.text = txt.text.Substring(2);
                if (txt == vulgaritySelectedTxt) {
                    txt.color = Color.white;
                }
                else txt.color = Colors.blackish_green;
            }
        }

        public void SelectVulgarity(Vulgarity v) {
            Button b = explicitLanguage;
            TMP_Text txt = explicitLangaugeTxt;
            if (vulgaritySelected) {
                vulgaritySelectedTxt.color = Colors.blackish_green;
            }

            switch (v) {
                case Vulgarity.Explicit:
                    vulgaritySelected = explicitLanguage;
                    vulgaritySelectedTxt = explicitLangaugeTxt;
                    break;
                case Vulgarity.MincedOaths:
                    vulgaritySelected = mincedOaths;
                    vulgaritySelectedTxt = mincedOathsTxt;
                    break;
                case Vulgarity.MomIsWatching:
                    vulgaritySelected = momIsWatching;
                    vulgaritySelectedTxt = momIsWatchingTxt;
                    break;
                default:
                    Debug.LogError("unhandled vulgarity " + v);
                    break;
            }

            if (vulgaritySelected != vulgarityHighlighted) vulgaritySelectedTxt.color = Color.white;
            else vulgaritySelectedTxt.color = Colors.lime; // in case we clicked on highlighted text
            if (vulgaritySelected.TryGetComponent(out RectTransform rt)) {
                vulgaritySelectedBackground.anchoredPosition = new Vector2(vulgaritySelectedBackground.anchoredPosition.x, rt.anchoredPosition.y);
            }

            PlayerPrefs.SetInt(Prefs_Vulgarity, (int)v);
        }

        private void ClickedExplicitLanguage() {
            AudioManager.am.sfxAso.PlayOneShot(selectedSound);
            SelectVulgarity(Vulgarity.Explicit);
        }

        private void ClickedMincedOaths() {
            AudioManager.am.sfxAso.PlayOneShot(selectedSound);
            SelectVulgarity(Vulgarity.MincedOaths);
        }

        private void ClickedMomIsWatching() {
            AudioManager.am.sfxAso.PlayOneShot(selectedSound);
            SelectVulgarity(Vulgarity.MomIsWatching);
        }

        private void OnEnable() {
            string mapName = RefereeeCustomizer.MIRROR_ACTION_MAP;
            RedMatch.AssignMap(mapName);
            var action = PlayerInput.all[0].actions.FindActionMap(mapName).FindAction("PrimaryAction");
            if (action != null) {
                action.started += ClickedOnWhistleMaybe;
            }

            action = PlayerInput.all[0].actions.FindActionMap(mapName).FindAction("Pause");
            if (action != null) {
                action.started += Unpaused;
            }

            float x = cursor.width / 2f;
            float y = cursor.height / 2f;
            Cursor.SetCursor(cursor, new Vector2(x, y), CursorMode.Auto);

            cachedVisibleCursor = Cursor.visible;
            cachedCursorLockMode = Cursor.lockState;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        
        private void OnDisable() {
            string mapName = RefereeeCustomizer.MIRROR_ACTION_MAP;
            if (PlayerInput.all.Count > 0) {
                var action = PlayerInput.all[0].actions.FindActionMap(mapName).FindAction("PrimaryAction");
                if (action != null) {
                    action.started -= ClickedOnWhistleMaybe;
                }
            }
            RedMatch.AssignMap(RedMatch.REFEREEING_ACTION_MAP);

            Cursor.visible = cachedVisibleCursor;
            Cursor.lockState = cachedCursorLockMode;
        }
    }
}
