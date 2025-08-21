using System.Text.Json.Serialization;
using Godot;

namespace RensaSimulator.objects;

public partial class RouteLight : Node2D {
	[Export] public Vector2 Id = Vector2.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() { }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) { }
}
