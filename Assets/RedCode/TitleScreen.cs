using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;


namespace RedCard {

    public class TitleScreen : MonoBehaviour {

        public bool makingTrailer = false;

        [Header("ASSIGNATIONS")]
        public Camera mainCamera;
        public Rigidbody rbWhistle;
        public Menu menu;

        [Header("WISHLIST GLOW")]
        public float wishlistSpeed = 10f;
        public Color[] wishlistColors = new Color[0];
        public Color colorA;
        public Color colorB;
        public Color colorC;
        public Color colorFrom = Color.red;
        public Color colorTo = Color.yellow;
        public float colorPeriod = 1f;
        float tColor;

        [Header("WHISTLE")]
        public AudioClip[] bumpedWhistles = new AudioClip[0];
        public float initialTorque = 1f;
        public float bumpPower = 10f;
        public float whistleStillThreshold = 1f;
        public float fadeOutDuration = .2f;

        private bool startPlaying = false;
        private float whistleIdleThreshold;
        private float whistleIdleTorqueAmp;
        private float whistleIdleTorqueFrequency;
        private float tWhistleIdle = 0f;
        private Ray lookRay;
        private bool whistleIsIdle = false;
        private float countdownToStart = 0f;

        private void Awake() {
            Menu.ResetPrefs();


            Common.Init();
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;

            KnockWhistle(.5f * Vector3.right);
            rbWhistle.AddTorque(Vector3.up * initialTorque * (Random.value > .5f ? 1f : -1f), ForceMode.Impulse);

            menu.title = this;
            menu.OpenTopLevelMenu();
            if (makingTrailer) {
                menu.rtTitle.gameObject.SetActive(false);
                menu.discordButton.gameObject.SetActive(false);
            }

            PointerHandler phWishlist = menu.wishlistButton.gameObject.AddComponent<PointerHandler>();
            phWishlist.onEnter += (_data) => {
                colorPeriod = 1f;
                wishlistSpeed = 20f;
                menu.asoWishlist.Play();
            };
            phWishlist.onExit += (_data) => {
                colorPeriod = 5f;
                wishlistSpeed = 2.5f;
                menu.asoWishlist.Stop();
            };
        }

        private void Update() {

            if (makingTrailer && Keyboard.current.spaceKey.wasPressedThisFrame) {
                KnockWhistle(new Vector3(Random.value - .5f, Random.value - .5f, Random.value - .5f));
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
                menu.fadeOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(1f, 0f, countdownToStart / fadeOutDuration));
                if (countdownToStart <= 0f) {
                    // hoping scene 1 is always the tunnels
                    UnityEngine.SceneManagement.SceneManager.LoadScene(1);
                }
            }

            TMP_Text text = menu.wishlistTxt;
            text.ForceMeshUpdate();
            TMP_TextInfo textInfo = text.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++) {
                if (!textInfo.characterInfo[i].isVisible) continue;

                var vertexIndex = textInfo.characterInfo[i].vertexIndex;
                var meshIndex = textInfo.characterInfo[i].materialReferenceIndex;

                var colors = textInfo.meshInfo[meshIndex].colors32;

                Color32 c = Color.Lerp(colorFrom, colorTo,
                    Mathf.Sin(-Time.time * wishlistSpeed + i));

                colors[vertexIndex + 0] = c;
                colors[vertexIndex + 1] = c;
                colors[vertexIndex + 2] = c;
                colors[vertexIndex + 3] = c;
            }

            text.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

            tColor += Time.deltaTime;
            if (tColor > colorPeriod) {
                tColor -= colorPeriod;
                colorC = colorB;
                colorB = colorA;
                colorA = wishlistColors.GetRandom();
                int tries = 0;
                while (colorA == colorB || colorA == colorC) {
                    colorA = wishlistColors.GetRandom();
                    tries++;
                    if (tries > 99) break;
                }
            }

            colorTo = Color.Lerp(colorB, colorA, tColor / colorPeriod);
            colorFrom = Color.Lerp(colorC, colorB, tColor / colorPeriod);
            Color cwl = Color.Lerp(colorFrom, colorTo, .5f);
            menu.wishlistGlow.color = new Color(cwl.r, cwl.g, cwl.b, 1f);
        }


        private void KnockWhistle(Vector3 direction) {
            if (rbWhistle) {
                whistleIsIdle = false;
                rbWhistle.AddForce(bumpPower * direction, ForceMode.Impulse);
            }
            else Debug.LogWarning("clicking on a menuwhistle with no rb");
        }

        public void ClickedOnWhistleMaybe() {
            lookRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
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


        public void PlayGame() {
            countdownToStart = fadeOutDuration;
            startPlaying = true;
            menu.fadeOverlay.gameObject.SetActive(true);
            menu.fadeOverlay.color = Color.clear;
        }
    }

}