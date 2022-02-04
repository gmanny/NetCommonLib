using System;
using System.Linq;
using MonitorCommon;
using Newtonsoft.Json;

namespace Monitor.ServiceCommon.Services.Json;

public class CascadeJsonConverter : CascadeJsonConverterBase
{
    private readonly JsonConverter wrappedConverter;

    public CascadeJsonConverter(Type wrappedConverterType, object[] wrappedConvConstructorArgs, object[] augmentConverters)
        : this(CreateConverter(wrappedConverterType, wrappedConvConstructorArgs), augmentConverters.Select(FromAttributeData).SkipNulls().ToArray())
    { }

    public CascadeJsonConverter(JsonConverter wrappedConverter, JsonConverter[] augmentConverters)
        : base(augmentConverters)
    {
        this.wrappedConverter = wrappedConverter;
    }

    private static JsonConverter CreateConverter(Type converterType, object[] convConstructorArgs)
    {
        if (!typeof(JsonConverter).IsAssignableFrom(converterType))
        {
            throw new ArgumentException($"Converter type should inherit from JsonConverter abstract type", nameof(converterType));
        }

        JsonConverter? result = (JsonConverter?) Activator.CreateInstance(converterType, convConstructorArgs);
        if (result == null)
        {
            throw new Exception($"Couldn't create and instance of converter type {converterType.FullName}");
        }

        return result;
    }

    public override bool CanConvert(Type objectType)
    {
        return wrappedConverter.CanConvert(objectType);
    }

    protected override void WriteJsonInner(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        wrappedConverter.WriteJson(writer, value, serializer);
    }

    protected override object? ReadJsonInner(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        return wrappedConverter.ReadJson(reader, objectType, existingValue, serializer);
    }
}