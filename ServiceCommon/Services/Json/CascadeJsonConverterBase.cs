using System;
using System.Linq;
using MonitorCommon;
using Newtonsoft.Json;

namespace Monitor.ServiceCommon.Services.Json
{
    public abstract class CascadeJsonConverterBase : JsonConverter
    {
        private readonly JsonConverter[] augmentConverters;

        protected CascadeJsonConverterBase() : this(new JsonConverter[0]) { }

        // this constructor is intended for use with JsonConverterAttribute
        protected CascadeJsonConverterBase(object[] augmentConverters)
            : this(augmentConverters.Select(FromAttributeData).ToArray())
        { }

        protected CascadeJsonConverterBase(JsonConverter[] augmentConverters)
        {
            this.augmentConverters = augmentConverters;
        }

        protected static JsonConverter FromAttributeData(object augmentConverterObj)
        {
            if (!(augmentConverterObj is object[] augmentConverter))
            {
                throw new ArgumentException($"Each augment converter data should be an object array", nameof(augmentConverters));
            }

            if (augmentConverter.Length < 1)
            {
                throw new ArgumentException($"Augment converter data should include at least one item", nameof(augmentConverters));
            }

            object augmentConverterType = augmentConverter[0];
            if (!(augmentConverterType is Type convType))
            {
                throw new ArgumentException($"Augment converter data should start with its type", nameof(augmentConverters));
            }

            if (!typeof(JsonConverter).IsAssignableFrom(convType))
            {
                throw new ArgumentException($"Augment converter type should inherit from JsonConverter abstract type", nameof(augmentConverters));
            }

            object converter = Activator.CreateInstance(convType, augmentConverter.SubArray(1, augmentConverter.Length - 1));
            return (JsonConverter)converter;
        }

        protected abstract void WriteJsonInner(JsonWriter writer, object value, JsonSerializer serializer);

        protected abstract object ReadJsonInner(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            using (AugmentedConverterScope(serializer))
            {
                WriteJsonInner(writer, value, serializer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            using (AugmentedConverterScope(serializer))
            {
                return ReadJsonInner(reader, objectType, existingValue, serializer);
            }
        }

        private AugmentedConverterScopeMgr AugmentedConverterScope(JsonSerializer serializer)
        {
            // add augmented converters
            CumulativeThreadsafeConverter.AddConverters(serializer, augmentConverters);

            return new AugmentedConverterScopeMgr(serializer, augmentConverters);
        }

        private class AugmentedConverterScopeMgr : IDisposable
        {
            private readonly JsonSerializer serializer;
            private readonly JsonConverter[] augmentConverters;

            public AugmentedConverterScopeMgr(JsonSerializer serializer, JsonConverter[] augmentConverters)
            {
                this.serializer = serializer;
                this.augmentConverters = augmentConverters;
            }

            public void Dispose()
            {
                // remove augmented converters
                CumulativeThreadsafeConverter.RemoveConverters(serializer, augmentConverters);
            }
        }
    }
}