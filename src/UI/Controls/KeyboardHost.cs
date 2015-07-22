using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.UI.Utilities;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards;
using log4net;
using Size = System.Windows.Size;
using StandardViews = JuliusSweetland.OptiKey.UI.Views.Keyboards.Standard;
using ConversationOnlyViews = JuliusSweetland.OptiKey.UI.Views.Keyboards.ConversationOnly;
using ViewModelKeyboards = JuliusSweetland.OptiKey.UI.ViewModels.Keyboards;
using YesNoQuestion = JuliusSweetland.OptiKey.UI.Views.Keyboards.ConversationOnly.English.YesNoQuestion;

namespace JuliusSweetland.OptiKey.UI.Controls
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
            Settings.Default.OnPropertyChanges(s => s.UxMode).Subscribe(_ => GenerateContent());

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
            Log.DebugFormat("GenerateContent called. Language setting is '{0}' and Keyboard type is '{1}'", 
                Settings.Default.Language, Keyboard != null ? Keyboard.GetType() : null);
            
            //Clear out point to key map and pause input service
            PointToKeyValueMap = null;
            if(InputService != null)
            {
                InputService.RequestSuspend();
            }
            
            object newContent = ErrorContent;

            switch (Settings.Default.UxMode)
            {
                case UxModes.Standard:
                    switch (Settings.Default.Language)
                    {
                        case Languages.AmericanEnglish:
                        case Languages.BritishEnglish:
                        case Languages.CanadianEnglish:
                            if (Keyboard is ViewModelKeyboards.Alpha)
                            {
                                newContent = new StandardViews.English.Alpha { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.Diacritic1)
                            {
                                newContent = new StandardViews.English.Diacritic1 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.Diacritic2)
                            {
                                newContent = new StandardViews.English.Diacritic2 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.Diacritic3)
                            {
                                newContent = new StandardViews.English.Diacritic3 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.Currencies1)
                            {
                                newContent = new StandardViews.English.Currencies1 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.Currencies2)
                            {
                                newContent = new StandardViews.English.Currencies2 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.Menu)
                            {
                                newContent = new StandardViews.English.Menu { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.Minimised)
                            {
                                newContent = new StandardViews.English.Minimised() { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.Mouse)
                            {
                                newContent = new StandardViews.English.Mouse { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.NumericAndSymbols2)
                            {
                                newContent = new StandardViews.English.NumericAndSymbols2 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.NumericAndSymbols3)
                            {
                                newContent = new StandardViews.English.NumericAndSymbols3 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.NumericAndSymbols1)
                            {
                                newContent = new StandardViews.English.NumericAndSymbols1 { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.PhysicalKeys)
                            {
                                newContent = new StandardViews.English.PhysicalKeys { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.Position)
                            {
                                newContent = new StandardViews.English.Position { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.Size)
                            {
                                newContent = new StandardViews.English.Size { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.YesNoQuestion)
                            {
                                newContent = new StandardViews.English.YesNoQuestion { DataContext = Keyboard };
                            }
                            break;
                    }
                    break;

                case UxModes.ConversationOnly:
                    switch (Settings.Default.Language)
                    {
                        case Languages.AmericanEnglish:
                        case Languages.BritishEnglish:
                        case Languages.CanadianEnglish:
                            if (Keyboard is ViewModelKeyboards.Alpha)
                            {
                                newContent = new ConversationOnlyViews.English.Alpha { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.Size)
                            {
                                newContent = new StandardViews.English.Size { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.Position)
                            {
                                newContent = new StandardViews.English.Position { DataContext = Keyboard };
                            }
                            else if (Keyboard is ViewModelKeyboards.YesNoQuestion)
                            {
                                newContent = new YesNoQuestion { DataContext = Keyboard };
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
                InputService.RequestResume();
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
