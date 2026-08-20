namespace BIMIntelligence.AI.Models;

public class ToolCallRequest
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ArgumentsJson { get; set; } = string.Empty;
}