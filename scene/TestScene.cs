using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using Godot.Logging;
using RensaSimulator.data;
using RensaSimulator.data.converter;
using RensaSimulator.objects;
using RensaSimulator.objects.route;
using Label = Godot.Label;

namespace RensaSimulator.scene;

public partial class TestScene : Node2D {
	private Map _mapData;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		LoadMapFromJson("export/test.json");

		DisplayMapInfo();

		CreateMapObjects();
	}

	private void LoadMapFromJson(string filePath) {
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

			_mapData = JsonSerializer.Deserialize<Map>(jsonContent, options);

			GodotLogger.LogInfo("Map data loaded!");
		}
		catch (Exception ex) {
			GodotLogger.LogError($"Failed to load map data: {ex.Message}");
			GodotLogger.LogError(ex.StackTrace);
		}
	}

	private void DisplayMapInfo() {
		if (_mapData == null) {
			GodotLogger.LogError("No map data loaded!");
			return;
		}

		GodotLogger.LogInfo($"{_mapData.Decorations?.Length ?? 0} Decorations");
		GodotLogger.LogInfo($"{_mapData.Labels?.Length ?? 0} Labels");
		GodotLogger.LogInfo($"{_mapData.ApproachSigns?.Length ?? 0} Approach Signs");
		GodotLogger.LogInfo($"{_mapData.RouteLights?.Length ?? 0} Route Lights");
		GodotLogger.LogInfo($"{_mapData.SignalLights?.Length ?? 0} Signal Lights");
		GodotLogger.LogInfo($"{_mapData.Switches?.Length ?? 0} Switches");
	}

	private void CreateMapObjects() {
		var decorationsParent = GetNode<Node2D>("Decorations");
		if (_mapData.Decorations != null) {
			foreach (var decoration in _mapData.Decorations) {
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

		var labelsParent = GetNode<Node2D>("Labels");
		if (_mapData.Labels != null) {
			foreach (var labelDto in _mapData.Labels) {
				var label = new Label();
				labelsParent.AddChild(label);

				labelDto.ApplyTo(label);
			}
		}

		var approachSignsParent = GetNode<Node2D>("ApproachSigns");
		if (_mapData.ApproachSigns != null) {
			foreach (var signDto in _mapData.ApproachSigns) {
				var signScene = GD.Load<PackedScene>("res://objects/ApproachSign.tscn");
				var sign = signScene.Instantiate<ApproachSign>();
				approachSignsParent.AddChild(sign);

				signDto.ApplyTo(sign);
			}
		}

		var routeLightsParent = GetNode<Node2D>("RouteLights");
		if (_mapData.RouteLights != null) {
			foreach (var lightDto in _mapData.RouteLights) {
				var lightScene = GD.Load<PackedScene>("res://objects/RouteLight.tscn");
				var light = lightScene.Instantiate<RouteLight>();
				routeLightsParent.AddChild(light);

				lightDto.ApplyTo(light);
			}
		}

		var signalLightsParent = GetNode<Node2D>("SignalLights");
		if (_mapData.SignalLights != null) {
			foreach (var lightDto in _mapData.SignalLights) {
				var lightScene = GD.Load<PackedScene>("res://objects/SignalLight.tscn");
				var light = lightScene.Instantiate<SignalLight>();
				signalLightsParent.AddChild(light);

				lightDto.ApplyTo(light);
			}
		}

		var switchesParent = GetNode<Node2D>("Switches");
		if (_mapData.Switches != null) {
			foreach (var switchDto in _mapData.Switches) {
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

// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) { }
}
