using Godot;
using RensaSimulator.data;

namespace RensaSimulator.objects.ui;

public partial class LoadSceneModal : Control {
	private Scene _scene;
	
	public Scene Scene {
		get => _scene;
		set {
			_scene = value;
			UpdateSceneInfo();
		}
	}
	
	private void UpdateSceneInfo() {
		if (_scene == null) return;
		
		GetNode<Label>("HBoxContainer/SceneName").Text = _scene.SceneName;
	}
	
	public override void _Ready() { }

	public override void _Process(double delta) { }
}
