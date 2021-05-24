namespace WpfAppCommon.CollectionSegmenting
{
    /// <summary>
    /// Describes the item block parent collection (normally, manager).
    /// </summary>
    /// <typeparam name="TOuterItem">type of the outer collection items</typeparam>
    /// <typeparam name="TInnerItem">type of the inner collection items</typeparam>
    public interface IItemBlockParent<in TOuterItem, TInnerItem>
    {
        /// <summary>
        /// Answers the question whether the given item is above the given item block in the outer collection.
        /// </summary>
        /// <param name="outerItem">item that position must be checked</param>
        /// <param name="itemBlock">item block which relative position must be checked</param>
        /// <returns>true, if the given item resides in a block above the <paramref name="itemBlock"/>, otherwise false</returns>
        bool IsAbove(TOuterItem outerItem, IItemBlock<TInnerItem> itemBlock);
    }
}