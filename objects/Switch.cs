using System;
using Godot;
using Godot.Logging;

namespace RensaSimulator.objects;

public partial class Switch : Node2D {
	[Export] public Boolean IsSignalSwitch;

	[Export] public Boolean IsLn;
	[Export] public Boolean IsNr;

	[Export] public String InnerText = "1";
	[Export] public int Id = 1;

	private int _state; // -1: Left, 0: Center, 1: Right

	private static Vector2[] _allShape = [
		new(-50, 0),
		new(-200, -85),
		new(-155, -145),
		new(-100, -180),
		new(-35, -200),
		new(20, -200),
		new(85, -190),
		new(135, -155),
		new(175, -125),
		new(200, -85),
		new(45, 0),
		new(55, 35),
		new(40, 75),
		new(10, 90),
		new(-30, 85),
		new(-50, 60),
		new(-55, 35),
	];
	private static Vector2[] _lnShape = [
		new(-50, 0),
		new(-200, -85),
		new(-160, -140),
		new(-95, -185),
		new(-25, -205),
		new(45, -200),
		new(75, -185),
		new(15, -15),
		new(50, 10),
		new(55, 40),
		new(45, 70),
		new(10, 95),
		new(-30, 90),
		new(-55, 60),
		new(-55, 35),
	];
	private static Vector2[] _nrShape = [
		new(-15, -20),
		new(-80, -190),
		new(-55, -200),
		new(-15, -205),
		new(45, -200),
		new(70, -195),
		new(125, -170),
		new(145, -155),
		new(175, -125),
		new(200, -90),
		new(45, 5),
		new(55, 40),
		new(45, 75),
		new(10, 95),
		new(-35, 90),
		new(-60, 55),
		new(-60, 30),
		new(-50, 5),
		new(-45, -10),

	];

	public override void _Ready() {
		LabelSettings labelSettings = new LabelSettings();
		labelSettings.FontSize = 80 - (InnerText.Length - 1) * 20;
		labelSettings.FontColor = Colors.Black;

		GetNode<Sprite2D>("Switch/Black").Visible = !IsSignalSwitch;
		GetNode<Sprite2D>("Switch/Red").Visible = IsSignalSwitch;

		GetNode<Sprite2D>("Background/NCR").Visible = !IsSignalSwitch;
		GetNode<Sprite2D>("Background/LNR").Visible = IsSignalSwitch && !IsNr && !IsLn;
		GetNode<Sprite2D>("Background/LN").Visible = IsSignalSwitch && IsLn && !IsNr;
		GetNode<Sprite2D>("Background/NR").Visible = IsSignalSwitch && !IsLn && IsNr;

		GetNode<Label>("Switch/Label").Text = InnerText;
		GetNode<Label>("Switch/Label").LabelSettings = labelSettings;
		GetNode<Label>("Switch/Label").Position += InnerText.Length > 1
			? new Vector2(-6 + (InnerText.Length - 1) * -2, (InnerText.Length - 1) * 13)
			: Vector2.Zero;

		GetNode<Node2D>("Tooltip").Visible = false;
		
		if (IsLn) {
			GetNode<CollisionPolygon2D>("Collision/Shape").Polygon = _lnShape;
		} else if (IsNr) {
			GetNode<CollisionPolygon2D>("Collision/Shape").Polygon = _nrShape;
		} else {
			GetNode<CollisionPolygon2D>("Collision/Shape").Polygon = _allShape;
		}
	}

	public override void _Process(double delta) { }

	public int TurnLeft() {
		if (_state == -1 || (IsNr && _state == 0)) {
			return _state;
		}

		_state -= 1;
		process_turn();
		generate_tooltip();
		return _state;
	}

	public int TurnRight() {
		if (_state == 1 || (IsLn && _state == 0)) {
			return _state;
		}

		_state += 1;
		process_turn();
		generate_tooltip();
		return _state;
	}

	public void HoverEnd() {
		GetNode<Node2D>("Tooltip").Visible = false;
	}

	public void HoverStart() {
		generate_tooltip();
		GetNode<Node2D>("Tooltip").Visible = true;
	}

	private void process_turn() {
		GodotLogger.LogInfo($"Switch {Id} turned to state {_state}");
		GetNode<Node2D>("Switch").RotationDegrees = 50 * _state;
		// GetNode<Node2D>("Switch").Transform = GetNode<Node2D>("Switch").Transform.Rotated((float.Pi / 3f - 0.2f) * _state);
	}

	private void generate_tooltip() {
		if (IsLn) { // Use Label2
			GetNode<Label>("Tooltip/Label3").Visible = false;
			var label = GetNode<Label>("Tooltip/Label2");
			var stateTexts = _state == -1 ? "→" : "←";
			label.Text = $"L   {stateTexts}   N";
			label.Visible = true;
		} else if (IsNr) {
			GetNode<Label>("Tooltip/Label3").Visible = false;
			var label = GetNode<Label>("Tooltip/Label2");
			var stateTexts = _state == 1 ? "←" : "→";
			label.Text = $"N   {stateTexts}   R";
			label.Visible = true;
		} else { // Use Label3
			GetNode<Label>("Tooltip/Label2").Visible = false;
			var label = GetNode<Label>("Tooltip/Label3");
			string[] posTexts = IsSignalSwitch ? ["L", "N", "R"] : ["N", "C", "R"];
			string[] stateTexts = [
				_state == -1 ? "→" : "←",
				_state == 1 ? "←" : "→",
			];
			label.Text = $"{posTexts[0]} {stateTexts[0]} {posTexts[1]} {stateTexts[1]} {posTexts[2]}";
			label.Visible = true;
		}
	}
}
