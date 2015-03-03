using System;
using System.Windows;

namespace JuliusSweetland.OptiKey.UI.Windows
{
    /// <summary>
    /// Interaction logic for MagnifyWindow.xaml
    /// </summary>
    public partial class MagnifyWindow : Window
    {
        private readonly Point point;
        private readonly int horizontalPixels;
        private readonly int verticalPixels;
        private readonly Action<Point?> onSelectionAction;

        public MagnifyWindow()
        {
            InitializeComponent();
        }

        public MagnifyWindow(Point point, int horizontalPixels, int verticalPixels, Action<Point?> onSelectionAction) : this()
        {
            this.point = point;
            this.horizontalPixels = horizontalPixels;
            this.verticalPixels = verticalPixels;
            this.onSelectionAction = onSelectionAction;
        }
    }
}
