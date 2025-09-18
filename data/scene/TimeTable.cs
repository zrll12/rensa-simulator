using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot.Logging;
using RensaSimulator.data.converter;
using RensaSimulator.data.game;

namespace RensaSimulator.data.scene;

public class TimeTable {
    public TrainDetail[] Trains { get; init; }
    
    public TimeTable() {
        Trains = [];
    }

    public TimeTable(string filePath) {
        Trains = [];
        try {
            if (!File.Exists(filePath)) {
                GodotLogger.LogError($"File not found: {filePath}");
                return;
            }

            var jsonContent = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            options.Converters.Add(new Vector2JsonConverter());
		    
            var deserialize = JsonSerializer.Deserialize<TimeTable>(jsonContent, options);
            Trains = deserialize!.Trains;
            DisplayRouteInfo();
            GodotLogger.LogInfo("Route data loaded!");
        }
        catch (Exception ex) {
            GodotLogger.LogError($"Failed to load map data: {ex.Message}");
            GodotLogger.LogError(ex.StackTrace);
        }
    }
    
    void DisplayRouteInfo() {
        GodotLogger.LogInfo($"{Trains.Length} Trains");
    }
}

public class TrainDetail {
    public required string Id { get; init; }
    public float Acceleration { get; init; }
    public float Braking { get; init; }
    public bool IsDownward { get; init; }
    
    public int EntryPoint { get; init; }
    public float EntryTime { get; init; }
    
    public Train SpawnTrain() {
        var entryPosition = GameManager.RouteManager.GetEntryExitPoint(EntryPoint)!;
        var direction = IsDownward
            ? RouteManager.SearchDirection.Downstream
            : RouteManager.SearchDirection.Upstream;
        var section = GameManager.RouteManager.GetSectionIdOfPosition(entryPosition, direction).Item1!;
        
        return new Train {
            Id = this.Id,
            EntryPoint = this.EntryPoint,
            EntryTime = this.EntryTime,
            Active = false,
            Acceleration = this.Acceleration,
            Braking = this.Braking,
            HeadPosition = new RoutePositionWithSection(entryPosition.Route, entryPosition.Position, section),
            TailPosition = null,
            Speed = 0f,
            IsDownward = this.IsDownward
        };
    }
}