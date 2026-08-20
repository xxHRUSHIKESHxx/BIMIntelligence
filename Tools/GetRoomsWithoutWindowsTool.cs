using System.Text.Json;
using BIMIntelligence.Revit;

namespace BIMIntelligence.Tools;

public class GetRoomsWithoutWindowsTool
{
    private readonly RoomQueryService _roomQueryService;

    public GetRoomsWithoutWindowsTool(
        RoomQueryService roomQueryService)
    {
        _roomQueryService = roomQueryService;
    }

    public ToolResult Execute()
    {
        try
        {
            var rooms =
                _roomQueryService.GetRoomsWithoutWindows();

            var result = rooms.Select(room => new
            {
                room.Name,
                room.Number,
                room.Level,
                AreaSqm = Math.Round(
                    room.AreaSqm,
                    2),
                room.DoorCount
            });

            return ToolResult.Ok(
                JsonSerializer.Serialize(result));
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(
                $"Unable to find rooms without windows: {ex.Message}");
        }
    }
}