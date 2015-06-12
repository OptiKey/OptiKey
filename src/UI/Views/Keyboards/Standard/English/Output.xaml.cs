using System.Windows;
using System.Windows.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Standard.English
{
    /// <summary>
    /// Interaction logic for Output.xaml
    /// </summary>
    public partial class Output : UserControl
    {
        public Output()
        {
            InitializeComponent();

            Loaded += (sender, args) => NumberOfSuggestionsDisplayed = 4;
        }

        public static readonly DependencyProperty NumberOfSuggestionsDisplayedProperty =
            DependencyProperty.Register("NumberOfSuggestionsDisplayed", typeof (int), typeof (Output), new PropertyMetadata(default(int)));

        public int NumberOfSuggestionsDisplayed
        {
            get { return (int) GetValue(NumberOfSuggestionsDisplayedProperty); }
            set { SetValue(NumberOfSuggestionsDisplayedProperty, value); }
        }
    }
}
