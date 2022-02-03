using System;
using System.Collections.Generic;
using LanguageExt;
using static LanguageExt.Prelude;

namespace MonitorCommon;

public static class CollectionUtil
{
    public static Option<T> IfDef<T>(bool condition, Func<T> gen) => condition ? Some(gen()) : None;
    public static Lst<T> IfDefLst<T>(bool condition, Func<T> gen) => condition ? List(gen()) : new Lst<T>();

    public static T WithConditional<T, X>(this T collection, X item, bool condition) where T : ICollection<X>
    {
        if (condition)
        {
            collection.Add(item);
        }

        return collection;
    }

    public static Option<TValue> TryGetOption<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key)
    {
        if (dic.TryGetValue(key, out TValue val))
        {
            return Some(val);
        }

        return None;
    }
}