using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace RedCard {

    public class MainMenu : MonoBehaviour {

        public Camera mainCam;
        public Rigidbody rbWhistle;
        public float bumpPower = 10f;
        public Texture2D cursor;

        private Ray lookRay;

        private void Awake() {

            float x = cursor.width / 2f;
            float y = cursor.height / 2f;
            Cursor.SetCursor(cursor, new Vector2(x, y), CursorMode.Auto);

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
        }

        private void ClickedOnWhistleMaybe(InputAction.CallbackContext ctx) {
            lookRay = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(lookRay, out RaycastHit hit, 99f)) {
                if (hit.collider.TryGetComponent(out MenuWhistle whistle)) {
                    Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                    randomDir.Normalize();
                    whistle.rb.AddForce(bumpPower * randomDir, ForceMode.Impulse);
                }
            }
        }
    }
}
