using System.Text.Json;
using BIMIntelligence.AI.Models;
using BIMIntelligence.Models;
using BIMIntelligence.Revit;

namespace BIMIntelligence.Tools;

public class ToolRegistry
{
    private readonly RoomQueryService _roomQueryService;
    private readonly LevelResolver _levelResolver;

    public ToolRegistry(
        RoomQueryService roomQueryService,
        LevelResolver levelResolver)
    {
        _roomQueryService = roomQueryService;
        _levelResolver = levelResolver;
    }

    // ============================================================
    // LLM TOOL-CALL ENTRY POINT
    // ============================================================

    public ToolResult Execute(ToolCallRequest toolCall)
    {
        return Execute(
            toolCall.Name,
            toolCall.ArgumentsJson);
    }


    // ============================================================
    // GENERIC TOOL ENTRY POINT
    // ============================================================

    public ToolResult Execute(
        string toolName,
        string argumentsJson)
    {
        try
        {
            return toolName switch
            {
                "get_room_statistics" =>
                    ExecuteRoomStatistics(argumentsJson),

                "get_rooms" =>
                    ExecuteGetRooms(argumentsJson),

                "get_level_door_counts" =>
           ExecuteLevelDoorCounts(argumentsJson),

                "get_rooms_without_windows" =>
                    ExecuteRoomsWithoutWindows(argumentsJson),

                _ =>
                    ToolResult.Fail(
                        $"Unknown tool: {toolName}")
            };
        }
        catch (Exception ex)
        {
            return ToolResult.Fail(
                $"Tool '{toolName}' failed: {ex.Message}");
        }
    }


    // ============================================================
    // GET ROOM STATISTICS
    // ============================================================

    private ToolResult ExecuteRoomStatistics(
        string argumentsJson)
    {
        using JsonDocument document =
            JsonDocument.Parse(argumentsJson);

        string requestedLevel =
            document.RootElement
                .GetProperty("level")
                .GetString()
            ?? throw new InvalidOperationException(
                "The 'level' argument is required.");

        string resolvedLevel =
            _levelResolver.ResolveLevelName(
                requestedLevel);

        RoomStatistics? result =
            _roomQueryService.GetRoomStatistics(
                resolvedLevel);

        if (result == null)
        {
            return ToolResult.Fail(
                $"No rooms were found on level '{requestedLevel}'.");
        }

        string json =
            JsonSerializer.Serialize(
                result,
                new JsonSerializerOptions
                {
                    WriteIndented = false
                });

        return ToolResult.Ok(json);
    }


    // ============================================================
    // GET ROOMS
    // ============================================================

    private ToolResult ExecuteGetRooms(
        string argumentsJson)
    {
        using JsonDocument document =
            JsonDocument.Parse(argumentsJson);

        string? requestedLevel = null;

        if (document.RootElement.TryGetProperty(
                "level",
                out JsonElement levelElement))
        {
            requestedLevel =
                levelElement.GetString();
        }

        double? maxAreaSqm = null;

        if (document.RootElement.TryGetProperty(
                "maxAreaSqm",
                out JsonElement maxAreaElement))
        {
            maxAreaSqm =
                maxAreaElement.GetDouble();
        }

        List<RoomData> rooms;

        // Both filters
        if (!string.IsNullOrWhiteSpace(requestedLevel) &&
            maxAreaSqm.HasValue)
        {
            string resolvedLevel =
                _levelResolver.ResolveLevelName(
                    requestedLevel);

            rooms =
                _roomQueryService
                    .GetRoomsByLevel(resolvedLevel)
                    .Where(room =>
                        room.AreaSqm < maxAreaSqm.Value)
                    .ToList();
        }
        // Level only
        else if (!string.IsNullOrWhiteSpace(requestedLevel))
        {
            string resolvedLevel =
                _levelResolver.ResolveLevelName(
                    requestedLevel);

            rooms =
                _roomQueryService
                    .GetRoomsByLevel(resolvedLevel);
        }
        // Area only
        else if (maxAreaSqm.HasValue)
        {
            rooms =
                _roomQueryService
                    .GetRoomsByMaxArea(
                        maxAreaSqm.Value);
        }
        // No filters
        else
        {
            rooms =
                _roomQueryService.GetRooms();
        }

        string json =
            JsonSerializer.Serialize(
                rooms,
                new JsonSerializerOptions
                {
                    WriteIndented = false
                });

        return ToolResult.Ok(json);
    }

    private ToolResult ExecuteLevelDoorCounts(
    string argumentsJson)
    {
        var result =
            _roomQueryService.GetLevelDoorCounts();

        string json =
            JsonSerializer.Serialize(result);

        return ToolResult.Ok(json);
    }

    private ToolResult ExecuteRoomsWithoutWindows(
    string argumentsJson)
    {
        var result =
            _roomQueryService.GetRoomsWithoutWindows();

        string json =
            JsonSerializer.Serialize(result);

        return ToolResult.Ok(json);
    }
}