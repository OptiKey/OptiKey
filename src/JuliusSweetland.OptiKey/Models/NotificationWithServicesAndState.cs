using JuliusSweetland.OptiKey.Services;
using Prism.Interactivity.InteractionRequest;

namespace JuliusSweetland.OptiKey.Models
{
    public class NotificationWithServicesAndState : INotification
    {
        public bool ModalWindow { get; set; }
        public IAudioService AudioService { get; set; }
        public IDictionaryService DictionaryService { get; set; }

        #region INotification

        public string Title { get; set; }
        public object Content { get; set; }

        #endregion
    }
}
