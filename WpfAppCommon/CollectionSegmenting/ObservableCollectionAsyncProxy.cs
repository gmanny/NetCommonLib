using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Threading;
using WpfAppCommon.CollectionSegmenting.SubHelpers;

namespace WpfAppCommon.CollectionSegmenting;

/// <summary>
/// Push changes in a wrapped collection to the WPF Dispatcher's thread by exposing <see cref="SyncedCollection"/> property
/// that is changed synchronously with the wrapped one
/// </summary>
public class ObservableCollectionAsyncProxy<TItem> : IDisposable
{
    private readonly ObservableCollection<TItem> frontCollection;
    private readonly Dispatcher dispatcher;
    private readonly DelegateCollectionSubscriptionHelper<TItem> collectionSubscription;

    public ObservableCollectionAsyncProxy([NotNull] ObservableCollection<TItem> backCollection,
                                          [NotNull] Dispatcher dispatcher)
        : this(backCollection, backCollection, dispatcher, new ObservableCollection<TItem>())
    { }

    public ObservableCollectionAsyncProxy([NotNull] IList<TItem> collection,
                                          [NotNull] INotifyCollectionChanged notifier,
                                          [NotNull] Dispatcher dispatcher)
        : this(collection, notifier, dispatcher, new ObservableCollection<TItem>())
    { }

    protected ObservableCollectionAsyncProxy([NotNull] IList<TItem> collection,
                                             [NotNull] INotifyCollectionChanged notifier,
                                             [NotNull] Dispatcher dispatcher,
                                             [NotNull] ObservableCollection<TItem> frontCollection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(frontCollection);

        this.dispatcher = dispatcher;
        this.frontCollection = frontCollection;

        // check front collection override
        if (frontCollection.Count > 0)
        {
            throw new InvalidOperationException("Overriden front collection should be empty (length of a collection was " + frontCollection.Count + ")");
        }

        // add initial collection contents to the outer collection
        foreach (TItem item in collection)
        {
            this.frontCollection.Add(item);
        }

        // subscribe for the collection, reminder: OnAnnotationAdded is called here for every item that's initially in the collection
        collectionSubscription = new DelegateCollectionSubscriptionHelper<TItem>(collection, notifier, OnItemAdded, OnItemRemoved, OnItemMoved, false);
    }

    /// <summary>
    /// Gets the collection that is guaranteed to be updated only on the Dispatcher's thread.
    /// </summary>
    public ObservableCollection<TItem> SyncedCollection => frontCollection;

    private void DoWork(Action work)
    {
        dispatcher.Invoke(work);
    }

    private void OnItemAdded(TItem item, int index)
    {
        DoWork(() => frontCollection.Insert(index, item));
    }

    private void OnItemRemoved(TItem item, int oldindex)
    {
        DoWork(() =>
        {
            if (frontCollection.Count == 0)
            {
                return;
            }

            if (oldindex == -1)
            {
                frontCollection.Clear();
            }
            else
            {
                frontCollection.RemoveAt(oldindex);
            }
        });
    }

    private void OnItemMoved(TItem item, int oldindex, int newindex)
    {
        DoWork(() => frontCollection.Move(oldindex, newindex));
    }

    public void Dispose()
    {
        // end the subscription
        collectionSubscription.Dispose();
    }
}