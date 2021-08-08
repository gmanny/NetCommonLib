using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Monitor.ServiceCommon.Services.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Ninject.Activation;

namespace Monitor.ServiceCommon.Services
{
    public class JsonSerializerSvc : Provider<JsonSerializer>
    {
        public JsonSerializer Serializer { get; }

        public JsonSerializerSvc(ILogger logger)
        {
            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = new InterfaceContractResolver(),
                TypeNameHandling = TypeNameHandling.Auto
            };

            CumulativeThreadsafeConverter.InitSerializer(serializer, logger);

            serializer.Converters.Add(new OptionJsonConverter());

            Serializer = serializer;
        }

        protected override JsonSerializer CreateInstance(IContext context)
        {
            return Serializer;
        }
    }

    public class InterfaceContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (member.DeclaringType != null)
            {
                if (member.GetCustomAttributes(typeof(JsonPropertyAttribute), true).Any())
                {
                    property.Ignored = member.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Any();
                }
                else
                {
                    // if the property itself doesn't have the JsonProperty attribute
                    // check JsonIgnore attributes in all the interfaces that define this property
                    Type[] interfaces = member.DeclaringType.GetInterfaces();
                    foreach (Type @interface in interfaces)
                    {
                        foreach (PropertyInfo interfaceProperty in @interface.GetProperties())
                        {
                            if (interfaceProperty.Name == member.Name &&
                                interfaceProperty.MemberType == member.MemberType)
                            {
                                if (interfaceProperty.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Any())
                                {
                                    // if the interface was found, ignore this property
                                    property.Ignored = true;
                                    return property;
                                }
                            }
                        }
                    }
                }
            }

            return property;
        }
    }
}