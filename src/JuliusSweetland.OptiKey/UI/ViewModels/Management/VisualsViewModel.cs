using System;
using System.Collections.Generic;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using Prism.Mvvm;
using FontStretches = JuliusSweetland.OptiKey.Enums.FontStretches;
using FontWeights = JuliusSweetland.OptiKey.Enums.FontWeights;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Management
{
    public class VisualsViewModel : BindableBase
    {
        #region Private Member Vars

        private const string RobotoUrl = "/Resources/Fonts/#Roboto";
        private const string CharisSILUrl = "/Resources/Fonts/#CharisSIL";

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion
        
        #region Ctor

        public VisualsViewModel()
        {
            Load();
        }
        
        #endregion
        
        #region Properties

        public List<KeyValuePair<string, string>> Themes
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(Resources.ANDROID_DARK, "/Resources/Themes/Android_Dark.xaml"),
                    new KeyValuePair<string, string>(Resources.ANDROID_LIGHT, "/Resources/Themes/Android_Light.xaml")
                };
            }
        }

        public List<KeyValuePair<string, string>> FontFamilies
        {
            get
            {
                return new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Roboto", RobotoUrl),
                    new KeyValuePair<string, string>("Charis SIL", CharisSILUrl)
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
                        return new List<FontStretches>
                        {
                            Enums.FontStretches.Normal, 
                            Enums.FontStretches.Condensed
                        };

                    case CharisSILUrl:
                        return new List<FontStretches>
                        {
                            Enums.FontStretches.Normal
                        };
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
                            case Enums.FontStretches.Normal:
                                return new List<FontWeights> 
                                            { 
                                                Enums.FontWeights.Thin, 
                                                Enums.FontWeights.Light, 
                                                Enums.FontWeights.Regular, 
                                                Enums.FontWeights.Medium, 
                                                Enums.FontWeights.Bold, 
                                                Enums.FontWeights.Black 
                                            };

                            case Enums.FontStretches.Condensed:
                                return new List<FontWeights> 
                                            { 
                                                Enums.FontWeights.Light, 
                                                Enums.FontWeights.Regular, 
                                                Enums.FontWeights.Bold
                                            };
                        }
                        break;

                    case CharisSILUrl:
                        return new List<FontWeights> {Enums.FontWeights.Regular};
                }

                return null;
            }
        }
        
        public List<KeyValuePair<string, Enums.Keyboards>> StartupKeyboards
        {
            get
            {
                return new List<KeyValuePair<string, Enums.Keyboards>>
                {
                    new KeyValuePair<string, Enums.Keyboards>(Resources.ALPHA_LAYOUT, Enums.Keyboards.Alpha),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.CONVERSATION_LAYOUT, Enums.Keyboards.ConversationAlpha),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.CONVERSATION_NUM_SYM_LAYOUT, Enums.Keyboards.ConversationNumericAndSymbols),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.CURRENCIES_LAYOUT_1, Enums.Keyboards.Currencies1),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.CURRENCIES_LAYOUT_2, Enums.Keyboards.Currencies2),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.CUSTOM_LAYOUT, Enums.Keyboards.CustomKeyboardFile),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.DYNAMIC_KEYBOARD_LAYOUT, Enums.Keyboards.DynamicKeyboard),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.DIACRITICS_LAYOUT_1, Enums.Keyboards.Diacritics1),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.DIACRITICS_LAYOUT_2, Enums.Keyboards.Diacritics2),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.DIACRITICS_LAYOUT_3, Enums.Keyboards.Diacritics3),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.MENU_LAYOUT, Enums.Keyboards.Menu),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.MINIMISED_LAYOUT, Enums.Keyboards.Minimised),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.MOUSE_LAYOUT, Enums.Keyboards.Mouse),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.NUMBERS_SYMBOLS_LAYOUT_1, Enums.Keyboards.NumericAndSymbols1),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.NUMBERS_SYMBOLS_LAYOUT_2, Enums.Keyboards.NumericAndSymbols2),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.NUMBERS_SYMBOLS_LAYOUT_3, Enums.Keyboards.NumericAndSymbols3),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.PHYSICAL_KEYS_LAYOUT, Enums.Keyboards.PhysicalKeys),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.SIZE_POSITION_LAYOUT, Enums.Keyboards.SizeAndPosition),
                    new KeyValuePair<string, Enums.Keyboards>(Resources.WEB_BROWSING_LAYOUT, Enums.Keyboards.WebBrowsing)
                };
            }
        }

        public List<KeyValuePair<string, Enums.MinimisedEdges>> MinimisedPositions
        {
            get
            {
                return new List<KeyValuePair<string, Enums.MinimisedEdges>>
                {
                    new KeyValuePair<string, Enums.MinimisedEdges>(Resources.SAME_AS_DOCK_POSITION, Enums.MinimisedEdges.SameAsDockedPosition),
                    new KeyValuePair<string, Enums.MinimisedEdges>(Resources.UP, Enums.MinimisedEdges.Top),
                    new KeyValuePair<string, Enums.MinimisedEdges>(Resources.RIGHT, Enums.MinimisedEdges.Right),
                    new KeyValuePair<string, Enums.MinimisedEdges>(Resources.BOTTOM, Enums.MinimisedEdges.Bottom),
                    new KeyValuePair<string, Enums.MinimisedEdges>(Resources.LEFT, Enums.MinimisedEdges.Left),
                };
            }
        }

        public List<KeyValuePair<string, Enums.Case>> KeyCases
        {
            get
            {
                return new List<KeyValuePair<string, Enums.Case>>
                {
                    new KeyValuePair<string, Enums.Case>(Resources.UPPER_CASE, Enums.Case.Upper),
                    new KeyValuePair<string, Enums.Case>(Resources.LOWER_CASE, Enums.Case.Lower),
                    new KeyValuePair<string, Enums.Case>(Resources.TITLE_CASE, Enums.Case.Title)
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

        private Case keyCase;
        public Case KeyCase
        {
            get { return keyCase; }
            set { SetProperty(ref keyCase, value); }
        }

        private int scratchpadNumberOfLines;
        public int ScratchpadNumberOfLines
        {
            get { return scratchpadNumberOfLines; }
            set { SetProperty(ref scratchpadNumberOfLines, value); }
        }

        private int toastNotificationHorizontalFillPercentage;
        public int ToastNotificationHorizontalFillPercentage
        {
            get { return toastNotificationHorizontalFillPercentage; }
            set { SetProperty(ref toastNotificationHorizontalFillPercentage, value); }
        }
        
        private int toastNotificationVerticalFillPercentage;
        public int ToastNotificationVerticalFillPercentage
        {
            get { return toastNotificationVerticalFillPercentage; }
            set { SetProperty(ref toastNotificationVerticalFillPercentage, value); }
        }

        private decimal toastNotificationSecondsPerCharacter;
        public decimal ToastNotificationSecondsPerCharacter
        {
            get { return toastNotificationSecondsPerCharacter; }
            set { SetProperty(ref toastNotificationSecondsPerCharacter, value); }
        }

        private int cursorWidthInPixels;
        public int CursorWidthInPixels
        {
            get { return cursorWidthInPixels; }
            set { SetProperty(ref cursorWidthInPixels, value); }
        }

        private int cursorHeightInPixels;
        public int CursorHeightInPixels
        {
            get { return cursorHeightInPixels; }
            set { SetProperty(ref cursorHeightInPixels, value); }
        }

        private double magnifySourcePercentageOfScreen;
        public double MagnifySourcePercentageOfScreen
        {
            get { return magnifySourcePercentageOfScreen; }
            set { SetProperty(ref magnifySourcePercentageOfScreen, value); }
        }

        private double magnifyDestinationPercentageOfScreen;
        public double MagnifyDestinationPercentageOfScreen
        {
            get { return magnifyDestinationPercentageOfScreen; }
            set { SetProperty(ref magnifyDestinationPercentageOfScreen, value); }
        }

        private bool conversationOnlyMode;
        public bool ConversationOnlyMode
        {
            get { return conversationOnlyMode; }
            set { SetProperty(ref conversationOnlyMode, value); }
        }

        private bool conversationconfirmEnable;
        public bool ConversationConfirmEnable
        {
            get { return conversationconfirmEnable; }
            set { SetProperty(ref conversationconfirmEnable, value); }
        }

        private bool conversationconfirmOnlyMode;
        public bool ConversationConfirmOnlyMode
        {
            get { return conversationconfirmOnlyMode; }
            set { SetProperty(ref conversationconfirmOnlyMode, value); }
        }

        private Enums.Keyboards startupKeyboard;
        public Enums.Keyboards StartupKeyboard
        {
            get { return startupKeyboard; }
            set { SetProperty(ref startupKeyboard, value); }
        }

        private Enums.MinimisedEdges minimsedPosition;
        public Enums.MinimisedEdges MinimisedPosition
        {
            get { return minimsedPosition; }
            set { SetProperty(ref minimsedPosition, value); }
        }

        private double mainWindowFullDockThicknessAsPercentageOfScreen;
        public double MainWindowFullDockThicknessAsPercentageOfScreen
        {
            get { return mainWindowFullDockThicknessAsPercentageOfScreen; }
            set { SetProperty(ref mainWindowFullDockThicknessAsPercentageOfScreen, value); }
        }

        private double mainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness;
        public double MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness
        {
            get { return mainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness; }
            set { SetProperty(ref mainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness, value); }
        }

        private Thickness conversationBorderThickness;
        public Thickness ConversationBorderThickness
        {
            get { return conversationBorderThickness; }
            set { SetProperty(ref conversationBorderThickness, value); }
        }

        public bool ChangesRequireRestart
        {
            get
            {
                return Settings.Default.ConversationOnlyMode != ConversationOnlyMode
                    || Settings.Default.ConversationConfirmEnable != ConversationConfirmEnable
                    || Settings.Default.ConversationConfirmOnlyMode != ConversationConfirmOnlyMode;
            }
        }

        private bool enableQuitKeys;
        public bool EnableQuitKeys
        {
            get { return enableQuitKeys; }
            set { SetProperty(ref enableQuitKeys, value); }
        }

        private bool enableAttentionKey;
        public bool EnableAttentionKey
        {
            get { return enableAttentionKey; }
            set { SetProperty(ref enableAttentionKey, value); }
        }

        private string pluginsLocation;
        public string PluginsLocation
        {
            get { return pluginsLocation; }
            set { SetProperty(ref pluginsLocation, value); }
        }

        private string dynamicKeyboardsLocation;
        public string DynamicKeyboardsLocation
        {
            get { return dynamicKeyboardsLocation; }
            set { SetProperty(ref dynamicKeyboardsLocation, value); }
        }

        private string startupKeyboardFile;
        public string StartupKeyboardFile
        {
            get { return startupKeyboardFile; }
            set { SetProperty(ref startupKeyboardFile, value); }
        }

        #endregion

        #region Methods

        private void Load()
        {
            Theme = Settings.Default.Theme;
            FontFamily = Settings.Default.FontFamily;
            FontStretch = (FontStretches)Enum.Parse(typeof(FontStretches), Settings.Default.FontStretch);
            FontWeight = (FontWeights)Enum.Parse(typeof(FontWeights), Settings.Default.FontWeight);
            ScratchpadNumberOfLines = Settings.Default.ScratchpadNumberOfLines;
            ToastNotificationVerticalFillPercentage = Settings.Default.ToastNotificationVerticalFillPercentage;
            ToastNotificationHorizontalFillPercentage = Settings.Default.ToastNotificationHorizontalFillPercentage;
            ToastNotificationSecondsPerCharacter = Settings.Default.ToastNotificationSecondsPerCharacter;
            CursorWidthInPixels = Settings.Default.CursorWidthInPixels;
            CursorHeightInPixels = Settings.Default.CursorHeightInPixels;
            MagnifySourcePercentageOfScreen = Settings.Default.MagnifySourcePercentageOfScreen;
            MagnifyDestinationPercentageOfScreen = Settings.Default.MagnifyDestinationPercentageOfScreen;
            ConversationOnlyMode = Settings.Default.ConversationOnlyMode;
            ConversationConfirmEnable = Settings.Default.ConversationConfirmEnable;
            ConversationConfirmOnlyMode = Settings.Default.ConversationConfirmOnlyMode;
            StartupKeyboard = Settings.Default.StartupKeyboard;
            MinimisedPosition = Settings.Default.MainWindowMinimisedPosition;
            KeyCase = Settings.Default.KeyCase;
            MainWindowFullDockThicknessAsPercentageOfScreen = Settings.Default.MainWindowFullDockThicknessAsPercentageOfScreen;
            MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness = Settings.Default.MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness;
            ConversationBorderThickness = Settings.Default.ConversationBorderThickness;
            EnableQuitKeys = Settings.Default.EnableQuitKeys;
            EnableAttentionKey = Settings.Default.EnableAttentionKey;
            PluginsLocation = Settings.Default.PluginsLocation;
            DynamicKeyboardsLocation = Settings.Default.DynamicKeyboardsLocation;
            StartupKeyboardFile = Settings.Default.StartupKeyboardFile;
        }

        public void ApplyChanges()
        {
            Settings.Default.Theme = Theme;
            Settings.Default.FontFamily = FontFamily;
            Settings.Default.FontStretch = FontStretch.ToString();
            Settings.Default.FontWeight = FontWeight.ToString();
            Settings.Default.ScratchpadNumberOfLines = ScratchpadNumberOfLines;
            Settings.Default.ToastNotificationVerticalFillPercentage = ToastNotificationVerticalFillPercentage;
            Settings.Default.ToastNotificationHorizontalFillPercentage = ToastNotificationHorizontalFillPercentage;
            Settings.Default.ToastNotificationSecondsPerCharacter = ToastNotificationSecondsPerCharacter;
            Settings.Default.CursorWidthInPixels = CursorWidthInPixels;
            Settings.Default.CursorHeightInPixels = CursorHeightInPixels;
            Settings.Default.MagnifySourcePercentageOfScreen = MagnifySourcePercentageOfScreen;
            Settings.Default.MagnifyDestinationPercentageOfScreen = MagnifyDestinationPercentageOfScreen;
            Settings.Default.ConversationOnlyMode = ConversationOnlyMode;
            Settings.Default.ConversationConfirmEnable = ConversationConfirmEnable;
            Settings.Default.ConversationConfirmOnlyMode = ConversationConfirmOnlyMode;
            Settings.Default.StartupKeyboard = StartupKeyboard;
            Settings.Default.MainWindowMinimisedPosition = MinimisedPosition;
            Settings.Default.KeyCase = KeyCase;
            Settings.Default.MainWindowFullDockThicknessAsPercentageOfScreen = MainWindowFullDockThicknessAsPercentageOfScreen;
            Settings.Default.MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness = MainWindowCollapsedDockThicknessAsPercentageOfFullDockThickness;
            Settings.Default.ConversationBorderThickness = ConversationBorderThickness;
            Settings.Default.EnableQuitKeys = EnableQuitKeys;
            Settings.Default.EnableAttentionKey = EnableAttentionKey;
            Settings.Default.PluginsLocation = PluginsLocation;
            Settings.Default.DynamicKeyboardsLocation = DynamicKeyboardsLocation;
            Settings.Default.StartupKeyboardFile = StartupKeyboardFile;
        }

        #endregion
    }
}
