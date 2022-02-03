using System;
using Microsoft.Extensions.Configuration;

namespace MonitorCommon.Configuration;

public class FallbackConfigurationSection : FallbackConfiguration, IConfigurationSection
{
    private readonly IConfigurationSection[] layers;

    // ReSharper disable once CoVariantArrayConversion
    public FallbackConfigurationSection(params IConfigurationSection[] layers) : base(layers)
    {
        if (layers.IsEmpty())
        {
            throw new ArgumentException("Can't have zero layers", nameof(layers));
        }

        this.layers = layers;
    }

    public string Key => layers[0].Key;
    public string Path => layers[0].Path;

    public string Value
    {
        get
        {
            foreach (IConfigurationSection layer in layers)
            {
                if (layer.Exists())
                {
                    return layer.Value;
                }
            }

            return null;
        }

        set => layers[0].Value = value;
    }
}