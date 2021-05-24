using System.Windows;

namespace WpfAppCommon.Prompt
{
    public class InputBox
    {
        public static string Show(string prompt = "", string title = "", MessageBoxImage icon = MessageBoxImage.Information, string defaultResponse = "", Window owner = null)
        {
            Prompt w = new Prompt();
            if (owner != null)
            {
                w.Owner = owner; 
                w.Icon = owner.Icon;
            }
            w.type = MessageBoxType.InputBox;
            w.prompt = prompt;
            w.title = title;
            w.button = MessageBoxButton.OKCancel;
            w.icon = icon;
            w.defaultResponse = defaultResponse;

            bool? dialogResult = w.ShowDialog();
            
            if (dialogResult == true)
            {
                if (w.messageboxResult == MessageBoxResult.OK)
                {
                    return w.tbInput.Text;
                }
            }

            return "";
        }
    }
}
