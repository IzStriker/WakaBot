namespace WakaBot.Extensions;

public static class ListExtension
{
    public static string ConcatForEach<T>(this List<T> enumerable, Func<T, bool, string> fetch)
    {
        string result = string.Empty;

        for (int i = 0; i < enumerable.Count(); i++)
        {
            result += fetch(enumerable[i]!, i >= enumerable.Count() - 1);
        }

        return result;
    }

    public static string ConcatForEach<T>(this List<T> array, int limit, Func<T, bool, string> fetch)
    {
        string result = string.Empty;

        for (int i = 0; i < array.Count && i < limit; i++)
        {
            result += fetch(array[i]!, i >= limit - 1 || i >= array.Count - 1);
        }

        return result;
    }
}