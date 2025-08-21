using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace RensaSimulator.data.converter;

public class Color2JsonConverter : JsonConverter<Color> {
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartArray) {
            throw new JsonException("Expected start of array");
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.Number) {
            throw new JsonException("Expected number for X coordinate");
        }

        float r = reader.GetSingle();

        reader.Read();
        if (reader.TokenType != JsonTokenType.Number) {
            throw new JsonException("Expected number for Y coordinate");
        }

        float g = reader.GetSingle();
        
        reader.Read();
        if (reader.TokenType != JsonTokenType.Number) {
            throw new JsonException("Expected number for Y coordinate");
        }

        float b = reader.GetSingle();
        
        reader.Read();
        if (reader.TokenType != JsonTokenType.Number) {
            throw new JsonException("Expected number for Y coordinate");
        }

        float a = reader.GetSingle();

        reader.Read();
        if (reader.TokenType != JsonTokenType.EndArray) {
            throw new JsonException("Expected end of array");
        }

        return new Color(r, g, b, a);
    }
    
    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options) {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.R);
        writer.WriteNumberValue(value.G);
        writer.WriteNumberValue(value.B);
        writer.WriteNumberValue(value.A);
        writer.WriteEndArray();
    }
}