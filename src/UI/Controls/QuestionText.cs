using System.Windows;
using System.Windows.Controls;

namespace JuliusSweetland.ETTA.UI.Controls
{
    public class QuestionText : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(QuestionText), new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
    }
}
