using System;
using Godot;

namespace RensaSimulator.objects;

public partial class ApproachSign : Node2D {
	private String _label = "上り接近";
		
	[Export] public String Label {
		get => _label;
		set {
			_label = value;
			UpdateLabel();
		}
	}
	
	public override void _Ready() {
	}

	public override void _Process(double delta) { }
	
	private void UpdateLabel() {
		var label = GetNode<Label>("Label");
		label.Text = Label;
	}

	public void SetActivated(bool activated) {
		Sprite2D light = GetNode<Sprite2D>("Light");
		light.Visible = activated;
	}
}
