namespace BIMIntelligence.Tools;

public class ToolResult
{
    public bool Success { get; init; }

    public string Data { get; init; } = string.Empty;

    public string? Error { get; init; }

    public static ToolResult Ok(string data)
    {
        return new ToolResult
        {
            Success = true,
            Data = data
        };
    }

    public static ToolResult Fail(string error)
    {
        return new ToolResult
        {
            Success = false,
            Error = error
        };
    }
}