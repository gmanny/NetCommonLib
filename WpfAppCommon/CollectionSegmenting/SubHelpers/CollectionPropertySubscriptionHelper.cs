using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows.Markup;

namespace WpfAppCommon.CollectionSegmenting.SubHelpers;

/// <summary>
/// Helper object that monitors specified property changes on the object, and then treats proeprty values like a collections which it monitors through the <see cref="DelegateCollectionSubscriptionHelper{TItem}"/>.
/// </summary>
/// <typeparam name="TItem">type of the collection item</typeparam>
public class CollectionPropertySubscriptionHelper<TItem> : IDisposable
{
    private readonly INotifyPropertyChanged objectWithProperty;
    private readonly string propertyName;
    private readonly PropertyInfo propertyInfo;

    private readonly ItemAdditionHandler<TItem> additionHandler;
    private readonly ItemRemovalHandler<TItem> removalHandler;
    private readonly ItemMoveHandler<TItem> moveHandler;

    private DelegateCollectionSubscriptionHelper<TItem> currentHelper;

    public CollectionPropertySubscriptionHelper([NotNull] INotifyPropertyChanged objectWithProperty, [NotNull] string propertyName, [NotNull] ItemAdditionHandler<TItem> additionHandler,
        [NotNull] ItemRemovalHandler<TItem> removalHandler, ItemMoveHandler<TItem> moveHandler)
    {
        // check parameters
        ArgumentNullException.ThrowIfNull(objectWithProperty);
        ArgumentNullException.ThrowIfNull(propertyName);
        ArgumentNullException.ThrowIfNull(additionHandler);
        ArgumentNullException.ThrowIfNull(removalHandler);

        // initialize object
        this.objectWithProperty = objectWithProperty;
        this.propertyName = propertyName;

        this.additionHandler = additionHandler;
        this.removalHandler = removalHandler;
        this.moveHandler = moveHandler;

        // -- initialize first collection helper --
        // get property of the object
        propertyInfo = objectWithProperty.GetType().GetProperty(propertyName);

        // check
        if (propertyInfo == null)
        {
            throw new InvalidOperationException($"Object of type {objectWithProperty.GetType().Name} doesn't have proeprty named `{propertyName}`");
        }

        BuildCollectionHelper();

        // subscribe for property changes
        objectWithProperty.PropertyChanged += OnObjectWithPropertyPropertyChanged;
    }

    private void BuildCollectionHelper()
    {
        // get initial property value
        object value = propertyInfo.GetValue(objectWithProperty, null); // not supporting indexer here

        // check it's nullness
        if (value == null)
        {
            return;
        }

        // cast it to both interfaces that wee need
        if (value is not IList<TItem> list)
        {
            throw new InvalidOperationException($"Value of the property `{propertyName}` was of the type {value.GetType().Name} which is not IList<{typeof(TItem).Name}>");
        }
        if (value is not INotifyCollectionChanged notifyCollectionChanged)
        {
            throw new InvalidOperationException($"Value of the property `{propertyName}` was of the type {value.GetType().Name} which is not INotifyCollectionChanged");
        }

        // now we construct the helper
        currentHelper = new DelegateCollectionSubscriptionHelper<TItem>(list, notifyCollectionChanged, additionHandler, removalHandler, moveHandler);
    }

    private void OnObjectWithPropertyPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // check the property name
        if (e.PropertyName != propertyName)
        {
            return;
        }

        // dispose the old helper
        currentHelper?.Dispose();

        BuildCollectionHelper();
    }

    public void Dispose()
    {
        // unsubscribe from property changes
        objectWithProperty.PropertyChanged -= OnObjectWithPropertyPropertyChanged;

        currentHelper?.Dispose();

        GC.SuppressFinalize(this);
    }
}