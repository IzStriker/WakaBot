using Newtonsoft.Json.Linq;

namespace WakaBot.Core.Extensions;

/// <summary>
/// Extensions to Newtonsoft.Json.Linq.JArray classes functionality.
/// </summary>
public static class JArrayExtension
{
    /// <summary>
    /// Runs for each element in array, concatenates result of specified delegate to returned string.
    /// </summary>
    /// <param name="array">The object this method is called on.</param>
    /// <param name="fetch">Delegate for what should be concatenated in each iteration.</param>
    /// <returns>Concatenated string of results from each iteration.</returns>
    public static string ConcatForEach(this JArray array, Func<dynamic, bool, string> fetch)
    {
        string result = string.Empty;

        for (int i = 0; i < array.Count; i++)
        {
            result += fetch(array[i], i >= array.Count - 1);
        }

        return result;
    }

    /// <summary>
    /// Runs for first N in array, concatenates result of specified delegate to returned string. 
    /// Where is the max number of iterations, specified by caller. 
    /// </summary>
    /// <param name="array">The object this method is called on.</param>
    /// <param name="limit">Number of iterations that should be run.</param>
    /// <param name="fetch">Delegate for what should be concatenated in each iteration.</param>
    /// <returns>Concatenated string of results from each iteration.</returns>
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