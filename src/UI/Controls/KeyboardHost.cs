using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Properties;
using JuliusSweetland.ETTA.Services;
using JuliusSweetland.ETTA.UI.Utilities;
using JuliusSweetland.ETTA.UI.ViewModels.Keyboards;
using log4net;
using Alpha = JuliusSweetland.ETTA.UI.Views.Keyboards.Standard.English.Alpha;
using AlternativeAlpha1 = JuliusSweetland.ETTA.UI.Views.Keyboards.Standard.English.AlternativeAlpha1;
using AlternativeAlpha2 = JuliusSweetland.ETTA.UI.Views.Keyboards.Standard.English.AlternativeAlpha2;
using AlternativeAlpha3 = JuliusSweetland.ETTA.UI.Views.Keyboards.Standard.English.AlternativeAlpha3;
using Currencies1 = JuliusSweetland.ETTA.UI.Views.Keyboards.Standard.English.Currencies1;
using Currencies2 = JuliusSweetland.ETTA.UI.Views.Keyboards.Standard.English.Currencies2;
using ManipulateWindow = JuliusSweetland.ETTA.UI.Views.Keyboards.Standard.English.ManipulateWindow;
using Menu = JuliusSweetland.ETTA.UI.Views.Keyboards.Standard.English.Menu;
using NumericAndSymbols1 = JuliusSweetland.ETTA.UI.Views.Keyboards.Standard.English.NumericAndSymbols1;
using NumericAndSymbols2 = JuliusSweetland.ETTA.UI.Views.Keyboards.Standard.English.NumericAndSymbols2;
using NumericAndSymbols3 = JuliusSweetland.ETTA.UI.Views.Keyboards.Standard.English.NumericAndSymbols3;
using PhysicalKeys = JuliusSweetland.ETTA.UI.Views.Keyboards.Standard.English.PhysicalKeys;
using YesNoQuestion = JuliusSweetland.ETTA.UI.Views.Keyboards.Standard.English.YesNoQuestion;

namespace JuliusSweetland.ETTA.UI.Controls
{
    public class KeyboardHost : ContentControl
    {
        #region Private member vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public KeyboardHost()
        {
            Settings.Default.OnPropertyChanges(s => s.Language).Subscribe(_ => GenerateContent());
            Settings.Default.OnPropertyChanges(s => s.KeyboardSet).Subscribe(_ => GenerateContent());

            Loaded += OnLoaded;
        }

        #endregion

        #region Properties

        public static readonly DependencyProperty InputServiceProperty =
            DependencyProperty.Register("InputService", typeof(IInputService),
                typeof(KeyboardHost), new PropertyMetadata(default(IInputService)));

        public IInputService InputService
        {
            get { return (InputService)GetValue(InputServiceProperty); }
            set { SetValue(InputServiceProperty, value); }
        }

        public static readonly DependencyProperty KeyboardProperty =
            DependencyProperty.Register("Keyboard", typeof (IKeyboard), typeof (KeyboardHost),
                new PropertyMetadata(default(IKeyboard),
                    (o, args) =>
                    {
                        var keyboardHost = o as KeyboardHost;
                        if (keyboardHost != null)
                        {
                            keyboardHost.GenerateContent();
                        }
                    }));

        public IKeyboard Keyboard
        {
            get { return (IKeyboard) GetValue(KeyboardProperty); }
            set { SetValue(KeyboardProperty, value); }
        }

        public static readonly DependencyProperty PointToKeyValueMapProperty =
            DependencyProperty.Register("PointToKeyValueMap", typeof(Dictionary<Rect, KeyValue>),
                typeof(KeyboardHost), new PropertyMetadata(default(Dictionary<Rect, KeyValue>)));

        public Dictionary<Rect, KeyValue> PointToKeyValueMap
        {
            get { return (Dictionary<Rect, KeyValue>)GetValue(PointToKeyValueMapProperty); }
            set { SetValue(PointToKeyValueMapProperty, value); }
        }

        public static readonly DependencyProperty ErrorContentProperty =
            DependencyProperty.Register("ErrorContent", typeof (object), typeof (KeyboardHost), new PropertyMetadata(default(object)));

        public object ErrorContent
        {
            get { return GetValue(ErrorContentProperty); }
            set { SetValue(ErrorContentProperty, value); }
        }

        #endregion

        #region OnLoaded - build key map

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Log.Debug("KeyboardHost loaded.");

            BuildPointToKeyMap();

            SubscribeToSizeChanges();

            var parentWindow = Window.GetWindow(this);

            if (parentWindow == null)
            {
                var windowException = new ApplicationException("Parent Window could not be identified. Unable to continue");

                Log.Error(windowException);

                throw windowException;
            }
            
            SubscribeToParentWindowMoves(parentWindow);

            Loaded -= OnLoaded; //Ensure this logic only runs once
        }

        #endregion

        #region Generate Content

        private void GenerateContent()
        {
            Log.Debug(string.Format("GenerateContent called. Language setting is '{0}' and Keyboard type is '{1}'", 
                Settings.Default.Language, Keyboard != null ? Keyboard.GetType() : null));
            
            //Clear out point to key map and pause input service
            PointToKeyValueMap = null;
            if(InputService != null)
            {
                InputService.State = RunningStates.Paused;
            }
            
            object newContent = ErrorContent;

            switch (Settings.Default.KeyboardSet)
            {
                case KeyboardsSets.Standard:
                    switch (Settings.Default.Language)
                    {
                        case Languages.AmericanEnglish:
                        case Languages.BritishEnglish:
                        case Languages.CanadianEnglish:
                            if (Keyboard is ViewModels.Keyboards.Alpha)
                            {
                                newContent = new Alpha { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModels.Keyboards.AlternativeAlpha1)
                            {
                                newContent = new AlternativeAlpha1 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModels.Keyboards.AlternativeAlpha2)
                            {
                                newContent = new AlternativeAlpha2 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModels.Keyboards.AlternativeAlpha3)
                            {
                                newContent = new AlternativeAlpha3 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModels.Keyboards.Currencies1)
                            {
                                newContent = new Currencies1 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModels.Keyboards.Currencies2)
                            {
                                newContent = new Currencies2 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModels.Keyboards.ManipulateWindow)
                            {
                                newContent = new ManipulateWindow { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModels.Keyboards.Menu)
                            {
                                newContent = new Menu { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModels.Keyboards.NumericAndSymbols2)
                            {
                                newContent = new NumericAndSymbols2 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModels.Keyboards.NumericAndSymbols3)
                            {
                                newContent = new NumericAndSymbols3 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModels.Keyboards.NumericAndSymbols1)
                            {
                                newContent = new NumericAndSymbols1 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModels.Keyboards.PhysicalKeys)
                            {
                                newContent = new PhysicalKeys { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModels.Keyboards.YesNoQuestion)
                            {
                                newContent = new YesNoQuestion { DataContext = Keyboard };
                            }
                            break;
                    }
                    break;

                case KeyboardsSets.SpeechOnly:
                    switch (Settings.Default.Language)
                    {
                        case Languages.AmericanEnglish:
                        case Languages.BritishEnglish:
                        case Languages.CanadianEnglish:
                            if (Keyboard is ViewModels.Keyboards.Alpha)
                            {
                                newContent = new Views.Keyboards.SpeechOnly.English.Alpha { DataContext = Keyboard };
                            }
                            break;
                    }
                    break;
            }
            
            var contentAsFrameworkElement = newContent as FrameworkElement;
            if (contentAsFrameworkElement != null)
            {
                if (contentAsFrameworkElement.IsLoaded)
                {
                    ReactToNewContentLoaded();
                }
                else
                {
                    RoutedEventHandler loaded = null;
                    loaded = (sender, args) =>
                    {
                        ReactToNewContentLoaded();
                        contentAsFrameworkElement.Loaded -= loaded;
                    };
                    contentAsFrameworkElement.Loaded += loaded;
                }
            }

            Content = newContent;
        }

        #endregion
        
        #region React To New Content Loaded
        
        private void ReactToNewContentLoaded()
        {
            BuildPointToKeyMap();
            
            if(InputService != null)
            {
                InputService.State = RunningStates.Running;
            }
        }
        
        #endregion

        #region Build Point To Key Map

        private void BuildPointToKeyMap()
        {
            Log.Debug("Building PointToKeyMap.");

            var allKeys = VisualAndLogicalTreeHelper.FindVisualChildren<Key>(this).ToList();

            var pointToKeyValueMap = new Dictionary<Rect, KeyValue>();

            var topLeftPoint = new Point(0, 0);

            foreach (var key in allKeys)
            {
                if (key.Value.FunctionKey != null
                    || key.Value.String != null)
                {
                    var rect = new Rect
                    {
                        Location = key.PointToScreen(topLeftPoint),
                        Size = (Size)key.GetTransformToDevice().Transform((Vector)key.RenderSize)
                    };

                    if (rect.Size.Width != 0 && rect.Size.Height != 0)
                    {
                        pointToKeyValueMap.Add(rect, key.Value);
                    }
                }
            }

            PointToKeyValueMap = pointToKeyValueMap;
        }

        #endregion

        #region Subscribe To Size Changes

        private void SubscribeToSizeChanges()
        {
            Observable.FromEventPattern<SizeChangedEventHandler, SizeChangedEventArgs>
                (h => SizeChanged += h,
                h => SizeChanged -= h)
                .Throttle(TimeSpan.FromSeconds(0.1))
                .ObserveOnDispatcher()
                .Subscribe(_ =>
                {
                    Log.Debug("SizeChanged event detected.");

                    BuildPointToKeyMap();
                });
        }

        #endregion

        #region Subscribe To Parent Window Moves

        private void SubscribeToParentWindowMoves(Window parentWindow)
        {
            //This event will also fire if the window is mimised, restored, or maximised, so no need to monitor StateChanged
            Observable.FromEventPattern<EventHandler, EventArgs>
                (h => parentWindow.LocationChanged += h,
                h => parentWindow.LocationChanged -= h)
                .Throttle(TimeSpan.FromSeconds(0.1))
                .ObserveOnDispatcher()
                .Subscribe(_ =>
                {
                    Log.Debug("Window's LocationChanged event detected.");
                    BuildPointToKeyMap();
                });
        }

        #endregion
    }
}
