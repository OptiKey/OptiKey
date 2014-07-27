using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace JuliusSweetland.ETTA.UI.Utilities
{
    public static class VisualAndLogicalTreeHelper
    {
        public static T FindVisualParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            // get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // we’ve reached the end of the tree
            if (parentObject == null) return null;

            // check if the parent matches the type we’re looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            
            // use recursion to proceed with next level
            return FindVisualParent<T>(parentObject);
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) 
            where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
