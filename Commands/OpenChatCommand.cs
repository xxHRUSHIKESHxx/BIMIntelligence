using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIMIntelligence.Revit;
using BIMIntelligence.UI;

namespace BIMIntelligence.Commands;

[Transaction(TransactionMode.ReadOnly)]
public class OpenChatCommand : IExternalCommand
{
    private static ChatWindow? _chatWindow;

    private static ExternalEvent? _externalEvent;

    private static ChatExternalEventHandler? _handler;

    public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
    {
        if (_chatWindow == null)
        {
            _handler =
                new ChatExternalEventHandler();

            _externalEvent =
                ExternalEvent.Create(
                    _handler);

            _chatWindow =
                new ChatWindow(
                    _externalEvent,
                    _handler);

            _chatWindow.Closed += (_, _) =>
            {
                _chatWindow = null;

                _externalEvent?.Dispose();

                _externalEvent = null;
                _handler = null;
            };

            _chatWindow.Show();
        }
        else
        {
            _chatWindow.Activate();
        }

        return Result.Succeeded;
    }
}