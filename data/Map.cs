using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;
using RensaSimulator.objects;
using RensaSimulator.data.converter;

namespace RensaSimulator.data;

public partial class Map {
    public Decoration[] Decorations { get; set; } = [];
    public LabelDto[] Labels { get; set; } = [];
    public ApproachSignDto[] ApproachSigns { get; set; } = [];
    public RouteLightDto[] RouteLights { get; set; } = [];
    public SignalLightDto[] SignalLights { get; set; } = [];
    public SwitchDto[] Switches { get; set; } = [];
}

public class ApproachSignDto {
    [JsonPropertyName("label")] public string Label { get; set; } = "";
    [JsonPropertyName("position")] public Vector2 Position { get; set; } = Vector2.Zero;
    
    public void ApplyTo(ApproachSign sign) {
        sign.Label = this.Label;
        sign.Position = this.Position;
    }
    
    public static implicit operator ApproachSignDto(ApproachSign sign) {
        return new ApproachSignDto {
            Label = sign.Label,
            Position = sign.Position
        };
    }
}

public class RouteLightDto {
    [JsonPropertyName("position")] public Vector2 Position { get; set; } = Vector2.Zero;
    [JsonPropertyName("id")] public Vector2 Id { get; set; } = Vector2.Zero;
    [JsonPropertyName("rotation")] public float Rotation { get; set; } = 0.0f;
    
    public void ApplyTo(RouteLight light) {
        light.Position = this.Position;
        light.Id = this.Id;
        light.Rotation = this.Rotation;
    }
    
    public static implicit operator RouteLightDto(RouteLight light) {
        return new RouteLightDto {
            Position = light.Position,
            Id = light.Id,
            Rotation = light.Rotation
        };
    }
}

public class SignalLightDto {
    [JsonPropertyName("position")] public Vector2 Position { get; set; } = Vector2.Zero;
    [JsonPropertyName("isLine")] public bool IsLine { get; set; } = false;
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    
    public void ApplyTo(SignalLight light) {
        light.Position = this.Position;
        light.IsLine = this.IsLine;
        light.Id = this.Id;
    }
    
    public static implicit operator SignalLightDto(SignalLight light) {
        return new SignalLightDto {
            Position = light.Position,
            IsLine = light.IsLine,
            Id = light.Id
        };
    }
}

public class SwitchDto {
    [JsonPropertyName("position")] public Vector2 Position { get; set; } = Vector2.Zero;
    [JsonPropertyName("id")] public int Id { get; set; } = 0;
    [JsonPropertyName("isSignalSwitch")] public bool IsSignalSwitch { get; set; } = false;
    [JsonPropertyName("isLn")] public bool IsLn { get; set; } = false;
    [JsonPropertyName("isNr")] public bool IsNr { get; set; } = false;
    [JsonPropertyName("innerText")] public string InnerText { get; set; } = "1";

    public void ApplyTo(Switch sw) {
        sw.Position = this.Position;
        sw.Id = this.Id;
        sw.IsSignalSwitch = this.IsSignalSwitch;
        sw.IsLn = this.IsLn;
        sw.IsNr = this.IsNr;
        sw.InnerText = this.InnerText;
    }
    
    public static implicit operator SwitchDto(Switch sw) {
        return new SwitchDto {
            Position = sw.Position,
            Id = sw.Id,
            IsSignalSwitch = sw.IsSignalSwitch,
            IsLn = sw.IsLn,
            IsNr = sw.IsNr,
            InnerText = sw.InnerText
        };
    }
}

public partial class Decoration {
    public string ScenePath { get; set; } = "";
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; }
    
    public Vector2 Size { get; set; }

    [JsonConverter(typeof(PropertiesJsonConverter))]
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}

public partial class LabelDto {
    [JsonPropertyName("text")] public string Text { get; set; } = "";
    [JsonPropertyName("position")] public Vector2 Position { get; set; } = Vector2.Zero;
    [JsonPropertyName("fontSize")] public int FontSize { get; set; } = 16;
    
    public void ApplyTo(Label label) {
        label.Text = Text;
        label.Position = Position;
        label.LabelSettings = new LabelSettings() {
            FontSize = FontSize
        };
    }
    
    public static implicit operator LabelDto(Label label) {
        var fontSize = label.GetThemeDefaultFontSize();
        if (label.LabelSettings != null) {
            fontSize = label.LabelSettings.FontSize;
        }
				
        return new LabelDto {
            Text = label.Text,
            Position = label.Position,
            FontSize = fontSize
        };
    }
}