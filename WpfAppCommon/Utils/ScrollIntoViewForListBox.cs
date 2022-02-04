using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

// from https://stackoverflow.com/a/8830961/579817
namespace WpfAppCommon.Utils;

[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
public class ScrollIntoViewForListBox : Behavior<ListBox>
{
    /// <summary>
    /// When Behavior is attached
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
    }

    /// <summary>
    /// On Selection Changed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem != null)
        {
            listBox.Dispatcher.BeginInvoke(() =>
            {
                listBox.UpdateLayout();
                if (listBox.SelectedItem != null)
                {
                    listBox.ScrollIntoView(listBox.SelectedItem);
                }
            });
        }
    }
    /// <summary>
    /// When behavior is detached
    /// </summary>
    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
    }
}