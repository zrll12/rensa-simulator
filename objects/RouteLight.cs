using System.Text.Json.Serialization;
using Godot;
using RensaSimulator.events;

namespace RensaSimulator.objects;

public partial class RouteLight : Node2D {
	[Export] public string Id;

	public override void _Ready() {
		EventManager.Instance.Subscribe<TrainMoveInEvent>(_on_train_entered);
	}

	public override void _Process(double delta) { }
	
	public void TurnYellow() {
		GetNode<Sprite2D>("Yellow").Visible = true;
		GetNode<Sprite2D>("Red").Visible = false;
		GetNode<Sprite2D>("Off").Visible = false;
	}
	
	public void TurnRed() {
		GetNode<Sprite2D>("Red").Visible = true;
		GetNode<Sprite2D>("Yellow").Visible = false;
		GetNode<Sprite2D>("Off").Visible = false;
	}
	
	public void TurnOff() {
		GetNode<Sprite2D>("Yellow").Visible = false;
		GetNode<Sprite2D>("Red").Visible = false;
		GetNode<Sprite2D>("Off").Visible = true;
	}

	private void _on_train_entered(TrainMoveInEvent e) {
		if (e.SectionId != Id) {
			return;
		}
		
		TurnRed();
	}
}
