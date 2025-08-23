namespace RensaSimulator.data;

public class TimeTable {
    public TrainDetail[] Trains { get; init; }
}

public class TrainDetail {
    public int Id { get; init; }
    public float Acceleration { get; init; }
    public float Braking { get; init; }
    
    public int EntryPoint { get; init; }
    public float EntryTime { get; init; }
}