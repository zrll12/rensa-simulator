using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using Godot.Logging;
using RensaSimulator.data.converter;
using RensaSimulator.objects;
using RensaSimulator.objects.route;

namespace RensaSimulator.data.scene;

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
    
    public Map(string filePath) {
	    try {
		    if (!File.Exists(filePath)) {
			    GD.PrintErr($"File not found: {filePath}");
			    return;
		    }

		    var jsonContent = File.ReadAllText(filePath);

		    var options = new JsonSerializerOptions {
			    PropertyNameCaseInsensitive = true,
			    ReferenceHandler = ReferenceHandler.IgnoreCycles,
			    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		    };
		    options.Converters.Add(new Vector2JsonConverter());
		    
		    var deserialize = JsonSerializer.Deserialize<Map>(jsonContent, options);
		    Decorations = deserialize.Decorations;
		    Labels = deserialize.Labels;
		    ApproachSigns = deserialize.ApproachSigns;
		    RouteLights = deserialize.RouteLights;
		    SignalLights = deserialize.SignalLights;
		    Switches = deserialize.Switches;
		    DisplayMapInfo();
		    GodotLogger.LogInfo("Map data loaded!");
	    }
	    catch (Exception ex) {
		    GodotLogger.LogError($"Failed to load map data: {ex.Message}");
		    GodotLogger.LogError(ex.StackTrace);
	    }
	}
    
	public void DisplayMapInfo() {
		GodotLogger.LogInfo($"{Decorations?.Length ?? 0} Decorations");
		GodotLogger.LogInfo($"{Labels?.Length ?? 0} Labels");
		GodotLogger.LogInfo($"{ApproachSigns?.Length ?? 0} Approach Signs");
		GodotLogger.LogInfo($"{RouteLights?.Length ?? 0} Route Lights");
		GodotLogger.LogInfo($"{SignalLights?.Length ?? 0} Signal Lights");
		GodotLogger.LogInfo($"{Switches?.Length ?? 0} Switches");
	}

	public void ApplyTo(Node2D node) {
		var decorationsParent = node.GetNode<Node2D>("Decorations");
		if (Decorations != null) {
			foreach (var decoration in Decorations) {
				if (!String.IsNullOrEmpty(decoration.ScenePath)) {
					var decorationScene = GD.Load<PackedScene>(decoration.ScenePath);
					if (decorationScene == null) continue;
					var decorationNode = decorationScene.Instantiate<Node2D>();
					decorationNode.Position = decoration.Position;
					decorationNode.Scale = decoration.Scale;
					decorationNode.Rotation = decoration.Rotation;
					decorationsParent.AddChild(decorationNode);

					ApplyDecorationProperties(decorationNode, decoration.Properties);
				} else {
					var type = decoration.Properties["Type"].ToString();
					switch (type) {
						case "ReferenceRect":
							if (decoration.Properties["BorderColor"] is Color color) {
								var item = new ReferenceRect();
								decorationsParent.AddChild(item);

								item.Position = decoration.Position;
								item.Scale = decoration.Scale;
								item.Rotation = decoration.Rotation;
								item.BorderColor = color;
								item.Size = decoration.Size;
								item.BorderWidth =
									float.Parse(decoration.Properties["BorderWidth"].ToString() ?? string.Empty);
								item.EditorOnly = false;
							}

							break;
						case "Line2D":
							var line = new Line2D();
							line.Position = decoration.Position;
							line.Scale = decoration.Scale;
							line.Rotation = decoration.Rotation;
							line.Width = float.Parse(decoration.Properties["Width"].ToString() ?? string.Empty);
							line.Points = decoration.Properties["Points"] is JsonElement pointsElement
								? pointsElement.EnumerateArray()
									.Select(p => new Vector2(p[0].GetSingle(), p[1].GetSingle()))
									.ToArray()
								: [];

							decorationsParent.AddChild(line);
							break;
					}
				}
			}
		}

		var labelsParent = node.GetNode<Node2D>("Labels");
		if (Labels != null) {
			foreach (var labelDto in Labels) {
				var label = new Label();
				labelsParent.AddChild(label);

				labelDto.ApplyTo(label);
			}
		}

		var approachSignsParent = node.GetNode<Node2D>("ApproachSigns");
		if (ApproachSigns != null) {
			foreach (var signDto in ApproachSigns) {
				var signScene = GD.Load<PackedScene>("res://objects/ApproachSign.tscn");
				var sign = signScene.Instantiate<ApproachSign>();
				approachSignsParent.AddChild(sign);

				signDto.ApplyTo(sign);
			}
		}

		var routeLightsParent = node.GetNode<Node2D>("RouteLights");
		if (RouteLights != null) {
			foreach (var lightDto in RouteLights) {
				var lightScene = GD.Load<PackedScene>("res://objects/RouteLight.tscn");
				var light = lightScene.Instantiate<RouteLight>();
				routeLightsParent.AddChild(light);

				lightDto.ApplyTo(light);
			}
		}

		var signalLightsParent = node.GetNode<Node2D>("SignalLights");
		if (SignalLights != null) {
			foreach (var lightDto in SignalLights) {
				var lightScene = GD.Load<PackedScene>("res://objects/SignalLight.tscn");
				var light = lightScene.Instantiate<SignalLight>();
				signalLightsParent.AddChild(light);

				lightDto.ApplyTo(light);
			}
		}

		var switchesParent = node.GetNode<Node2D>("Switches");
		if (Switches != null) {
			foreach (var switchDto in Switches) {
				var lightScene = GD.Load<PackedScene>("res://objects/Switch.tscn");
				var light = lightScene.Instantiate<Switch>();
				switchesParent.AddChild(light);

				switchDto.ApplyTo(light);
			}
		}
	}
	
	private void ApplyDecorationProperties(Node2D decorationNode, Dictionary<string, object> properties) {
		if (properties == null) return;

		switch (decorationNode) {
			case SimpleColorReplace simpleColorReplace:
				if (properties.ContainsKey("RouteColor") && properties["RouteColor"] is Color simpleRouteColor) {
					simpleColorReplace.RouteColor = simpleRouteColor;
				}

				break;

			case RouteEnd routeEnd:
				if (properties.ContainsKey("RouteColor") && properties["RouteColor"] is Color endRouteColor) {
					routeEnd.RouteColor = endRouteColor;
				}

				break;

			case RouteDiagonalSeparate routeDiagonalSeparate:
				if (properties.ContainsKey("Route1Color") && properties["Route1Color"] is Color route1Color) {
					routeDiagonalSeparate.Route1Color = route1Color;
				}

				if (properties.ContainsKey("Route2Color") && properties["Route2Color"] is Color route2Color) {
					routeDiagonalSeparate.Route2Color = route2Color;
				}

				break;
		}
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