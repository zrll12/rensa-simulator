using System;
using Godot;
using Godot.Logging;

namespace RensaSimulator.objects;

public partial class ApproachSign : Node2D {
	[Export]
	public String Label = "上り接近";
 
	public override void _Ready() {
		Label _label = GetNode<Label>("Label");
		_label.Text = Label;
	}

	public override void _Process(double delta) { }

	public void SetActivated(bool activated) {
		Sprite2D _light = GetNode<Sprite2D>("Light");
		_light.Visible = activated;
	}
}
