using System.Collections.ObjectModel;

namespace WpfAppCommon.CollectionSegmenting;

public class SimpleItemBlock<TItem> : IItemBlock<TItem>
{
    private readonly ObservableCollection<TItem> itemCollection = new();

    public SimpleItemBlock() { }

    public SimpleItemBlock(ObservableCollection<TItem> itemCollection)
    {
        this.itemCollection = itemCollection;
    }

    public ObservableCollection<TItem> ItemCollection => itemCollection;
}