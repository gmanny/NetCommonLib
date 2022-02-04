using System.Windows;

namespace WpfAppCommon.Prompt;

public class InputBox
{
    public static string Show(string prompt = "", string title = "", MessageBoxImage icon = MessageBoxImage.Information, string defaultResponse = "", Window? owner = null)
    {
        Prompt w = new()
        {
            type = MessageBoxType.InputBox,
            prompt = prompt,
            title = title,
            button = MessageBoxButton.OKCancel,
            icon = icon,
            defaultResponse = defaultResponse
        };
        if (owner != null)
        {
            w.Owner = owner;
            w.Icon = owner.Icon;
        }

        bool? dialogResult = w.ShowDialog();
            
        if (dialogResult is true && w.messageboxResult == MessageBoxResult.OK)
        {
            return w.tbInput.Text;
        }

        return "";
    }
}