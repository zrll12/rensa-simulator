using System.Collections.Generic;
using System.Linq;
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

        float acceleration = 0;
        var speedLimit = GameManager.RouteManager.GetSectionById(Position.SectionId).SpeedLimit;
        if (Speed < speedLimit - 2.0) {
            acceleration = Acceleration;
        } else if (Speed > speedLimit - 1.5) {
            acceleration = -Braking;
        }

        Speed += acceleration * (float)deltaTime;

        // Update position based on speed and direction
        // d = v * t + 0.5 * a * t^2
        var distance = Speed * (float)deltaTime + 0.5F * acceleration * (float)(deltaTime * deltaTime);
        if (IsDownward) {
            distance = -distance;
        }

        var moveResult = GameManager.RouteManager.MoveAlong(Id, Position, distance);
        Position = moveResult.Item1;
        Active = moveResult.Item2;

        // Log current position for debugging
        // GodotLogger.LogInfo($"Train {Id} at Route {Position.Route}, Position {Position.Position}, Section: {Position.SectionId}, Speed {Speed}");
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
                var Position = GameManager.RouteManager.GetEntryExitPoint(nextTrain.EntryPoint);
                var Direction = nextTrain.IsDownward
                    ? RouteManager.SearchDirection.Downstream
                    : RouteManager.SearchDirection.Upstream;

                nextTrain.Active = true;
                nextTrain.Position = new RoutePositionWithSection {
                    Route = Position.Route,
                    Position = Position.Position,
                    SectionId = GameManager.RouteManager.GetSectionIdOfPosition(Position, Direction).Item1
                };

                _spawnedIndex++;

                GodotLogger.LogInfo($"Train {nextTrain.Id} activated at time {nextTrain.EntryTime}");
            }
        }

        // Tick all active trains
        foreach (var train in Trains.Values) {
            if (train.Active) {
                train.Tick(deltaTime);
            }
        }
    }

    public void PrintTrains() {
        foreach (var train in Trains.Values) {
            GodotLogger.LogInfo(
                $"Train {train.Id}: Active={train.Active}, Position=({train.Position?.Route}, {train.Position?.Position}), Speed={train.Speed}, IsDownward={train.IsDownward}");
        }
    }
}