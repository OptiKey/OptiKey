namespace JuliusSweetland.OptiKey.Models
{
    public class DictionaryEntry
    {

        public DictionaryEntry(string entry) : this(entry, 0) { }
        public DictionaryEntry(string entry, int usageCount)
        {
            Entry = entry;
            UsageCount = usageCount;
        }

        public string Entry { get; }
        public int UsageCount { get; set; }
    }
}
