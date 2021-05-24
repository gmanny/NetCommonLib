using System.Collections.ObjectModel;

namespace WpfAppCommon.CollectionSegmenting
{
    public class SimpleItemBlock<TItem> : IItemBlock<TItem>
    {
        private readonly ObservableCollection<TItem> itemCollection = new ObservableCollection<TItem>();

        public SimpleItemBlock() { }

        public SimpleItemBlock(ObservableCollection<TItem> itemCollection)
        {
            this.itemCollection = itemCollection;
        }

        public ObservableCollection<TItem> ItemCollection => itemCollection;
    }
}