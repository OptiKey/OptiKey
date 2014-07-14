using System.Linq;

namespace JuliusSweetland.ETTA.Extensions
{
    public static class CharExtensions
    {
        public static char ToUpperAndRemoveDiacritics(this char c)
        {
            //Convert char to string (for the next operations)
            var cAsString = c.ToString();

            //Remove diacritics
            cAsString.RemoveDiacritics();

            //Make uppercase
            cAsString = cAsString.ToUpper();

            //Convert back to char
            return cAsString.First();
        }
    }
}
