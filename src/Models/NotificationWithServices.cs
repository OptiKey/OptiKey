using JuliusSweetland.ETTA.Services;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;

namespace JuliusSweetland.ETTA.Models
{
    public class NotificationWithServices : INotification
    {
        public IAudioService AudioService { get; set; }
        public IDictionaryService DictionaryService { get; set; }

        #region INotification

        public string Title { get; set; }
        public object Content { get; set; }

        #endregion
    }
}
