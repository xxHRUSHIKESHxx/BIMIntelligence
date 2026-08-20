using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using BIMIntelligence.Models;

namespace BIMIntelligence.Revit;

public class RoomQueryService
{
    private readonly Document _document;

    public RoomQueryService(Document document)
    {
        _document = document;
    }

    public List<RoomData> GetRooms()
    {
        var rooms = new FilteredElementCollector(_document)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .Cast<Room>()
            .Where(room => room.Area > 0)
            .ToList();

        var doorCounts = BuildElementCountsByRoom(
            BuiltInCategory.OST_Doors);

        var windowCounts = BuildElementCountsByRoom(
            BuiltInCategory.OST_Windows);

        var result = new List<RoomData>();

        foreach (Room room in rooms)
        {
            string levelName = room.Level?.Name ?? "Unknown";

            double areaSqm = UnitUtils.ConvertFromInternalUnits(
                room.Area,
                UnitTypeId.SquareMeters);

            int doorCount = doorCounts.TryGetValue(
                room.Id,
                out int doors)
                ? doors
                : 0;

            int windowCount = windowCounts.TryGetValue(
                room.Id,
                out int windows)
                ? windows
                : 0;

            result.Add(new RoomData
            {
                Id = room.Id,
                Name = room.Name,
                Number = room.Number,
                Level = levelName,
                AreaSqm = areaSqm,
                DoorCount = doorCount,
                WindowCount = windowCount
            });
        }

        return result;
    }

    public Dictionary<string, int> GetLevelDoorCounts()
    {
        var rooms = GetRooms();

        return rooms
            .GroupBy(room => room.Level)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(room => room.DoorCount));
    }

    public RoomStatistics? GetRoomStatistics(string levelName)
    {
        var rooms = GetRooms()
            .Where(room =>
                string.Equals(
                    room.Level,
                    levelName,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (rooms.Count == 0)
        {
            return null;
        }

        return new RoomStatistics
        {
            Level = levelName,
            RoomCount = rooms.Count,
            AverageAreaSqm = rooms.Average(room => room.AreaSqm),
            MinimumAreaSqm = rooms.Min(room => room.AreaSqm),
            MaximumAreaSqm = rooms.Max(room => room.AreaSqm)
        };
    }


    public List<RoomData> GetRoomsWithoutWindows()
    {
        return GetRooms()
            .Where(room => room.WindowCount == 0)
            .ToList();
    }

    public List<RoomData> GetRoomsByMaxArea(double maxAreaSqm)
    {
        return GetRooms()
            .Where(room => room.AreaSqm < maxAreaSqm)
            .ToList();
    }
    public List<RoomData> GetRoomsByLevel(string levelName)
    {
        return GetRooms()
            .Where(room =>
                string.Equals(
                    room.Level,
                    levelName,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
    private Dictionary<ElementId, int> BuildElementCountsByRoom(
        BuiltInCategory category)
    {
        var roomElements =
            new Dictionary<ElementId, HashSet<ElementId>>();

        var instances = new FilteredElementCollector(_document)
            .OfCategory(category)
            .WhereElementIsNotElementType()
            .OfType<FamilyInstance>();

        foreach (FamilyInstance instance in instances)
        {
            AddElementToRoom(
                roomElements,
                instance.Id,
                instance.Room);

            AddElementToRoom(
                roomElements,
                instance.Id,
                instance.FromRoom);

            AddElementToRoom(
                roomElements,
                instance.Id,
                instance.ToRoom);
        }

        return roomElements.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Count);
    }

    private static void AddElementToRoom(
        Dictionary<ElementId, HashSet<ElementId>> roomElements,
        ElementId elementId,
        Room? room)
    {
        if (room == null)
        {
            return;
        }

        if (!roomElements.TryGetValue(
                room.Id,
                out HashSet<ElementId>? elements))
        {
            elements = new HashSet<ElementId>();

            roomElements[room.Id] = elements;
        }

        elements.Add(elementId);
    }
}