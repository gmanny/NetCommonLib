using System;
using System.IO;
using System.Text;
using MonitorCommon;
using Newtonsoft.Json;

namespace Monitor.ServiceCommon.Util;

// put here because MonitorCommon doesn't have Json.NET as a dependency
public static class JsonExtensions
{
    public class JsonDeserializationSnippetException : Exception
    {
        public const int SnippetLength = 256;

        public JsonDeserializationSnippetException(Exception cause, string data)
            : base($"{cause.Message}, data snippet: {data.Substring(0, Math.Min(SnippetLength, data.Length))}", cause)
        { }
    }

    public static T Deserialize<T>(this JsonSerializer ser, Stream value)
    {
        using SnippetCachingStream snippet = new SnippetCachingStream(value, snippetLength: JsonDeserializationSnippetException.SnippetLength);
        using StreamReader reader = new StreamReader(snippet);
        using JsonReader jsonReader = new JsonTextReader(reader);

        try
        {
            return ser.Deserialize<T>(jsonReader);
        }
        catch (Exception e)
        {
            string snip;
            try
            {
                byte[] byteSnip = snippet.Snippet.ToArray();
                try
                {
                    snip = Encoding.UTF8.GetString(byteSnip);
                }
                catch
                {
                    /*ignored, maybe the snippet is not a valid UTF-8*/
                    snip = Convert.ToBase64String(byteSnip);
                }
            }
            catch (Exception ex)
            {
                throw new AggregateException(
                    new JsonDeserializationSnippetException(e, "<failed-to-get>"),
                    new Exception("Failed to get a stream snippet", ex)
                );
            }

            throw new JsonDeserializationSnippetException(e, snip);
        }
    }

    public static T Deserialize<T>(this JsonSerializer ser, string value)
    {
        ArgumentNullException.ThrowIfNull(ser);

        using StringReader reader = new StringReader(value);
        using JsonReader jsonReader = new JsonTextReader(reader);

        try
        {
            return ser.Deserialize<T>(jsonReader);
        }
        catch (Exception e)
        {
            throw new JsonDeserializationSnippetException(e, value);
        }
    }

    public static void Serialize<T>(this JsonSerializer ser, Stream stream, T value, bool beautify = false)
    {
        using StreamWriter writer = new StreamWriter(stream);
        using JsonWriter jsonWriter = new JsonTextWriter(writer) {Formatting = beautify ? Formatting.Indented : Formatting.None};

        ser.Serialize(jsonWriter, value);
    }

    public static string Serialize<T>(this JsonSerializer ser, T value, bool beautify = false)
    {
        using StringWriter writer = new StringWriter();
        using JsonWriter jsonWriter = new JsonTextWriter(writer) {Formatting = beautify ? Formatting.Indented : Formatting.None};
        
        ser.Serialize(jsonWriter, value);

        return writer.ToString();
    }
}