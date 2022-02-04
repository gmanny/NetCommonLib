using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WpfAppCommon.CollectionSegmenting;

public class CollectionManager<TOuterItem, TInnerItem> : ICollectionManager<TOuterItem, TInnerItem>, IItemBlockParent<TOuterItem, TInnerItem>, IItemBlock<TOuterItem> where TInnerItem : notnull
{
    private readonly ObservableCollection<TOuterItem> collection;
    private readonly InternalItemBlock frontItemBlock = new();
    private readonly InternalItemBlock backItemBlock = new();
    private readonly ObservableCollection<ItemBlockHandler<TOuterItem, TInnerItem>> blockHandlers = new();
    private readonly Dictionary<IItemBlock<TInnerItem>, ItemBlockHandler<TOuterItem, TInnerItem>> blockHandlerHash = new();
    private readonly Dictionary<TInnerItem, DefaultCollectionBlock> itemBlockTypeHash = new();
    private readonly Func<TInnerItem, TOuterItem> itemConverter;

    public CollectionManager(Func<TInnerItem, TOuterItem> itemConverter)
        : this(itemConverter, new ObservableCollection<TOuterItem>())
    { }

    /// <summary>
    /// Creates collection manager with the overriden outer collection.
    /// </summary>
    /// <param name="itemConverter">function that's used to conver inner item to outer item</param>
    /// <param name="collection">outer collection that this manager should manage, this collection should be empty</param>
    public CollectionManager(Func<TInnerItem, TOuterItem> itemConverter, ObservableCollection<TOuterItem> collection)
    {
        ArgumentNullException.ThrowIfNull(itemConverter);
        ArgumentNullException.ThrowIfNull(collection);
        if (collection.Count != 0)
        {
            throw new ArgumentException("Supplied outer collection should be empty", nameof(collection));
        }

        this.itemConverter = itemConverter;
        this.collection = collection;

        // insert two default blocks
        InsertItemBlock(0, frontItemBlock);
        InsertItemBlock(1, backItemBlock);
    }

    private class InternalItemBlock : IItemBlock<TInnerItem>
    {
        private readonly ObservableCollection<TInnerItem> itemCollection = new();

        public ObservableCollection<TInnerItem> ItemCollection => itemCollection;

        public void AddItem(TInnerItem item) => itemCollection.Add(item);

        public void RemoveItem(TInnerItem item) => itemCollection.Remove(item);
    }

    public ObservableCollection<TOuterItem> Collection => collection;

    public ObservableCollection<TOuterItem> ItemCollection => collection;

    public bool IsAbove(TOuterItem outerItem, IItemBlock<TInnerItem> itemBlock)
    {
        ArgumentNullException.ThrowIfNull(outerItem);
        ArgumentNullException.ThrowIfNull(itemBlock);

        // walk blocks from top to bottom
        for (int i = 0; i < blockHandlers.Count; i++)
        {
            // get current block
            ItemBlockHandler<TOuterItem, TInnerItem> block = blockHandlers[i];

            // check that it's not our itemBlock
            if (itemBlock == block.Block)
            {
                // we've reached our itemBlock, nothing was found above it
                return false;
            }

            // check that it has our item
            if (!block.HasItem(outerItem))
            {
                continue;
            }

            return true;
        }

        throw new InvalidOperationException("Didn't find the item block which has specified item.");
    }

    /// <summary>
    /// Inserts item block into the specific position.
    /// </summary>
    /// <param name="index">position at which to insert the block</param>
    /// <param name="block">block to insert</param>
    private void InsertItemBlock(int index, IItemBlock<TInnerItem> block)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (index < 0 || index > blockHandlers.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"Index should be in the bounds of block handler collection (collection cound: {blockHandlers.Count}, index: {index})");
        }

        // calculate initial collection index
        int initialCollectionIndex;
        if (index == blockHandlers.Count)
        {
            initialCollectionIndex = collection.Count;
        }
        else
        {
            initialCollectionIndex = blockHandlers[index].CollectionIndex;
        }

        // wrap the item block into the block handler, the addition happens automatically
        ItemBlockHandler<TOuterItem, TInnerItem> blockHandler = new(block, collection, itemConverter, this, initialCollectionIndex, helper => blockHandlers.Insert(index, helper));

        // remember block handler for the block
        blockHandlerHash.Add(block, blockHandler);
    }

    public void AddItemBlock(IItemBlock<TInnerItem> itemBlock)
    {
        InsertItemBlock(blockHandlers.Count - 1, itemBlock);
    }

    public void RemoveItemBlock(IItemBlock<TInnerItem> itemBlock)
    {
        // get block handler
        if (!blockHandlerHash.TryGetValue(itemBlock, out ItemBlockHandler<TOuterItem, TInnerItem>? blockHandler))
        {
            throw new InvalidOperationException("Didn't find the block handler for the block to be removed. (Maybe, there was no such block in this manager.)");
        }

        // dispose of the block handler (it'll remove every item from the collection)
        blockHandler.Dispose();

        // remove block from our collections
        blockHandlers.Remove(blockHandler);
        blockHandlerHash.Remove(itemBlock);
    }

    public void AddItem(TInnerItem item, DefaultCollectionBlock blockType)
    {
        // remember item block type
        itemBlockTypeHash.Add(item, blockType);

        // add item to one of the blocks
        switch (blockType)
        {
            case DefaultCollectionBlock.Front:
                frontItemBlock.AddItem(item);
                break;

            case DefaultCollectionBlock.Back:
                backItemBlock.AddItem(item);
                break;
        }
    }

    public void RemoveItem(TInnerItem item)
    {
        // get item block type
        if (!itemBlockTypeHash.TryGetValue(item, out DefaultCollectionBlock blockType))
        {
            throw new InvalidOperationException("The specified item wasn't found in any of the blocks in current manager.");
        }

        // remove item from the appropriate block
        switch (blockType)
        {
            case DefaultCollectionBlock.Front:
                frontItemBlock.RemoveItem(item);
                break;

            case DefaultCollectionBlock.Back:
                backItemBlock.RemoveItem(item);
                break;
        }

        // forget the block type for this item
        itemBlockTypeHash.Remove(item);
    }

    public void Dispose()
    {
        // dispose of all the block handlers
        while (blockHandlers.Count > 0)
        {
            // get last block handler
            ItemBlockHandler<TOuterItem, TInnerItem> blockHandler = blockHandlers[^1];

            // dispose of it
            blockHandler.Dispose();

            // remove it from the handler collection
            blockHandlers.RemoveAt(blockHandlers.Count - 1);
        }

        // clear other stuff
        blockHandlerHash.Clear();
        itemBlockTypeHash.Clear();

        GC.SuppressFinalize(this);
    }
}