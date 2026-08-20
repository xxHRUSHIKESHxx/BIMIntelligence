using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIMIntelligence.Revit;
using BIMIntelligence.Tools;


namespace BIMIntelligence.Commands;

[Transaction(TransactionMode.ReadOnly)]
public class RoomDataCommand : IExternalCommand
{
    public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
    {
        UIApplication uiApp = commandData.Application;

        UIDocument? uiDoc = uiApp.ActiveUIDocument;

        if (uiDoc == null)
        {
            message = "There is no active Revit document.";
            return Result.Failed;
        }

        Document document = uiDoc.Document;

        //RoomQueryService roomQueryService =
        //    new RoomQueryService(document);

        var service =
     new RoomQueryService(document);

        var levelResolver =
            new LevelResolver(document);

        var registry =
            new ToolRegistry(
                service,
                levelResolver);

        ToolResult result =
            registry.Execute(
                "get_rooms",
                """
                {
                    "level": "L1",
                    "maxAreaSqm": 20
                }
                """);

        if (!result.Success)
        {
            TaskDialog.Show(
                "Tool Error",
                result.Error ?? "Unknown error.");

            return Result.Failed;
        }

        TaskDialog.Show(
            "Tool Registry Test",
            result.Data);
        return Result.Succeeded;
    }
}


//using Autodesk.Revit.Attributes;
//using Autodesk.Revit.DB;
//using Autodesk.Revit.UI;

//namespace BIMIntelligence.Commands;

//[Transaction(TransactionMode.ReadOnly)]
//public class RoomDataCommand : IExternalCommand
//{
//    public Result Execute(
//        ExternalCommandData commandData,
//        ref string message,
//        ElementSet elements)
//    {
//        UIApplication uiApp = commandData.Application;

//        UIDocument? uiDoc = uiApp.ActiveUIDocument;

//        if (uiDoc == null)
//        {
//            message = "There is no active Revit document.";
//            return Result.Failed;
//        }

//        Document document = uiDoc.Document;

//        string documentTitle = document.Title;

//        int elementCount = new FilteredElementCollector(document)
//            .WhereElementIsNotElementType()
//            .GetElementCount();

//        int roomCount = new FilteredElementCollector(document)
//            .OfCategory(BuiltInCategory.OST_Rooms)
//            .WhereElementIsNotElementType()
//            .GetElementCount();

//        TaskDialog.Show(
//            "Revit Model Information",
//            $"Document: {documentTitle}\n\n" +
//            $"Total elements: {elementCount}\n" +
//            $"Room elements: {roomCount}"
//        );

//        return Result.Succeeded;
//    }
//}