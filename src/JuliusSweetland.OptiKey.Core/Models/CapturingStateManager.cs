// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using log4net;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Models
{
    public class CapturingStateManager : BindableBase, ICapturingStateManager
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                    Log.DebugFormat("CapturingMultiKeySelection changed to {0}", value);

                    audioService.PlaySound(
                        value
                            ? Settings.Default.MultiKeySelectionCaptureStartSoundFile
                            : Settings.Default.MultiKeySelectionCaptureEndSoundFile,
                        value
                            ? Settings.Default.MultiKeySelectionCaptureStartSoundVolume
                            : Settings.Default.MultiKeySelectionCaptureEndSoundVolume);
                }
            }
        }
    }
}
