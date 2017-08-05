using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WindowsInput.Native;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Properties;
using log4net;
using Prism.Mvvm;

namespace JuliusSweetland.OptiKey.Services
{
    public class KeyboardOutputService : BindableBase, IKeyboardOutputService
    {
        #region Private Member Vars

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IKeyStateService keyStateService;
        private readonly ISuggestionStateService suggestionService;
        private readonly IPublishService publishService;
        private readonly IDictionaryService dictionaryService;
        private readonly Action<KeyValue> fireKeySelectionEvent;
        private readonly Dictionary<bool, KeyboardOutputServiceState> state = new Dictionary<bool, KeyboardOutputServiceState>();

        private string text;
        private string lastTextChange;
        private bool lastTextChangeWasSuggestion;
        private bool lastTextChangeWasMultiKey;
        private bool suppressNextAutoSpace = true;
        private bool keyboardIsShiftAware;
        private bool shiftStateSetAutomatically;

        #endregion

        #region Ctor

        public KeyboardOutputService(
            IKeyStateService keyStateService,
            ISuggestionStateService suggestionService,
            IPublishService publishService,
            IDictionaryService dictionaryService,
            Action<KeyValue> fireKeySelectionEvent)
        {
            this.keyStateService = keyStateService;
            this.suggestionService = suggestionService;
            this.publishService = publishService;
            this.dictionaryService = dictionaryService;
            this.fireKeySelectionEvent = fireKeySelectionEvent;

            ReactToSimulateKeyStrokesChanges();
            ReactToShiftStateChanges();
            ReactToPublishableKeyDownStateChanges();
            ReactToKeyboardIsShiftAwareChanges();
            ReactToSuppressAutoCapitaliseIntelligentlyChanges();
            AutoPressShiftIfAppropriate();
            GenerateSuggestions(true);
        }

        #endregion

        #region Properties

        public string Text
        {
            get { return text; }
            private set { SetProperty(ref text, value); }
        }

        public bool KeyboardIsShiftAware //Not on interface as only accessed via databinding
        {
            get { return keyboardIsShiftAware; }
            set { SetProperty(ref keyboardIsShiftAware, value); }
        }

        #endregion

        #region Methods - IKeyboardOutputService

        public void ProcessFunctionKey(FunctionKeys functionKey)
        {
            Log.DebugFormat("Processing captured function key '{0}'", functionKey);

            switch (functionKey)
            {
                case FunctionKeys.BackMany:
                    if (!string.IsNullOrEmpty(Text))
                    {
                        var backManyCount = Text.CountBackToLastCharCategoryBoundary();

                        dictionaryService.DecrementEntryUsageCount(Text.Substring(Text.Length - backManyCount, backManyCount).Trim());

                        var textAfterBackMany = Text.Substring(0, Text.Length - backManyCount);
                        var textChangedByBackMany = Text != textAfterBackMany;
                        Text = textAfterBackMany;

                        if (backManyCount == 0) backManyCount = 1; //Always publish at least one backspace

                        for (int i = 0; i < backManyCount; i++)
                        {
                            PublishKeyPress(FunctionKeys.BackOne);
                            ReleaseUnlockedKeys();
                        }

                        if (textChangedByBackMany
                            || string.IsNullOrEmpty(Text))
                        {
                            AutoPressShiftIfAppropriate();
                        }

                        StoreLastTextChange(null);
                        GenerateSuggestions(true);

                        Log.Debug("Suppressing next auto space.");
                        suppressNextAutoSpace = true;
                    }
                    else
                    {
                        //Scratchpad is empty, but publish 1 backspace anyway, as per the behaviour for 'BackOne'
                        PublishKeyPress(FunctionKeys.BackOne);
                    }

                    lastTextChangeWasSuggestion = false;
                    break;

                case FunctionKeys.BackOne:
                    var backOneCount = string.IsNullOrEmpty(lastTextChange)
                        ? 1 //Default to removing one character if no lastTextChange
                        : lastTextChange.Length;

                    var textChangedByBackOne = false;

                    if (!string.IsNullOrEmpty(Text))
                    {
                        if (Text.Length < backOneCount)
                        {
                            backOneCount = Text.Length; //Coallesce backCount if somehow the Text length is less
                        }

                        var textAfterBackOne = Text.Substring(0, Text.Length - backOneCount);
                        textChangedByBackOne = Text != textAfterBackOne;

                        if (backOneCount > 1)
                        {
                            //Removing more than one character - only decrement removed string
                            dictionaryService.DecrementEntryUsageCount(Text.Substring(Text.Length - backOneCount, backOneCount).Trim());
                        }
                        else if (!string.IsNullOrEmpty(lastTextChange)
                            && lastTextChange.Length == 1
                            && !char.IsWhiteSpace(lastTextChange[0]))
                        {
                            dictionaryService.DecrementEntryUsageCount(Text.InProgressWord(Text.Length)); //We are removing a non-whitespace character - decrement the in progress word
                            dictionaryService.IncrementEntryUsageCount(textAfterBackOne.InProgressWord(Text.Length)); //And increment the in progress word that is left after the removal
                        }

                        Text = textAfterBackOne;
                    }

                    for (int i = 0; i < backOneCount; i++)
                    {
                        PublishKeyPress(FunctionKeys.BackOne);
                        ReleaseUnlockedKeys();
                    }

                    if (textChangedByBackOne
                        || string.IsNullOrEmpty(Text))
                    {
                        AutoPressShiftIfAppropriate();
                    }

                    StoreLastTextChange(null);
                    GenerateSuggestions(false);

                    Log.Debug("Suppressing next auto space.");
                    suppressNextAutoSpace = true;
                    lastTextChangeWasSuggestion = false;
                    break;

                case FunctionKeys.ClearScratchpad:
                    Text = null;
                    StoreLastTextChange(null);
                    ClearSuggestions();
                    AutoPressShiftIfAppropriate();
                    Log.Debug("Suppressing next auto space.");
                    suppressNextAutoSpace = true;
                    lastTextChangeWasSuggestion = false;
                    GenerateSuggestions(false);
                    break;

                case FunctionKeys.CommuniKate_Home:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "";
                    break;

                case FunctionKeys.CommuniKate_1_235:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_1_235";
                    break;

                case FunctionKeys.CommuniKate_1_236:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_1_236";
                    break;

                case FunctionKeys.CommuniKate_1_237:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_1_237";
                    break;

                case FunctionKeys.CommuniKate_1_238:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_1_238";
                    break;

                case FunctionKeys.CommuniKate_1_239:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_1_239";
                    break;

                case FunctionKeys.CommuniKate_1_240:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_1_240";
                    break;

                case FunctionKeys.CommuniKate_1_241:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_1_241";
                    break;

                case FunctionKeys.CommuniKate_1_242:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_1_242";
                    break;

                case FunctionKeys.CommuniKate_1_243:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_1_243";
                    break;

                case FunctionKeys.CommuniKate_1_244:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_1_244";
                    break;

                case FunctionKeys.CommuniKate_1_245:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_1_245";
                    break;

                case FunctionKeys.CommuniKate_1_246:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_1_246";
                    break;

                case FunctionKeys.CommuniKate_1_247:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_1_247";
                    break;

                case FunctionKeys.CommuniKate_1_248:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_1_248";
                    break;

                case FunctionKeys.CommuniKate_1_249:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_1_249";
                    break;

                case FunctionKeys.CommuniKate_insects:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_insects";
                    break;

                case FunctionKeys.CommuniKate_money:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_money";
                    break;

                case FunctionKeys.CommuniKate_shape:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_shape";
                    break;

                case FunctionKeys.CommuniKate_mp3:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_mp3";
                    break;

                case FunctionKeys.CommuniKate_questions:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_questions";
                    break;

                case FunctionKeys.CommuniKate_inserttitlehere:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_inserttitlehere";
                    break;

                case FunctionKeys.CommuniKate_birds:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_birds";
                    break;

                case FunctionKeys.CommuniKate_workthings:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_workthings";
                    break;

                case FunctionKeys.CommuniKate_aboutme:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_aboutme";
                    break;

                case FunctionKeys.CommuniKate_thingsthathelp:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_thingsthathelp";
                    break;

                case FunctionKeys.CommuniKate_wml:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_wml";
                    break;

                case FunctionKeys.CommuniKate_nzcaps:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_nzcaps";
                    break;

                case FunctionKeys.CommuniKate_foodchat:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_foodchat";
                    break;

                case FunctionKeys.CommuniKate_food:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_food";
                    break;

                case FunctionKeys.CommuniKate_leisure:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_leisure";
                    break;

                case FunctionKeys.CommuniKate_lunch:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_lunch";
                    break;

                case FunctionKeys.CommuniKate_flavour:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_flavour";
                    break;

                case FunctionKeys.CommuniKate_fizzydrinks:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_fizzydrinks";
                    break;

                case FunctionKeys.CommuniKate_furniture:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_furniture";
                    break;

                case FunctionKeys.CommuniKate_fastfood:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_fastfood";
                    break;

                case FunctionKeys.CommuniKate_places:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_places";
                    break;

                case FunctionKeys.CommuniKate_meal:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_meal";
                    break;

                case FunctionKeys.CommuniKate_clothes:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_clothes";
                    break;

                case FunctionKeys.CommuniKate_spellingcaps:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_spellingcaps";
                    break;

                case FunctionKeys.CommuniKate_gardening:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_gardening";
                    break;

                case FunctionKeys.CommuniKate_people:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_people";
                    break;

                case FunctionKeys.CommuniKate_specialdays:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_specialdays";
                    break;

                case FunctionKeys.CommuniKate_iwant:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_iwant";
                    break;

                case FunctionKeys.CommuniKate_computer:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_computer";
                    break;

                case FunctionKeys.CommuniKate_home:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_home";
                    break;

                case FunctionKeys.CommuniKate_sauces:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_sauces";
                    break;

                case FunctionKeys.CommuniKate_drinks:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_drinks";
                    break;

                case FunctionKeys.CommuniKate_describing:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_describing";
                    break;

                case FunctionKeys.CommuniKate_alcohol:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_alcohol";
                    break;

                case FunctionKeys.CommuniKate_supportstaff:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_supportstaff";
                    break;

                case FunctionKeys.CommuniKate_colours:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_colours";
                    break;

                case FunctionKeys.CommuniKate_moreactionwords:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_moreactionwords";
                    break;

                case FunctionKeys.CommuniKate_farmanimals:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_farmanimals";
                    break;

                case FunctionKeys.CommuniKate_pets:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_pets";
                    break;

                case FunctionKeys.CommuniKate_privateparts:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_privateparts";
                    break;

                case FunctionKeys.CommuniKate_christmas:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_christmas";
                    break;

                case FunctionKeys.CommuniKate_outsideplaces:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_outsideplaces";
                    break;

                case FunctionKeys.CommuniKate_nature:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_nature";
                    break;

                case FunctionKeys.CommuniKate_meatandfish:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_meatandfish";
                    break;

                case FunctionKeys.CommuniKate_idontfeelwell:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_idontfeelwell";
                    break;

                case FunctionKeys.CommuniKate_job:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_job";
                    break;

                case FunctionKeys.CommuniKate_petcare:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_petcare";
                    break;

                case FunctionKeys.CommuniKate_aboutyou:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_aboutyou";
                    break;

                case FunctionKeys.CommuniKate_wateranimals:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_wateranimals";
                    break;

                case FunctionKeys.CommuniKate_months:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_months";
                    break;

                case FunctionKeys.CommuniKate_days:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_days";
                    break;

                case FunctionKeys.CommuniKate_milkshake:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_milkshake";
                    break;

                case FunctionKeys.CommuniKate_artthings:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_artthings";
                    break;

                case FunctionKeys.CommuniKate_educationstaff:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_educationstaff";
                    break;

                case FunctionKeys.CommuniKate_cookingwords:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_cookingwords";
                    break;

                case FunctionKeys.CommuniKate_family:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_family";
                    break;

                case FunctionKeys.CommuniKate_myday:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_myday";
                    break;

                case FunctionKeys.CommuniKate_feelings:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_feelings";
                    break;

                case FunctionKeys.CommuniKate_channels:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_channels";
                    break;

                case FunctionKeys.CommuniKate_weather:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_weather";
                    break;

                case FunctionKeys.CommuniKate_toys:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_toys";
                    break;

                case FunctionKeys.CommuniKate_tools:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_tools";
                    break;

                case FunctionKeys.CommuniKate_yetmoreactionwords:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_yetmoreactionwords";
                    break;

                case FunctionKeys.CommuniKate_describingdrinks:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_describingdrinks";
                    break;

                case FunctionKeys.CommuniKate_unknown:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_unknown";
                    break;

                case FunctionKeys.CommuniKate_vegetables:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_vegetables";
                    break;

                case FunctionKeys.CommuniKate_sports:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_sports";
                    break;

                case FunctionKeys.CommuniKate_music:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_music";
                    break;

                case FunctionKeys.CommuniKate_kitchenequipment:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_kitchenequipment";
                    break;

                case FunctionKeys.CommuniKate_showpictures:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_showpictures";
                    break;

                case FunctionKeys.CommuniKate_bodyparts:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_bodyparts";
                    break;

                case FunctionKeys.CommuniKate_chatting:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_chatting";
                    break;

                case FunctionKeys.CommuniKate_opposites:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_opposites";
                    break;

                case FunctionKeys.CommuniKate_commonthings:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_commonthings";
                    break;

                case FunctionKeys.CommuniKate_animals:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_animals";
                    break;

                case FunctionKeys.CommuniKate_salad:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_salad";
                    break;

                case FunctionKeys.CommuniKate_snackspudding:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_snackspudding";
                    break;

                case FunctionKeys.CommuniKate_actionwords:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_actionwords";
                    break;

                case FunctionKeys.CommuniKate_personalcare:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_personalcare";
                    break;

                case FunctionKeys.CommuniKate_shops:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_shops";
                    break;

                case FunctionKeys.CommuniKate_scwfriends:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_scwfriends";
                    break;

                case FunctionKeys.CommuniKate_littlewords:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_littlewords";
                    break;

                case FunctionKeys.CommuniKate_juice:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_juice";
                    break;

                case FunctionKeys.CommuniKate_numbers:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_numbers";
                    break;

                case FunctionKeys.CommuniKate_breakfast:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_breakfast";
                    break;

                case FunctionKeys.CommuniKate_things:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_things";
                    break;

                case FunctionKeys.CommuniKate_nz:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_nz";
                    break;

                case FunctionKeys.CommuniKate_transport:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_transport";
                    break;

                case FunctionKeys.CommuniKate_positionwords:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_positionwords";
                    break;

                case FunctionKeys.CommuniKate_fruit:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_fruit";
                    break;

                case FunctionKeys.CommuniKate_toppage:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_toppage";
                    break;

                case FunctionKeys.CommuniKate_spelling:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_spelling";
                    break;

                case FunctionKeys.CommuniKate_friends:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_friends";
                    break;

                case FunctionKeys.CommuniKate_wildanimals:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_wildanimals";
                    break;

                case FunctionKeys.CommuniKate_potato:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_potato";
                    break;

                case FunctionKeys.CommuniKate_dvd:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_dvd";
                    break;

                case FunctionKeys.CommuniKate_toiletries:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_toiletries";
                    break;

                case FunctionKeys.CommuniKate_materials:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_materials";
                    break;

                case FunctionKeys.CommuniKate_mystories:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_mystories";
                    break;

                case FunctionKeys.CommuniKate_time:
                    // Settings.Default.CommuniKateKeyboardCurrentContext = "CommuniKate_time";
                    break;

                case FunctionKeys.ConversationConfirmYes:
                    Text = null;
                    StoreLastTextChange(null);
                    ClearSuggestions();
                    AutoPressShiftIfAppropriate();
                    Log.Debug("Suppressing next auto space.");
                    suppressNextAutoSpace = true;
                    Text = Resources.YES;
                    break;

                case FunctionKeys.ConversationConfirmNo:
                    Text = null;
                    StoreLastTextChange(null);
                    ClearSuggestions();
                    AutoPressShiftIfAppropriate();
                    Log.Debug("Suppressing next auto space.");
                    suppressNextAutoSpace = true;
                    Text = Resources.NO;
                    break;

                case FunctionKeys.SimplifiedAlphaClear:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "";
                    break;

                case FunctionKeys.SimplifiedAlphaABCDEFGHI:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "ABCDEFGHI";
                    break;

                case FunctionKeys.SimplifiedAlphaJKLMNOPQR:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "JKLMNOPQR";
                    break;

                case FunctionKeys.SimplifiedAlphaSTUVWXYZ:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "STUVWXYZ";
                    break;

                case FunctionKeys.SimplifiedAlphaABC:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "ABC";
                    break;

                case FunctionKeys.SimplifiedAlphaDEF:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "DEF";
                    break;

                case FunctionKeys.SimplifiedAlphaGHI:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "GHI";
                    break;

                case FunctionKeys.SimplifiedAlphaJKL:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "JKL";
                    break;

                case FunctionKeys.SimplifiedAlphaMNO:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "MNO";
                    break;

                case FunctionKeys.SimplifiedAlphaPQR:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "PQR";
                    break;

                case FunctionKeys.SimplifiedAlphaSTU:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "STU";
                    break;

                case FunctionKeys.SimplifiedAlphaVWX:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "VWX";
                    break;

                case FunctionKeys.SimplifiedAlphaYZ:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "YZ";
                    break;

                case FunctionKeys.SimplifiedAlphaNum:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "Num";
                    break;

                case FunctionKeys.SimplifiedAlpha123:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "123";
                    break;

                case FunctionKeys.SimplifiedAlpha456:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "456";
                    break;

                case FunctionKeys.SimplifiedAlpha789:
                    Settings.Default.SimplifiedKeyboardCurrentContext = "789";
                    break;

                case FunctionKeys.Suggestion1:
                    SwapLastTextChangeForSuggestion(0);
                    lastTextChangeWasSuggestion = true;
                    break;

                case FunctionKeys.Suggestion2:
                    SwapLastTextChangeForSuggestion(1);
                    lastTextChangeWasSuggestion = true;
                    break;

                case FunctionKeys.Suggestion3:
                    SwapLastTextChangeForSuggestion(2);
                    lastTextChangeWasSuggestion = true;
                    break;

                case FunctionKeys.Suggestion4:
                    SwapLastTextChangeForSuggestion(3);
                    lastTextChangeWasSuggestion = true;
                    break;

                case FunctionKeys.Suggestion5:
                    SwapLastTextChangeForSuggestion(4);
                    lastTextChangeWasSuggestion = true;
                    break;

                case FunctionKeys.Suggestion6:
                    SwapLastTextChangeForSuggestion(5);
                    lastTextChangeWasSuggestion = true;
                    break;

                case FunctionKeys.LeftShift:
                    shiftStateSetAutomatically = false;
                    GenerateSuggestions(lastTextChangeWasSuggestion);
                    break;

                default:
                    if (functionKey.ToVirtualKeyCode() != null)
                    {
                        //Key corresponds to physical keyboard key
                        GenerateSuggestions(false);

                        //If the key cannot be pressed or locked down (these are handled in
                        //ReactToPublishableKeyDownStateChanges) then publish it and release unlocked keys
                        var keyValue = new KeyValue(functionKey);
                        if (!KeyValues.KeysWhichCanBePressedOrLockedDown.Contains(keyValue))
                        {
                            PublishKeyPress(functionKey);
                            ReleaseUnlockedKeys();
                        }
                    }

                    lastTextChangeWasSuggestion = false;
                    break;
            }
        }

        public void ProcessSingleKeyText(string capturedText)
        {
            if (KeyValues.CombiningKeys.Any(k => k.String == capturedText))
            {
                //These are dead keys - do nothing until we have a letter to combine the pressed dead keys with
                Log.InfoFormat("Suppressing processing on {0} as it is a dead key", capturedText.ToPrintableString());
                return;
            }

            Log.DebugFormat("Processing single key captured text '{0}'", capturedText.ToPrintableString());
            var capturedTextAfterComposition = ComposeDiacritics(capturedText);
            ProcessText(capturedTextAfterComposition, true);
            lastTextChangeWasSuggestion = false;

            //Special handling for simplified keyboards
            if (Settings.Default.UseSimplifiedKeyboardLayout)
            {
                char last = capturedText.LastOrDefault();
                if (last != null)
                {
                    if (char.IsLetter(last))
                    {
                        Settings.Default.SimplifiedKeyboardCurrentContext = "";
                    }
                    else if (char.IsNumber(last))
                    {
                        Settings.Default.SimplifiedKeyboardCurrentContext = "Num";
                    }
                    else if (char.IsPunctuation(last))
                    {
                        Settings.Default.SimplifiedKeyboardCurrentContext = "";
                    }
                }
            }
        }

        private string ComposeDiacritics(string input)
        {
            Log.InfoFormat("Composing diacritics on '{0}'", input);
            var sb = new StringBuilder(input);
            KeyValues.CombiningKeys.ForEach(combiningKey =>
            {
                if (keyStateService.KeyDownStates[combiningKey].Value.IsDownOrLockedDown())
                {
                    Log.DebugFormat("Appending '{0}' onto '{1}'", combiningKey.String.ToPrintableString(), sb.ToString());
                    sb.Append(combiningKey.String);
                }
            });

            var output = sb.ToString().ComposeDiacritics();
            return output;
        }

        public void ProcessMultiKeyTextAndSuggestions(List<string> captureAndSuggestions)
        {
            Log.DebugFormat("Processing {0} captured multi-key selection results",
                captureAndSuggestions != null ? captureAndSuggestions.Count : 0);

            if (captureAndSuggestions == null || !captureAndSuggestions.Any()) return;

            StoreSuggestions(
                ModifySuggestions(captureAndSuggestions.Count > 1
                    ? captureAndSuggestions.Skip(1).ToList()
                    : null));

            ProcessText(captureAndSuggestions.First(), false);

            lastTextChangeWasSuggestion = false;
            lastTextChangeWasMultiKey = true;
        }

        #endregion

        #region Methods - private

        private void ProcessText(string captureText, bool generateSuggestions)
        {
            Log.DebugFormat("Processing captured text '{0}'", captureText.ToPrintableString());

            if (string.IsNullOrEmpty(captureText)) return;

            //Suppress auto space if...
            if (string.IsNullOrWhiteSpace(lastTextChange))
            {
                //We have no text change history, or the last capture was whitespace.
                Log.Debug("Suppressing auto space before this capture as the last text change was null or white space.");
                suppressNextAutoSpace = true;
            }
            else if(lastTextChange.Length == 1
                && captureText.Length == 1
                && !lastTextChangeWasSuggestion
                && !(keyStateService.KeyDownStates[KeyValues.MultiKeySelectionIsOnKey].Value.IsDownOrLockedDown() && char.IsLetter(captureText.First())))
            {
                //We are capturing single chars and are on the 2nd+ character,
                //the last capture wasn't a suggestion (as these can also be 1 character and we want to inject the space if it is),
                //and the current capture is not a multi-key selection of a single letter (as we also want to inject the space for this scenario).
                Log.Debug("Suppressing auto space before this capture as the user appears to be typing one char at a time. Also the last text change was not a suggestion, and the current capture is not a single letter captured with multi-key selection enabled.");
                suppressNextAutoSpace = true;
            }
            else if (captureText.Length == 1
                && !char.IsLetter(captureText.First()))
            {
                //We have captured a single char which is not a letter
                Log.Debug("Suppressing auto space before this capture as this capture is a single character which is not a letter.");
                suppressNextAutoSpace = true;
            }
            else if (lastTextChange.Length == 1
                && !new[] {'.', '!', '?', ',', ':', ';', ')', ']', '}', '>'}.Contains(lastTextChange.First())
                && !char.IsLetter(lastTextChange.First()))
            {
                //The current capture (which we know is a letter or multi-key capture) follows a single character
                //which is not a letter, or a closing or mid-sentence punctuation; e.g. whitespace or a symbol
                Log.Debug("Suppressing auto space before this capture as it follows a single character which is not a letter, or a closing or mid-sentence punctuation mark.");
                suppressNextAutoSpace = true;
            }

            var textBeforeCaptureText = Text;

            //Modify the capture and apply to Text
            var modifiedCaptureText = ModifyText(captureText);
            if (!string.IsNullOrEmpty(modifiedCaptureText))
            {
                var spaceAdded = AutoAddSpace();
                if (spaceAdded)
                {
                    //Auto space added - recalc whether shift should be auto-pressed
                    var shiftPressed = AutoPressShiftIfAppropriate();

                    if (shiftPressed)
                    {
                        //LeftShift has been auto-pressed - re-apply modifiers to captured text and suggestions
                        modifiedCaptureText = ModifyText(captureText);
                        StoreSuggestions(ModifySuggestions(suggestionService.Suggestions));

                        //Ensure suggestions do not contain the modifiedText
                        if (!string.IsNullOrEmpty(modifiedCaptureText)
                            && suggestionService.Suggestions != null
                            && suggestionService.Suggestions.Contains(modifiedCaptureText))
                        {
                            suggestionService.Suggestions = suggestionService.Suggestions.Where(s => s != modifiedCaptureText).ToList();
                        }
                    }
                }

                Text = string.Concat(Text, modifiedCaptureText);
            }

            //Increment/decrement usage counts
            if (captureText.Length > 1)
            {
                dictionaryService.IncrementEntryUsageCount(captureText);
            }
            else if (captureText.Length == 1
                && !char.IsWhiteSpace(captureText[0]))
            {
                if (!string.IsNullOrEmpty(textBeforeCaptureText))
                {
                    var previousInProgressWord = textBeforeCaptureText.InProgressWord(textBeforeCaptureText.Length);
                    dictionaryService.DecrementEntryUsageCount(previousInProgressWord);
                }

                if (!string.IsNullOrEmpty(Text))
                {
                    var currentInProgressWord = Text.InProgressWord(Text.Length);
                    dictionaryService.IncrementEntryUsageCount(currentInProgressWord);
                }
            }

            //Publish each character (if SimulatingKeyStrokes), releasing 'on' (but not 'locked') modifier keys as appropriate
            for (int index = 0; index < captureText.Length; index++)
            {
                PublishKeyPress(modifiedCaptureText != null && modifiedCaptureText.Length == captureText.Length
                        ? modifiedCaptureText[index]
                        : captureText[index]);

                ReleaseUnlockedKeys();
            }

            if (!string.IsNullOrEmpty(modifiedCaptureText))
            {
                AutoPressShiftIfAppropriate();
                suppressNextAutoSpace = false;
            }

            StoreLastTextChange(modifiedCaptureText);

            if (generateSuggestions)
            {
                GenerateSuggestions(false);
            }
        }

        private bool AutoPressShiftIfAppropriate()
        {
            if (Settings.Default.AutoCapitalise
                && Text.NextCharacterWouldBeStartOfNewSentence()
                && keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Up)
            {
                Log.Debug("Auto-pressing shift.");
                keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value = KeyDownStates.Down;
                if (fireKeySelectionEvent != null) fireKeySelectionEvent(KeyValues.LeftShiftKey);
                shiftStateSetAutomatically = true;
                SuppressOrReinstateAutoCapitalisation();
                return true;
            }
            return false;
        }

        private void ReactToKeyboardIsShiftAwareChanges()
        {
            this.OnPropertyChanges(tos => tos.KeyboardIsShiftAware)
                .Subscribe(_ => SuppressOrReinstateAutoCapitalisation());
        }

        private void ReactToSuppressAutoCapitaliseIntelligentlyChanges()
        {
            Settings.Default.OnPropertyChanges(s => s.SuppressAutoCapitaliseIntelligently)
                .Subscribe(_ => ReactToSuppressAutoCapitaliseIntelligentlyChanges());
        }

        private void SuppressOrReinstateAutoCapitalisation()
        {
            if (Settings.Default.AutoCapitalise
                && Settings.Default.SuppressAutoCapitaliseIntelligently
                && shiftStateSetAutomatically)
            {
                if (KeyboardIsShiftAware
                    && keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Up)
                {
                    keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value = KeyDownStates.Down;
                    if (fireKeySelectionEvent != null) fireKeySelectionEvent(KeyValues.LeftShiftKey);
                    return;
                }

                if (!KeyboardIsShiftAware
                    && keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Down)
                {
                    keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value = KeyDownStates.Up;
                    if (fireKeySelectionEvent != null) fireKeySelectionEvent(KeyValues.LeftShiftKey);
                    return;
                }
            }
        }

        private void ReactToShiftStateChanges()
        {
            keyStateService.KeyDownStates[KeyValues.LeftShiftKey].OnPropertyChanges(np => np.Value)
                .Subscribe(_ =>
                {
                    if (!shiftStateSetAutomatically)
                    {
                        GenerateSuggestions(false);
                    }
                });
        }

        private void ReactToSimulateKeyStrokesChanges()
        {
            Log.Info("Adding SimulateKeyStrokes change handlers.");
            keyStateService.OnPropertyChanges(kss => kss.SimulateKeyStrokes).Subscribe(_ => SimulateKeyStrokesHasChanged(true));
            SimulateKeyStrokesHasChanged(false);
        }

        private void SimulateKeyStrokesHasChanged(bool saveCurrentState)
        {
            var newStateKey = keyStateService.SimulateKeyStrokes;
            var currentStateKey = !newStateKey;

            if (saveCurrentState)
            {
                //Save old state values
                var lastState = new KeyboardOutputServiceState(
                    currentStateKey,
                    () => text, s => Text = s, //Set property (not field) to trigger bindings
                    () => lastTextChange, s => lastTextChange = s,
                    () => lastTextChangeWasSuggestion, b => lastTextChangeWasSuggestion = b,
                    () => suppressNextAutoSpace, b => suppressNextAutoSpace = b,
                    () => shiftStateSetAutomatically, b => shiftStateSetAutomatically = b,
                    () => suggestionService.Suggestions, s => suggestionService.Suggestions = s);
                if (state.ContainsKey(currentStateKey))
                {
                    state[currentStateKey] = lastState;
                }
                else
                {
                    state.Add(currentStateKey, lastState);
                }
            }

            //Restore state or default state
            if (state.ContainsKey(newStateKey))
            {
                state[newStateKey].RestoreState();
            }
            else
            {
                Log.InfoFormat("No stored KeyboardOutputService state to restore for SimulateKeyStrokes={0} - defaulting state.", newStateKey);
                Text = null;
                StoreLastTextChange(null);
                ClearSuggestions();
                ReleaseUnlockedKeys();
                AutoPressShiftIfAppropriate();
                Log.Debug("Suppressing next auto space.");
                suppressNextAutoSpace = true;
            }

            //Release all down keys
            publishService.ReleaseAllDownKeys();

            if (keyStateService.SimulateKeyStrokes)
            {
                //SimulatingKeyStrokes is on so publish key down events for all down/locked down keys
                KeyValues.KeysWhichCanBePressedOrLockedDown
                    .Where(key => keyStateService.KeyDownStates[key].Value.IsDownOrLockedDown() && key.FunctionKey != null)
                    .Select(key => key.FunctionKey.Value.ToVirtualKeyCode())
                    .Where(virtualKeyCode => virtualKeyCode != null)
                    .ToList()
                    .ForEach(virtualKeyCode => publishService.KeyDown(virtualKeyCode.Value));
            }
        }

        private void ReactToPublishableKeyDownStateChanges()
        {
            foreach (var key in KeyValues.KeysWhichCanBePressedOrLockedDown
                .Where(k => k.FunctionKey != null && k.FunctionKey.Value.ToVirtualKeyCode() != null))
            {
                var keyCopy = key; //Access to foreach variable in modified

                keyStateService.KeyDownStates[key].OnPropertyChanges(s => s.Value)
                    .Subscribe(value =>
                    {
                        if (keyStateService.SimulateKeyStrokes)
                        {
                            // ReSharper disable PossibleInvalidOperationException
                            var virtualKeyCode = keyCopy.FunctionKey.Value.ToVirtualKeyCode().Value;
                            // ReSharper restore PossibleInvalidOperationException

                            if (value.IsDownOrLockedDown())
                            {
                                publishService.KeyDown(virtualKeyCode);
                            }
                            else
                            {
                                publishService.KeyUp(virtualKeyCode);
                            }
                        }
                    });
            }
        }

        private void StoreLastTextChange(string textChange)
        {
            Log.DebugFormat("Storing last text change '{0}'", textChange);
            lastTextChange = textChange;
        }

        private void GenerateSuggestions(bool nextWord)
        {
            if (Settings.Default.SuggestWords)
            {
                Log.DebugFormat("Generating auto complete suggestions from '{0}'.", Text);

                var inProgressWord = Text == null ? null : Text.InProgressWord(Text.Length);
                var root = Text;

                if (!Settings.Default.SuggestNextWords)
                {
                    nextWord = false;
                    root = inProgressWord;
                }
                else if (lastTextChangeWasMultiKey)
                {
                    lastTextChangeWasMultiKey = false;
                    lastTextChangeWasSuggestion = true;
                }

                var suggestions = dictionaryService.GetSuggestions(root, nextWord)
                    .Take(Settings.Default.MaxDictionaryMatchesOrSuggestions)
                    .ToList();

                Log.DebugFormat("{0} suggestions generated (possibly capped to {1} by MaxDictionaryMatchesOrSuggestions setting)",
                    suggestions.Count(), Settings.Default.MaxDictionaryMatchesOrSuggestions);

                if (!nextWord & inProgressWord != null)
                {
                    // Ensure that the entered word is in the list of suggestions...
                    if (!suggestions.Contains(inProgressWord, StringComparer.CurrentCultureIgnoreCase))
                    {
                        suggestions.Insert(0, inProgressWord);
                        suggestions = suggestions.Take(Settings.Default.MaxDictionaryMatchesOrSuggestions).ToList();
                    }
                    else
                    {
                        // ...and that it is at the front of the list.
                        var index =
                            suggestions.FindIndex(
                                s => string.Equals(s, inProgressWord, StringComparison.CurrentCultureIgnoreCase));
                        if (index > 0)
                        {
                            suggestions.Swap(0, index);
                        }
                    }
                }

                //Correctly case suggestions
                var leftShiftIsDown = keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Down;
                var leftShiftIsLockedDown = keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.LockedDown;
                suggestionService.Suggestions = suggestions.Select(suggestion =>
                {
                    var suggestionChars = suggestion.ToCharArray();
                    for (var index = 0; index < suggestionChars.Length; index++)
                    {
                        bool makeUppercase = false;
                        if(nextWord || inProgressWord == null)
                        {
                            if (index == 0 && (leftShiftIsDown || leftShiftIsLockedDown)) //First character should be uppercase
                            {
                                makeUppercase = true;
                            }
                            else if (index > 0 && leftShiftIsLockedDown)
                            {
                                makeUppercase = true; //Shift is locked down, so all characters after the in progress word should be uppercase
                            }
                            else if (index > 0 && leftShiftIsLockedDown)
                            {
                                makeUppercase = true; //Shift is locked down, so all characters after the in progress word should be uppercase
                            }
                        }
                        else
                        {
                            if (index < inProgressWord.Length
                                && char.IsUpper(inProgressWord[index]))
                            {
                                makeUppercase = true; //In progress word is uppercase as this index
                            }
                            else if (index == inProgressWord.Length
                                && (leftShiftIsDown || leftShiftIsLockedDown))
                            {
                                makeUppercase = true; //Shift is down, so the next character after the end of the in progress word should be uppercase
                            }
                            else if (index > inProgressWord.Length
                                        && leftShiftIsLockedDown)
                            {
                                makeUppercase = true; //Shift is locked down, so all characters after the in progress word should be uppercase
                            }
                        }

                        if (makeUppercase)
                        {
                            suggestionChars[index] = char.ToUpper(suggestion[index], Settings.Default.KeyboardAndDictionaryLanguage.ToCultureInfo());
                        }
                    }

                    return new string(suggestionChars);
                })
                .Distinct() //Changing the casing can result in multiple identical entries (e.g. "am" and "AM" both could become "am")
                .ToList();

                return;
            }
        }

        private void ClearSuggestions()
        {
            Log.Info("Clearing suggestions.");
            suggestionService.Suggestions = null;
        }

        private List<string> ModifySuggestions(List<string> suggestions)
        {
            Log.DebugFormat("Modifying {0} suggestions.", suggestions != null ? suggestions.Count : 0);

            if (suggestions == null || !suggestions.Any()) return null;

            var modifiedSuggestions = suggestions
                .Select(ModifyText)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToList();

            Log.DebugFormat("After applying modifiers there are {0} modified suggestions.", modifiedSuggestions.Count);

            return modifiedSuggestions.Any() ? modifiedSuggestions : null;
        }

        private void StoreSuggestions(List<string> suggestions)
        {
            Log.DebugFormat("Storing {0} suggestions.", suggestions != null ? suggestions.Count : 0);

            suggestionService.Suggestions = suggestions != null && suggestions.Any()
                ? suggestions
                : null;
        }

        private void PublishKeyPress(FunctionKeys functionKey)
        {
            if (keyStateService.SimulateKeyStrokes)
            {
                var virtualKeyCode = functionKey.ToVirtualKeyCode();
                if (virtualKeyCode != null)
                {
                    Log.InfoFormat("Publishing function key '{0}' => as virtual key code {1}", functionKey, virtualKeyCode);
                    publishService.KeyDownUp(virtualKeyCode.Value);
                }
            }
        }

        private void PublishKeyPress(char character)
        {
            if (keyStateService.SimulateKeyStrokes)
            {
                var virtualKeyCode = character.ToVirtualKeyCode();
                if (virtualKeyCode != null)
                {
                    Log.InfoFormat("Publishing '{0}' => as virtual key code {1} (using hard coded mapping)",
                        character.ToPrintableString(), virtualKeyCode);
                    publishService.KeyDownUp(virtualKeyCode.Value);
                    return;
                }

                if (!Settings.Default.PublishVirtualKeyCodesForCharacters)
                {
                    Log.InfoFormat("Publishing '{0}' as text", character.ToPrintableString());
                    publishService.TypeText(character.ToString());
                    return;
                }

                //Get keyboard layout of currently focused window
                IntPtr hWnd = PInvoke.GetForegroundWindow();
                int lpdwProcessId;
                int winThreadProcId = PInvoke.GetWindowThreadProcessId(hWnd, out lpdwProcessId);
                IntPtr keyboardLayout = PInvoke.GetKeyboardLayout(winThreadProcId);

                //Convert this into a culture string for logging
                string keyboardCulture = "Unknown";
                var installedInputLanguages = InputLanguage.InstalledInputLanguages;
                for (int i = 0; i < installedInputLanguages.Count; i++)
                {
                    if (keyboardLayout == installedInputLanguages[i].Handle)
                    {
                        keyboardCulture = installedInputLanguages[i].Culture.DisplayName;
                        break;
                    }
                }

                //Attempt to lookup virtual key code (and modifier states)
                var vkKeyScan = PInvoke.VkKeyScanEx(character, keyboardLayout);
                var vkCode = vkKeyScan & 0xff;
                var shift = (vkKeyScan & 0x100) > 0;
                var ctrl = (vkKeyScan & 0x200) > 0;
                var alt = (vkKeyScan & 0x400) > 0;

                if (vkKeyScan != -1)
                {
                    Log.InfoFormat("Publishing '{0}' => as virtual key code {1}(0x{1:X}){2}{3}{4} (using VkKeyScanEx with keyboard layout:{5})",
                        character.ToPrintableString(), vkCode, shift ? "+SHIFT" : null,
                        ctrl ? "+CTRL" : null, alt ? "+ALT" : null, keyboardCulture);

                    bool releaseShift = false;
                    bool releaseCtrl = false;
                    bool releaseAlt = false;

                    if (shift && keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Up)
                    {
                        publishService.KeyDown(FunctionKeys.LeftShift.ToVirtualKeyCode().Value);
                        releaseShift = true;
                    }

                    if (ctrl && keyStateService.KeyDownStates[KeyValues.LeftCtrlKey].Value == KeyDownStates.Up)
                    {
                        publishService.KeyDown(FunctionKeys.LeftCtrl.ToVirtualKeyCode().Value);
                        releaseCtrl = true;
                    }

                    if (alt && keyStateService.KeyDownStates[KeyValues.LeftAltKey].Value == KeyDownStates.Up)
                    {
                        publishService.KeyDown(FunctionKeys.LeftAlt.ToVirtualKeyCode().Value);
                        releaseAlt = true;
                    }

                    publishService.KeyDownUp((VirtualKeyCode)vkCode);

                    if (releaseShift)
                    {
                        publishService.KeyUp(FunctionKeys.LeftShift.ToVirtualKeyCode().Value);
                    }
                    if (releaseCtrl)
                    {
                        publishService.KeyUp(FunctionKeys.LeftCtrl.ToVirtualKeyCode().Value);
                    }
                    if (releaseAlt)
                    {
                        publishService.KeyUp(FunctionKeys.LeftAlt.ToVirtualKeyCode().Value);
                    }
                }
                else
                {
                    Log.InfoFormat("No virtual key code found for '{0}' so publishing as text (keyboard:{1})",
                        character.ToPrintableString(), keyboardCulture);
                    publishService.TypeText(character.ToString());
                }
            }
        }

        private void ReleaseUnlockedKeys()
        {
            Log.Info("ReleaseUnlockedKeys called.");

            foreach (var key in keyStateService.KeyDownStates.Keys)
            {
                if (keyStateService.KeyDownStates[key].Value == KeyDownStates.Down)
                {
                    Log.DebugFormat("Releasing {0} key.", key);
                    keyStateService.KeyDownStates[key].Value = KeyDownStates.Up;
                    if (fireKeySelectionEvent != null) fireKeySelectionEvent(key);
                }
            }

            //This method is called by manual user actions so the shift key would be released if it was automatically set
            shiftStateSetAutomatically = false;
        }

        private void SwapLastTextChangeForSuggestion(int index)
        {
            Log.DebugFormat("SwapLastTextChangeForSuggestion called with index {0}", index);

            var suggestionIndex = (suggestionService.SuggestionsPage * suggestionService.SuggestionsPerPage) + index;
            if (suggestionService.Suggestions.Count > suggestionIndex)
            {
                if (!string.IsNullOrEmpty(lastTextChange)
                    && lastTextChange.Length > 1
                    && lastTextChangeWasMultiKey)
                {
                    //We are swapping out a multi-key capture
                    var replacedText = lastTextChange;
                    SwapText(lastTextChange, suggestionService.Suggestions[suggestionIndex]);
                    var newSuggestions = suggestionService.Suggestions.ToList();
                    newSuggestions[suggestionIndex] = replacedText;
                    StoreSuggestions(newSuggestions);
                }
                else
                {
                    var inProgressWord = Text == null ? null : Text.InProgressWord(Text.Length);
                    if (!Settings.Default.SuggestNextWords || !lastTextChangeWasSuggestion && !string.IsNullOrEmpty(inProgressWord) && Char.IsLetterOrDigit(inProgressWord.Last()))
                    {
                        //We are auto-completing a word with a suggestion
                        SwapText(inProgressWord, suggestionService.Suggestions[suggestionIndex]);
                        GenerateSuggestions(true);
                    }
                    else
                    {
                        //We are writing the first word or adding a whole word
                        ProcessText(suggestionService.Suggestions[suggestionIndex], false);
                        GenerateSuggestions(true);
                    }
                }
                suppressNextAutoSpace = false;
            }
        }

        private void SwapText(string textToSwapOut, string textToSwapIn)
        {
            Log.DebugFormat("SwapText called to swap '{0}' for '{1}'.", textToSwapOut, textToSwapIn);

            if (!string.IsNullOrEmpty(textToSwapOut)
                && !string.IsNullOrEmpty(textToSwapIn)
                && Text != null
                && Text.Length >= textToSwapOut.Length)
            {
                dictionaryService.DecrementEntryUsageCount(textToSwapOut);
                dictionaryService.IncrementEntryUsageCount(textToSwapIn);

                Text = string.Concat(Text.Substring(0, Text.Length - textToSwapOut.Length), textToSwapIn);

                var textHasSameRoot = textToSwapIn.StartsWith(textToSwapOut, StringComparison.Ordinal);
                if (!textHasSameRoot) //Only backspace the old word if it doesn't share the same root as the new word
                {
                    for (int i = 0; i < textToSwapOut.Length; i++)
                    {
                        PublishKeyPress(FunctionKeys.BackOne);
                    }
                }

                var publishText = textHasSameRoot ? textToSwapIn.Substring(textToSwapOut.Length) : textToSwapIn;
                foreach (char c in publishText)
                {
                    PublishKeyPress(c);
                }

                StoreLastTextChange(textToSwapIn);
            }
        }

        private bool AutoAddSpace()
        {
            if (Settings.Default.AutoAddSpace
                && Text != null
                && Text.Any()
                && !suppressNextAutoSpace)
            {
                Log.Info("Publishing auto space and adding auto space to Text.");
                PublishKeyPress(' ');
                Text = string.Concat(Text, " ");
                return true;
            }

            return false;
        }

        private string ModifyText(string textToModify)
        {
            //TODO Handle LeftAlt modified captures - LeftAlt+Code = unicode characters

            if (KeyValues.KeysWhichPreventTextCaptureIfDownOrLocked.Any(kv =>
                keyStateService.KeyDownStates[kv].Value.IsDownOrLockedDown()))
            {
                Log.DebugFormat("A key which prevents text capture is down - modifying '{0}' to null.", textToModify.ToPrintableString());
                return null;
            }

            if (!string.IsNullOrEmpty(textToModify))
            {
                if (keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.Down)
                {
                    var modifiedText = textToModify.FirstCharToUpper();
                    Log.DebugFormat("LeftShift is on so modifying '{0}' to '{1}.", textToModify, modifiedText);
                    return modifiedText;
                }

                if (keyStateService.KeyDownStates[KeyValues.LeftShiftKey].Value == KeyDownStates.LockedDown)
                {
                    var modifiedText = textToModify.ToUpper(Settings.Default.KeyboardAndDictionaryLanguage.ToCultureInfo());
                    Log.DebugFormat("LeftShift is locked so modifying '{0}' to '{1}.", textToModify, modifiedText);
                    return modifiedText;
                }
            }

            return textToModify;
        }

        #endregion
    }
}
