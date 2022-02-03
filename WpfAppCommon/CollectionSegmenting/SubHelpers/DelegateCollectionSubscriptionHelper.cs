using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace WpfAppCommon.CollectionSegmenting.SubHelpers;

/// <summary>
/// Helper object that calls three supplied delegates when one of three collection actions occurs.
/// </summary>
/// <typeparam name="TItem">type of the collection item</typeparam>
public class DelegateCollectionSubscriptionHelper<TItem> : AbstractCollectionSubscriptionHelper<TItem>
{
    private readonly ItemAdditionHandler<TItem> additionHandler;
    private readonly ItemRemovalHandler<TItem> removalHandler;
    private readonly ItemMoveHandler<TItem> moveHandler;

    public DelegateCollectionSubscriptionHelper([NotNull] IList<TItem> collection, [NotNull] INotifyCollectionChanged notifier, [NotNull] ItemAdditionHandler<TItem> additionHandler,
        [NotNull] ItemRemovalHandler<TItem> removalHandler, ItemMoveHandler<TItem> moveHandler = null, bool collectionOwnerMode = true) 
        : base(collection, notifier, collectionOwnerMode)
    {
        ArgumentNullException.ThrowIfNull(additionHandler);
        ArgumentNullException.ThrowIfNull(removalHandler);

        this.additionHandler = additionHandler;
        this.removalHandler = removalHandler;
        this.moveHandler = moveHandler;

        Start();
    }

    protected override void OnItemAdded(TItem item, int index)
    {
        additionHandler(item, index);
    }

    // if oldIndex == - 1 - collection Clear was invoked
    protected override void OnItemRemoved(TItem item, int oldIndex)
    {
        removalHandler(item, oldIndex);
    }

    protected override void OnItemMoved(TItem item, int oldIndex, int newIndex)
    {
        moveHandler?.Invoke(item, oldIndex, newIndex);
    }
}