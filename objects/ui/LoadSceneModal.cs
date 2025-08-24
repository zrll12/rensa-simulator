using System.IO;
using Godot;
using RensaSimulator.data;
using RensaSimulator.data.game;
using RensaSimulator.data.scene;
using RensaSimulator.scene;
using Scene = RensaSimulator.data.scene.Scene;
using TimeTableEntry = RensaSimulator.data.scene.TimeTableEntry;

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
		GameManager.CurrentMap = new Map(BaseFolder + _scene.MapFilePath);
		GameManager.CurrentRouteDto = new RouteDto(BaseFolder + _scene.RouteFilePath);
		GameManager.CurrentTimeTable = new TimeTable(BaseFolder + entry.TimeTableFilePath);

		GetTree().ChangeSceneToPacked(GD.Load<PackedScene>("res://scene/GameScene.tscn"));
	}
	
	public override void _Ready() { }

	public override void _Process(double delta) { }
}
