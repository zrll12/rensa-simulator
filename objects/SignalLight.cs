using System;
using Godot;
using Godot.Logging;

namespace RensaSimulator.objects;

public partial class SignalLight : Node2D {
	[Export]
	public Boolean IsLine;
 
	public override void _Ready() {
		GetNode<Sprite2D>("Cross").Visible = !IsLine;
		GetNode<Sprite2D>("Line").Visible = IsLine;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) { }
}
