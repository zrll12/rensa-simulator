namespace RensaSimulator.events;

public class TrainMoveInEvent : IEvent {
    public string TrainId { get; }
    public string SectionId { get; }

    public TrainMoveInEvent(string trainId, string sectionId) {
        TrainId = trainId;
        SectionId = sectionId;
    }
}