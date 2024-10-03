using System;
using System.IO;
using Memoria.FrontMission1.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Memoria.FrontMission1.HarmonyHooks;

public static class StructuredJson
{
    // We cannot use JsonSerializer because of framework version
    public static void Write(String outputPath, OrderedDictionary<String, TransifexEntry> map)
    {
        using (StreamWriter output = File.CreateText(outputPath))
        {
            JsonTextWriter writer = new JsonTextWriter(output);
            writer.Formatting = Formatting.Indented;
            writer.WriteStartObject();
            foreach ((string key, TransifexEntry value) in map)
            {
                writer.WritePropertyName(key);
                writer.WriteStartObject();
                {
                    writer.WritePropertyName("string");
                    writer.WriteValue(value.Text);

                    if (value.Context != null)
                    {
                        writer.WritePropertyName("context");
                        writer.WriteValue(value.Context);
                    }
                    
                    if (value.Comment != null)
                    {
                        writer.WritePropertyName("developer_comment");
                        writer.WriteValue(value.Comment);
                    }
                    
                    if (value.CharacterLimit != null)
                    {
                        writer.WritePropertyName("character_limit");
                        writer.WriteValue(value.CharacterLimit);
                    }
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }
    }

    public static OrderedDictionary<String, TransifexEntry> Read(String inputPath)
    {
        using (StreamReader input = File.OpenText(inputPath))
            return Read(input);
    }

    public static OrderedDictionary<String, TransifexEntry> Read(Stream inputStream)
    {
        using (StreamReader input = new StreamReader(inputStream))
            return Read(input);
    }

    // We cannot use JsonSerializer because of framework version
    public static OrderedDictionary<String, TransifexEntry> Read(StreamReader input)
    {
        JsonTextReader reader = new JsonTextReader(input);

        OrderedDictionary<String, TransifexEntry> orderedDictionary = new OrderedDictionary<String, TransifexEntry>();
        if (!reader.Read())
            return orderedDictionary;

        ReadStartObject(reader);

        while (reader.TokenType != JsonToken.None && reader.TokenType != JsonToken.EndObject)
        {
            String key = ReadPropertyName(reader);
            ReadStartObject(reader);
            {
                TransifexEntry entry = new TransifexEntry();

                while (reader.TokenType != JsonToken.EndObject)
                {
                    switch (ReadPropertyName(reader))
                    {
                        case "string":
                            entry.Text = ReadString(reader);
                            break;
                        case "context":
                            entry.Context = ReadString(reader);
                            break;
                        case "developer_comment":
                            entry.Comment = ReadString(reader);
                            break;
                        case "character_limit":
                            entry.CharacterLimit = ReadInt32(reader);
                            break;
                    }
                }
                ReadEndObject(reader);

                if (entry.Text is null)
                    throw new NotSupportedException("if (entry.Text is null)");

                orderedDictionary.AddOrUpdate(key, entry);
            }
        }

        return orderedDictionary;
    }

    private static String ReadString(JsonTextReader reader)
    {
        JsonToken currentToken = reader.TokenType;
        if (currentToken != JsonToken.String)
            throw new FormatException($"if (currentToken != JsonToken.String): {currentToken} (line: {reader.LinePosition})");

        var value = (String)reader.Value;
        SkipComments(reader);
        return value;
    }

    private static Int32 ReadInt32(JsonTextReader reader)
    {
        JsonToken currentToken = reader.TokenType;
        if (currentToken != JsonToken.Integer)
            throw new FormatException($"if (currentToken != JsonToken.Integer): {currentToken} (line: {reader.LinePosition})");

        var value = (Int32)reader.Value;
        SkipComments(reader);
        return value;
    }

    private static String ReadPropertyName(JsonTextReader reader)
    {
        JsonToken currentToken = reader.TokenType;
        if (currentToken != JsonToken.PropertyName)
            throw new FormatException($"if (currentToken != JsonToken.PropertyName): {currentToken} (line: {reader.LinePosition})");

        var value = (String)reader.Value;
        SkipComments(reader);
        return value;
    }

    private static void ReadStartObject(JsonTextReader reader)
    {
        JsonToken currentToken = reader.TokenType;
        if (currentToken != JsonToken.StartObject)
            throw new FormatException($"if (currentToken != JsonToken.StartObject): {currentToken} (line: {reader.LinePosition})");
        SkipComments(reader);
    }
    
    private static void ReadEndObject(JsonTextReader reader)
    {
        JsonToken currentToken = reader.TokenType;
        if (currentToken != JsonToken.EndObject)
            throw new FormatException($"if (currentToken != JsonToken.EndObject): {currentToken} (line: {reader.LinePosition})");
        SkipComments(reader);
    }

    private static JsonToken SkipComments(JsonTextReader reader)
    {
        do
        {
            reader.Read();
        } while (reader.TokenType == JsonToken.Comment);

        return reader.TokenType;
    }
}