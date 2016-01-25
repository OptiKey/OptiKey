using System.Diagnostics;

namespace JuliusSweetland.OptiKey.Models
{
    [DebuggerDisplay("'{Entry}' used {UsageCount}")]
    public class DictionaryEntry
    {
        public DictionaryEntry(string entry, int usageCount = 0)
        {
            Entry = entry;
            UsageCount = usageCount;
        }

        public string Entry { get; private set; }

        public int UsageCount { get; set; }
    }
}