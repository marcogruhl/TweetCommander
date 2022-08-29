using System;
using System.Collections.Generic;
using System.Linq;

namespace TweetCommander.Helper;

public class LambdaComparer<T> : IEqualityComparer<T>
{
    private readonly Func<T, T, bool> _expression;

    public LambdaComparer(Func<T, T, bool> lambda)
    {
        _expression = lambda;
    }

    public bool Equals(T x, T y)
    {
        return _expression(x, y);
    }

    public int GetHashCode(T obj)
    {
        /*
         If you just return 0 for the hash the Equals comparer will kick in. 
         The underlying evaluation checks the hash and then short circuits the evaluation if it is false.
         Otherwise, it checks the Equals. If you force the hash to be true (by assuming 0 for both objects), 
         you will always fall through to the Equals check which is what we are always going for.
        */
        return 0;
    }
}

public static class LambdaExtension
{
    public static IEnumerable<T> Distinct<T>(this IEnumerable<T> list, Func<T, T, bool> lambda)
    {
        return list.Distinct(new LambdaComparer<T>(lambda));
    }

    public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
    {
        foreach (T item in list)
            action(item);
    }
}

public static class StringExtension
{
    public static string ReplaceFirst(this string text, string search, string replace)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
        {
            return text;
        }
        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }
}