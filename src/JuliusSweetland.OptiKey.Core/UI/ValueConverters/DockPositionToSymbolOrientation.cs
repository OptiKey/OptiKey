// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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
    public class DockPositionToSymbolOrientation : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var dockPosition = (DockEdges) value;
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
