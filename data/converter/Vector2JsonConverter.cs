using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace RensaSimulator.data.converter;

public class Vector2JsonConverter : JsonConverter<Vector2> {
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartArray) {
            throw new JsonException("Expected start of array");
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.Number) {
            throw new JsonException("Expected number for X coordinate");
        }

        float x = reader.GetSingle();

        reader.Read();
        if (reader.TokenType != JsonTokenType.Number) {
            throw new JsonException("Expected number for Y coordinate");
        }

        float y = reader.GetSingle();

        reader.Read();
        if (reader.TokenType != JsonTokenType.EndArray) {
            throw new JsonException("Expected end of array");
        }

        return new Vector2(x, y);
    }

    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options) {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteEndArray();
    }
}