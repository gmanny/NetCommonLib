namespace WpfAppCommon.CollectionSegmenting.SubHelpers
{
    public delegate void ItemRemovalHandler<in TItem>(TItem item, int oldIndex);
}