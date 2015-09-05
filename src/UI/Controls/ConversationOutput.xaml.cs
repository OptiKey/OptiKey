using System.Windows;
using System.Windows.Controls;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    /// <summary>
    /// Interaction logic for ConversationOutput.xaml
    /// </summary>
    public partial class ConversationOutput : UserControl
    {
        public ConversationOutput()
        {
            InitializeComponent();

            Loaded += (sender, args) => NumberOfSuggestionsDisplayed = 4;
        }

        public static readonly DependencyProperty NumberOfSuggestionsDisplayedProperty =
            DependencyProperty.Register("NumberOfSuggestionsDisplayed", typeof(int), typeof(ConversationOutput), new PropertyMetadata(default(int)));

        public int NumberOfSuggestionsDisplayed
        {
            get { return (int) GetValue(NumberOfSuggestionsDisplayedProperty); }
            set { SetValue(NumberOfSuggestionsDisplayedProperty, value); }
        }
    }
}
