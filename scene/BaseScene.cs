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
using ApproachSign = RensaSimulator.objects.ApproachSign;
using Switch = RensaSimulator.objects.Switch;

namespace RensaSimulator.scene;

public partial class BaseScene : Node2D {
	private bool _isMouseDown;
	private Vector2 _startPos = Vector2.Zero;
	private int _hoveredValue;
	private Switch _hoveredItem;
	private bool _endHoverWhilePressing;
	private Switch _preHoveredItem;

	public override void _Ready() {
		var options = new JsonSerializerOptions {
			WriteIndented = true,
			ReferenceHandler = ReferenceHandler.IgnoreCycles,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};
		options.Converters.Add(new Vector2JsonConverter());
		options.Converters.Add(new Color2JsonConverter());

		var map = export_map();
		var json = JsonSerializer.Serialize(map, options);
		Directory.CreateDirectory("export");

		File.WriteAllText("export/test.json", json);
	}

	private void OnSwitchSceneButtonPressed() {
		GetTree().ChangeSceneToFile("res://scene/TestScene.tscn");
	}

	public override void _Process(double delta) { }

	public override void _Input(InputEvent @event) {
		switch (@event) {
			case InputEventMouseButton mouseButton:
				_isMouseDown = mouseButton.Pressed;
				_startPos = mouseButton.Position;

				if (mouseButton.IsPressed()) {
					_hoveredValue = Int32.MaxValue;
					break;
				}

				if (_endHoverWhilePressing && _hoveredItem != null) {
					_hoveredItem.HoverEnd();
					_hoveredItem = null;
					_endHoverWhilePressing = false;
				}

				if (_preHoveredItem != null) {
					_preHoveredItem.HoverStart();
					_hoveredItem = _preHoveredItem;
					_preHoveredItem = null;
				}

				break;
			case InputEventMouseMotion mouseMotion:
				GetNode<Node2D>("MousePosition").Position = mouseMotion.Position - GetViewportRect().Size / 2;
				on_mouse_motion(mouseMotion.Position);
				break;
			case InputEventScreenTouch touch:
				GodotLogger.LogInfo($"Touch Button Pressed: {touch.Pressed}");
				break;
		}
	}

	private void on_area_entered(Area2D area) {
		if (area is not Node2D node || node.GetParent() is not Switch parent) return;

		if (_isMouseDown) {
			_preHoveredItem = parent;
			return;
		}

		if (_hoveredItem != null && _hoveredItem.Id != parent.Id) {
			_hoveredItem.HoverEnd();
		}

		parent.HoverStart();
		_hoveredItem = parent;
	}

	private void on_area_exited(Area2D area) {
		if (area is not Node2D node || node.GetParent() is not Switch parent) return;

		if (_isMouseDown) {
			if (_hoveredItem.Id == parent.Id) {
				_endHoverWhilePressing = true;
				return;
			}

			if (_preHoveredItem.Id == parent.Id) {
				_preHoveredItem = null;
				return;
			}
		}

		if (_hoveredItem.Id == parent.Id) {
			parent.HoverEnd();
			_hoveredItem = null;
		}
	}

	private void on_mouse_motion(Vector2 mousePosition) {
		if (!_isMouseDown || _hoveredItem == null) return;

		var delta = mousePosition.X - _startPos.X;
		switch (delta) {
			case > 60:
				_startPos = mousePosition;
				_hoveredValue = _hoveredItem.TurnRight();
				break;
			case < -60:
				_startPos = mousePosition;
				_hoveredValue = _hoveredItem.TurnLeft();
				break;
		}
	}

	private Map export_map() {
		var decorations = GetNode<Node2D>("Decorations")
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

		var labels = GetNode<Node2D>("Labels")
			.GetChildren()
			.OfType<Label>()
			.Select(l => (LabelDto)l)
			.ToArray();

		var approachSigns = GetNode<Node2D>("ApproachSigns")
			.GetChildren()
			.OfType<ApproachSign>()
			.Select(sign => (ApproachSignDto)sign)
			.ToArray();

		var routeLights = GetNode<Node2D>("RouteLights")
			.GetChildren()
			.OfType<RouteLight>()
			.Select(light => (RouteLightDto)light)
			.ToArray();

		var signalLights = GetNode<Node2D>("SignalLights")
			.GetChildren()
			.OfType<SignalLight>()
			.Select(light => (SignalLightDto)light)
			.ToArray();

		var switches = GetNode<Node2D>("Switches")
			.GetChildren()
			.OfType<Switch>()
			.Select(sw => (SwitchDto)sw)
			.ToArray();

		var map = new Map {
			Decorations = decorations,
			Labels = labels,
			ApproachSigns = approachSigns,
			RouteLights = routeLights,
			SignalLights = signalLights,
			Switches = switches
		};

		return map;
	}
}
