using UnityEngine;
using UnityEngine.InputSystem;

namespace RedCard {

    public class Book : MonoBehaviour {
        public GameObject openBook;
        public GameObject closedBook;
        public Rigidbody rb;

        private void Awake() {
            openBook.SetActive(false);
            closedBook.SetActive(true);
            rb.isKinematic = false;

            gameObject.layer = RefControls.Item_Layer;

            if (TryGetComponent(out Item it)) {
                it.onPrimary += OpenBook;
                it.onSecondary += CloseBook;
                it.onDropped += Dropped;
            }
            else Debug.LogError("book has no item attached?");
        }

        private bool OpenBook(InputAction.CallbackContext ctx, RefControls arbitro) {
            if (openBook.gameObject.activeSelf) {
                // let's raycast! see what we hit!
                Ray lookRay = arbitro.cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(lookRay, out RaycastHit hit, 99f)) {
                    if (hit.collider.TryGetComponent(out BookPage page)) {
                        if (page.side == Chirality.Left) {
                            print("clicked left side");
                        }
                        else if (page.side == Chirality.Right) {
                            print("clicked right side");
                        }
                    }
                }
                return true; /////earlyreturn////
            }

            openBook.gameObject.SetActive(true);
            closedBook.gameObject.SetActive(false);

            RedMatch.AssignMap(RefereeCustomizer.UI_MAP);

            Cursor.visible = true;
            Cursor.lockState = (PlayerPrefs.GetInt(Menu.Prefs_UnconfineCursor) == 1) ? CursorLockMode.None : CursorLockMode.Confined;

            return true;
        }

        private bool CloseBook(InputAction.CallbackContext ctx, RefControls arbitro) {
            closedBook.gameObject.SetActive(false);
            openBook.gameObject.SetActive(true);

            RedMatch.AssignMap(RedMatch.REFEREEING_ACTION_MAP);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            return true;
        }

        private bool Dropped(InputAction.CallbackContext ctx, RefControls arbitro) {
            return CloseBook(new InputAction.CallbackContext(), arbitro);
        }
    }

}
