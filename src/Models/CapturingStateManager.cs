using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.Services;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.Models
{
    public class CapturingStateManager : BindableBase, ICapturingStateManager
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IAudioService audioService;

        public CapturingStateManager(IAudioService audioService)
        {
            this.audioService = audioService;
        }

        private bool capturingMultiKeySelection;
        public bool CapturingMultiKeySelection
        {
            get { return capturingMultiKeySelection; }
            set
            {
                if (SetProperty(ref capturingMultiKeySelection, value))
                {
                    Log.Debug(string.Format("CapturingMultiKeySelection changed to {0}", value));

                    audioService.PlaySound(value
                        ? Settings.Default.MultiKeySelectionCaptureStartSoundFile
                        : Settings.Default.MultiKeySelectionCaptureEndSoundFile);
                }
            }
        }
    }
}
