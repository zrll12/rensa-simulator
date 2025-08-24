using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Logging;
using RensaSimulator.data.scene;

namespace RensaSimulator.data.game;

public class Train {
    public string Id { get; init; }
    public int EntryPoint { get; init; }
    public float EntryTime { get; init; }
    public bool Active { get; set; }

    public float Acceleration { get; init; }
    public float Braking { get; init; }

    public RoutePositionWithSection Position { get; set; }
    public float Speed { get; set; }
    public bool IsDownward { get; set; }
    
    public void Tick(double deltaTime) {
        if (!Active) return;
        
        // Update position based on speed and direction
        var distance = Speed * (float)deltaTime;
        if (IsDownward) {
            distance = -distance;
        }
        
        // Position = Position.MoveAlongRoute(distance);
        
        // Log current position for debugging
        GodotLogger.LogInfo($"Train {Id} at Route {Position.Route}, Position {Position.Position}, Speed {Speed}");
    }
}

public class TrainManager {
    private OrderedDictionary<string, Train> Trains { get; }
    private int _spawnedIndex;


    public TrainManager(TimeTable timeTable) {
        _spawnedIndex = 0;
        Trains = new OrderedDictionary<string, Train>();
        foreach (var entry in timeTable.Trains) {
            Trains.Add(entry.Id, entry.SpawnTrain());
        }
    }

    public void Tick(double deltaTime) {
        // Check for activation of new trains
        var oldTime = GameManager.Time;
        var newTime = oldTime + deltaTime;
        if (_spawnedIndex < Trains.Count) {
            var nextTrain = Trains.ElementAt(_spawnedIndex).Value;
            if (nextTrain.EntryTime >= oldTime && nextTrain.EntryTime < newTime) {
                nextTrain.Active = true;
                // nextTrain.Position = GameManager.CurrentRouteDto.GetEntryPointPosition(nextTrain.EntryPoint);
                _spawnedIndex++;
                
                GodotLogger.LogInfo($"Train {nextTrain.Id} activated at time {newTime}");
            }
        }
    }
}