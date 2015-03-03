using System;
using System.Windows;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace JuliusSweetland.OptiKey.Models
{
    public class NotificationWithMagnificationArgs : INotification
    {
        public Point Point { get; set; }
        public int HorizontalPixels { get; set; }
        public int VerticalPixels { get; set; }
        public double FillHorizontalPercentageOfScreen { get; set; }
        public double FillVerticalPercentageOfScreen { get; set; }
        public Action<Point?> OnSelectionAction { get; set; }

        #region INotification

        public string Title { get; set; }
        public object Content { get; set; }

        #endregion
    }
}
