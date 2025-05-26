// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

using System.Linq;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.UI.ViewModels.Management;
using log4net;
using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;

namespace JuliusSweetland.OptiKey.UI.ViewModels
{
    public class ManagementViewModel : BindableBase
    {
        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion
        
        #region Ctor

        public ManagementViewModel(
            IAudioService audioService,
            IDictionaryService dictionaryService,
            IWindowManipulationService windowManipulationService)
        {
            //Instantiate child VMs
            DictionaryViewModel = new DictionaryViewModel(dictionaryService);
            GesturesViewModel = new GesturesViewModel();
            PointingAndSelectingViewModel = new PointingAndSelectingViewModel();
            SoundsViewModel = new SoundsViewModel(audioService);
            VisualsViewModel = new VisualsViewModel(windowManipulationService);
            FeaturesViewModel = new FeaturesViewModel();
            WordsViewModel = new WordsViewModel(dictionaryService);
            
            //Instantiate interaction requests and commands
            ConfirmationRequest = new InteractionRequest<Confirmation>();
            OkCommand = new DelegateCommand<Window>(Ok); //Can always click Ok
            CancelCommand = new DelegateCommand<Window>(Cancel); //Can always click Cancel
            ApplyCommand = new DelegateCommand<Window>(Apply); //Can always click Cancel

        }
        
        #endregion
        
        #region Properties
        
        public bool ChangesRequireRestart
        {
            get
            {
                return DictionaryViewModel.ChangesRequireRestart
                    || PointingAndSelectingViewModel.ChangesRequireRestart
                    || SoundsViewModel.ChangesRequireRestart
                    || VisualsViewModel.ChangesRequireRestart
                    || FeaturesViewModel.ChangesRequireRestart
                    || WordsViewModel.ChangesRequireRestart;
            }
        }

        public DictionaryViewModel DictionaryViewModel { get; private set; }
        public GesturesViewModel GesturesViewModel { get; private set; }
        public PointingAndSelectingViewModel PointingAndSelectingViewModel { get; private set; }
        public SoundsViewModel SoundsViewModel { get; private set; }
        public VisualsViewModel VisualsViewModel { get; private set; }
        public FeaturesViewModel FeaturesViewModel { get; private set; }
        public WordsViewModel WordsViewModel { get; private set; }
        
        public InteractionRequest<Confirmation> ConfirmationRequest { get; private set; }
        public DelegateCommand<Window> OkCommand { get; private set; }
        public DelegateCommand<Window> CancelCommand { get; private set; }
        public DelegateCommand<Window> ApplyCommand { get; private set; }

        #endregion

        #region Methods

        private void CoerceValues()
        {
            CoercePersianSettings();
            CoerceUrduSettings();
            CoerceTouchSettings();
        }

        private void CoercePersianSettings()
        {
            if (WordsViewModel.KeyboardAndDictionaryLanguage == Languages.PersianIran
                && WordsViewModel.UiLanguage != Languages.PersianIran)
            {
                ConfirmationRequest.Raise(
                    new Confirmation
                    {
                        Title = Resources.UILANGUAGE_AND_KEYBOARDANDDICTIONARYLANGUAGE_DIFFER_TITLE,
                        Content = Resources.DEFAULT_UILANGUAGE_TO_PERSIAN
                    }, confirmation =>
                    {
                        if (confirmation.Confirmed)
                        {
                            Log.Info("Prompting user to change the UiLanguage to Persian as the KeyboardAndDictionaryLanguage is Persian. The UiLanguage controls whether the scratchpad has text flow RightToLeft, which Persian requires.");
                            WordsViewModel.UiLanguage = Languages.PersianIran;
                        }
                    });
            }

            if ((WordsViewModel.KeyboardAndDictionaryLanguage == Languages.PersianIran
                 || WordsViewModel.UiLanguage == Languages.PersianIran)
                && !new[]
                {
                    VisualsViewModel.ElhamUrl,
                    VisualsViewModel.HomaUrl,
                    VisualsViewModel.KoodakUrl,
                    VisualsViewModel.NazliUrl,
                    VisualsViewModel.RoyaUrl,
                    VisualsViewModel.TerafikUrl,
                    VisualsViewModel.TitrUrl
                }.Contains(VisualsViewModel.FontFamily))
            {
                ConfirmationRequest.Raise(
                    new Confirmation
                    {
                        Title = Resources.LANGUAGE_SPECIFIC_FONT_RECOMMENDED,
                        Content = Resources.FONTFAMILY_IS_NOT_COMPATIBLE_WITH_PERSIAN_LANGUAGE
                    }, confirmation =>
                    {
                        if (confirmation.Confirmed)
                        {
                            Log.Info("Prompting user to change the font to an Persian compatible font. If another font is used then text may be displayed incorrectly.");
                            VisualsViewModel.FontFamily = VisualsViewModel.NazliUrl;
                            VisualsViewModel.FontStretch = Enums.FontStretches.Normal;
                            VisualsViewModel.FontWeight = Enums.FontWeights.Regular;
                        }
                    });
            }
        }

        private void CoerceUrduSettings()
        {
            if (WordsViewModel.KeyboardAndDictionaryLanguage == Languages.UrduPakistan
                && WordsViewModel.UiLanguage != Languages.UrduPakistan)
            {
                ConfirmationRequest.Raise(
                    new Confirmation
                    {
                        Title = Resources.UILANGUAGE_AND_KEYBOARDANDDICTIONARYLANGUAGE_DIFFER_TITLE,
                        Content = Resources.DEFAULT_UILANGUAGE_TO_URDU
                    }, confirmation =>
                    {
                        if (confirmation.Confirmed)
                        {
                            Log.Info("Prompting user to change the UiLanguage to Urdu as the KeyboardAndDictionaryLanguage is Urdu. The UiLanguage controls whether the scratchpad has text flow RightToLeft, which Urdu requires.");
                            WordsViewModel.UiLanguage = Languages.UrduPakistan;
                        }
                    });
            }

            if ((WordsViewModel.KeyboardAndDictionaryLanguage == Languages.UrduPakistan
                 || WordsViewModel.UiLanguage == Languages.UrduPakistan)
                && !new[]
                {
                    VisualsViewModel.FajerNooriNastaliqueUrl,
                    VisualsViewModel.NafeesWebNaskhUrl,
                    VisualsViewModel.PakNastaleeqUrl
                }.Contains(VisualsViewModel.FontFamily))
            {
                ConfirmationRequest.Raise(
                    new Confirmation
                    {
                        Title = Resources.LANGUAGE_SPECIFIC_FONT_RECOMMENDED,
                        Content = Resources.FONTFAMILY_IS_NOT_COMPATIBLE_WITH_URDU_LANGUAGE
                    }, confirmation =>
                    {
                        if (confirmation.Confirmed)
                        {
                            Log.Info("Prompting user to change the font to an Urdu compatible font. If another font is used then text (especially numbers which are only displayed correctly in Urdu if an Urdu font is used) may be displayed incorrectly.");
                            VisualsViewModel.FontFamily = VisualsViewModel.NafeesWebNaskhUrl;
                            VisualsViewModel.FontStretch = Enums.FontStretches.Normal;
                            VisualsViewModel.FontWeight = Enums.FontWeights.Regular;
                        }
                    });
            }
        }

        private void CoerceTouchSettings()
        {
            // If PointSource has changed from (not Touch) => Touch then check for recommended settings
            if (PointingAndSelectingViewModel.PointsSource == PointsSources.TouchScreenPosition &&
                Settings.Default.PointsSource != PointsSources.TouchScreenPosition)
            {

                // Use touch as trigger also
                if (PointingAndSelectingViewModel.PointSelectionTriggerSource != TriggerSources.TouchDownUps ||
                     PointingAndSelectingViewModel.PointSelectionTriggerSource != TriggerSources.TouchDownUps ||
                     PointingAndSelectingViewModel.KeySelectionTriggerSource != TriggerSources.TouchDownUps ||
                     PointingAndSelectingViewModel.KeySelectionTriggerSource != TriggerSources.TouchDownUps)
                {
                    ConfirmationRequest.Raise(
                        new Confirmation
                        {
                            Title = Resources.TOUCH_INPUT_AND_TRIGGER_DIFFER_TITLE,
                            Content = Resources.TOUCH_INPUT_AND_TRIGGER_DESCRIPTION
                        }, confirmation =>
                        {
                            if (confirmation.Confirmed)
                            {
                                Log.Info("Prompting user to change the trigger source to touch");
                                PointingAndSelectingViewModel.KeySelectionTriggerSource = TriggerSources.TouchDownUps;
                                PointingAndSelectingViewModel.PointSelectionTriggerSource = TriggerSources.TouchDownUps;
                            }
                        });
                }

                // Turn down minimum dwell time for multikey selection
                if (PointingAndSelectingViewModel.MultiKeySelectionFixationMinDwellTimeInMs > 100)
                {
                    ConfirmationRequest.Raise(
                        new Confirmation
                        {
                            Title = Resources.TOUCH_INPUT_AND_MULTIKEY_TITLE,
                            Content = Resources.TOUCH_INPUT_AND_MULTIKEY_DESCRIPTION
                        }, confirmation =>
                        {
                            if (confirmation.Confirmed)
                            {
                                Log.Info("Prompting user to change the multikey selection minimum dwell time");
                                PointingAndSelectingViewModel.MultiKeySelectionFixationMinDwellTimeInMs = 50;
                            }
                        });
                }
            }

            // If PointSource has changed from Touch => (not Touch) then check for recommended settings
            if (PointingAndSelectingViewModel.PointsSource != PointsSources.TouchScreenPosition &&
                Settings.Default.PointsSource == PointsSources.TouchScreenPosition)
            {

                // Don't use touch as trigger 
                if (PointingAndSelectingViewModel.PointSelectionTriggerSource == TriggerSources.TouchDownUps ||
                    PointingAndSelectingViewModel.KeySelectionTriggerSource == TriggerSources.TouchDownUps)
                {
                    ConfirmationRequest.Raise(
                        new Confirmation
                        {
                            Title = Resources.NONTOUCH_INPUT_AND_TRIGGER_DIFFER_TITLE,
                            Content = Resources.NONTOUCH_INPUT_AND_TRIGGER_DESCRIPTION
                        }, confirmation =>
                        {
                            if (confirmation.Confirmed)
                            {
                                Log.Info("Prompting user to change the trigger source away from touch");
                                if (PointingAndSelectingViewModel.KeySelectionTriggerSource != TriggerSources.TouchDownUps)
                                {
                                    // If they changed point trigger, make key trigger use the same source
                                    PointingAndSelectingViewModel.PointSelectionTriggerSource = PointingAndSelectingViewModel.KeySelectionTriggerSource;
                                    // Match sub-option too
                                    switch (PointingAndSelectingViewModel.KeySelectionTriggerSource)
                                    {
                                        case TriggerSources.KeyboardKeyDownsUps:
                                            PointingAndSelectingViewModel.PointSelectionTriggerKeyboardKeyDownUpKey =
                                              PointingAndSelectingViewModel.KeySelectionTriggerKeyboardKeyDownUpKey;
                                            break;
                                        case TriggerSources.MouseButtonDownUps:
                                            PointingAndSelectingViewModel.PointSelectionTriggerMouseDownUpButton =
                                                PointingAndSelectingViewModel.KeySelectionTriggerMouseDownUpButton;
                                            break;
                                        case TriggerSources.DirectInputButtonDownUps:
                                            PointingAndSelectingViewModel.PointSelectionTriggerGamepadDirectInputController =
                                                PointingAndSelectingViewModel.KeySelectionTriggerGamepadDirectInputController;
                                            PointingAndSelectingViewModel.PointSelectionTriggerGamepadDirectInputButtonDownUpButton =
                                                PointingAndSelectingViewModel.KeySelectionTriggerGamepadDirectInputButtonDownUpButton;
                                            break;
                                        case TriggerSources.XInputButtonDownUps:
                                            PointingAndSelectingViewModel.PointSelectionTriggerGamepadXInputController =
                                                PointingAndSelectingViewModel.KeySelectionTriggerGamepadXInputController;
                                            PointingAndSelectingViewModel.PointSelectionTriggerGamepadXInputButtonDownUpButton =
                                                PointingAndSelectingViewModel.KeySelectionTriggerGamepadXInputButtonDownUpButton;
                                            break;
                                        case TriggerSources.Fixations:
                                            PointingAndSelectingViewModel.PointSelectionTriggerFixationLockOnTimeInMs =
                                                PointingAndSelectingViewModel.KeySelectionTriggerFixationLockOnTimeInMs;
                                            break;
                                    }
                                }
                                else
                                {
                                    // otherwise default to fixations
                                    PointingAndSelectingViewModel.PointSelectionTriggerSource = TriggerSources.Fixations;
                                    PointingAndSelectingViewModel.KeySelectionTriggerSource = TriggerSources.Fixations;
                                }
                            }
                        });
                }

                // Turn up minimum dwell time for multikey selection
                double defaultMinDwellTimeInMs;
                string propName = "MultiKeySelectionFixationMinDwellTime";
                try
                {
                    var prop = Properties.Settings.Default.Properties[propName];                    
                    System.TimeSpan defaultMinDwellTime = System.TimeSpan.Parse((string)prop.DefaultValue);
                    defaultMinDwellTimeInMs = defaultMinDwellTime.TotalMilliseconds;
                }
                catch (System.Exception e)
                {
#if DEBUG_MODE                
                    throw;
#endif
                    Log.Error($"Could not read property {propName}");
                    defaultMinDwellTimeInMs = 250;
                }

                if (PointingAndSelectingViewModel.MultiKeySelectionFixationMinDwellTimeInMs < defaultMinDwellTimeInMs)
                {
                    ConfirmationRequest.Raise(
                        new Confirmation
                        {
                            Title = Resources.NONTOUCH_INPUT_AND_MULTIKEY_TITLE,
                            Content = Resources.NONTOUCH_INPUT_AND_MULTIKEY_DESCRIPTION
                        }, confirmation =>
                        {
                            if (confirmation.Confirmed)
                            {
                                Log.Info("Prompting user to change the multikey selection minimum dwell time");
                                PointingAndSelectingViewModel.MultiKeySelectionFixationMinDwellTimeInMs = defaultMinDwellTimeInMs;
                            }
                        });
                }
            }
        }

        private void ApplyChanges()
        {
            DictionaryViewModel.ApplyChanges();
            GesturesViewModel.ApplyChanges();
            PointingAndSelectingViewModel.ApplyChanges();
            SoundsViewModel.ApplyChanges();
            VisualsViewModel.ApplyChanges();
            FeaturesViewModel.ApplyChanges();
            WordsViewModel.ApplyChanges();
        }

        private void ProcessChanges(Window window, bool closeWindow)
        {
            CoerceValues();

            if (ChangesRequireRestart)
            {
                //Warn if restart required and prompt for Confirmation before restarting
                ConfirmationRequest.Raise(
                    new Confirmation
                    {
                        Title = Resources.VERIFY_RESTART,
                        Content = Resources.RESTART_MESSAGE
                    }, confirmation =>
                    {
                        if (confirmation.Confirmed)
                        {
                            Log.Info("Applying management changes and attempting to restart OptiKey");
                            ApplyChanges();
                            Settings.Default.Save();
                            try
                            {
                                Settings.Default.CleanShutdown = true;
                                Settings.Default.Save();
                                OptiKeyApp.RestartApp();
                            }
                            catch { } //Swallow any exceptions (e.g. DispatcherExceptions) - we're shutting down so it doesn't matter.
                            Application.Current.Shutdown();
                        }
                    });
            }
            else
            {
                Log.Info("Applying management changes");
                ApplyChanges();
                if (closeWindow)
                {
                    window.Close();
                }
            }
        }

        private void Ok(Window window)
        {
            ProcessChanges(window, true);
        }

        private static void Cancel(Window window)
        {
            window.Close();
        }

        private void Apply(Window window)
        {
            ProcessChanges(window, false);
        }

        #endregion
    }
}
