using Godot;
using RensaSimulator.data;

namespace RensaSimulator.objects.ui;

public partial class SceneEntryCard : Control {
	[Signal]
	public delegate void LoadSceneEventHandler(Scene scene);

	[Signal]
	public delegate void OpenSceneDetailsEventHandler(Scene scene);


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

		GetNode<Label>("HBoxContainer/VBoxContainer/HBoxContainer/Name").Text = _scene.SceneName;
		GetNode<Label>("HBoxContainer/VBoxContainer/HBoxContainer2/Description").Text = _scene.Description;
		GetNode<Label>("HBoxContainer/VBoxContainer/HBoxContainer2/Author").Text = _scene.Author;
		GetNode<Label>("HBoxContainer/VBoxContainer/HBoxContainer2/Version").Text = 'v' + _scene.Version;
	}

	private void OnLoadButtonPressed() {
		EmitSignal(SignalName.LoadScene, Scene);
	}

	private void OnOpenDetailsButtonPressed() {
		EmitSignal(SignalName.OpenSceneDetails, Scene);
	}

	public override Vector2 _GetMinimumSize() {
		return new Vector2(900, 90);
	}

	public override void _Ready() { }

	public override void _Process(double delta) { }
}
