using System.Windows;
using System.Windows.Controls;

namespace PublishToProGet
{
    /// <summary>
    /// Interaction logic for WizardHeader.xaml
    /// </summary>
    public partial class WizardHeader : UserControl
    {
        public WizardHeader()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(WizardHeader));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
    }
}
