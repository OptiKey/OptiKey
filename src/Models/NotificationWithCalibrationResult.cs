using System;
using Prism.Interactivity.InteractionRequest;

namespace JuliusSweetland.OptiKey.Models
{
    public class NotificationWithCalibrationResult : INotification
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }

        #region INotification

        public string Title { get; set; }
        public object Content { get; set; }

        #endregion
    }
}
