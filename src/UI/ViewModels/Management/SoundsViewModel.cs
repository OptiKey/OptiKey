using System.Collections.Generic;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Management
{
    public class SoundsViewModel : BindableBase
    {
        #region Private Member Vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IAudioService audioService;

        #endregion
        
        #region Ctor

        public SoundsViewModel(IAudioService audioService)
        {
            this.audioService = audioService;
            
            Load();
        }
        
        #endregion
        
        #region Properties

        public List<string> SpeechVoices
        {
            get { return audioService.GetAvailableVoices(); }
        }
        
        public List<KeyValuePair<string, string>> InfoSoundFiles
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Info 1", @"Resources\Sounds\Info1.wav"),
                    new KeyValuePair<string, string>("Info 2", @"Resources\Sounds\Info2.wav")
                };
            }
        }
        
        public List<KeyValuePair<string, string>> SelectionSoundFiles
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Selection 1", @"Resources\Sounds\Selection1.wav"),
                    new KeyValuePair<string, string>("Selection 2", @"Resources\Sounds\Selection2.wav")
                };
            }
        }
        
        public List<KeyValuePair<string, string>> ErrorSoundFiles
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Error 1", @"Resources\Sounds\Error1.wav"),
                    new KeyValuePair<string, string>("Error 2", @"Resources\Sounds\Error2.wav")
                };
            }
        }
        
        public List<KeyValuePair<string, string>> MultiKeySelectionCaptureStartSoundFiles
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Start Capture 1", @"Resources\Sounds\MultiKeyCaptureStart1.wav")
                };
            }
        }
        
        public List<KeyValuePair<string, string>> MultiKeySelectionCaptureEndSoundFiles
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("End Capture 1", @"Resources\Sounds\MultiKeyCaptureEnd1.wav")
                };
            }
        }
        
        private string speechVoice;
        public string SpeechVoice
        {
            get { return speechVoice; }
            set { SetProperty(ref speechVoice, value); }
        }
        
        private int speechVolume;
        public int SpeechVolume
        {
            get { return speechVolume; }
            set { SetProperty(ref speechVolume, value); }
        }

        private int speechRate;
        public int SpeechRate
        {
            get { return speechRate; }
            set { SetProperty(ref speechRate, value); }
        }
        
        private string infoSoundFile;
        public string InfoSoundFile
        {
            get { return infoSoundFile; }
            set { SetProperty(ref infoSoundFile, value); }
        }

        private string selectionSoundFile;
        public string SelectionSoundFile
        {
            get { return selectionSoundFile; }
            set { SetProperty(ref selectionSoundFile, value); }
        }

        private string errorSoundFile;
        public string ErrorSoundFile
        {
            get { return errorSoundFile; }
            set { SetProperty(ref errorSoundFile, value); }
        }

        private string multiKeySelectionCaptureStartSoundFile;
        public string MultiKeySelectionCaptureStartSoundFile
        {
            get { return multiKeySelectionCaptureStartSoundFile; }
            set { SetProperty(ref multiKeySelectionCaptureStartSoundFile, value); }
        }

        private string multiKeySelectionCaptureEndSoundFile;
        public string MultiKeySelectionCaptureEndSoundFile
        {
            get { return multiKeySelectionCaptureEndSoundFile; }
            set { SetProperty(ref multiKeySelectionCaptureEndSoundFile, value); }
        }

        public bool ChangesRequireRestart
        {
            get { return false; }
        }
        
        #endregion
        
        #region Methods

        private void Load()
        {
            SpeechVoice = Settings.Default.SpeechVoice;
            SpeechVolume = Settings.Default.SpeechVolume;
            SpeechRate = Settings.Default.SpeechRate;
            InfoSoundFile = Settings.Default.InfoSoundFile;
            SelectionSoundFile = Settings.Default.SelectionSoundFile;
            ErrorSoundFile = Settings.Default.ErrorSoundFile;
            MultiKeySelectionCaptureStartSoundFile = Settings.Default.MultiKeySelectionCaptureStartSoundFile;
            MultiKeySelectionCaptureEndSoundFile = Settings.Default.MultiKeySelectionCaptureEndSoundFile;
        }

        public void ApplyChanges()
        {
            Settings.Default.SpeechVoice = SpeechVoice;
            Settings.Default.SpeechVolume = SpeechVolume;
            Settings.Default.SpeechRate = SpeechRate;
            Settings.Default.InfoSoundFile = InfoSoundFile;
            Settings.Default.SelectionSoundFile = SelectionSoundFile;
            Settings.Default.ErrorSoundFile = ErrorSoundFile;
            Settings.Default.MultiKeySelectionCaptureStartSoundFile = MultiKeySelectionCaptureStartSoundFile;
            Settings.Default.MultiKeySelectionCaptureEndSoundFile = MultiKeySelectionCaptureEndSoundFile;
            Settings.Default.Save();
        }

        #endregion
    }
}
