namespace JuliusSweetland.OptiKey.Enums
{
    public enum UxModes
    {
        Standard,
        ConversationOnly
    }

    public static partial class EnumExtensions
    {
        public static string ToDescription(this UxModes uxMode)
        {
            switch (uxMode)
            {
                case UxModes.Standard: return "Standard";
                case UxModes.ConversationOnly: return "Conversation only";
            }

            return uxMode.ToString();
        }
    }
}
