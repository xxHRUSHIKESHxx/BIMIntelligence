using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BIMIntelligence.Commands
{
    [Transaction(TransactionMode.ReadOnly)]
    public class HelloWorldCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            TaskDialog.Show(
                "BIM Intelligence",
                "Hello World!\n\nThe Revit plugin is working."
            );

            return Result.Succeeded;
        }
    }
}
