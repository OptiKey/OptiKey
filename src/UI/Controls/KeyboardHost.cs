using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.UI.Utilities;
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

            RebuildPointToKeyValueMapOnSizeChanged();

            var parentWindow = Window.GetWindow(this);

            if (parentWindow == null)
            {
                throw new ApplicationException("Parent Window could not be identified. Unable to continue");
            }

            RebuildPointToKeyValueMapOnParentWindowMove(parentWindow);

            Loaded -= OnLoaded; //Ensure this logic only runs once
        }

        #endregion

        #region Content Changed

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            Debug.Print("*** KeyboardHost: ContentChanged");
            base.OnContentChanged(oldContent, newContent);
        }

        protected override void OnContentTemplateChanged(DataTemplate oldContentTemplate, DataTemplate newContentTemplate)
        {
            Debug.Print("*** KeyboardHost: OnContentTemplateChanged");
            base.OnContentTemplateChanged(oldContentTemplate, newContentTemplate);
        }

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            Debug.Print("*** KeyboardHost: OnTemplateChanged");
            base.OnTemplateChanged(oldTemplate, newTemplate);
        }

        #endregion

        #region Build Point To Key Map

        private void BuildPointToKeyMap()
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
                        Size = (Size)key.GetTransformToDevice().Transform((Vector)key.RenderSize)
                    };

                    pointToKeyValueMap.Add(rect, key.Value);
                }
            }

            PointToKeyValueMap = pointToKeyValueMap;
        }

        #endregion
        
        #region Size Changed

        private void RebuildPointToKeyValueMapOnSizeChanged()
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

        private void RebuildPointToKeyValueMapOnParentWindowMove(Window parentWindow)
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
