using System.Collections.Generic;

namespace RedCard {

    public enum Words {
        IsDominantChecked,
        IsDominantUnchecked,
    }

    public static class Language {

        public static Dictionary<Words, string> current = new Dictionary<Words, string>();

        public static Dictionary<Words, string> english = new() {
            { Words.IsDominantChecked, "is dominant [x]"},
            { Words.IsDominantUnchecked, "is dominant [  ]"},
        };

    }
}
