using System.IO;
using System.Linq;
using Godot;
using RensaSimulator.data;
using RensaSimulator.objects.ui;

namespace RensaSimulator.scene;

public partial class SceneSelectScreen : Node2D {
	private string _selectedFolderPath;
	private bool _modalOpened = false;

	private string SelectedFolderPath {
		get => _selectedFolderPath;
		set {
			_selectedFolderPath = value;
			UpdateSelectedFolderPath();
		}
	}

	public override void _Ready() {
		SelectedFolderPath = ProjectSettings.GlobalizePath("user://scenes");
	}

	public override void _Process(double delta) { }

	private void UpdateSelectedFolderPath() {
		if (!_selectedFolderPath.EndsWith('/')) {
			_selectedFolderPath += '/';
		}
		GetNode<LineEdit>("LineEdit").Text = _selectedFolderPath;
		OnLoadFiles();
	}

	private void OnSelectFolder() {
		DisplayServer.FileDialogShow("Select Scene Folder", SelectedFolderPath, "*", true,
			DisplayServer.FileDialogMode.OpenDir, [],
			Callable.From((bool status, string[] paths, int _) => {
				if (status && paths.Length > 0) {
					SelectedFolderPath = paths[0];
				}
			}));
	}

	private void OnResetDefault() {
		SelectedFolderPath = ProjectSettings.GlobalizePath("user://scenes");
	}

	private void OnLoadFiles() {
		Directory.CreateDirectory(SelectedFolderPath);
		var dir = Directory.EnumerateDirectories(SelectedFolderPath)
			.Select(path => {
				var sceneFile = Scene.LoadSceneInfo(path + "/scene.json");
				return sceneFile;
			}).ToArray();
		
		var container = GetNode<VBoxContainer>("ScrollContainer/EntriesContainer");
		foreach (var node in container.GetChildren()) {
			node.QueueFree();
		}

		foreach (var sceneInfo in dir) {
			var pack = GD.Load<PackedScene>("res://objects/ui/SceneEntryCard.tscn");
			var entry = pack.Instantiate<SceneEntryCard>();
			container.AddChild(entry);
			
			entry.Scene = sceneInfo;
			entry.LoadScene += OnLoadScene;
		}
	}
	
	private void OnLoadScene(Scene scene) {
		if (_modalOpened) return;
		_modalOpened = true;
		var cover = GetNode<ColorRect>("ModalCover");
		cover.Visible = true;
		
		var container = GetNode<VBoxContainer>("ModalCover/Container");

		var modal = GD.Load<PackedScene>("res://objects/ui/LoadSceneModal.tscn").Instantiate<LoadSceneModal>();
		modal.Scene = scene;
		container.AddChild(modal);
	}

	private void OnCancel() {
		GetTree().ChangeSceneToFile("res://scene/HomeScreen.tscn");
	}
}
