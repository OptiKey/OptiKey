using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Properties;
using log4net;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.UI.ViewModels.Management
{
    public class VisualsViewModel : BindableBase
    {
        #region Private Member Vars

        private const string RobotoUrl = "/Resources/Fonts/#Roboto";

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion
        
        #region Ctor

        public VisualsViewModel()
        {
            LoadSettings();
        }
        
        #endregion
        
        #region Properties

        public List<KeyValuePair<string, string>> Themes
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Android Dark", "/Resources/Themes/Android_Dark.xaml"),
                    new KeyValuePair<string, string>("Android Light", "/Resources/Themes/Android_Light.xaml")
                };
            }
        }

        public List<KeyValuePair<string, string>> FontFamilies
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Roboto", RobotoUrl)
                };
            }
        }

        public List<FontStretches> FontStretches
        {
            get
            {
                switch (FontFamily)
                {
                    case RobotoUrl:
                        return new List<FontStretch> { FontStretches.Normal, FontStretches.Condensed };
                }

                return null;
            }
        }

        public List<FontWeights> FontWeights
        {
            get
            {
                switch (FontFamily)
                {
                    case RobotoUrl:
                        switch (FontStretch)
                        {
                            case FontStretches.Normal:
                                return new List<FontWeight> 
                                            { 
                                                FontWeights.Thin, 
                                                FontWeights.Light, 
                                                FontWeights.Regular, 
                                                FontWeights.Medium, 
                                                FontWeights.Bold, 
                                                FontWeights.Black 
                                            };

                            case FontStretches.Condensed:
                                return new List<FontWeight> 
                                            { 
                                                FontWeights.Light, 
                                                FontWeights.Regular, 
                                                FontWeights.Bold
                                            };
                        }
                        break;
                }

                return null;
            }
        }

        public List<KeyValuePair<string, VisualModes>> VisualModes
        {
            get
            {
                return new List<KeyValuePair<string, VisualModes>>
                {
                    new KeyValuePair<string, VisualModes>("Standard", Enums.VisualModes.Standard),
                    new KeyValuePair<string, VisualModes>("Speech Only", Enums.VisualModes.SpeechOnly)
                };
            }
        }

        private string theme;
        public string Theme
        {
            get { return theme; }
            set { SetProperty(ref theme, value); }
        }

        private string fontFamily;
        public string FontFamily
        {
            get { return fontFamily; }
            set
            {
                SetProperty(ref fontFamily, value);
                OnPropertyChanged(() => FontStretches);
                OnPropertyChanged(() => FontWeights);
            }
        }

        private FontStretches fontStretch;
        public FontStretches FontStretch
        {
            get { return fontStretch; }
            set
            {
                SetProperty(ref fontStretch, value);
                OnPropertyChanged(() => FontWeights);
            }
        }

        private FontWeights fontWeight;
        public FontWeights FontWeight
        {
            get { return fontWeight; }
            set { SetProperty(ref fontWeight, value); }
        }

        private int scratchpadNumberOfLines;
        public int ScratchpadNumberOfLines
        {
            get { return scratchpadNumberOfLines; }
            set { SetProperty(ref scratchpadNumberOfLines, value); }
        }
        
        private int toastNotificationTitleFontSize;
        public int ToastNotificationTitleFontSize
        {
            get { return toastNotificationTitleFontSize; }
            set { SetProperty(ref toastNotificationTitleFontSize, value); }
        }
        
        private int toastNotificationContentFontSize;
        public int ToastNotificationContentFontSize
        {
            get { return toastNotificationContentFontSize; }
            set { SetProperty(ref toastNotificationContentFontSize, value); }
        }

        private VisualModes visualMode;
        public VisualModes VisualMode
        {
            get { return visualMode; }
            set { SetProperty(ref visualMode, value); }
        }

        public bool ChangesRequireRestart
        {
            get { return false; }
        }
        
        #endregion
        
        #region Methods

        private void LoadSettings()
        {
            Theme = Settings.Default.Theme;
            FontFamily = Settings.Default.FontFamily;
            FontStretch = Settings.Default.FontStretch;
            FontWeight = Settings.Default.FontWeight;
            ScratchpadNumberOfLines = Settings.Default.ScratchpadNumberOfLines;
            ToastNotificationTitleFontSize = Settings.Default.ToastNotificationTitleFontSize;
            ToastNotificationContentFontSize = Settings.Default.ToastNotificationContentFontSize;
            VisualMode = Settings.Default.VisualMode;
        }

        public void ApplyChanges()
        {
            Settings.Default.Theme = Theme;
            Settings.Default.FontFamily = FontFamily;
            Settings.Default.FontStretch = FontStretch;
            Settings.Default.FontWeight = fontWeight;
            Settings.Default.ScratchpadNumberOfLines = ScratchpadNumberOfLines;
            Settings.Default.ToastNotificationTitleFontSize = ToastNotificationTitleFontSize;
            Settings.Default.ToastNotificationContentFontSize = ToastNotificationContentFontSize;
            Settings.Default.VisualMode = VisualMode;
            Settings.Default.Save();
        }

        #endregion
    }
}
