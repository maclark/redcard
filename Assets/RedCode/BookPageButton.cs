using UnityEngine;

namespace RedCard {

    public enum PageNavigation {
        ContentsShortcut,
        FlipLeft,
        FlipRight,
        SetBookmark,
        JumpToBookmark,
        ChapterHeading,
    }

    public class BookPageButton : MonoBehaviour {
        public PageNavigation navigation;
        public int number = -1;
    }
}
