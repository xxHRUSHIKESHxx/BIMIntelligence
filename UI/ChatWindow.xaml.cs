using Autodesk.Revit.UI;
using BIMIntelligence.Revit;
using System.Windows;
using System.Windows.Controls;

namespace BIMIntelligence.UI;

public partial class ChatWindow : Window
{
    private readonly ExternalEvent _externalEvent;

    private readonly ChatExternalEventHandler _handler;

    public ChatWindow(
        ExternalEvent externalEvent,
        ChatExternalEventHandler handler)
    {
        InitializeComponent();

        _externalEvent = externalEvent;

        _handler = handler;

        _handler.ResponseReady +=
            OnResponseReady;
    }

    private void SendButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string message =
            MessageInput.Text.Trim();

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        AddMessage(
            "You",
            message);

        MessageInput.Clear();

        _handler.UserMessage =
            message;

        SendButton.IsEnabled = false;
        SendButton.Content = "Thinking...";
        _externalEvent.Raise();
    }

    private void OnResponseReady(
        string response)
    {
        Dispatcher.BeginInvoke(
            new Action(() =>
            {
                AddMessage(
                    "BIM Intelligence",
                    response);

                SendButton.Content = "Send";
                SendButton.IsEnabled = true;
            }));
    }

    private void AddMessage(
       string sender,
       string message)
    {
        bool isUser =
            sender.Equals(
                "You",
                StringComparison.OrdinalIgnoreCase);

        var container =
            new StackPanel
            {
                Margin =
                    new Thickness(0, 0, 0, 12)
            };

        var senderText =
            new TextBlock
            {
                Text = sender,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(4, 0, 4, 4)
            };

        var messageText =
            new TextBlock
            {
                Text = message,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                Padding = new Thickness(10),
                MaxWidth = 430
            };

        var messageBorder =
            new Border
            {
                Child = messageText,
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                HorizontalAlignment =
                    isUser
                        ? HorizontalAlignment.Right
                        : HorizontalAlignment.Left
            };

        container.Children.Add(senderText);
        container.Children.Add(messageBorder);

        ChatPanel.Children.Add(container);

        ChatScrollViewer.ScrollToEnd();
    }

    private void MessageInput_KeyDown(
    object sender,
    System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key ==
            System.Windows.Input.Key.Enter)
        {
            SendButton_Click(
                SendButton,
                new RoutedEventArgs());

            e.Handled = true;
        }
    }

    protected override void OnClosed(
        EventArgs e)
    {
        _handler.ResponseReady -=
            OnResponseReady;

        base.OnClosed(e);
    }
}