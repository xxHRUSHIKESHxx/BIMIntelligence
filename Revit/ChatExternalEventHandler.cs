using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMIntelligence.AI;
using BIMIntelligence.AI.Models;
using BIMIntelligence.Tools;

namespace BIMIntelligence.Revit;

public class ChatExternalEventHandler : IExternalEventHandler
{
    private readonly GroqService _groqService;

    public string UserMessage { get; set; } = string.Empty;

    public event Action<string>? ResponseReady;

    public ChatExternalEventHandler()
    {
        _groqService = new GroqService();
    }

    public void Execute(UIApplication app)
    {
        try
        {
            Document document =
                app.ActiveUIDocument.Document;

            // ---------------------------------------------
            // 1. Create Revit services
            // ---------------------------------------------

            var roomService =
                new RoomQueryService(document);

            var levelResolver =
                new LevelResolver(document);

            var registry =
                new ToolRegistry(
                    roomService,
                    levelResolver);

            // ---------------------------------------------
            // 2. Ask Groq which tool to use
            // ---------------------------------------------

            string rawResponse =
                _groqService
                    .AskWithToolsAsync(
                        UserMessage)
                    .GetAwaiter()
                    .GetResult();

            // ---------------------------------------------
            // 3. Try to extract a tool call
            // ---------------------------------------------

            ToolCallRequest? toolCall;

            try
            {
                toolCall =
                    _groqService.ParseToolCall(
                        rawResponse);
            }
            catch (System.Text.Json.JsonException ex)
            {
                // Log technical error
                GroqLogger.Log(
                    $"Failed to parse Groq tool response: {ex}");

                RaiseResponse(
                    "I couldn't interpret the AI response. " +
                    "Please try asking the question again.");

                return;
            }

            // ---------------------------------------------
            // 4. Groq may answer directly
            // ---------------------------------------------

            if (toolCall == null)
            {
                try
                {
                    string finalAnswer =
                        _groqService.ParseFinalResponse(
                            rawResponse);

                    RaiseResponse(finalAnswer);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    GroqLogger.Log(
                        $"Failed to parse direct Groq response: {ex}");

                    RaiseResponse(
                        "I couldn't generate a response. " +
                        "Please try asking the question again.");
                }

                return;
            }

            // ---------------------------------------------
            // 5. Execute Revit tool
            // ---------------------------------------------

            ToolResult toolResult =
                registry.Execute(toolCall);

            // ---------------------------------------------
            // 6. Handle tool failure
            // ---------------------------------------------

            if (!toolResult.Success)
            {
                GroqLogger.Log(
                    $"Tool execution failed. " +
                    $"Tool: {toolCall.Name}. " +
                    $"Error: {toolResult.Error}");

                string friendlyResponse =
                    BuildUnsupportedToolResponse(
                        UserMessage,
                        toolCall,
                        toolResult);

                RaiseResponse(friendlyResponse);

                return;
            }

            // ---------------------------------------------
            // 7. Send tool result back to Groq
            // ---------------------------------------------

            string finalResponse =
                _groqService.GetFinalResponse(
                    UserMessage,
                    toolCall,
                    toolResult);

            // ---------------------------------------------
            // 8. Send final answer to UI
            // ---------------------------------------------

            RaiseResponse(finalResponse);
        }
        catch (Exception ex)
        {
            GroqLogger.Log(
                $"Chat execution failed: {ex}");

            RaiseResponse(
                "I couldn't process that request. " +
                "Please try asking the question again.");
        }
    }


    private string BuildUnsupportedToolResponse(
    string userMessage,
    ToolCallRequest toolCall,
    ToolResult toolResult)
    {
        return
              "I don't currently have a Revit tool that can answer " +
        "that question. I can currently help with room counts, " +
        "room areas, doors, windows, and level-specific " +
        "information.";
    }
    private void RaiseResponse(string response)
    {
        ResponseReady?.Invoke(response);
    }

    public string GetName()
    {
        return "BIM Intelligence Chat Handler";
    }
}