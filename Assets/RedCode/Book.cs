
using UnityEngine;
using UnityEngine.InputSystem;

namespace RedCard {

    public enum BookTitle {
        Rules,
        MensHealth,
        ProgressAndPoverty,
        Newspaper,
    }

    [System.Serializable]
    public class BookText {
        public BookTitle title;
        public string[] pages = new string[2];
    }

    public class Book : MonoBehaviour {

        [Header("ASSIGNATIONS")]
        public LayerMask pageMask;
        public GameObject openBook;
        public GameObject closedBook;
        public Transform contents;
        public Transform bookmark;
        public MeshRenderer bookmarkRenderer;
        public Rigidbody rb;

        [Header("SETTINGS")]
        public BookText bookText;


        [Header("VARS")]
        public int pageIndex = 0;
        public int bookmarkIndex = 0;
        public RefControls reader;


        private void Awake() {
            Debug.Assert(bookText != null);

            openBook.SetActive(false);
            closedBook.SetActive(true);
            rb.isKinematic = false;

            gameObject.layer = RefControls.Item_Layer;

            if (TryGetComponent(out Item it)) {
                it.onPrimary += ClickedWithBook;
                it.onSecondary += CloseBook;
                it.onDropped += Dropped;
            }
            else Debug.LogError("book has no item attached?");
        }

        public void ShowPage() {
            // expect some kind of book object with an array of pages and we have page index
            if (pageIndex < 0 || pageIndex >= bookText.pages.Length) {
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

            BookMaker.maker.WritePages(pageIndex, bookText.pages[pageIndex], bookText.pages[pageIndex + 1], null);
            print("showing page " + pageIndex + " of book " + bookText.title);
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
                                if (pageIndex >= bookText.pages.Length) {
                                    pageIndex = bookText.pages.Length - 2;
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
                reader.freeLook = false;

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
                reader.freeLook = true;
                reader = null;
            }

            return true;
        }

        private bool Dropped(InputAction.CallbackContext ctx, RefControls arbitro) {
            print("dropping book");
            CloseBook(ctx, arbitro);
            return false; // returning true would block further execution in RefControls
        }
    }

}
