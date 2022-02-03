using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace MonitorCommon;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class UtilExtensions
{
    public static IEnumerable<(A, B)> CMul<A, B>(this IEnumerable<A> a, IEnumerable<B> b) =>
        a.SelectMany(a_ => b.Select(b_ => (a_, b_)));

    public static IDictionary<A, B> ToDictionary<A, B>(this IEnumerable<(A k, B v)> a) =>
        a.ToDictionary(v => v.k, v => v.v);

    public static ISet<A> ToSet<A>(this IEnumerable<A> a) => new System.Collections.Generic.HashSet<A>(a);

    public static V GetOrElse<K, V>(this IDictionary<K, V> dict, K key, V def)
    {
        if (!dict.TryGetValue(key, out V res))
        {
            return def;
        }

        return res;
    }

    public static V GetOrElse<K, V>(this IDictionary<K, V> dict, K key, Func<V> def)
    {
        if (!dict.TryGetValue(key, out V res))
        {
            return def();
        }

        return res;
    }

    public static bool IsNaN(this double n) => double.IsNaN(n);
    public static double WhenNaN(this double n, double other) => double.IsNaN(n) ? other : n;
    public static bool IsNaN(this float n) => float.IsNaN(n);

    public static bool IsEmpty<K, V>(this IDictionary<K, V> c) => c == null || c.Count == 0;
    public static bool IsEmpty<T>(this ICollection<T> c) => c == null || c.Count == 0;
    public static bool IsEmptyRe<T>(this IReadOnlyCollection<T> c) => c == null || c.Count == 0;
    public static bool IsEmpty<T>(this Option<T> o) => o.IsNone;
    public static bool NonEmpty<K, V>(this IDictionary<K, V> c) => c != null && c.Count != 0;
    public static bool NonEmpty<T>(this ICollection<T> c) => c != null && c.Count != 0;
    public static bool NonEmptyRe<T>(this IReadOnlyCollection<T> c) => c != null && c.Count != 0;
    public static bool NonEmpty<T>(this Option<T> o) => o.IsSome;

    public static string AbbreviatedRange<T>(this IReadOnlyList<T> c, string prefix = "[", string separator = ", ", string postfix = "]", int chunkLength = 4)
    {
        if (c.IsEmptyRe() || c.Count <= chunkLength * 2 + 1)
        {
            return c.CommaString();
        }

        StringBuilder builder = new StringBuilder();

        builder.Append(c.Take(chunkLength).JoinedString(prefix, separator));
            
        builder.Append(separator);
        builder.Append("...");
        builder.Append(separator);
            
        builder.Append(c[c.Count / 2]);

        builder.Append(separator);
        builder.Append("...");
        builder.Append(separator);

        IEnumerable<T> LastIterator()
        {
            for (int i = c.Count - chunkLength; i < c.Count; i++)
            {
                yield return c[i];
            }
        }

        builder.Append(LastIterator().JoinedString(separator: separator, postfix: postfix));

        return builder.ToString();
    }

    public static string CommaString<T>(this IEnumerable<T> c) => c.JoinedString("[", ", ", "]");
    public static string JoinedString<T>(this IEnumerable<T> c, string prefix = null, string separator = null, string postfix = null)
    {
        StringBuilder builder = new StringBuilder();

        if (c != null)
        {
            if (!prefix.IsNullOrEmpty())
            {
                builder.Append(prefix);
            }

            bool first = true;
            foreach (T i in c)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (!separator.IsNullOrEmpty())
                    {
                        builder.Append(separator);
                    }
                }

                builder.Append(i);
            }

            if (!postfix.IsNullOrEmpty()) 
            {
                builder.Append(postfix);
            }
        }
        else
        {
            builder.Append("<null>");
        }

        return builder.ToString();
    }

    public static Option<T> AsOptional<T>(this T obj) where T : class => Optional(obj);
    public static Option<T> AsOptional<T>(this Result<T> res) => res.Map(Optional).IfFail(Option<T>.None);

    public static T Get<T>(this Option<T> o) => o.IfNone(() => throw new InvalidOperationException("None.Get"));
    public static T GetOrElse<T>(this Option<T> o, T defaultVal) => o.IsSome ? o.Get() : defaultVal;
    public static T GetOrElse<T>(this Option<T> o, Func<T> defaultVal) => o.IsSome ? o.Get() : defaultVal();
    public static T GetOrDefault<T>(this Option<T> o) => o.IsSome ? o.Get() : default;
    public static bool TryGet<T>(this Option<T> o, out T value)
    {
        if (!o.IsSome)
        {
            value = default;
            return false;
        }

        value = o.Get();
        return true;
    }

    public static Option<T> OrElse<T>(this Option<T> o, Func<Option<T>> newVal) => o.IsSome ? o : newVal();

    public static bool HasMoreThan<T>(this IEnumerable<T> enu, long items)
    {
        long count = 0;
        foreach (T _ in enu)
        {
            count++;
            if (count > items)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsEmptyEnu<T>(this IEnumerable<T> enu) => !enu.HasMoreThan(0);

    public static IEnumerable<T> SingleItemEnumerable<T>(this T item) => Enumerable.Repeat(item, 1);

    public static void ForEach<T>(this IEnumerable<T> c, Action<T> action)
    {
        foreach (T i in c)
        {
            action(i);
        }
    }

    public static void ForEach<T>(this T? c, Action<T> action) where T : struct
    {
        if (c.HasValue)
        {
            action(c.Value);
        }
    }

    public static int CountUntil<T>(this IEnumerable<T> en, Func<T, bool> predicate)
    {
        int count = 0;
        foreach (T i in en)
        {
            if (predicate(i))
            {
                return count;
            }

            count++;
        }

        return count;
    }

    public static int CountBackUntil<T>(this IList<T> en, Func<T, bool> predicate)
    {
        int count = 0;
        for (int i = en.Count - 1; i >= 0; i--)
        {
            if (predicate(en[i]))
            {
                return count;
            }

            count++;
        }

        return count;
    }

    public static IEnumerable<int> Indices(this ICollection c) => Enumerable.Range(0, c.Count);

    public static int FirstIndexWhere<T>(this IEnumerable<T> en, Func<T, bool> predicate)
    {
        int index = 0;
        foreach (T item in en)
        {
            if (predicate(item))
            {
                return index;
            }

            index += 1;
        }

        return -1;
    }

    public static (int index, T value) FirstIndexAndValueWhere<T>(this IEnumerable<T> en, Func<T, bool> predicate)
    {
        int index = 0;
        foreach (T item in en)
        {
            if (predicate(item))
            {
                return (index, item);
            }

            index += 1;
        }

        return (-1, default);
    }

    public static (int count, T last) CountAndLast<T>(this IEnumerable<T> en, int atMost = -1)
    {
        T last = default;
        int count = 0;
        foreach (T i in en)
        {
            count++;
            last = i;

            if (atMost > 0 && count >= atMost)
            {
                break;
            }
        }

        return (count, last);
    }

    public static (int count, T max) CountAndMax<T>(this IEnumerable<T> en, Comparison<T> comparison, int atMost = -1)
    {
        T max = default;
        int count = 0;
        foreach (T i in en)
        {
            count++;
            if (Object.Equals(max, default(T)) || comparison(max, i) <= 0)
            {
                max = i;
            }

            if (atMost > 0 && count >= atMost)
            {
                break;
            }
        }

        return (count, max);
    }

    public static bool IsNullOrEmpty(this string str) => String.IsNullOrEmpty(str);

    public static string TakeString(this string str, int chars) => str.Substring(0, Math.Min(str.Length, chars));

    public static string[] SplitAt(this string str, int index) => new[] {str.Substring(0, index), str.Substring(index)};

    public static string Truncate(this string value, int maxLength)
    {
        if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
        {
            return value[..maxLength];
        }

        return value;
    }

    public static string Truncate2(this string value, int maxLength)
    {
        if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
        {
            return $"{value[..(maxLength / 2 - 1)]}...{value[^(maxLength / 2 - 1)..]}";
        }

        return value;
    }

    public static UInt64 KnuthHash(this string read)
    {
        UInt64 hashedValue = 3074457345618258791ul;
        for (int i = 0; i < read.Length; i++)
        {
            hashedValue += read[i];
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }

    public static long NextLong(this Random rnd)
    {
        byte[] bytes = new byte[sizeof(long)];
        rnd.NextBytes(bytes);

        return BitConverter.ToInt64(bytes, 0);
    }
}