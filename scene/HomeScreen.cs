using Godot;

namespace RensaSimulator.scene;

public partial class HomeScreen : Node2D {
	public override void _Ready() { }
	public override void _Process(double delta) { }
	
	private void OnExit() {
		GetTree().Quit();
	}
	
	private void OnLoadExample() {
		GetTree().ChangeSceneToFile("res://scene/ExampleScene.tscn");
	}

	private void OnSinglePlayer() {
		GetTree().ChangeSceneToFile("res://scene/SceneSelectScreen.tscn");
	}
}
