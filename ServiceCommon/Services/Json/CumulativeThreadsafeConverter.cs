using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Monitor.ServiceCommon.Services.Json;

// a converter that allows thread-safe adjustments to the serializer's Converters field.
public class CumulativeThreadsafeConverter : JsonConverter
{
    private static readonly object StaticSync = new();
    private static readonly ConcurrentDictionary<JsonSerializer, CumulativeThreadsafeConverter> Pairings = new();

    private readonly ILogger? logger;

    private ImmutableList<JsonConverter> converters = ImmutableList<JsonConverter>.Empty;

    private CumulativeThreadsafeConverter(ILogger? logger)
    {
        this.logger = logger;
    }

    public void AddConverters(IEnumerable<JsonConverter> converter) => ImmutableInterlocked.Update(ref converters, c => c.AddRange(converter));
    public void RemoveConverters(IEnumerable<JsonConverter> converter) => ImmutableInterlocked.Update(ref converters, c => c.RemoveRange(converter));

    public static CumulativeThreadsafeConverter InitSerializer(JsonSerializer serializer, ILogger? logger = null, bool keepReference = true)
    {
        if (keepReference)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if (Pairings.TryGetValue(serializer, out CumulativeThreadsafeConverter? cnv))
            {
                return cnv;
            }
        }

        lock (StaticSync)
        {
            if (serializer.Converters.FirstOrDefault(x => x is CumulativeThreadsafeConverter) is CumulativeThreadsafeConverter prior)
            {
                return prior;
            }

            var converter = new CumulativeThreadsafeConverter(logger);
            serializer.Converters.Add(new CumulativeThreadsafeConverter(logger));

            if (keepReference)
            {
                Pairings[serializer] = converter;
            }

            return converter;
        }
    }

    public static void AddConverters(JsonSerializer serializer, IEnumerable<JsonConverter> converters, ILogger? logger = null, bool keepReference = true) => InitSerializer(serializer, logger, keepReference).AddConverters(converters);
    public static void RemoveConverters(JsonSerializer serializer, IEnumerable<JsonConverter> converters, ILogger? logger = null, bool keepReference = true) => InitSerializer(serializer, logger, keepReference).RemoveConverters(converters);

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            logger?.LogTrace($"Encountered null value");
            writer.WriteNull();

            return;
        }

        Type type = value.GetType();
        JsonConverter? currentConv = converters.Find(c => c.CanConvert(type));
        if (currentConv == null)
        {
            logger?.LogWarning($"Illegal state reached - couldn't find a converter to write type {type.FullName}, falling back to serializer");
            serializer.Serialize(writer, value);

            return;
        }

        currentConv.WriteJson(writer, value, serializer);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JsonConverter? currentConv = converters.Find(c => c.CanConvert(objectType));
        if (currentConv == null)
        {
            logger?.LogWarning($"Illegal state reached - couldn't find a converter to read type {objectType.FullName}, falling back to null");
            return GetDefault(objectType);
        }

        return currentConv.ReadJson(reader, objectType, existingValue, serializer);
    }

    public override bool CanConvert(Type objectType) => converters.Any(c => c.CanConvert(objectType));

    public static object? GetDefault(Type type) => type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
}