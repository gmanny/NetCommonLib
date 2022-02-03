using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using LanguageExt;
using Newtonsoft.Json;

namespace Monitor.ServiceCommon.Services.Json;

public class OptionJsonConverter : JsonConverter
{
    private static readonly object[] NullParams = { null };
    private static readonly ConcurrentDictionary<Type, ReflectionTypeData> CachedReflection = new();

    private ReflectionTypeData GetForOptionType(Type optionType)
    {
        return CachedReflection.GetOrAdd(optionType.GetGenericArguments().First(), _ => new ReflectionTypeData(optionType));
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType.IsGenericType && objectType.GetGenericTypeDefinition().IsAssignableFrom(typeof(Option<>));
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        ReflectionTypeData typeData = GetForOptionType(value.GetType());

        // ReSharper disable once PossibleNullReferenceException
        if ((bool)typeData.IsNoneProp.GetValue(value))
        {
            writer.WriteNull();
            return;
        }

        serializer.Serialize(writer, typeData.IfNoneMethod.Invoke(value, NullParams));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        ReflectionTypeData typeData = GetForOptionType(objectType);

        if (reader.TokenType == JsonToken.Null)
        {
            return typeData.NoneField.GetValue(null);
        }

        object result = serializer.Deserialize(reader, objectType.GetGenericArguments().First());

        return typeData.SomeMethod.Invoke(null, new[] { result });
    }
        
    private class ReflectionTypeData
    {
        public ReflectionTypeData(Type optionType)
        {
            IsNoneProp = optionType.GetProperty(nameof(Option<object>.IsNone), BindingFlags.Instance | BindingFlags.Public);
            NoneField = optionType.GetField(nameof(Option<object>.None), BindingFlags.Static | BindingFlags.Public);
            SomeMethod = optionType.GetMethod(nameof(Option<object>.Some), BindingFlags.Static | BindingFlags.Public);
            IfNoneMethod = optionType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Filter(m => m.Name == nameof(Option<object>.IfNone)
                             && m.GetParameters().Length == 1
                             && !(m.GetParameters()[0].ParameterType.IsGenericType && m.GetParameters()[0]
                                 .ParameterType.GetGenericTypeDefinition().IsAssignableFrom(typeof(Func<>))))
                .First();
        }

        public PropertyInfo IsNoneProp { get; }
        public MethodInfo IfNoneMethod { get; }
        public FieldInfo NoneField { get; }
        public MethodInfo SomeMethod { get; }
    }
}