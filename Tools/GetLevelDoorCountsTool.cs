using System.Text.Json;
using BIMIntelligence.Revit;

namespace BIMIntelligence.Tools;

public class GetLevelDoorCountsTool
{
    private readonly RoomQueryService _roomQueryService;

    public GetLevelDoorCountsTool(
        RoomQueryService roomQueryService)
    {
        _roomQueryService = roomQueryService;
    }

    public ToolResult Execute()
    {
        try
        {
            var counts = _roomQueryService.GetLevelDoorCounts();

            var result = counts
                .OrderByDescending(pair => pair.Value)
                .Select(pair => new
                {
                    Level = pair.Key,
                    DoorCount = pair.Value
                })
                .ToList();

            return ToolResult.Ok(
                JsonSerializer.Serialize(result));
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(
                $"Unable to retrieve door counts: {ex.Message}");
        }
    }
}