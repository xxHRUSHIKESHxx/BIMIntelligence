namespace BIMIntelligence.Models;

public class RoomStatistics
{
    public string Level { get; set; } = string.Empty;

    public int RoomCount { get; set; }

    public double AverageAreaSqm { get; set; }

    public double MinimumAreaSqm { get; set; }

    public double MaximumAreaSqm { get; set; }
}