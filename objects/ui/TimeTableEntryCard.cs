using Godot;
using RensaSimulator.data;
using TimeTableEntry = RensaSimulator.data.scene.TimeTableEntry;

namespace RensaSimulator.objects.ui;

public partial class TimeTableEntryCard : Control {
	[Signal]
	public delegate void LoadTimeTableEventHandler(TimeTableEntry entry);
	
	private TimeTableEntry _timeTable;
	
	public TimeTableEntry TimeTable {
		get => _timeTable;
		set {
			_timeTable = value;
			UpdateSceneInfo();
		}
	}
	
	private void UpdateSceneInfo() {
		if (_timeTable == null) return;
		
		GetNode<Label>("HBoxContainer/VBoxContainer2/Name").Text = _timeTable.TimeTableName;
	}

	private void OnLoadButtonPressed() {
		EmitSignal(SignalName.LoadTimeTable, TimeTable);
	}
	
	public override Vector2 _GetMinimumSize() {
		return new Vector2(650, 40);
	}

	public override void _Ready() { }

	public override void _Process(double delta) { }
}
