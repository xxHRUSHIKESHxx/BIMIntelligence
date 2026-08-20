using Autodesk.Revit.DB;

namespace BIMIntelligence.Models;

public class RoomData
{
    public ElementId Id { get; set; } = ElementId.InvalidElementId;

    public string Name { get; set; } = string.Empty;

    public string Number { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public double AreaSqm { get; set; }

    public int DoorCount { get; set; }

    public int WindowCount { get; set; }
}