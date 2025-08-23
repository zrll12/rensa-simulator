namespace RensaSimulator.data;

public class Route {
    public Section[] Sections { get; init; }
    public Turnout[] Turnouts { get; init; }
    public EntryExitPoint[] EntryExitPoints { get; init; }
}

public class Section {
    public int Id { get; init; }
    
    public int Route { get; init; }
    public float StartPosition { get; init; }
    public float EndPosition { get; init; }
}

public class Turnout {
    public int Id { get; init; }
    public int SwitchId { get; init; }
    public RoutePosition StaticPosition { get; init; }
    public RoutePosition NeutralPosition { get; init; }
    public RoutePosition ReversePosition { get; init; }
}

public class RoutePosition {
    public int Route { get; init; }
    public float Point { get; init; }
}

public class EntryExitPoint {
    public int Id { get; init; }
    public int Route { get; init; }
    public float Point { get; init; }
}