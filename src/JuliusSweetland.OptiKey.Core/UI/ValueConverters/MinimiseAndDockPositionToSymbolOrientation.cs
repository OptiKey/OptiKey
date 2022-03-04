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
     * Computes symbol orientation for a 'Minimise' symbol, based on current dock position
     * and the preference for Optikey gets minimis
     */
    public class MinimiseAndDockPositionToSymbolOrientation : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2
                && values.All(v => v != DependencyProperty.UnsetValue))
            {
                var minimisedPosition = (MinimisedEdges)values[0];
                var dockPosition = (DockEdges)values[1];

                switch (minimisedPosition == MinimisedEdges.SameAsDockedPosition ? dockPosition.ToMinimisedEdge() : minimisedPosition)
                {
                    case MinimisedEdges.Right:
                        return SymbolOrientations.Right;

                    case MinimisedEdges.Bottom:
                        return SymbolOrientations.Bottom;

                    case MinimisedEdges.Left:
                        return SymbolOrientations.Left;

                    default: //case MinimisedEdges.Top:
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
