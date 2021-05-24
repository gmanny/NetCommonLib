namespace WpfAppCommon.CollectionSegmenting.SubHelpers
{
    public delegate void ItemMoveHandler<in TItem>(TItem item, int oldIndex, int newIndex);
}