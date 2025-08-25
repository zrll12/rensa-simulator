using System;
using Godot;
using Godot.Logging;
using RensaSimulator.data.game;
using RensaSimulator.objects;

namespace RensaSimulator.scene;

public partial class GameScene : Node2D {
	public override void _Ready() {
		GenerateMap();
	}

	public override void _Process(double delta) { }

	public override void _PhysicsProcess(double delta) {
		GameManager.Tick(delta);
	}

	// Map
	private void GenerateMap() {
		GameManager.CurrentMap.ApplyTo(this);
	}


	// Inputs
	private bool _isMouseDown;
	private Vector2 _startPos = Vector2.Zero;
	private int _hoveredValue;
	private Switch _hoveredItem;
	private bool _endHoverWhilePressing;
	private Switch _preHoveredItem;

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
				OnMouseMotion(mouseMotion.Position);
				break;
			case InputEventScreenTouch touch:
				GodotLogger.LogInfo($"Touch Button Pressed: {touch.Pressed}");
				break;
		}
	}

	private void OnMouseAreaEntered(Area2D area) {
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

	private void OnMouseAreaExited(Area2D area) {
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

	private void OnMouseMotion(Vector2 mousePosition) {
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

	private void OnGetTrains() {
		GameManager.TrainManager.PrintTrains();
	}
}
