using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xaml;

namespace JuliusSweetland.ETTA.UI.Controls
{
    public class TextBlockScaler : ContentControl
    {
        private TextBlock textBlock;

        public static readonly DependencyProperty MinFontSizeProperty =
            DependencyProperty.Register("MinFontSize", typeof (double?), typeof (TextBlockScaler), new PropertyMetadata(default(double?)));

        public double? MinFontSize
        {
            get { return (double?) GetValue(MinFontSizeProperty); }
            set { SetValue(MinFontSizeProperty, value); }
        }

        public static readonly DependencyProperty MaxFontSizeProperty =
            DependencyProperty.Register("MaxFontSize", typeof (double?), typeof (TextBlockScaler), new PropertyMetadata(default(double?)));

        public double? MaxFontSize
        {
            get { return (double?) GetValue(MaxFontSizeProperty); }
            set { SetValue(MaxFontSizeProperty, value); }
        }

        public static readonly DependencyProperty ResizeThrottleInMsProperty =
            DependencyProperty.Register("ResizeThrottleInMs", typeof (double), typeof (TextBlockScaler), new PropertyMetadata(default(double)));

        public double ResizeThrottleInMs
        {
            get { return (double) GetValue(ResizeThrottleInMsProperty); }
            set { SetValue(ResizeThrottleInMsProperty, value); }
        }

        public TextBlockScaler()
        {
            if (IsLoaded)
            {
                AttachAllHandlers();
            }
            else
            {
                RoutedEventHandler loaded = null;
                loaded = (_, __) =>
                    {
                        AttachAllHandlers();
                        Loaded -= loaded; //Ensure this is only run once
                    };
                Loaded += loaded;
            }
        }

        private void AttachAllHandlers()
        {
            AttachTextBlockScalerHandlers();
            AttachTextBlockHandlers();
        }

        private void AttachTextBlockScalerHandlers()
        {
            Observable.FromEventPattern<SizeChangedEventHandler, SizeChangedEventArgs>
                (h => SizeChanged += h, h => SizeChanged -= h)
                .Throttle(TimeSpan.FromMilliseconds(ResizeThrottleInMs))
                .SubscribeOnDispatcher()
                .ObserveOnDispatcher()
                .Subscribe(_ => CalculateTextBlockFontSize());
        }

        private void AttachTextBlockHandlers()
        {
            var children = LogicalTreeHelper.GetChildren(this);
            textBlock = children.OfType<TextBlock>().FirstOrDefault();

            if(textBlock == null) throw new XamlException("TextBlockScaler cannot find a TextBlock in its collection of child elements");

            if (textBlock.IsLoaded) //Loaded event is fired from the root down; we may be here before the child TextBlock has loaded, so check
            {
                SetupTextBlockHandlers();
                CalculateTextBlockFontSize();
            }
            else
            {
                RoutedEventHandler textBlockLoaded = null;
                textBlockLoaded = (_, __) =>
                    {
                        SetupTextBlockHandlers();
                        CalculateTextBlockFontSize();
                        textBlock.Loaded -= textBlockLoaded;
                    };
                textBlock.Loaded += textBlockLoaded;
            }
        }

        private void SetupTextBlockHandlers()
        {
            DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock))
                .AddValueChanged(textBlock, (_, __) => CalculateTextBlockFontSize());

            DependencyPropertyDescriptor.FromProperty(TextBlock.FontFamilyProperty, typeof(TextBlock))
                .AddValueChanged(textBlock, (_, __) => CalculateTextBlockFontSize());

            DependencyPropertyDescriptor.FromProperty(TextBlock.FontStyleProperty, typeof(TextBlock))
                .AddValueChanged(textBlock, (_, __) => CalculateTextBlockFontSize());

            DependencyPropertyDescriptor.FromProperty(TextBlock.FontWeightProperty, typeof(TextBlock))
                .AddValueChanged(textBlock, (_, __) => CalculateTextBlockFontSize());

            DependencyPropertyDescriptor.FromProperty(TextBlock.FontStretchProperty, typeof(TextBlock))
                .AddValueChanged(textBlock, (_, __) => CalculateTextBlockFontSize());

            DependencyPropertyDescriptor.FromProperty(FlowDirectionProperty, typeof(TextBlock))
                .AddValueChanged(textBlock, (_, __) => CalculateTextBlockFontSize());
        }

        private void CalculateTextBlockFontSize()
        {
            double fontSize = textBlock.FontSize;

            //Enlarge to fit
            while (!IsTextTrimmedAtThisFontSize(fontSize + 1)
                && (MaxFontSize == null || (fontSize + 1) <= MaxFontSize.Value))
            {
                fontSize++;
            }

            //Shrink to fit
            while (IsTextTrimmedAtThisFontSize(fontSize)
                   && (MinFontSize == null || fontSize > MinFontSize.Value))
            {
                fontSize--;
            }

            //Coalesce font size
            if (MaxFontSize != null
                && fontSize > MaxFontSize.Value)
            {
                fontSize = MaxFontSize.Value;
            }

            if (MinFontSize != null
                && fontSize < MinFontSize.Value)
            {
                fontSize = MinFontSize.Value;
            }

            textBlock.FontSize = fontSize;
        }

        private bool IsTextTrimmedAtThisFontSize(double fontSize)
        {
            var typeface = new Typeface(
                textBlock.FontFamily,
                textBlock.FontStyle,
                textBlock.FontWeight,
                textBlock.FontStretch);

            var formattedText = new FormattedText(
                textBlock.Text,
                Thread.CurrentThread.CurrentCulture,
                textBlock.FlowDirection,
                typeface,
                fontSize,
                new SolidColorBrush(Colors.Black))
            {
                MaxTextWidth = ActualWidth, //Limit the max width to the current width of this TextBlockScaler to produce wrapping behaviour
                Trimming = TextTrimming.None
            };

            return formattedText.Height > ActualHeight; //If the formatted text needs more height than we are providing then it would be trimmed
        }
    }
}
