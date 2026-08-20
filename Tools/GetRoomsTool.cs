using System.Text.Json;
using BIMIntelligence.Revit;

namespace BIMIntelligence.Tools;

public class GetRoomsTool
{
    private readonly RoomQueryService _roomQueryService;

    public GetRoomsTool(RoomQueryService roomQueryService)
    {
        _roomQueryService = roomQueryService;
    }

    public ToolResult Execute(
        string? level = null,
        double? maxAreaSqm = null)
    {
        try
        {
            var rooms = _roomQueryService.GetRooms();

            if (!string.IsNullOrWhiteSpace(level))
            {
                rooms = rooms
                    .Where(room =>
                        string.Equals(
                            room.Level,
                            level,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (maxAreaSqm.HasValue)
            {
                rooms = rooms
                    .Where(room =>
                        room.AreaSqm < maxAreaSqm.Value)
                    .ToList();
            }

            var result = rooms.Select(room => new
            {
                room.Name,
                room.Number,
                room.Level,
                AreaSqm = Math.Round(room.AreaSqm, 2),
                room.DoorCount,
                room.WindowCount
            });

            return ToolResult.Ok(
                JsonSerializer.Serialize(result));
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(
                $"Unable to retrieve room data: {ex.Message}");
        }
    }
}