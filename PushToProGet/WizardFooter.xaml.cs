using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PushToProGet
{
    /// <summary>
    /// Interaction logic for WizardFooter.xaml
    /// </summary>
    public partial class WizardFooter : UserControl
    {
        public WizardFooter()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(WizardFooter));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty PrimaryButtonLabelProperty = DependencyProperty.Register(nameof(PrimaryButtonLabel), typeof(string), typeof(WizardFooter));

        public string PrimaryButtonLabel
        {
            get { return (string)GetValue(PrimaryButtonLabelProperty); }
            set { SetValue(PrimaryButtonLabelProperty, value); }
        }

        public static readonly DependencyProperty SecondaryButtonLabelProperty = DependencyProperty.Register(nameof(SecondaryButtonLabel), typeof(string), typeof(WizardFooter));

        public string SecondaryButtonLabel
        {
            get { return (string)GetValue(SecondaryButtonLabelProperty); }
            set { SetValue(SecondaryButtonLabelProperty, value); }
        }

        public event RoutedEventHandler PrimaryButtonClicked;

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            PrimaryButtonClicked?.Invoke(sender, e);
        }

        public event RoutedEventHandler SecondaryButtonClicked;

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            SecondaryButtonClicked?.Invoke(sender, e);
        }
    }
}
