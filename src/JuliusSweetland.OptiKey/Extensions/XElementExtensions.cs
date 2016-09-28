using System.Globalization;
using System.Linq;
using WindowsInput.Native;
using JuliusSweetland.OptiKey.Enums;
using System.Xml.Linq;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class XElementExtensions
    {
        // Helper methods to get optional elements from XML
            public static string GetStringElementIfExists(this XElement @this, string element, string defaultString)
            {
                return @this.Element(element) != null ? @this.Element(element).Value : defaultString;
            }

            public static int GetIntElementIfExists(this XElement @this, string element, int defaultInt)
            {
                return @this.Element(element) != null ? (int)@this.Element(element) : defaultInt;
            }

            public static double GetDoubleElementIfExists(this XElement @this, string element, double defaultInt)
            {
                return @this.Element(element) != null ? (int)@this.Element(element) : defaultInt;
            }

            public static bool ElementExists(this XElement @this, string element)
            {
                return @this.Element(element) != null;
            }
        }
    
}
