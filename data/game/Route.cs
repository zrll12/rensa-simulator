using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Logging;
using RensaSimulator.data.scene;

namespace RensaSimulator.data.game;

public class Section {
    public string Id { get; set; }
    public int Route { get; set; }

    public float UpEndPosition { get; set; } // in meters
    public float DownEndPosition { get; set; } // in meters
    public float Length => UpEndPosition - DownEndPosition;
    public int SpeedLimit { get; set; } // in km/h

    public string UpSideSectionId { get; set; }
    public string DownSideSectionId { get; set; }
    public bool IsUpSideTurnout { get; set; }
    public bool IsDownSideTurnout { get; set; }
}

public class Turnout {
    public string Id { get; set; }

    public string StaticPositionSectionId { get; set; }
    public string NeutralPositionSectionId { get; set; }
    public string ReversePositionSectionId { get; set; }

    public bool IsStaticPositionTurnout { get; set; }
    public bool IsNeutralPositionTurnout { get; set; }
    public bool IsReversePositionTurnout { get; set; }
    
    public bool IsStaticPositionDownStream { get; set; }
    public bool IsNeutralPositionDownStream { get; set; }
    public bool IsReversePositionDownStream { get; set; }

    public bool IsOnReversePosition { get; set; }
}

public class RoutePositionWithSection {
    public int Route { get; set; }
    public float Position { get; set; } // in meters
    public string SectionId { get; set; }
}

public class EntryExitPoint {
    public int Id { get; set; }
    public int Route { get; set; }
    public float Position { get; set; } // in meters
}

public class RouteManager {
    Dictionary<string, Section> Sections = new Dictionary<string, Section>();
    Dictionary<string, Turnout> Turnouts = new Dictionary<string, Turnout>();
    // Dictionary<int>

    public RouteManager(RouteDto routeDto) {
        // Add sections without upstream/downstream references
        foreach (var section in routeDto.Sections) {
            Sections.Add(section.Id, new Section {
                    Id = section.Id,
                    Route = section.Route,
                    UpEndPosition = section.EndPosition,
                    DownEndPosition = section.StartPosition,
                    SpeedLimit = section.SpeedLimit
                }
            );
        }

        // Add turnouts
        foreach (var routeDtoTurnout in routeDto.Turnouts) {
            Turnouts.Add(routeDtoTurnout.Id, new Turnout {
                Id = routeDtoTurnout.Id,
                IsOnReversePosition = false
            });

            // Set section references for each position if available
            var staticPos = GetSectionIdOfPosition(routeDtoTurnout.StaticPosition, SearchDirection.Both);
            if (staticPos.Item1 != null) {
                Turnouts[routeDtoTurnout.Id].StaticPositionSectionId = staticPos.Item1;
                Turnouts[routeDtoTurnout.Id].IsStaticPositionDownStream = staticPos.Item2 == SearchDirection.Downstream;
            }

            var neutralPos = GetSectionIdOfPosition(routeDtoTurnout.NeutralPosition, SearchDirection.Both);
            if (neutralPos.Item1 != null) {
                Turnouts[routeDtoTurnout.Id].NeutralPositionSectionId = neutralPos.Item1;
                Turnouts[routeDtoTurnout.Id].IsNeutralPositionDownStream = neutralPos.Item2 == SearchDirection.Downstream;
            }

            var reversePos = GetSectionIdOfPosition(routeDtoTurnout.ReversePosition, SearchDirection.Both);
            if (reversePos.Item1 != null) {
                Turnouts[routeDtoTurnout.Id].ReversePositionSectionId = reversePos.Item1;
                Turnouts[routeDtoTurnout.Id].IsReversePositionDownStream = reversePos.Item2 == SearchDirection.Downstream;
            }


            // Set turnout references for each position if available
            foreach (var dtoTurnout in routeDto.Turnouts) {
                if (dtoTurnout.Id == routeDtoTurnout.Id) continue;

                if (Turnouts[routeDtoTurnout.Id].StaticPositionSectionId == null &&
                    (Equals(routeDtoTurnout.StaticPosition, dtoTurnout.StaticPosition) ||
                     Equals(routeDtoTurnout.StaticPosition, dtoTurnout.NeutralPosition) ||
                     Equals(routeDtoTurnout.StaticPosition, dtoTurnout.ReversePosition))) {
                    Turnouts[routeDtoTurnout.Id].IsStaticPositionTurnout = true;
                    Turnouts[routeDtoTurnout.Id].StaticPositionSectionId = dtoTurnout.Id;
                }

                if (Turnouts[routeDtoTurnout.Id].NeutralPositionSectionId == null &&
                    (Equals(routeDtoTurnout.NeutralPosition, dtoTurnout.StaticPosition) ||
                     Equals(routeDtoTurnout.NeutralPosition, dtoTurnout.NeutralPosition) ||
                     Equals(routeDtoTurnout.NeutralPosition, dtoTurnout.ReversePosition))) {
                    Turnouts[routeDtoTurnout.Id].IsNeutralPositionTurnout = true;
                    Turnouts[routeDtoTurnout.Id].NeutralPositionSectionId = dtoTurnout.Id;
                }
                
                if (Turnouts[routeDtoTurnout.Id].ReversePositionSectionId == null &&
                    (Equals(routeDtoTurnout.ReversePosition, dtoTurnout.StaticPosition) ||
                     Equals(routeDtoTurnout.ReversePosition, dtoTurnout.NeutralPosition) ||
                     Equals(routeDtoTurnout.ReversePosition, dtoTurnout.ReversePosition))) {
                    Turnouts[routeDtoTurnout.Id].IsReversePositionTurnout = true;
                    Turnouts[routeDtoTurnout.Id].ReversePositionSectionId = dtoTurnout.Id;
                }
            }
            
            // Print turnout info for debugging
            // GodotLogger.LogInfo($"Turnout {routeDtoTurnout.Id}: " +
            //                    $"StaticPositionSectionId: {(Turnouts[routeDtoTurnout.Id].StaticPositionSectionId ?? "None")} " +
            //                    $"{(Turnouts[routeDtoTurnout.Id].IsStaticPositionTurnout ? "[Turnout]" : "")}, " +
            //                    $"NeutralPositionSectionId: {(Turnouts[routeDtoTurnout.Id].NeutralPositionSectionId ?? "None")} " +
            //                    $"{(Turnouts[routeDtoTurnout.Id].IsNeutralPositionTurnout ? "[Turnout]" : "")}, " +
            //                    $"ReversePositionSectionId: {(Turnouts[routeDtoTurnout.Id].ReversePositionSectionId ?? "None")} " +
            //                    $"{(Turnouts[routeDtoTurnout.Id].IsReversePositionTurnout ? "[Turnout]" : "")}");
        }

        // Then set upstream/downstream references
        foreach (var section in Sections.Values) {
            // Search other sections
            var upSection = GetSectionIdOfPosition(new RoutePosition(section.Route, section.UpEndPosition), SearchDirection.Upstream).Item1;
            var downSection = GetSectionIdOfPosition(new RoutePosition(section.Route, section.DownEndPosition), SearchDirection.Downstream).Item1;
            
            // Search turnouts
            if (upSection == null) {
                foreach (var turnout in Turnouts.Values) {
                    if (turnout.StaticPositionSectionId == section.Id && !turnout.IsStaticPositionDownStream) {
                        upSection = turnout.Id;
                        section.IsUpSideTurnout = true;
                        break;
                    }
                    if (turnout.NeutralPositionSectionId == section.Id && !turnout.IsNeutralPositionDownStream) {
                        upSection = turnout.Id;
                        section.IsUpSideTurnout = true;
                        break;
                    }
                    if (turnout.ReversePositionSectionId == section.Id && !turnout.IsReversePositionDownStream) {
                        upSection = turnout.Id;
                        section.IsUpSideTurnout = true;
                        break;
                    }
                }
            }

            if (downSection == null) {
                foreach (var turnout in Turnouts.Values) {
                    if (turnout.StaticPositionSectionId == section.Id && turnout.IsStaticPositionDownStream) {
                        downSection = turnout.Id;
                        section.IsDownSideTurnout = true;
                        break;
                    }
                    if (turnout.NeutralPositionSectionId == section.Id && turnout.IsNeutralPositionDownStream) {
                        downSection = turnout.Id;
                        section.IsDownSideTurnout = true;
                        break;
                    }
                    if (turnout.ReversePositionSectionId == section.Id && turnout.IsReversePositionDownStream) {
                        downSection = turnout.Id;
                        section.IsDownSideTurnout = true;
                        break;
                    }
                }
            }
            
            if (upSection != null) {
                section.UpSideSectionId = upSection;
            }
            if (downSection != null) {
                section.DownSideSectionId = downSection;
            }
            
            // Display linked section info for debugging
            // GodotLogger.LogInfo($"Section {section.Id} (Route {section.Route}, {section.DownEndPosition}m to {section.UpEndPosition}m): " +
            //                    $"Upstream: {(section.UpSideSectionId ?? "None")} {(section.IsUpSideTurnout ? "[Turnout]" : "")}, " +
            //                    $"Downstream: {(section.DownSideSectionId ?? "None")} {(section.IsDownSideTurnout ? "[Turnout]" : "")}");
        }
    }

    public (string, SearchDirection) GetSectionIdOfPosition(RoutePosition position, SearchDirection direction) {
        string resultSectionId = null;
        SearchDirection resultDirection = SearchDirection.Both;
        foreach (var section in Sections.Values.Where(section => section.Route == position.Route)) {
            // If totally in section
            if (section.DownEndPosition < position.Position && section.UpEndPosition > position.Position) {
                resultSectionId = section.Id;
                break;
            }

            // Find Upstream section
            if (direction == SearchDirection.Upstream || direction == SearchDirection.Both) {
                if (Math.Abs(section.DownEndPosition - position.Position) < 0.01f) {
                    resultSectionId = section.Id;
                    resultDirection = SearchDirection.Downstream;
                    break;
                }
            }

            // Find Downstream section
            if (direction == SearchDirection.Downstream || direction == SearchDirection.Both) {
                if (Math.Abs(section.UpEndPosition - position.Position) < 0.01f) {
                    resultSectionId = section.Id;
                    resultDirection = SearchDirection.Upstream;
                    break;
                }
            }
        }

        return (resultSectionId, resultDirection);
    }

    public enum SearchDirection {
        Upstream,
        Downstream,
        Both,
    }
}