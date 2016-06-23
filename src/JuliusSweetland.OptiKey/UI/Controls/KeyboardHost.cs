using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using JuliusSweetland.OptiKey.UI.Utilities;
using JuliusSweetland.OptiKey.UI.ViewModels.Keyboards.Base;
using log4net;
using CommonViews = JuliusSweetland.OptiKey.UI.Views.Keyboards.Common;
using CroatianViews = JuliusSweetland.OptiKey.UI.Views.Keyboards.Croatian;
using DutchViews = JuliusSweetland.OptiKey.UI.Views.Keyboards.Dutch;
using EnglishViews = JuliusSweetland.OptiKey.UI.Views.Keyboards.English;
using FrenchViews = JuliusSweetland.OptiKey.UI.Views.Keyboards.French;
using GermanViews = JuliusSweetland.OptiKey.UI.Views.Keyboards.German;
using GreekViews = JuliusSweetland.OptiKey.UI.Views.Keyboards.Greek;
using ItalianViews = JuliusSweetland.OptiKey.UI.Views.Keyboards.Italian;
using RussianViews = JuliusSweetland.OptiKey.UI.Views.Keyboards.Russian;
using SpanishViews = JuliusSweetland.OptiKey.UI.Views.Keyboards.Spanish;
using TurkishViews = JuliusSweetland.OptiKey.UI.Views.Keyboards.Turkish;
using ViewModelKeyboards = JuliusSweetland.OptiKey.UI.ViewModels.Keyboards;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    public class KeyboardHost : ContentControl
    {
        #region Private member vars

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public KeyboardHost()
        {
            Settings.Default.OnPropertyChanges(s => s.KeyboardAndDictionaryLanguage).Subscribe(_ => GenerateContent());
            Settings.Default.OnPropertyChanges(s => s.UiLanguage).Subscribe(_ => GenerateContent());
            Settings.Default.OnPropertyChanges(s => s.MouseKeyboardDockSize).Subscribe(_ => GenerateContent());
            Settings.Default.OnPropertyChanges(s => s.ConversationOnlyMode).Subscribe(_ => GenerateContent());

            Loaded += OnLoaded;

            var contentDp = DependencyPropertyDescriptor.FromProperty(ContentProperty, typeof(KeyboardHost));
            if (contentDp != null)
            {
                contentDp.AddValueChanged(this, ContentChangedHandler);
            }
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
                var windowException = new ApplicationException(Properties.Resources.PARENT_WINDOW_COULD_NOT_BE_FOUND);

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
            Log.DebugFormat("GenerateContent called. Keyboard language is '{0}' and Keyboard type is '{1}'", 
                Settings.Default.KeyboardAndDictionaryLanguage, Keyboard != null ? Keyboard.GetType() : null);

            //Clear out point to key map and pause input service
            PointToKeyValueMap = null;
            if(InputService != null)
            {
                InputService.RequestSuspend();
            }
            
            object newContent = ErrorContent;

            if (Keyboard is ViewModelKeyboards.Alpha)
            {
                switch (Settings.Default.KeyboardAndDictionaryLanguage)
                {
                    case Languages.CroatianCroatia:
                        newContent = new CroatianViews.Alpha { DataContext = Keyboard };
                        break;
                    case Languages.DutchBelgium:
                        newContent = new DutchViews.BelgiumAlpha { DataContext = Keyboard };
                        break;
                    case Languages.DutchNetherlands:
                        newContent = new DutchViews.NetherlandsAlpha { DataContext = Keyboard };
                        break;
                    case Languages.FrenchFrance:
                        newContent = new FrenchViews.Alpha { DataContext = Keyboard };
                        break;
                    case Languages.GermanGermany:
                        newContent = new GermanViews.Alpha { DataContext = Keyboard };
                        break;
                    case Languages.GreekGreece:
                        newContent = new GreekViews.Alpha { DataContext = Keyboard };
                        break;
                    case Languages.ItalianItaly:
                        newContent = new ItalianViews.Alpha { DataContext = Keyboard };
                        break;
                    case Languages.RussianRussia:
                        newContent = new RussianViews.Alpha { DataContext = Keyboard };
                        break;
                    case Languages.SpanishSpain:
                        newContent = new SpanishViews.Alpha { DataContext = Keyboard };
                        break;
                    case Languages.TurkishTurkey:
                        newContent = new TurkishViews.Alpha { DataContext = Keyboard };
                        break;
                    default:
                        newContent = new EnglishViews.Alpha { DataContext = Keyboard };
                        break;
                }
            }
            else if (Keyboard is ViewModelKeyboards.ConversationAlpha)
            {
                switch (Settings.Default.KeyboardAndDictionaryLanguage)
                {
                    case Languages.CroatianCroatia:
                        newContent = new CroatianViews.ConversationAlpha { DataContext = Keyboard };
                        break;
                    case Languages.DutchBelgium:
                        newContent = new DutchViews.BelgiumConversationAlpha { DataContext = Keyboard };
                        break;
                    case Languages.DutchNetherlands:
                        newContent = new DutchViews.NetherlandsConversationAlpha { DataContext = Keyboard };
                        break;
                    case Languages.FrenchFrance:
                        newContent = new FrenchViews.ConversationAlpha { DataContext = Keyboard };
                        break;
                    case Languages.GermanGermany:
                        newContent = new GermanViews.ConversationAlpha { DataContext = Keyboard };
                        break;
                    case Languages.GreekGreece:
                        newContent = new GreekViews.ConversationAlpha { DataContext = Keyboard };
                        break;
                    case Languages.ItalianItaly:
                        newContent = new ItalianViews.ConversationAlpha { DataContext = Keyboard };
                        break;
                    case Languages.RussianRussia:
                        newContent = new RussianViews.ConversationAlpha { DataContext = Keyboard };
                        break;
                    case Languages.SpanishSpain:
                        newContent = new SpanishViews.ConversationAlpha { DataContext = Keyboard };
                        break;
                    case Languages.TurkishTurkey:
                        newContent = new TurkishViews.ConversationAlpha { DataContext = Keyboard };
                        break;
                    default:
                        newContent = new EnglishViews.ConversationAlpha { DataContext = Keyboard };
                        break;
                }
            }
            else if (Keyboard is ViewModelKeyboards.ConversationNumericAndSymbols)
            {
                newContent = new CommonViews.ConversationNumericAndSymbols { DataContext = Keyboard };
            }
            else if (Keyboard is ViewModelKeyboards.Currencies1)
            {
                newContent = new CommonViews.Currencies1 { DataContext = Keyboard };
            }
            else if (Keyboard is ViewModelKeyboards.Currencies2)
            {
                newContent = new CommonViews.Currencies2 { DataContext = Keyboard };
            }
            else if (Keyboard is ViewModelKeyboards.Diacritics1)
            {
                newContent = new CommonViews.Diacritics1 { DataContext = Keyboard };
            }
            else if (Keyboard is ViewModelKeyboards.Diacritics2)
            {
                newContent = new CommonViews.Diacritics2 { DataContext = Keyboard };
            }
            else if (Keyboard is ViewModelKeyboards.Diacritics3)
            {
                newContent = new CommonViews.Diacritics3 { DataContext = Keyboard };
            }
            else if (Keyboard is ViewModelKeyboards.Language)
            {
                newContent = new CommonViews.Language { DataContext = Keyboard };
            }
            else if (Keyboard is ViewModelKeyboards.Menu)
            {
                newContent = new CommonViews.Menu { DataContext = Keyboard };
            }
            else if (Keyboard is ViewModelKeyboards.Minimised)
            {
                newContent = new CommonViews.Minimised { DataContext = Keyboard };
            }
            else if (Keyboard is ViewModelKeyboards.Mouse)
            {
                newContent = new CommonViews.Mouse { DataContext = Keyboard };
            }
            else if (Keyboard is ViewModelKeyboards.NumericAndSymbols2)
            {
                newContent = new CommonViews.NumericAndSymbols2 { DataContext = Keyboard };
            }
            else if (Keyboard is ViewModelKeyboards.NumericAndSymbols3)
            {
                newContent = new CommonViews.NumericAndSymbols3 { DataContext = Keyboard };
            }
            else if (Keyboard is ViewModelKeyboards.NumericAndSymbols1)
            {
                newContent = new CommonViews.NumericAndSymbols1 { DataContext = Keyboard };
            }
            else if (Keyboard is ViewModelKeyboards.PhysicalKeys)
            {
                newContent = new CommonViews.PhysicalKeys { DataContext = Keyboard };
            }
            else if (Keyboard is ViewModelKeyboards.SizeAndPosition)
            {
                newContent = new CommonViews.SizeAndPosition { DataContext = Keyboard };
            }
            else if (Keyboard is ViewModelKeyboards.YesNoQuestion)
            {
                newContent = new CommonViews.YesNoQuestion { DataContext = Keyboard };
            }

            Content = newContent;
        }

        #endregion
        
        #region Content Change Handler

        private static void ContentChangedHandler(object sender, EventArgs e)
        {
            var keyboardHost = sender as KeyboardHost;
            if (keyboardHost != null)
            {
                keyboardHost.BuildPointToKeyMap();

                if (keyboardHost.InputService != null)
                {
                    keyboardHost.InputService.RequestResume();
                }
            }
        }
        
        #endregion

        #region Build Point To Key Map

        private void BuildPointToKeyMap()
        {
            Log.Debug("Building PointToKeyMap.");

            var contentAsFrameworkElement = Content as FrameworkElement;
            if (contentAsFrameworkElement != null)
            {
                if (contentAsFrameworkElement.IsLoaded)
                {
                    TraverseAllKeysAndBuildPointToKeyValueMap();
                }
                else
                {
                    RoutedEventHandler loaded = null;
                    loaded = (sender, args) =>
                    {
                        TraverseAllKeysAndBuildPointToKeyValueMap();
                        contentAsFrameworkElement.Loaded -= loaded;
                    };
                    contentAsFrameworkElement.Loaded += loaded;
                }
            }
        }

        private void TraverseAllKeysAndBuildPointToKeyValueMap()
        {
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
                        Size = (Size) key.GetTransformToDevice().Transform((Vector) key.RenderSize)
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
