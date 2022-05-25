using Newtonsoft.Json.Linq;

namespace WakaBot.Extensions;

public static class JArrayExtension
{
    public static string ConcatForEach(this JArray array, Func<dynamic, bool, string> fetch)
    {
        string result = string.Empty;

        for (int i = 0; i < array.Count; i++)
        {
            result += fetch(array[i], i >= array.Count - 1);
        }

        return result;
    }

    public static string ConcatForEach(this JArray array, int limit, Func<dynamic, bool, string> fetch)
    {
        string result = string.Empty;

        for (int i = 0; i < array.Count && i < limit; i++)
        {
            result += fetch(array[i], i >= limit - 1 || i >= array.Count - 1);
        }

        return result;
    }
}