using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;
using RensaSimulator.objects;
using RensaSimulator.data.converter;
using RensaSimulator.objects.route;

namespace RensaSimulator.data;

public class Map {
    public Decoration[] Decorations { get; init; } = [];
    public LabelDto[] Labels { get; init; } = [];
    public ApproachSignDto[] ApproachSigns { get; init; } = [];
    public RouteLightDto[] RouteLights { get; init; } = [];
    public SignalLightDto[] SignalLights { get; init; } = [];
    public SwitchDto[] Switches { get; init; } = [];
    
    public Map() {}

    public Map(Node2D scene) {
        Decorations = scene.GetNode<Node2D>("Decorations")
			.GetChildren()
			.Select(node => {
				return node switch {
					SimpleColorReplace simpleColorReplace => new Decoration {
						Position = simpleColorReplace.Position,
						Scale = simpleColorReplace.Scale,
						Rotation = simpleColorReplace.Rotation,
						ScenePath = simpleColorReplace.SceneFilePath,
						Properties = new Dictionary<string, object> { { "RouteColor", simpleColorReplace.RouteColor }, }
					},
					RouteEnd routeEnd => new Decoration {
						Position = routeEnd.Position,
						Scale = routeEnd.Scale,
						Rotation = routeEnd.Rotation,
						ScenePath = routeEnd.SceneFilePath,
						Properties = new Dictionary<string, object> { { "RouteColor", routeEnd.RouteColor }, }
					},
					RouteDiagonalSeparate routeDiagonalSeparate => new Decoration {
						Position = routeDiagonalSeparate.Position,
						Scale = routeDiagonalSeparate.Scale,
						Rotation = routeDiagonalSeparate.Rotation,
						ScenePath = routeDiagonalSeparate.SceneFilePath,
						Properties = new Dictionary<string, object> {
							{ "Route1Color", routeDiagonalSeparate.Route1Color },
							{ "Route2Color", routeDiagonalSeparate.Route2Color },
						}
					},
					ReferenceRect referenceRect => new Decoration {
						Position = referenceRect.Position,
						Scale = referenceRect.Scale,
						Rotation = referenceRect.Rotation,
						Size = referenceRect.Size,
						Properties = new Dictionary<string, object> {
							{ "Type", "ReferenceRect" },
							{ "BorderColor", referenceRect.BorderColor },
							{ "BorderWidth", referenceRect.BorderWidth },
						}
					},
					Line2D line2D => new Decoration {
						Position = line2D.Position,
						Scale = line2D.Scale,
						Rotation = line2D.Rotation,
						Properties = new Dictionary<string, object> {
							{ "Type", "Line2D" }, { "Width", line2D.Width }, { "Points", line2D.Points }
						}
					},
					_ => null
				};
			})
			.ToArray();

		Labels = scene.GetNode<Node2D>("Labels")
			.GetChildren()
			.OfType<Label>()
			.Select(l => (LabelDto)l)
			.ToArray();

		ApproachSigns = scene.GetNode<Node2D>("ApproachSigns")
			.GetChildren()
			.OfType<ApproachSign>()
			.Select(sign => (ApproachSignDto)sign)
			.ToArray();

		RouteLights = scene.GetNode<Node2D>("RouteLights")
			.GetChildren()
			.OfType<RouteLight>()
			.Select(light => (RouteLightDto)light)
			.ToArray();

		SignalLights = scene.GetNode<Node2D>("SignalLights")
			.GetChildren()
			.OfType<SignalLight>()
			.Select(light => (SignalLightDto)light)
			.ToArray();

		Switches = scene.GetNode<Node2D>("Switches")
			.GetChildren()
			.OfType<Switch>()
			.Select(sw => (SwitchDto)sw)
			.ToArray();
    }
}

public class ApproachSignDto {
    public string Label { get; init; } = "";
    public Vector2 Position { get; init; } = Vector2.Zero;
    
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
    public Vector2 Position { get; init; } = Vector2.Zero;
    public int Id { get; init; } = 0;
    public float Rotation { get; init; } = 0.0f;
    
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
    public Vector2 Position { get; init; } = Vector2.Zero;
    public bool IsLine { get; init; } = false;
    public string Id { get; init; } = "";
    
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
    public Vector2 Position { get; init; } = Vector2.Zero;
    public int Id { get; init; } = 0;
    public bool IsSignalSwitch { get; init; } = false;
    public bool IsLn { get; init; } = false;
    public bool IsNr { get; init; } = false;
    public string InnerText { get; init; } = "1";

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

public class Decoration {
    public string ScenePath { get; init; } = "";
    public Vector2 Position { get; init; } = Vector2.Zero;
    public Vector2 Scale { get; init; } = Vector2.One;
    public float Rotation { get; init; }
    
    public Vector2 Size { get; init; }

    [JsonConverter(typeof(PropertiesJsonConverter))]
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
}

public class LabelDto {
    public string Text { get; init; } = "";
    public Vector2 Position { get; init; } = Vector2.Zero;
    public int FontSize { get; init; } = 16;
    
    public void ApplyTo(Label label) {
        label.Text = Text;
        label.Position = Position;
        label.LabelSettings = new LabelSettings {
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