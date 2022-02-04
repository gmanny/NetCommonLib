using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WpfAppCommon.CollectionSegmenting.SubHelpers;

namespace WpfAppCommon.CollectionSegmenting;

/// <summary>
/// Handles individual <see cref="IItemBlock{TInnerItem}"/> for the <see cref="ICollectionManager{TOuterItem, TInnerItem}"/>.
/// </summary>
/// <typeparam name="TOuterItem">type of the outer collection items</typeparam>
/// <typeparam name="TInnerItem">type of the inner collection items</typeparam>
public class ItemBlockHandler<TOuterItem, TInnerItem> : IDisposable
{
    private readonly IItemBlock<TInnerItem> block;
    private readonly ObservableCollection<TOuterItem> outerCollection;
    private readonly IItemBlockParent<TOuterItem, TInnerItem> itemBlockParent;
    private readonly DelegateCollectionSubscriptionHelper<TInnerItem> itemBlockSubscription;
    private readonly DelegateCollectionSubscriptionHelper<TOuterItem> outerCollectionSubscription;
    private readonly Stack<TInnerItem> itemAdditionStack = new();
    private readonly HashSet<TOuterItem> itemSet = new();
    private readonly Func<TInnerItem, TOuterItem> itemConverter;
    private int collectionIndex;

    /// <summary>
    /// Initializes the block handler and inserts the block's contents into the specified index in the collection.
    /// </summary>
    /// <param name="block">block to maintain</param>
    /// <param name="outerCollection">collection into which the block should be placed</param>
    /// <param name="itemBlockParent">block parent</param>
    /// <param name="initialCollectionIndex">index at which block elements should reside initially</param>
    /// <param name="beforeInsertion">operation to run before this block inserts its contents into the collection</param>
    /// <param name="itemConverter">function that converts inner item to outer item</param>
    public ItemBlockHandler(IItemBlock<TInnerItem> block, ObservableCollection<TOuterItem> outerCollection, Func<TInnerItem, TOuterItem> itemConverter, IItemBlockParent<TOuterItem, TInnerItem> itemBlockParent, int initialCollectionIndex, Action<ItemBlockHandler<TOuterItem, TInnerItem>> beforeInsertion)
    {
        // check parameters
        ArgumentNullException.ThrowIfNull(block);
        ArgumentNullException.ThrowIfNull(outerCollection);
        ArgumentNullException.ThrowIfNull(itemConverter);
        ArgumentNullException.ThrowIfNull(itemBlockParent);
        if (initialCollectionIndex < 0 || initialCollectionIndex > outerCollection.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(initialCollectionIndex),
                "Index should be inside the outer collection bounds [0, " + outerCollection.Count + "], but instead was " + initialCollectionIndex);
        }

        // save data
        this.block = block;
        this.outerCollection = outerCollection;
        this.itemBlockParent = itemBlockParent;
        this.itemConverter = itemConverter;
            
        // run the "before insertion" instructions
        beforeInsertion(this);

        // subscribe for outer collection changes
        outerCollectionSubscription = new DelegateCollectionSubscriptionHelper<TOuterItem>(outerCollection, outerCollection, OnItemAdded, OnItemRemoved, OnItemMoved, false);

        // set collection index (before it all additions to the outer collection are ignored)
        collectionIndex = initialCollectionIndex;

        // subscribe for block changes (here all the initial elements are added)
        itemBlockSubscription = new DelegateCollectionSubscriptionHelper<TInnerItem>(block.ItemCollection, block.ItemCollection, OnBlockItemAdded, OnBlockItemRemoved, OnBlockItemMoved);
    }

    private void OnBlockItemAdded(TInnerItem item, int index)
    {
        // insert item into outer collection
        InsertItem(item, collectionIndex + index);
    }

    private void OnBlockItemRemoved(TInnerItem item, int oldindex)
    {
        RemoveItem(item, oldindex == -1 ? -1 : collectionIndex + oldindex);
    }

    private void OnBlockItemMoved(TInnerItem item, int oldindex, int newindex)
    {
        // move the item in outer collection accordingly
        outerCollection.Move(collectionIndex + oldindex, collectionIndex + newindex);
    }

    /// <summary>
    /// Adds item from our block into the collection.
    /// </summary>
    /// <param name="item">item to insert</param>
    /// <param name="index">index in outer collection at which to insert the item</param>
    private void InsertItem(TInnerItem item, int index)
    {
        TOuterItem outerItem = itemConverter(item);

        // remember this item as ours
        itemSet.Add(outerItem);

        // insert it into the outer collection
        using (new InnerItemAddition(this, item))
        {
            outerCollection.Insert(index, outerItem);
        }
    }

    private void RemoveItem(TInnerItem item, int index)
    {
        TOuterItem outerItem = itemConverter(item);

        // remove item from our items set
        if (!itemSet.Remove(outerItem))
        {
            throw new InvalidOperationException("Removing item that wasn't in our set of added items.");
        }

        // if the index of item is unknown
        if (index == -1)
        {
            // try to find it
            for (int i = collectionIndex; i < outerCollection.Count; i++)
            {
                // get item from the collection
                TOuterItem currentItem = outerCollection[i];

                // check if it's our item
                if (!Object.Equals(currentItem, outerItem))
                {
                    continue;
                }

                // remove found item
                outerCollection.RemoveAt(i);

                // end search and proc
                return;
            }

            throw new InvalidOperationException("Couldn't find item to remove from outer collection.");
        }

        // get item at index
        TOuterItem collectionItem = outerCollection[index];

        // check
        if (!Object.Equals(collectionItem, outerItem))
        {
            throw new InvalidOperationException($"Found another item at the specified removal index (block index: {collectionIndex}, removal index: {index})");
        }

        // remove
        outerCollection.RemoveAt(index);
    }

    private void OnItemAdded(TOuterItem item, int index)
    {
        // check
        if (collectionIndex == -1)
        {
            throw new InvalidOperationException("Received item addition before the collection index of this block was set.");
        }

        // change our index, if the item was added before us
        if (index < collectionIndex)
        {
            // item was added above our block
            collectionIndex++;
        }
        else if (index == collectionIndex && !itemSet.Contains(item))
        {
            // new item has index that equals our start, check the Item position
            if (itemBlockParent.IsAbove(item, block))
            {
                collectionIndex++;
            }
        }
    }

    private void OnItemRemoved(TOuterItem item, int oldindex)
    {
        // check that the collection wasn't cleared
        if (oldindex == -1)
        {
            throw new InvalidOperationException("Clearing ItemCollection is not supported.");
        }

        // change our index, if the item was added before us
        if (oldindex < collectionIndex)
        {
            collectionIndex--;
        }
    }

    private void OnItemMoved(TOuterItem item, int oldindex, int newindex)
    {
        // check that the item wasn't moved from above to below us
        if (oldindex < collectionIndex && newindex >= collectionIndex ||
            oldindex >= collectionIndex && newindex < collectionIndex)
        {
            throw new InvalidOperationException($"Moving item from across the item blocks is not allowed. (Old index: {oldindex}, new index: {newindex}, this block's start: {collectionIndex})");
        }
    }

    /// <summary>
    /// Gets the block this info is about.
    /// </summary>
    public IItemBlock<TInnerItem> Block => block;

    /// <summary>
    /// Gets the index of this block's first item in the outer collection.
    /// </summary>
    public int CollectionIndex => collectionIndex;

    /// <summary>
    /// Answers a question whether this item belongs to this block handler's block.
    /// </summary>
    /// <param name="item">item to check</param>
    /// <returns>true, if this item belongs to this item block</returns>
    public bool HasItem(TOuterItem item) => itemSet.Contains(item);

    public void Dispose()
    {
        // end both subscriptions (items are removed automatically)
        outerCollectionSubscription.Dispose();
        itemBlockSubscription.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Answers a question, whether it's our item and we're in the process of adding it.
    /// </summary>
    /// <param name="item">item which addition should be checked</param>
    /// <returns>true, if this item is being added right now, otherwise false</returns>
// ReSharper disable UnusedMember.Local
#pragma warning disable IDE0051 // Remove unused private members
    private bool InsertingThisItem(TOuterItem item)
#pragma warning restore IDE0051 // Remove unused private members
// ReSharper restore UnusedMember.Local
    {
        // shortcut
        if (itemAdditionStack.Count == 0)
        {
            return false;
        }

        // try to find it
        foreach (TInnerItem innerItem in itemAdditionStack)
        {
            // convert to outer item
            TOuterItem outerItem = itemConverter(innerItem);

            if (Object.Equals(outerItem, item))
            {
                return true;
            }
        }

        // nothing was found
        return false;
    }

    private class InnerItemAddition : IDisposable
    {
        private readonly ItemBlockHandler<TOuterItem, TInnerItem> handler;
        private readonly TInnerItem item;

        public InnerItemAddition(ItemBlockHandler<TOuterItem, TInnerItem> handler, TInnerItem item)
        {
            this.handler = handler;
            this.item = item;

            // add item to the stack
            handler.itemAdditionStack.Push(item);
        }

        public void Dispose()
        {
            // pop item from the stack
            TInnerItem removedItem = handler.itemAdditionStack.Pop();

            // check stack consistency
            if (!Object.Equals(removedItem, item))
            {
                throw new InvalidOperationException("Somehow item addition stack was corrupted.");
            }
        }
    }
}