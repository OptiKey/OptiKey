// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.UI.ValueConverters
{
    /*
     * Computes symbol orientation for a symbol, based on current dock position     
     */
    public class DockPositionToSymbolOrientation : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 1
                && values.All(v => v != DependencyProperty.UnsetValue))
            {
                var dockPosition = (DockEdges)values[0];
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
            
            return SymbolOrientations.Top; //Default
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
