using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Monitor.ServiceCommon.Util;

public abstract class ArrayJsonConverter<T> : JsonConverter<T>
{
    protected abstract int ArrayElementCount { get; }
    protected virtual bool ArrayElementCountStrict => false;

    protected abstract T ReadJson(JArray item, JsonSerializer serializer);
    protected abstract object[] WriteJson(T value, JsonSerializer serializer);

    public sealed override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.StartArray)
        {
            throw new JsonException($"{nameof(T)} is represented by an array, but got {reader.TokenType} token");
        }

        JArray item = JArray.Load(reader);

        if (ArrayElementCountStrict ? item.Count != ArrayElementCount : item.Count < ArrayElementCount)
        {
            throw new JsonException($"{nameof(T)} array representation should contain {ArrayElementCount} items, but got {item.Count}: {item}");
        }

        return ReadJson(item, serializer);
    }

    public sealed override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
    {
        JArray arr = new JArray(WriteJson(value, serializer));

        arr.WriteTo(writer);
    }
}