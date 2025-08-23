using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using Godot.Logging;
using RensaSimulator.data;
using RensaSimulator.data.converter;
using Switch = RensaSimulator.objects.Switch;

namespace RensaSimulator.scene;

public partial class ExampleScene : Node2D {
	private bool _isMouseDown;
	private Vector2 _startPos = Vector2.Zero;
	private int _hoveredValue;
	private Switch _hoveredItem;
	private bool _endHoverWhilePressing;
	private Switch _preHoveredItem;

	public override void _Ready() { }

	private void OnSwitchSceneButtonPressed() {
		GetTree().ChangeSceneToFile("res://scene/TestScene.tscn");
	}

	private void OnExportMapButtonPressed() {
		var options = new JsonSerializerOptions {
			WriteIndented = true,
			ReferenceHandler = ReferenceHandler.IgnoreCycles,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};
		options.Converters.Add(new Vector2JsonConverter());
		options.Converters.Add(new Color2JsonConverter());

		var map = new Map(this);
		var json = JsonSerializer.Serialize(map, options);
		Directory.CreateDirectory("export");

		File.WriteAllText("export/test.json", json);

		GodotLogger.LogInfo("Map exported to export/test.json");
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
}
