using UnityEngine;


public static class Common
{
    public static string[] int_strings = new string[1000];

    private static bool initialized = false;

    public static void Init() {
        if (initialized) return;
        initialized = true;

        for (int i = 0; i < int_strings.Length; i++) {
            int_strings[i] = i.ToString();
        }
    }

    public static string ToRomanLower(this int number) {

        if (number < 0) {
            Debug.LogError("Roman numeral requested for a negative number.");
            return string.Empty;
        }

        if (number == 0)
            return "n"; // Romans sometimes used N for zero, optional.

        // Standard Roman numeral mappings
        (int value, string numeral)[] map = new (int, string)[]
            {
            (1000, "m"),
            (900,  "cm"),
            (500,  "d"),
            (400,  "cd"),
            (100,  "c"),
            (90,   "xc"),
            (50,   "l"),
            (40,   "xl"),
            (10,   "x"),
            (9,    "ix"),
            (5,    "v"),
            (4,    "iv"),
            (1,    "i")
        };

        int remaining = number;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var (value, numeral) in map) {
            while (remaining >= value) {
                sb.Append(numeral);
                remaining -= value;
            }
        }

        return sb.ToString();
    }

    public static string ToRomanUpper(this int number) {
        if (number < 0) {
            Debug.LogError("Roman numeral requested for a negative number.");
            return string.Empty;
        }

        if (number == 0)
            return "N"; // Romans sometimes used N for zero, optional.

        // Standard Roman numeral mappings
        (int value, string numeral)[] map = new (int, string)[]
        {
            (1000, "M"),
            (900,  "CM"),
            (500,  "D"),
            (400,  "CD"),
            (100,  "C"),
            (90,   "XC"),
            (50,   "L"),
            (40,   "XL"),
            (10,   "X"),
            (9,    "IX"),
            (5,    "V"),
            (4,    "IV"),
            (1,    "I")
        };

        int remaining = number;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var (value, numeral) in map) {
            while (remaining >= value) {
                sb.Append(numeral);
                remaining -= value;
            }
        }

        return sb.ToString();
    }
}
