using System;
using System.Linq;
using System.Windows;

namespace JuliusSweetland.ETTA.UI.Utilities
{
    /// <summary>
    /// Inherits ResourceDictionary, used to identify a theme resource dictionary in the merged dictionaries collection
    /// </summary>
    public class ThemeResourceDictionary : ResourceDictionary
    {
    }

    /// <summary>
    /// Credit to Svetoslav Savov for the ThemeSelector class
    /// http://svetoslavsavov.blogspot.co.uk/2009/07/switching-wpf-interface-themes-at.html
    /// </summary>
    public class ThemeSelector : DependencyObject
    {
        public static readonly DependencyProperty CurrentThemeDictionaryProperty =
            DependencyProperty.RegisterAttached("CurrentThemeDictionary", typeof (Uri),
                typeof (ThemeSelector),
                new UIPropertyMetadata(null, CurrentThemeDictionaryChanged));

        public static Uri GetCurrentThemeDictionary(DependencyObject obj)
        {
            return (Uri) obj.GetValue(CurrentThemeDictionaryProperty);
        }

        public static void SetCurrentThemeDictionary(DependencyObject obj, Uri value)
        {
            obj.SetValue(CurrentThemeDictionaryProperty, value);
        }

        private static void CurrentThemeDictionaryChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is FrameworkElement) // works only on FrameworkElement objects
            {
                ApplyTheme(obj as FrameworkElement, GetCurrentThemeDictionary(obj));
            }
        }

        private static void ApplyTheme(FrameworkElement targetElement, Uri dictionaryUri)
        {
            if (targetElement == null) return;

            ThemeResourceDictionary themeDictionary = null;
            if (dictionaryUri != null)
            {
                themeDictionary = new ThemeResourceDictionary {Source = dictionaryUri};

                // add the new dictionary to the collection of merged dictionaries of the target object
                targetElement.Resources.MergedDictionaries.Insert(0, themeDictionary);
            }

            // find if the target element already has a theme applied
            var existingDictionaries =
                (from dictionary in targetElement.Resources.MergedDictionaries.OfType<ThemeResourceDictionary>()
                    select dictionary).ToList();

            // remove the existing dictionaries 
            foreach (ThemeResourceDictionary thDictionary in existingDictionaries)
            {
                if (themeDictionary == thDictionary) continue; // don't remove the newly added dictionary
                targetElement.Resources.MergedDictionaries.Remove(thDictionary);
            }
        }
    }
}
