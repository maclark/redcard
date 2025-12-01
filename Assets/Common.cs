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
    
}
