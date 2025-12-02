
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
        public void ShowPage(BookMaker maker, int pageIndex);
        public int GetPageCount();
    }

    [RequireComponent(typeof(IBookText))]
    public class Book : MonoBehaviour {

        [Header("ASSIGNATIONS")]
        public LayerMask pageMask;
        public GameObject openBook;
        public GameObject closedBook;
        public Transform contents;
        public Transform bookmark;
        public MeshRenderer bookmarkRenderer;
        public Rigidbody rb;


        [Header("VARS")]
        public int pageIndex = 0;
        public int bookmarkIndex = 0;
        public RefControls reader;
        public IBookText bookText;


        private void Awake() {
            bookText = GetComponent<IBookText>();
            Debug.Assert(bookText != null);

            openBook.SetActive(false);
            closedBook.SetActive(true);
            rb.isKinematic = false;

            gameObject.layer = RefControls.Item_Layer;

            if (TryGetComponent(out Item it)) {
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

            bookText.ShowPage(BookMaker.maker, pageIndex);
        }

        private bool ClickedWithBook(InputAction.CallbackContext ctx, RefControls arbitro) {
            if (!ctx.started) return false; ////// early return ///////// 

            if (openBook.gameObject.activeSelf) {
                // let's raycast! see what we hit!
                Ray lookRay = arbitro.cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(lookRay, out RaycastHit hit, 99f, pageMask)) {
                    if (hit.collider.TryGetComponent(out BookPageButton page)) {
                        print("clicked on page " + page.navigation);
                        switch (page.navigation) {
                            case PageNavigation.ContentsShortcut:
                                pageIndex = 1;
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
                                    CloseBook(new InputAction.CallbackContext(), null);
                                }
                                break;
                            case PageNavigation.ChapterHeading:
                                pageIndex = page.chapterPageStart;
                                break;
                            default:
                                Debug.LogError("unhandled page navigation " + page.navigation);
                                break;
                        }

                        ShowPage();
                    }
                }
            }
            else {

                openBook.gameObject.SetActive(true);
                closedBook.gameObject.SetActive(false);

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
            closedBook.gameObject.SetActive(true);
            openBook.gameObject.SetActive(false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            if (reader) {
                reader.canLookAround = true;
                reader = null;
            }

            return true;
        }

        private bool Grabbed(InputAction.CallbackContext ctx, RefControls arbitro) {
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
