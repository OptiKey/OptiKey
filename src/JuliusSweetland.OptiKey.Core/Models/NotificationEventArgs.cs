// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.Models
{
    public class NotificationEventArgs
    {
        public NotificationEventArgs(string title, string content, NotificationTypes notificationType, Action callback)
        {
            Title = title;
            Content = content;
            NotificationType = notificationType;
            Callback = callback;
        }

        public string Title { get; private set; }
        public string Content { get; private set; }
        public NotificationTypes NotificationType { get; private set; }
        public Action Callback { get; private set; }
    }
}
