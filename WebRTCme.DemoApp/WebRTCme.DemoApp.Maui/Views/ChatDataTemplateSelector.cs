using WebRTCme.Middleware;

namespace WebRTCme.DemoApp.Maui.Views
{
    public class ChatDataTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate _incomingChatView = new DataTemplate(typeof(IncomingChatView));
        private readonly DataTemplate _outputChatView = new DataTemplate(typeof(OutgoingChatView));
        private readonly DataTemplate _systemChatView = new DataTemplate(typeof(SystemChatView));
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var dataParameters = item as DataParameters;
            if (dataParameters is null)
                return null;
            return dataParameters.From switch
            {
                DataFromType.System => _systemChatView,
                DataFromType.Incoming => _incomingChatView,
                DataFromType.Outgoing => _outputChatView,
                _ => throw new NotSupportedException()
            };
        }
    }
}
