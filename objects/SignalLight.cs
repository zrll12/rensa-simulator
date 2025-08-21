using System;
using System.Text.Json.Serialization;
using Godot;
using Godot.Logging;

namespace RensaSimulator.objects;

public partial class SignalLight : Node2D {
	private Boolean _isLine;

	[Export]
	public Boolean IsLine {
		get => _isLine;
		set {
			_isLine = value;
			UpdateIsLine();
		}
	}
	[Export] public String Id;
	
	private void UpdateIsLine() {
		GetNode<Sprite2D>("Cross").Visible = !IsLine;
		GetNode<Sprite2D>("Line").Visible = IsLine;
	}

	public override void _Ready() { }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) { }
}
