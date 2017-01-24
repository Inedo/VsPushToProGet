using System.Windows;
using System.Windows.Controls;

namespace PushToProGet
{
    public class PlaceholderTextBox : TextBox
    {
        private static readonly DependencyPropertyKey ShouldShowPlaceholderKey = DependencyProperty.RegisterAttachedReadOnly("ShouldShowPlaceholder",
            typeof(bool), typeof(PlaceholderTextBox), new FrameworkPropertyMetadata(true, null, CoerceValue));
        public static readonly DependencyProperty ShouldShowPlaceholderProperty = ShouldShowPlaceholderKey.DependencyProperty;

        public PlaceholderTextBox()
        {
            this.GotFocus += OnFocusChanged;
            this.LostFocus += OnFocusChanged;
            this.TextChanged += OnTextChanged;
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            var textBox = (PlaceholderTextBox)d;
            return ComputeShouldShowPlaceholder(textBox);
        }

        private static bool ComputeShouldShowPlaceholder(TextBox textBox)
        {
            return !textBox.IsFocused && string.IsNullOrEmpty(textBox.Text);
        }

        private static void OnFocusChanged(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            textBox.SetValue(ShouldShowPlaceholderKey, ComputeShouldShowPlaceholder(textBox));
        }

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            textBox.SetValue(ShouldShowPlaceholderKey, ComputeShouldShowPlaceholder(textBox));
        }

        public bool ShouldShowPlaceholder
        {
            get
            {
                return (bool)this.GetValue(ShouldShowPlaceholderProperty);
            }
        }
    }
}
