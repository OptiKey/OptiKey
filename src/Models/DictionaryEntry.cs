namespace JuliusSweetland.OptiKey.Models
{
    public class DictionaryEntry
    {

        private readonly string entry;

        public DictionaryEntry(string entry) : this(entry, 0) { }
        public DictionaryEntry(string entry, int usageCount)
        {
            this.entry = entry;
            UsageCount = usageCount;
        }

        public string Entry {
            get { return entry; }
        }
        public int UsageCount { get; set; }
    }
}
