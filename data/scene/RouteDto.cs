using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using Godot.Logging;
using RensaSimulator.data.converter;

namespace RensaSimulator.data.scene;

public class RouteDto {
    public SectionDto[] Sections { get; init; }
    public TurnoutDto[] Turnouts { get; init; }
    public EntryExitPoint[] EntryExitPoints { get; init; }
    
    public RouteDto() {}
    public RouteDto(string filePath) {
        try {
            if (!File.Exists(filePath)) {
                GD.PrintErr($"File not found: {filePath}");
                return;
            }

            var jsonContent = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            options.Converters.Add(new Vector2JsonConverter());
		    
            var deserialize = JsonSerializer.Deserialize<RouteDto>(jsonContent, options);
            Sections = deserialize.Sections;
            Turnouts = deserialize.Turnouts;
            EntryExitPoints = deserialize.EntryExitPoints;
            DisplayRouteInfo();
            GodotLogger.LogInfo("Route data loaded!");
        }
        catch (Exception ex) {
            GodotLogger.LogError($"Failed to load map data: {ex.Message}");
            GodotLogger.LogError(ex.StackTrace);
        }
    }
    
    void DisplayRouteInfo() {
        GodotLogger.LogInfo($"{Sections.Length} Sections");
        GodotLogger.LogInfo($"{Turnouts.Length} Turnouts");
        GodotLogger.LogInfo($"{EntryExitPoints.Length} Entry/Exit Points");
    }
}

public class SectionDto {
    public string Id { get; init; }
    
    public int Route { get; init; }
    public float StartPosition { get; init; }
    public float EndPosition { get; init; }

    public int SpeedLimit { get; init; } = 120;
}

public class TurnoutDto {
    public string Id { get; init; }
    public int SwitchId { get; init; }
    public RoutePosition StaticPosition { get; init; }
    public RoutePosition NeutralPosition { get; init; }
    public RoutePosition ReversePosition { get; init; }
}

public class RoutePosition {
    public int Route { get; set; }
    public float Position { get; set; }

    public override bool Equals(object obj) {
        if(obj is not RoutePosition other) return base.Equals(obj);
        return this.Route == other.Route && Math.Abs(this.Position - other.Position) < 0.01f;
    }

    public RoutePosition(int route, float position) {
        Route = route;
        Position = position;
    }
}

public class EntryExitPoint {
    public int Id { get; init; }
    public int Route { get; init; }
    public float Point { get; init; }
}