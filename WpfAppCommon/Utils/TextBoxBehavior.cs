using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfAppCommon.Utils
{
    // from https://stackoverflow.com/a/17859206/579817
    public static class TextBoxBehavior
    {
        public static readonly DependencyProperty SelectAllTextOnFocusProperty =
            DependencyProperty.RegisterAttached(
                "SelectAllTextOnFocus",
                typeof (bool),
                typeof (TextBoxBehavior),
                new UIPropertyMetadata(false, OnSelectAllTextOnFocusChanged));

        public static bool GetSelectAllTextOnFocus(TextBox textBox) => (bool) textBox.GetValue(SelectAllTextOnFocusProperty);
        public static void SetSelectAllTextOnFocus(TextBox textBox, bool value) => textBox.SetValue(SelectAllTextOnFocusProperty, value);

        public static readonly DependencyProperty UnselectAllTextOnFocusLeaveProperty =
            DependencyProperty.RegisterAttached(
                "UnselectAllTextOnFocusLeave",
                typeof (bool),
                typeof (TextBoxBehavior),
                new UIPropertyMetadata(false, OnUnselectAllTextOnFocusLeaveChanged));

        public static bool GetUnselectAllTextOnFocusLeave(TextBox textBox) => (bool) textBox.GetValue(UnselectAllTextOnFocusLeaveProperty);
        public static void SetUnselectAllTextOnFocusLeave(TextBox textBox, bool value) => textBox.SetValue(UnselectAllTextOnFocusLeaveProperty, value);

        private static void OnSelectAllTextOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBox textBox))
            {
                return;
            }

            if (!(e.NewValue is bool newVal))
            {
                return;
            }

            if (newVal)
            {
                //textBox.GotFocus += SelectAll;
                textBox.GotKeyboardFocus += SelectAll;
                textBox.PreviewMouseDown += IgnoreMouseButton;
            }
            else
            {
                //textBox.GotFocus -= SelectAll;
                textBox.GotKeyboardFocus -= SelectAll;
                textBox.PreviewMouseDown -= IgnoreMouseButton;
            }
        }

        private static void OnUnselectAllTextOnFocusLeaveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBox textBox))
            {
                return;
            }

            if (!(e.NewValue is bool newVal))
            {
                return;
            }

            if (newVal)
            {
                textBox.LostFocus += UnselectAll;
            }
            else
            {
                textBox.LostFocus -= UnselectAll;
            }
        }

        private static void SelectAll(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is TextBox textBox))
            {
                return;
            }
            
            textBox.SelectAll();
        }

        private static void UnselectAll(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is TextBox textBox))
            {
                return;
            }
            
            textBox.SelectAll();
        }

        private static void IgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is TextBox textBox))
            {
                return;
            }

            if (!textBox.IsReadOnly && textBox.IsKeyboardFocusWithin)
            {
                return;
            }

            e.Handled = true;
            if (textBox.IsKeyboardFocusWithin)
            {
                return;
            }

            textBox.Focus();
        }
    }
}