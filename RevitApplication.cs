using Autodesk.Revit.UI;
using System.Reflection;
using BIMIntelligence.Commands;



namespace BIMIntelligence;
public class RevitApplication : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        string assemblyPath = Assembly.GetExecutingAssembly().Location;

        RibbonPanel ribbonPanel =
            application.CreateRibbonPanel("BIM Intelligence");


        PushButtonData chatButtonData =
        new PushButtonData(
        "cmdOpenChat",
         "🤖\nBIM Chat",
        assemblyPath,
        "BIMIntelligence.Commands.OpenChatCommand");

        PushButton chatButton =
            ribbonPanel.AddItem(chatButtonData)
                as PushButton;

        chatButton.ToolTip =
            "Open the BIM Intelligence AI chatbot.";

        

        return Result.Succeeded;
    }


    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }
}