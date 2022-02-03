using System.Windows;
using System.Windows.Controls;

namespace WpfAppCommon.Utils;

public class ResourceItemContainerTemplateSelector : ItemContainerTemplateSelector
{
    public string TemplateKey { get; set; }

    public override DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl)
    {
        return (DataTemplate) parentItemsControl.FindResource(TemplateKey);
    }
}