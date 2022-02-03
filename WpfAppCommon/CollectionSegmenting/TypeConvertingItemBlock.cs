using System.Collections.ObjectModel;

namespace WpfAppCommon.CollectionSegmenting;

public class TypeConvertingItemBlock<TInnerItem, TOuterItem> : IItemBlock<TOuterItem>
    where TInnerItem : TOuterItem
{
    private readonly CollectionManager<TOuterItem, TInnerItem> mgr = new(i => i);

    public TypeConvertingItemBlock(ObservableCollection<TInnerItem> itemCollection)
    {
        mgr.AddItemBlock(new SimpleItemBlock<TInnerItem>(itemCollection));
    }

    public ObservableCollection<TOuterItem> ItemCollection => mgr.Collection;
}