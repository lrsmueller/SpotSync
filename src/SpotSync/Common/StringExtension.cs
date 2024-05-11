namespace SpotSync.Common;

public static class StringExtension
{
    public static bool IsNullOrWhiteSpace(this string str)
    {

        if (str == null) return true;
        if (str.Length == 0) return true;
        if (str == string.Empty) return true;
        if (str == "") return true;
        return false;
    }

}
