using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace WpfAppCommon.Utils;

public class ResourceItemContainerTemplateSelector : ItemContainerTemplateSelector
{
    public string? TemplateKey { get; set; }

    public override DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl)
    {
        if (TemplateKey == null)
        {
            throw new NoNullAllowedException($"{nameof(TemplateKey)} can not be empty");
        }

        return (DataTemplate) parentItemsControl.FindResource(TemplateKey);
    }
}