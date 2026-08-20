namespace BIMIntelligence.AI;

public static class ToolDefinitions
{
    public static object[] GetTools()
    {
        return
        [
            new
            {
                type = "function",

                function = new
                {
                    name = "get_rooms",

                    description =
                        "List individual rooms from the currently open Revit model. " +
                        "Use this tool when the user asks to list, filter, or inspect " +
                        "specific rooms. Supports filtering by level and maximum area. " +
                        "Do not use this tool merely to calculate aggregate room statistics." ,

                    parameters = new
                    {
                        type = "object",

                        properties = new
                        {
                            level = new
                            {
                                type = "string",
                                description =
                                    "Revit level name, for example L1 or L2."
                            },

                            max_area_sqm = new
                            {
                                type = "number",
                                description =
                                    "Return only rooms with area less than this value in square meters."
                            }
                        },

                        required = Array.Empty<string>()
                    }
                }
            },

            new
            {
                type = "function",

                function = new
                {
                    name = "get_level_door_counts",

            description =
                "Return the total number of doors associated with rooms on each Revit level. " +
                "Use this when the user asks about door counts by level or which level has the most doors.",

        parameters = new
                    {
                        type = "object",

                        properties = new { },

                        required = Array.Empty<string>()
                    }
                }
            },

            new
            {
                type = "function",

                function = new
                {
                    name = "get_room_statistics",

                description =
    "Calculate aggregate statistics for rooms on a Revit level, " +
    "including room count, average area, minimum area, and maximum area. " +
    "Use this tool when the user asks for average, minimum, maximum, " +
    "or statistical information about rooms." , 

                    parameters = new
                    {
                        type = "object",

                        properties = new
                        {
                            level = new
                            {
                                type = "string",
                                description =
                                    "Revit level name, for example L1 or L2."
                            }
                        },

                        required = new[]
                        {
                            "level"
                        }
                    }
                }
            },

            new
            {
                type = "function",

                function = new
                {
                    name = "get_rooms_without_windows",
        description =
            "Find all rooms in the current Revit model that have zero windows. " +
            "Use this when the user asks which rooms have no windows." , 

        parameters = new
                    {
                        type = "object",

                        properties = new { },

                        required = Array.Empty<string>()
                    }
                }
            }
        ];
    }
}