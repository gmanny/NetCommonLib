using System.Windows;
using System.Windows.Controls;

namespace WpfAppCommon.Utils
{
    // use together with ItemTemplate="{x:Null}" on the ItemsControl
    public class SimpleItemContainerTemplateSelector : ItemContainerTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl)
        {
            object res = parentItemsControl.TryFindResource(new ItemContainerTemplateKey(item.GetType())) ??
                         parentItemsControl.TryFindResource(new DataTemplateKey(item.GetType())) ??
                         parentItemsControl.TryFindResource(item.GetType());

            return (DataTemplate) res;
        }
    }
}