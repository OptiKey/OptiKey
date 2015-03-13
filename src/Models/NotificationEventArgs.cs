using System;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Models
{
    public class NotificationEventArgs
    {
        public NotificationEventArgs(string title, string content, NotificationTypes notificationType, Action onPopupClose)
        {
            Title = title;
            Content = content;
            NotificationType = notificationType;
            OnPopupClose = onPopupClose;
        }

        public string Title { get; private set; }
        public string Content { get; private set; }
        public NotificationTypes NotificationType { get; private set; }
        public Action OnPopupClose { get; private set; }
    }
}
