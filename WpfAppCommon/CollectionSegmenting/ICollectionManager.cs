using System;
using System.Collections.ObjectModel;

namespace WpfAppCommon.CollectionSegmenting
{
    /// <summary>
    /// Describes the service which manages an outer collection consisting from items from the several block.
    /// </summary>
    /// <typeparam name="TOuterItem">type of the outer collection items</typeparam>
    /// <typeparam name="TInnerItem">type of the inner collection items</typeparam>
    public interface ICollectionManager<TOuterItem, TInnerItem> : IDisposable
    {
        /// <summary>
        /// Gets an outer collection.
        /// </summary>
        ObservableCollection<TOuterItem> Collection { get; }

        /// <summary>
        /// Adds item to the default block in the manager
        /// </summary>
        /// <param name="item">item to add</param>
        /// <param name="blockType">type of the block to add item to</param>
        void AddItem(TInnerItem item, DefaultCollectionBlock blockType);

        /// <summary>
        /// Removes item which was added to one of the default blocks.
        /// </summary>
        /// <param name="item">item to remove</param>
        void RemoveItem(TInnerItem item);

        /// <summary>
        /// Adds item block to the collection manager.
        /// 
        /// Block is added into the unspecified place between the Front Default Block and Back Default Block.
        /// </summary>
        /// <param name="itemBlock">block to add into the manager</param>
        void AddItemBlock(IItemBlock<TInnerItem> itemBlock);

        /// <summary>
        /// Removes the previously added item block.
        /// </summary>
        /// <param name="itemBlock">block to remove from the manager</param>
        void RemoveItemBlock(IItemBlock<TInnerItem> itemBlock);
    }
}