using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Logging;
using RensaSimulator.data.scene;
using RensaSimulator.events;

namespace RensaSimulator.data.game;

public class Section {
    public required string Id { get; init; }
    public int Route { get; init; }

    public float UpEndPosition { get; init; } // in meters
    public float DownEndPosition { get; init; } // in meters
    public float Length => UpEndPosition - DownEndPosition;
    public int SpeedLimit { get; set; } // in km/h

    public string? UpSideSectionId { get; set; }
    public string? DownSideSectionId { get; set; }
    public bool IsUpSideTurnout { get; set; }
    public bool IsDownSideTurnout { get; set; }
}

public class Turnout {
    public required string Id { get; init; }

    public string? StaticPositionSectionId { get; set; }
    public string? NeutralPositionSectionId { get; set; }
    public string? ReversePositionSectionId { get; set; }

    public bool IsStaticPositionTurnout { get; set; }
    public bool IsNeutralPositionTurnout { get; set; }
    public bool IsReversePositionTurnout { get; set; }
    
    public bool IsStaticPositionDownStream { get; set; }
    public bool IsNeutralPositionDownStream { get; set; }
    public bool IsReversePositionDownStream { get; set; }

    public bool IsOnReversePosition { get; set; }
}

public class RoutePositionWithSection(int route, float position, string sectionId) {
    public int Route { get; set; } = route;
    public float Position { get; set; } = position; // in meters
    public string SectionId { get; set; } = sectionId;
}

public class EntryExitPoint {
    public int Id { get; set; }
    public int Route { get; set; }
    public float Position { get; set; } // in meters
}

public class RouteManager {
    private readonly Dictionary<string, Section> _sections = new Dictionary<string, Section>();
    private readonly Dictionary<string, Turnout> _turnouts = new Dictionary<string, Turnout>();
    private readonly Dictionary<int, RoutePosition> _entryExitPoints = new Dictionary<int, RoutePosition>();

    public RouteManager(RouteDto routeDto) {
        // Add sections without upstream/downstream references
        foreach (var section in routeDto.Sections) {
            _sections.Add(section.Id, new Section {
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
            _turnouts.Add(routeDtoTurnout.Id, new Turnout {
                Id = routeDtoTurnout.Id,
                IsOnReversePosition = false
            });

            // Set section references for each position if available
            var staticPos = GetSectionIdOfPosition(routeDtoTurnout.StaticPosition, SearchDirection.Both);
            if (staticPos.Item1 != null) {
                _turnouts[routeDtoTurnout.Id].StaticPositionSectionId = staticPos.Item1;
                _turnouts[routeDtoTurnout.Id].IsStaticPositionDownStream = staticPos.Item2 == SearchDirection.Downstream;
            }

            var neutralPos = GetSectionIdOfPosition(routeDtoTurnout.NeutralPosition, SearchDirection.Both);
            if (neutralPos.Item1 != null) {
                _turnouts[routeDtoTurnout.Id].NeutralPositionSectionId = neutralPos.Item1;
                _turnouts[routeDtoTurnout.Id].IsNeutralPositionDownStream = neutralPos.Item2 == SearchDirection.Downstream;
            }

            var reversePos = GetSectionIdOfPosition(routeDtoTurnout.ReversePosition, SearchDirection.Both);
            if (reversePos.Item1 != null) {
                _turnouts[routeDtoTurnout.Id].ReversePositionSectionId = reversePos.Item1;
                _turnouts[routeDtoTurnout.Id].IsReversePositionDownStream = reversePos.Item2 == SearchDirection.Downstream;
            }


            // Set turnout references for each position if available
            foreach (var dtoTurnout in routeDto.Turnouts) {
                if (dtoTurnout.Id == routeDtoTurnout.Id) continue;

                if (_turnouts[routeDtoTurnout.Id].StaticPositionSectionId == null &&
                    (Equals(routeDtoTurnout.StaticPosition, dtoTurnout.StaticPosition) ||
                     Equals(routeDtoTurnout.StaticPosition, dtoTurnout.NeutralPosition) ||
                     Equals(routeDtoTurnout.StaticPosition, dtoTurnout.ReversePosition))) {
                    _turnouts[routeDtoTurnout.Id].IsStaticPositionTurnout = true;
                    _turnouts[routeDtoTurnout.Id].StaticPositionSectionId = dtoTurnout.Id;
                }

                if (_turnouts[routeDtoTurnout.Id].NeutralPositionSectionId == null &&
                    (Equals(routeDtoTurnout.NeutralPosition, dtoTurnout.StaticPosition) ||
                     Equals(routeDtoTurnout.NeutralPosition, dtoTurnout.NeutralPosition) ||
                     Equals(routeDtoTurnout.NeutralPosition, dtoTurnout.ReversePosition))) {
                    _turnouts[routeDtoTurnout.Id].IsNeutralPositionTurnout = true;
                    _turnouts[routeDtoTurnout.Id].NeutralPositionSectionId = dtoTurnout.Id;
                }
                
                if (_turnouts[routeDtoTurnout.Id].ReversePositionSectionId == null &&
                    (Equals(routeDtoTurnout.ReversePosition, dtoTurnout.StaticPosition) ||
                     Equals(routeDtoTurnout.ReversePosition, dtoTurnout.NeutralPosition) ||
                     Equals(routeDtoTurnout.ReversePosition, dtoTurnout.ReversePosition))) {
                    _turnouts[routeDtoTurnout.Id].IsReversePositionTurnout = true;
                    _turnouts[routeDtoTurnout.Id].ReversePositionSectionId = dtoTurnout.Id;
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
            
            // Add Entry/Exit points
            foreach (var entryExit in routeDto.EntryExitPoints) {
                _entryExitPoints[entryExit.Id] = new RoutePosition(entryExit.Route, entryExit.Position);
            }
        }

        // Then set upstream/downstream references
        foreach (var section in _sections.Values) {
            // Search other sections
            var upSection = GetSectionIdOfPosition(new RoutePosition(section.Route, section.UpEndPosition), SearchDirection.Upstream).Item1;
            var downSection = GetSectionIdOfPosition(new RoutePosition(section.Route, section.DownEndPosition), SearchDirection.Downstream).Item1;
            
            // Search turnouts
            if (upSection == null) {
                foreach (var turnout in _turnouts.Values) {
                    if (turnout.StaticPositionSectionId == section.Id && !turnout.IsStaticPositionDownStream 
                        || turnout.NeutralPositionSectionId == section.Id && !turnout.IsNeutralPositionDownStream 
                        || turnout.ReversePositionSectionId == section.Id && !turnout.IsReversePositionDownStream) {
                        upSection = turnout.Id;
                        section.IsUpSideTurnout = true;
                        break;
                    }
                }
            }

            if (downSection == null) {
                foreach (var turnout in _turnouts.Values) {
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
            GodotLogger.LogInfo($"Section {section.Id} (Route {section.Route}, {section.DownEndPosition}m to {section.UpEndPosition}m): " +
                               $"Upstream: {(section.UpSideSectionId ?? "None")} {(section.IsUpSideTurnout ? "[Turnout]" : "")}, " +
                               $"Downstream: {(section.DownSideSectionId ?? "None")} {(section.IsDownSideTurnout ? "[Turnout]" : "")}");
        }
    }

    public (string?, SearchDirection) GetSectionIdOfPosition(RoutePosition position, SearchDirection direction) {
        string? resultSectionId = null;
        SearchDirection resultDirection = SearchDirection.Both;
        foreach (var section in _sections.Values.Where(section => section.Route == position.Route)) {
            // If totally in section
            if (section.DownEndPosition < position.Position && section.UpEndPosition > position.Position) {
                resultSectionId = section.Id;
                break;
            }

            // Find Upstream section
            if (direction is SearchDirection.Upstream or SearchDirection.Both) {
                if (Math.Abs(section.DownEndPosition - position.Position) < 0.01f) {
                    resultSectionId = section.Id;
                    resultDirection = SearchDirection.Downstream;
                    break;
                }
            }

            // Find Downstream section
            if (direction is SearchDirection.Downstream or SearchDirection.Both) {
                if (Math.Abs(section.UpEndPosition - position.Position) < 0.01f) {
                    resultSectionId = section.Id;
                    resultDirection = SearchDirection.Upstream;
                    break;
                }
            }
        }

        return (resultSectionId, resultDirection);
    }

    public (RoutePositionWithSection, bool) MoveAlong(string trainId, RoutePositionWithSection position, float distance) {
        var distanceLeft = distance;
        var lastSectionId = position.SectionId;
        var currentPosition = position;
        var isOnTurnout = false;
        var isActive = true;
        var isUpstream = distance > 0;

        while (distanceLeft != 0 || isOnTurnout) {
            if (isOnTurnout) {
                var turnout = _turnouts[currentPosition.SectionId];
                var isNextSectionTurnout = false;
                string? nextSectionId = null;
                if (turnout.StaticPositionSectionId == lastSectionId) {
                    nextSectionId = turnout.IsOnReversePosition ? turnout.ReversePositionSectionId : turnout.NeutralPositionSectionId;
                    isNextSectionTurnout = turnout.IsOnReversePosition
                        ? turnout.IsReversePositionTurnout
                        : turnout.IsNeutralPositionTurnout;
                }
                if (turnout.NeutralPositionSectionId == lastSectionId) {
                    if (turnout.IsOnReversePosition) {
                        isActive = false;
                        GodotLogger.LogInfo($"Train {trainId} moves into turnout {lastSectionId} that has been set to wrong direction.");
                        break;
                    }
                    nextSectionId = turnout.StaticPositionSectionId;
                    isNextSectionTurnout = turnout.IsStaticPositionTurnout;
                }
                if (turnout.ReversePositionSectionId == lastSectionId) {
                    if (!turnout.IsOnReversePosition) {
                        isActive = false;
                        GodotLogger.LogInfo($"Train {trainId} moves into turnout {lastSectionId} that has been set to wrong direction.");
                        break;
                    }
                    nextSectionId = turnout.StaticPositionSectionId;
                    isNextSectionTurnout = turnout.IsStaticPositionTurnout;
                }

                if (nextSectionId != null) {
                    var positionInNextSection = isUpstream
                        ? GetSectionById(nextSectionId)!.DownEndPosition
                        : GetSectionById(nextSectionId)!.UpEndPosition;
                
                    currentPosition.SectionId = nextSectionId;
                    currentPosition.Position = positionInNextSection;
                }

                isOnTurnout = isNextSectionTurnout;
                GodotLogger.LogInfo($"Train {trainId} moved through turnout {lastSectionId} to section {currentPosition.SectionId} at position {currentPosition.Position}m");
                
                var moveInEvent = new TrainMoveInEvent(trainId, currentPosition.SectionId);
                EventManager.Instance.Publish(moveInEvent);
                
                continue;
            }
            
            var currentSection = GetSectionById(currentPosition.SectionId);
            if (currentSection == null) {
                GodotLogger.LogError($"Train {trainId} is on an invalid section {currentPosition.SectionId}");
                isActive = false;
                break;
            }

            var distanceToEnd = distanceLeft switch {
                > 0 => currentSection.UpEndPosition - currentPosition.Position,
                < 0 => currentSection.DownEndPosition - currentPosition.Position,
                _ => 0F
            };

            if (Math.Abs(distanceToEnd) > Math.Abs(distanceLeft)) {
                // Move within the current section
                currentPosition.Position += distanceLeft;
                distanceLeft = 0;
            } else {
                // Move to the next section
                var nextSectionId =
                    distanceLeft > 0 ? currentSection.UpSideSectionId : currentSection.DownSideSectionId;
                if (nextSectionId == null) {
                    // Reached the end of the route
                    GodotLogger.LogInfo(
                        $"Train {trainId} has reached the end of the route at section {currentSection.Id}");
                    isActive = false;
                    break;
                }
                
                isOnTurnout = distanceLeft > 0 ? currentSection.IsUpSideTurnout : currentSection.IsDownSideTurnout;
                lastSectionId = currentSection.Id;
                distanceLeft -= distanceToEnd;
                currentPosition.Position = distanceLeft > 0 ? currentSection.UpEndPosition : currentSection.DownEndPosition;
                currentPosition.SectionId = nextSectionId;
                GodotLogger.LogInfo($"Train {trainId} moved to section {currentPosition.SectionId} at position {currentPosition.Position}m");
                
                var moveInEvent = new TrainMoveInEvent(trainId, currentPosition.SectionId);
                EventManager.Instance.Publish(moveInEvent);
            }
        }

        return (currentPosition, isActive);
    }
    
    public Section? GetSectionById(string id) {
        return _sections.GetValueOrDefault(id);
    }
    
    public RoutePosition? GetEntryExitPoint(int id) {
        return _entryExitPoints.GetValueOrDefault(id);
    }

    public enum SearchDirection {
        Upstream,
        Downstream,
        Both,
    }
}