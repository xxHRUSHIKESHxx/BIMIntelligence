using System.Text.Json;
using BIMIntelligence.Revit;

namespace BIMIntelligence.Tools;

public class GetRoomStatisticsTool
{
    private readonly RoomQueryService _roomQueryService;

    public GetRoomStatisticsTool(
        RoomQueryService roomQueryService)
    {
        _roomQueryService = roomQueryService;
    }

    public ToolResult Execute(string level)
    {
        if (string.IsNullOrWhiteSpace(level))
        {
            return ToolResult.Fail(
                "A level must be provided.");
        }

        try
        {
            var statistics =
                _roomQueryService.GetRoomStatistics(level);

            if (statistics == null)
            {
                return ToolResult.Fail(
                    $"No rooms were found on level '{level}'.");
            }

            var result = new
            {
                statistics.Level,
                statistics.RoomCount,
                AverageAreaSqm =
                    Math.Round(
                        statistics.AverageAreaSqm,
                        2),
                MinimumAreaSqm =
                    Math.Round(
                        statistics.MinimumAreaSqm,
                        2),
                MaximumAreaSqm =
                    Math.Round(
                        statistics.MaximumAreaSqm,
                        2)
            };

            return ToolResult.Ok(
                JsonSerializer.Serialize(result));
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(
                $"Unable to calculate room statistics: {ex.Message}");
        }
    }
}