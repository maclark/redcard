using UnityEngine;
using UnityEngine.InputSystem;


namespace RedCard {

    public class TitleScreen : MonoBehaviour {

        [Header("ASSIGNATIONS")]
        public Camera mainCamera;
        public Rigidbody rbWhistle;
        public Texture2D cursor;
        public MainMenu menu;

        [Header("WHISTLE")]
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
        private float countdownToStart = 0f;

        private void Awake() {
            MainMenu.ResetPrefs(); 
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;

            float x = cursor.width / 2f;
            float y = cursor.height / 2f;
            Cursor.SetCursor(cursor, new Vector2(x, y), CursorMode.Auto);
            Cursor.visible = false;
            usingMouse = false;
            
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
                menu.fadeOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(1f, 0f, countdownToStart / fadeOutDuration));
                if (countdownToStart <= 0f) {
                    // hoping scene 1 is always the tunnels
                    UnityEngine.SceneManagement.SceneManager.LoadScene(1);
                }
            }
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