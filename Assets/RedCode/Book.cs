
using UnityEngine;
using UnityEngine.InputSystem;

namespace RedCard {

    public enum BookTitle {
        Rules,
        MensHealth,
        ProgressAndPoverty,
        Newspaper,
    }

    public interface IBookText {
        public void ShowPage(PrintingPress maker, int pageIndex);
        public int GetPageCount();
    }

    [RequireComponent(typeof(IBookText))]
    public class Book : MonoBehaviour {

        [Header("ASSIGNATIONS")]
        public LayerMask pageMask;
        public GameObject openBook;
        public GameObject closedBook;
        public Collider closedBookCollider;
        public Transform contents;
        public Transform bookmark;
        public Transform bookmarkHint;
        public MeshRenderer bookmarkRenderer;
        public Rigidbody rb;


        [Header("VARS")]
        public int pageIndex = 0;
        public int bookmarkIndex = 0;
        public RefControls reader;
        public IBookText bookText;

        private Ray lookRay;
        private BookPageButton lookingAt;



        private void Awake() {
            bookText = GetComponent<IBookText>();
            Debug.Assert(bookText != null);

            openBook.SetActive(false);
            closedBook.SetActive(true);
            rb.isKinematic = false;

            gameObject.layer = RefControls.Item_Layer;

            if (TryGetComponent(out Item it)) {
                it.onHeld += FollowGaze;
                it.onPrimary += ClickedWithBook;
                it.onSecondary += CloseBook;
                it.onGrabbed += Grabbed;
                it.onDropped += Dropped;
            }
            else Debug.LogError("book has no item attached?");
        }

        public void ShowPage() {
            // expect some kind of book object with an array of pages and we have page index
            if (pageIndex < 0 || pageIndex >= bookText.GetPageCount() - 1) {
                // remember, pageIndex is just for the left page shown
                Debug.LogError("pageIndex oob " + pageIndex);
                pageIndex = 0;
            }

            contents.gameObject.SetActive(pageIndex == 2);
            if (bookmarkIndex == pageIndex && bookmarkIndex > 2) {
                bookmarkRenderer.enabled = true;
            }
            else {
                bookmarkRenderer.enabled = false;
            }

            bookText.ShowPage(PrintingPress.press, pageIndex);
        }

        private bool FollowGaze(InputAction.CallbackContext _ctx, RefControls arbitro) {

            var was = lookingAt;
            if (openBook.gameObject.activeSelf) {
                lookRay = arbitro.cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(lookRay, out RaycastHit hit, 99f, pageMask)) {
                    lookingAt = hit.collider.GetComponent<BookPageButton>();
                }
                else lookingAt = null;
            }
            else lookingAt = null;

            // is this hacky? i don't know
            if (RedMatch.match.paused) lookingAt = null;

            if (was != lookingAt) {
                Texture2D cursor = arbitro.hud.cursor;
                Cursor.visible = true; //
                bookmarkHint.gameObject.SetActive(true);
                if (lookingAt) {
                    if (lookingAt.navigation == PageNavigation.FlipLeft) {
                        cursor = arbitro.hud.leftArrowCursor;
                    }
                    else if (lookingAt.navigation == PageNavigation.FlipRight) {
                        cursor = arbitro.hud.rightArrowCursor;
                    }
                    else if (lookingAt.navigation == PageNavigation.SetBookmark) {
                        Cursor.visible = false;
                        bookmarkHint.gameObject.SetActive(true);
                    }
                    else {
                        cursor = arbitro.hud.goToCursor;
                    }
                }

                float x = cursor.width / 2f;
                float y = cursor.height / 2f;
                Cursor.SetCursor(cursor, new Vector2(x, y), CursorMode.ForceSoftware);
            }
            // else was and lookingAt were either same nav or both null


            return false;
        }

        private bool ClickedWithBook(InputAction.CallbackContext ctx, RefControls arbitro) {
            if (!ctx.started) return false; ////// early return ///////// 

            if (openBook.gameObject.activeSelf) {
                if (lookingAt) {

                    int oldPageIndex = pageIndex;
                    print("clicked on page " + lookingAt.navigation);
                    switch (lookingAt.navigation) {
                        case PageNavigation.ContentsShortcut:
                            pageIndex = 2;
                            break;
                        case PageNavigation.SetBookmark:
                            if (pageIndex > 1) bookmarkIndex = pageIndex;
                            break;
                        case PageNavigation.JumpToBookmark:
                            pageIndex = bookmarkIndex;
                            break;
                        case PageNavigation.FlipLeft:
                            pageIndex -= 2;
                            if (pageIndex < 0) pageIndex = 0;
                            break;
                        case PageNavigation.FlipRight:
                            pageIndex += 2;
                            if (pageIndex >= bookText.GetPageCount()) {
                                pageIndex = bookText.GetPageCount() - 2;
                                CloseBook(new InputAction.CallbackContext(), arbitro);
                            }
                            break;
                        case PageNavigation.ChapterHeading:
                            pageIndex = lookingAt.number;
                            break;
                        default:
                            Debug.LogError("unhandled page navigation " + lookingAt.navigation);
                            break;
                    }

                    int pagesFlipped = Mathf.Abs(pageIndex - oldPageIndex);
                    // duh, if you flip page, index increases by two, bc you're viewing new spread!
                    if (pagesFlipped == 2) AudioManager.PlaySFXOneShot(PrintingPress.press.pageFlip);
                    else if (pagesFlipped > 2) AudioManager.PlaySFXOneShot(PrintingPress.press.pageFlipping);
                    ShowPage();
                }
            }
            else {

                AudioManager.PlaySFXOneShot(PrintingPress.press.bookOpening);
                openBook.gameObject.SetActive(true);
                closedBook.gameObject.SetActive(false);
                closedBookCollider.enabled = closedBook.gameObject.activeSelf;

                Cursor.visible = true;
                Cursor.lockState = (PlayerPrefs.GetInt(Menu.Prefs_UnconfineCursor) == 1) ? CursorLockMode.None : CursorLockMode.Confined;

                reader = arbitro;
                reader.canLookAround = false; // can move CURSOR around heh

                pageIndex = bookmarkIndex;
                ShowPage();
            }
            return true;
        }

        private bool CloseBook(InputAction.CallbackContext _ctx, RefControls arbitro) {

            if (!closedBook.gameObject.activeSelf) {
                AudioManager.PlaySFXOneShot(PrintingPress.press.bookClosing);
            }

            closedBook.gameObject.SetActive(true);
            closedBookCollider.enabled = closedBook.gameObject.activeSelf;
            openBook.gameObject.SetActive(false);

            // in case cursor was messed with
            Texture2D cursor = arbitro.hud.cursor;
            float x = cursor.width / 2f;
            float y = cursor.height / 2f;
            Cursor.SetCursor(cursor, new Vector2(x, y), CursorMode.ForceSoftware);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            if (reader) {
                reader.canLookAround = true;
                reader = null;
            }

            return true;
        }

        private bool Grabbed(InputAction.CallbackContext ctx, RefControls arbitro) {
            AudioManager.PlaySFXOneShot(PrintingPress.press.bookOpening);
            rb.isKinematic = true;
            transform.localRotation = Quaternion.Euler(-5f, 180f, 0f);
            foreach (Transform t in closedBook.transform) {
                t.gameObject.layer = RefControls.RefArms_Layer;
            }
            return false; // allowing GrabItem in RefControl to continue execution
        }

        private bool Dropped(InputAction.CallbackContext ctx, RefControls arbitro) {
            foreach (Transform t in closedBook.transform) {
                t.gameObject.layer = RefControls.Item_Layer;
            }
            CloseBook(ctx, arbitro);
            return false; // returning true would block further execution in RefControls
        }
    }

}
