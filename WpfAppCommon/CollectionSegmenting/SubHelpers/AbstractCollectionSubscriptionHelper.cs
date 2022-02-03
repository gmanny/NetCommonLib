using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace WpfAppCommon.CollectionSegmenting.SubHelpers;

/// <summary>
/// base implementation for the collection subscription helper
/// </summary>
/// <typeparam name="TItem">collection item type</typeparam>
public abstract class AbstractCollectionSubscriptionHelper<TItem> : IDisposable
{
    private readonly bool collectionOwnerMode;
    private readonly IList<TItem> collection;
    private readonly INotifyCollectionChanged notifier;
    private readonly ObservableCollection<TItem> savedItems = new();

    private bool isDisposed;

    protected AbstractCollectionSubscriptionHelper([NotNull] IList<TItem> collection, [NotNull] INotifyCollectionChanged notifier, bool collectionOwnerMode = true)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(notifier);

        // set fields
        this.collectionOwnerMode = collectionOwnerMode;
        this.collection = collection;
        this.notifier = notifier;
    }

    /// <summary>
    /// Must be called after your constructor has initialized the class.
    /// </summary>
    protected void Start()
    {
        AddAllItems(true);

        // subscribe for the collection updates
        notifier.CollectionChanged += OnNotifierCollectionChanged;
    }

    private void AddAllItems(bool initial)
    {
        // process current items
        for (int i = 0; i < collection.Count; i++)
        {
            // get an item
            TItem item = collection[i];

            // process it
            AddItem(item, i, initial);
        }
    }

    private void AddItem(TItem item, int index, bool initial)
    {
        // check consistency
        if (index < 0 || index > savedItems.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"{GetType().Name} has become inconsistent: tried to insert an item at the index {index} when the cached collection length was {savedItems.Count}");
        }

        // save the item
        savedItems.Insert(index, item);

        if (!initial || collectionOwnerMode)
        {
            // notify about item addition
            OnItemAdded(item, index);
        }
    }

    private void MoveItem(TItem item, int oldIndex, int newIndex)
    {
        // check indicies
        if (oldIndex < 0 || oldIndex >= savedItems.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(oldIndex), $"{GetType().Name} has become inconsistent: tried to move an item from an index {oldIndex} while the length of the cached collection was {savedItems.Count}");
        }
        if (newIndex < 0 || newIndex >= savedItems.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(newIndex), $"{GetType().Name} has become inconsistent: tried to move an item to an index {newIndex} while the length of the cached collection was {savedItems.Count}");
        }

        // change the item's position in cache
        savedItems.Move(oldIndex, newIndex);

        // signal the move
        OnItemMoved(item, oldIndex, newIndex);
    }

    private void RemoveItem(TItem item, int index)
    {
        // check consistency
        if (index < 0 || index >= savedItems.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"{GetType().Name} has become inconsistent: tried to remove an item at the index {index} when the cached collection length was {savedItems.Count}");
        }

        // remove item from the cache
        savedItems.RemoveAt(index);

        // signal remove
        OnItemRemoved(item, index);
    }

    private void RemoveAllItems()
    {
        // walk all items
        for (int i = 0; i < savedItems.Count; i++)
        {
            // get an item
            TItem item = savedItems[i];

            // notify
            OnItemRemoved(item, -1);
        }

        // clear the cache
        savedItems.Clear();
    }

    /// <summary>
    /// This method processes item addition.
    /// </summary>
    /// <param name="item">item that was added</param>
    /// <param name="index">index at which the item now resides</param>
    protected abstract void OnItemAdded(TItem item, int index);

    /// <summary>
    /// This method processes item removal.
    /// </summary>
    /// <param name="item">item that was removed</param>
    /// <param name="oldIndex">index which item had in collection before removal, or -1 if the index is unknown</param>
    protected abstract void OnItemRemoved(TItem item, int oldIndex);

    /// <summary>
    /// This method processes item move from one position to another.
    /// </summary>
    /// <param name="item">item that was moved</param>
    /// <param name="oldIndex">old item index</param>
    /// <param name="newIndex">new item index</param>
    protected abstract void OnItemMoved(TItem item, int oldIndex, int newIndex);

    private void OnNotifierCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        bool addRemove = false;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                // ReSharper disable once PossibleNullReferenceException
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    // get a new item
                    object itemObject = e.NewItems[i];

                    // try to cast it
                    TItem item = (TItem) itemObject;

                    // signal item addition
                    AddItem(item, e.NewStartingIndex + i, false);
                }
            } break;

            case NotifyCollectionChangedAction.Move:
            {
                // ReSharper disable once PossibleNullReferenceException
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    // get moved item
                    object itemObject = e.NewItems[i];

                    // try to cast it
                    TItem item = (TItem) itemObject;

                    // signal item move
                    MoveItem(item, e.OldStartingIndex + i, e.NewStartingIndex + i);
                }
            } break;

            case NotifyCollectionChangedAction.Remove:
            {
                // ReSharper disable once PossibleNullReferenceException
                for (int i = 0; i < e.OldItems.Count; i++)
                {
                    // get removed item
                    object itemObject = e.OldItems[i];

                    // try to cast it
                    TItem item = (TItem) itemObject;

                    // signal item removal
                    RemoveItem(item, e.OldStartingIndex + i);
                }
            }
                    
            if (addRemove)
            {
                goto case NotifyCollectionChangedAction.Add;
            }
                
            break;

            case NotifyCollectionChangedAction.Replace:
            {
                // just remove old items and then add new ones
                addRemove = true;

                goto case NotifyCollectionChangedAction.Remove;
            }

            case NotifyCollectionChangedAction.Reset:
            {
                // first, clear the collection
                RemoveAllItems();

                // add what's in the new one
                AddAllItems(false);
            } break;

            default: throw new ArgumentOutOfRangeException($"Unknown collection change action: {e.Action}");
        }
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException(collection.GetType().Name, "Already disposed");
        }

        isDisposed = true;

        // unsubscribe from the collection
        notifier.CollectionChanged -= OnNotifierCollectionChanged;

        if (collectionOwnerMode)
        {
            // process all children
            RemoveAllItems();
        }

        GC.SuppressFinalize(this);
    }
}