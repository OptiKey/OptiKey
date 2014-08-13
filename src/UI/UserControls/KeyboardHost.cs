using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.UI.Utilities;
using log4net;

namespace JuliusSweetland.ETTA.UI.UserControls
{
    public class KeyboardHost : ContentControl
    {
        #region Private member vars

        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Ctor

        public KeyboardHost()
        {
            DependencyPropertyDescriptor.FromProperty(ContentProperty, typeof(KeyboardHost)).AddValueChanged(this, ContentChanged);
            
            Loaded += OnLoaded;
        }

        #endregion

        #region Properties
        
        public static readonly DependencyProperty PointToKeyValueMapProperty =
            DependencyProperty.Register("PointToKeyValueMap", typeof(Dictionary<Rect, KeyValue>),
                typeof(KeyboardHost), new PropertyMetadata(default(Dictionary<Rect, KeyValue>)));

        public Dictionary<Rect, KeyValue> PointToKeyValueMap
        {
            get { return (Dictionary<Rect, KeyValue>)GetValue(PointToKeyValueMapProperty); }
            set { SetValue(PointToKeyValueMapProperty, value); }
        }

        #endregion

        #region OnLoaded - build key map

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Log.Debug("Rebuilding Point To Key Value Map");

            BuildPointToKeyMap();

            BuildOfPointToKeyValueMapOnSizeChanged();

            var parentWindow = Window.GetWindow(this);

            if (parentWindow == null)
            {
                throw new ApplicationException("Parent Window could not be identified. Unable to continue");
            }

            BuildPointToKeyValueMapOnParentWindowMove(parentWindow);
        }

        #endregion

        #region Content Changed

        private void ContentChanged(object sender, EventArgs e)
        {
            var keyboardHost = sender as KeyboardHost;
            if (keyboardHost != null)
            {
                keyboardHost.BuildPointToKeyMap();
            }
        }

        #endregion

        #region Build Point To Key Map

        private void BuildPointToKeyMap()
        {
            var allBorders = VisualAndLogicalTreeHelper.FindVisualChildren<Border>(this).ToList();

            var pointToKeyValueMap = new Dictionary<Rect, KeyValue>();

            var topLeftPoint = new Point(0, 0);

            foreach (var border in allBorders)
            {
                if (border.Tag is KeyValue)
                {
                    var keyValue = (KeyValue)border.Tag;
                    var rect = new Rect
                    {
                        Location = border.PointToScreen(topLeftPoint),
                        Size = border.RenderSize
                    };

                    pointToKeyValueMap.Add(rect, keyValue);
                }
            }

            PointToKeyValueMap = pointToKeyValueMap;
        }

        #endregion
        
        #region Size Changed

        private void BuildOfPointToKeyValueMapOnSizeChanged()
        {
            Observable.FromEventPattern<SizeChangedEventHandler, SizeChangedEventArgs>
                (h => this.SizeChanged += h,
                h => this.SizeChanged -= h)
                .Throttle(TimeSpan.FromSeconds(0.1))
                .ObserveOnDispatcher()
                .Subscribe(_ =>
                {
                    Log.Debug("SizeChanged event detected - Point To Key Value Map");
                    BuildPointToKeyMap();
                });
        }

        #endregion

        #region Window Move

        private void BuildPointToKeyValueMapOnParentWindowMove(Window parentWindow)
        {
            //This event will also fire if the window is mimised, restored, or maximised, so no need to monitor StateChanged
            Observable.FromEventPattern<EventHandler, EventArgs>
                (h => parentWindow.LocationChanged += h,
                h => parentWindow.LocationChanged -= h)
                .Throttle(TimeSpan.FromSeconds(0.1))
                .ObserveOnDispatcher()
                .Subscribe(_ =>
                {
                    Log.Debug("Window's LocationChanged event detected - Rebuilding Point To Key Value Map");
                    BuildPointToKeyMap();
                });
        }

        #endregion
    }
}
