using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace RensaSimulator.data.converter;

public class PropertiesJsonConverter : JsonConverter<Dictionary<string, object>> {
    public override Dictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options) {
        var properties = new Dictionary<string, object>();

        if (reader.TokenType != JsonTokenType.StartObject) {
            throw new JsonException("Expected start of object");
        }

        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName) {
                throw new JsonException("Expected property name");
            }

            string propertyName = reader.GetString();
            reader.Read();

            // Now we handle the property value based on its name
            if (propertyName == null) continue;
            if (propertyName.EndsWith("Color") || propertyName.EndsWith('C')) {
                // Color
                if (reader.TokenType != JsonTokenType.StartArray) {
                    properties[propertyName] = null;
                    continue;
                }

                var colorArray = new float[4];
                var index = 0;

                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray) {
                    if (reader.TokenType == JsonTokenType.Number && index < 4) {
                        colorArray[index++] = reader.GetSingle();
                    }
                }

                if (index == 4) {
                    properties[propertyName] = new Color(colorArray[0], colorArray[1], colorArray[2], colorArray[3]);
                } else {
                    properties[propertyName] = colorArray;
                }
            } else if (propertyName.EndsWith("Float") || propertyName.EndsWith('F')) {
                if (!reader.TryGetSingle(out var floatValue)) {
                    properties[propertyName] = null;
                }

                properties[propertyName] = floatValue;
            } else {
                properties[propertyName] = JsonSerializer.Deserialize<object>(ref reader, options);
            }
        }

        return properties;
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options) {
        writer.WriteStartObject();

        foreach (var kvp in value) {
            writer.WritePropertyName(kvp.Key);

            if (kvp.Value is Color color) {
                writer.WriteStartArray();
                writer.WriteNumberValue(color.R);
                writer.WriteNumberValue(color.G);
                writer.WriteNumberValue(color.B);
                writer.WriteNumberValue(color.A);
                writer.WriteEndArray();
            } else {
                JsonSerializer.Serialize(writer, kvp.Value, options);
            }
        }

        writer.WriteEndObject();
    }
}