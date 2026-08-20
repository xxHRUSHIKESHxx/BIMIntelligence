using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMIntelligence.AI;
using BIMIntelligence.AI.Models;
using BIMIntelligence.Revit;
using BIMIntelligence.Tools;

namespace BIMIntelligence.Commands
{
    [Transaction(TransactionMode.ReadOnly)]
    public class GroqTestCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            try
            {
                const string question =
                    "Which rooms have no windows?";

                UIApplication uiApplication =
                    commandData.Application;

                UIDocument uiDocument =
                    uiApplication.ActiveUIDocument;

                Document document =
                    uiDocument.Document;

                // Revit services
                var roomService =
                    new RoomQueryService(document);

                var levelResolver =
                    new LevelResolver(document);

                var registry =
                    new ToolRegistry(
                        roomService,
                        levelResolver);

                // AI service
                var groqService =
                    new GroqService();

                // ------------------------------------------------
                // STEP 1: Ask Groq which tool to use
                // ------------------------------------------------

                string rawResponse =
                    groqService
                        .AskWithToolsAsync(question)
                        .GetAwaiter()
                        .GetResult();

                ToolCallRequest? toolCall =
                    groqService.ParseToolCall(
                        rawResponse);

                if (toolCall == null)
                {
                    TaskDialog.Show(
                        "BIM Intelligence",
                        "Groq did not request a tool.");

                    return Result.Succeeded;
                }

                GroqLogger.Log(
                    $"Tool selected: {toolCall.Name}");

                GroqLogger.Log(
                    $"Tool arguments: {toolCall.ArgumentsJson}");

                // ------------------------------------------------
                // STEP 2: Execute tool on Revit API thread
                // ------------------------------------------------

                ToolResult toolResult =
                    registry.Execute(toolCall);

                if (!toolResult.Success)
                {
                    TaskDialog.Show(
                        "Tool Error",
                        toolResult.Error
                        ?? "Unknown tool error.");

                    return Result.Failed;
                }

                GroqLogger.Log(
                    $"Tool execution successful.");

                GroqLogger.Log(
                    $"Tool result length: " +
                    $"{toolResult.Data.Length}");

                // ------------------------------------------------
                // STEP 3: Send tool result back to Groq
                // ------------------------------------------------

                string finalAnswer =
                    groqService.GetFinalResponse(
                        question,
                        toolCall,
                        toolResult);

                // ------------------------------------------------
                // STEP 4: Display final answer
                // ------------------------------------------------

                TaskDialog.Show(
                    "BIM Intelligence",
                    finalAnswer);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                GroqLogger.Log(
                    $"GROQ TEST FAILED: {ex}");

                message = ex.ToString();

                return Result.Failed;
            }
        }
    }
}