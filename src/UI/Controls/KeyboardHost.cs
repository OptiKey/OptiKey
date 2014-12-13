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
using JuliusSweetland.ETTA.UI.Utilities;
using JuliusSweetland.ETTA.UI.ViewModels.Keyboards;
using log4net;

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
            Settings.Default.OnPropertyChanges(s => s.Language)
                            .Subscribe(_ => GenerateContent());

            Loaded += OnLoaded;
        }

        #endregion

        #region Properties

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

            RebuildPointToKeyMap();

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
            object newContent = ErrorContent;

            switch (Settings.Default.Language)
            {
                case Languages.AmericanEnglish:
                case Languages.BritishEnglish:
                case Languages.CanadianEnglish:
                    if (Keyboard is Alpha)
                    {
                        newContent = new Views.Keyboards.English.Alpha { DataContext = Keyboard };
                    }
                    else if (Keyboard is NumericAndSymbols1)
                    {
                        newContent = new Views.Keyboards.English.NumericAndSymbols1 { DataContext = Keyboard };
                    }
                    else if (Keyboard is Symbols2)
                    {
                        newContent = new Views.Keyboards.English.Symbols2 { DataContext = Keyboard };
                    }
                    else if (Keyboard is Symbols3)
                    {
                        newContent = new Views.Keyboards.English.Symbols3 { DataContext = Keyboard };
                    }
                    else if (Keyboard is Currencies1)
                    {
                        newContent = new Views.Keyboards.English.Currencies1 { DataContext = Keyboard };
                    }
                    else if (Keyboard is Currencies2)
                    {
                        newContent = new Views.Keyboards.English.Currencies2 { DataContext = Keyboard };
                    }
                    else if (Keyboard is AlternativeAlpha1)
                    {
                        newContent = new Views.Keyboards.English.AlternativeAlpha1 { DataContext = Keyboard };
                    }
                    else if (Keyboard is AlternativeAlpha2)
                    {
                        newContent = new Views.Keyboards.English.AlternativeAlpha2 { DataContext = Keyboard };
                    }
                    else if (Keyboard is AlternativeAlpha3)
                    {
                        newContent = new Views.Keyboards.English.AlternativeAlpha3 { DataContext = Keyboard };
                    }
                    else if (Keyboard is More)
                    {
                        newContent = new Views.Keyboards.English.More { DataContext = Keyboard };
                    }
                    else if (Keyboard is PhysicalKeys)
                    {
                        newContent = new Views.Keyboards.English.PhysicalKeys { DataContext = Keyboard };
                    }
                    else if (Keyboard is YesNoQuestion)
                    {
                        newContent = new Views.Keyboards.English.YesNoQuestion { DataContext = Keyboard };
                    }
                    break;
            }

            var contentAsFrameworkElement = newContent as FrameworkElement;
            if (contentAsFrameworkElement != null)
            {
                if (contentAsFrameworkElement.IsLoaded)
                {
                    RebuildPointToKeyMap();
                }
                else
                {
                    RoutedEventHandler loaded = null;
                    loaded = (sender, args) =>
                    {
                        RebuildPointToKeyMap();
                        contentAsFrameworkElement.Loaded -= loaded;
                    };
                    contentAsFrameworkElement.Loaded += loaded;
                }
            }

            Log.Debug(string.Format("GenerateContent called. Language setting is '{0}' and Keyboard type is '{1}'", 
                Settings.Default.Language, Keyboard != null ? Keyboard.GetType() : null));

            Content = newContent;
        }

        #endregion

        #region Rebuild Point To Key Map

        private void RebuildPointToKeyMap()
        {
            Log.Debug("Rebuilding PointToKeyMap.");

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

                    RebuildPointToKeyMap();
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
                    RebuildPointToKeyMap();
                });
        }

        #endregion
    }
}
