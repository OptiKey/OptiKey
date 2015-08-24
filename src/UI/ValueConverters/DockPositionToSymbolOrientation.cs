using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    public class DockPositionToSymbolOrientation : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var dockPosition = (DockEdges)values[0];;

            if (values.Length == 1 || (WindowStates)values[1] == WindowStates.Docked)
            {
                switch (dockPosition)
                {
                    case DockEdges.Right:
                        return SymbolOrientations.Right;

                    case DockEdges.Bottom:
                        return SymbolOrientations.Bottom;

                    case DockEdges.Left:
                        return SymbolOrientations.Left;

                    default: //case DockEdges.Top:
                        return SymbolOrientations.Top;
                }
            }
            
            return SymbolOrientations.Top;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
