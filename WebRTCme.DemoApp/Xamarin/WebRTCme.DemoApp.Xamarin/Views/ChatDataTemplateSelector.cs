using System;
using System.Collections.Generic;
using System.Text;
using WebRTCme.DemoApp.Xamarin.Enums;
using WebRTCme.DemoApp.Xamarin.Models;
using Xamarin.Forms;

namespace WebRTCme.DemoApp.Xamarin.Views
{
    public class ChatDataTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate _incomingChatView = new DataTemplate(typeof(IncomingChatView));
        private readonly DataTemplate _outputChatView = new DataTemplate(typeof(OutgoingChatView));
        private readonly DataTemplate _systemChatView = new DataTemplate(typeof(SystemChatView));
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var chatParameters = item as ChatParameters;
            if (chatParameters is null)
                return null;
            return chatParameters.ChatMessageType switch
            {
                ChatMessageType.System => _systemChatView,
                ChatMessageType.Incoming => _incomingChatView,
                ChatMessageType.Outgoing => _outputChatView,
                _ => throw new NotSupportedException()
            };
        }
    }
}
