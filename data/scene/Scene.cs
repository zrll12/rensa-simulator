using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace RensaSimulator.data.scene;

public partial class Scene : GodotObject {
    public string SceneName { get; init; } = "UntitledScene";
    public string Author { get; init; } = "Unknown";
    public string Description { get; init; } = "No description provided.";
    public string Version { get; init; } = "1.0.0";
    public string MapFilePath { get; init; } = "";
    public string RouteFilePath { get; init; } = "";
    public TimeTableEntry[] TimeTables { get; init; }
    
    public static Scene? LoadSceneInfo(string filePath) {
        if (!File.Exists(filePath)) {
            GD.PrintErr($"Scene file not found: {filePath}");
            return null;
        }

        var jsonContent = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Deserialize<Scene>(jsonContent, options);
    }
}

public partial class TimeTableEntry : GodotObject {
    public string TimeTableName { get; init; } = "UntitledTimeTable";
    public string TimeTableFilePath { get; init; } = "TimeTable.json";
}