using System.IO;
using Godot;
using RensaSimulator.data;

namespace RensaSimulator.objects.ui;

public partial class LoadSceneModal : Control {
	[Signal]
	public delegate void CancelLoadEventHandler();
	
	public string BaseFolder;
	
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

		var timeTablesNode = GetNode<VBoxContainer>("ScrollContainer/Container");
		foreach (Node child in timeTablesNode.GetChildren()) {
			child.QueueFree();
		}

		foreach (TimeTableEntry sceneTimeTable in Scene.TimeTables) {
			var paced = GD.Load<PackedScene>("res://objects/ui/TimeTableEntryCard.tscn");
			var card = paced.Instantiate<TimeTableEntryCard>();
			timeTablesNode.AddChild(card);
			
			card.TimeTable = sceneTimeTable;
			card.LoadTimeTable += OnLoadTimeTable;
		}
	}

	private void OnCancel() {
		EmitSignal(SignalName.CancelLoad);
	}

	private void OnLoadTimeTable(TimeTableEntry entry) {
		var file = File.ReadAllText(BaseFolder + entry.TimeTableFilePath);
		GD.Print(file);
	}
	
	public override void _Ready() { }

	public override void _Process(double delta) { }
}
