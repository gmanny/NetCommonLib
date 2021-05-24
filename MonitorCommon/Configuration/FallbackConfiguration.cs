using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace MonitorCommon.Configuration
{
    public class FallbackConfiguration : IConfiguration
    {
        private readonly IConfiguration[] layers;

        public FallbackConfiguration(params IConfiguration[] layers)
        {
            if (layers.IsEmpty())
            {
                throw new ArgumentException("Can't have zero layers", nameof(layers));
            }

            this.layers = layers;
        }

        public IConfigurationSection GetSection(string key)
        {
            IConfigurationSection[] sections = layers.Select(x => x.GetSection(key)).ToArray();

            return new FallbackConfigurationSection(sections);
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            ISet<string> keys = new HashSet<string>();
            IDictionary<(string key, IConfiguration layer), IConfigurationSection> index = new Dictionary<(string key, IConfiguration layer), IConfigurationSection>();

            foreach (IConfiguration layer in layers)
            {
                foreach (IConfigurationSection child in layer.GetChildren())
                {
                    keys.Add(child.Key);

                    index.Add((child.Key, layer), child);
                }
            }

            foreach (string key in keys)
            {
                yield return new FallbackConfigurationSection(
                    layers.Select(l => index.GetOrElse((key, l), () => l.GetSection(key))).ToArray()
                );
            }
        }

        public IChangeToken GetReloadToken()
        {
            return new CompositeChangeToken(layers.Select(x => x.GetReloadToken()).ToList());
        }

        public string this[string key]
        {
            get
            {
                foreach (IConfiguration layer in layers)
                {
                    string current = layer[key];

                    if (current != null)
                    {
                        return current;
                    }
                }

                return null;
            }

            set => layers[0][key] = value;
        }
    }
}