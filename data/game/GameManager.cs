using RensaSimulator.data.scene;

namespace RensaSimulator.data.game;

public static class GameManager {
    // Saves Data
    public static Map CurrentMap { get; set; } = null!;

    public static RouteDto CurrentRouteDto {
        set => RouteManager = new RouteManager(value);
    }

    public static TimeTable CurrentTimeTable {
        set => TrainManager = new TrainManager(value);
    }

    // Runtime Data
    public static double Time { get; set; }
    public static TrainManager TrainManager = null!;
    public static RouteManager RouteManager = null!;

    public static void Tick(double deltaTime) {
        TrainManager.Tick(deltaTime);
        Time += deltaTime;
    }
}