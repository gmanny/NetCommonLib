using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace WpfAppCommon.CollectionSegmenting.SubHelpers
{
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
            if (objectWithProperty == null) throw new ArgumentNullException("objectWithProperty");
            if (propertyName == null) throw new ArgumentNullException("propertyName");
            if (additionHandler == null) throw new ArgumentNullException("additionHandler");
            if (removalHandler == null) throw new ArgumentNullException("removalHandler");

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
                throw new InvalidOperationException("Object of type " + objectWithProperty.GetType().Name + " doesn't have proeprty named `" + propertyName + "`");
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
            IList<TItem> list = value as IList<TItem>;
            INotifyCollectionChanged notifyCollectionChanged = value as INotifyCollectionChanged;

            // check
            if (list == null)
            {
                throw new InvalidOperationException("Value of the property `" + propertyName + "` was of the type " + value.GetType().Name + " which is not IList<" + typeof(TItem).Name + ">");
            }
            if (notifyCollectionChanged == null)
            {
                throw new InvalidOperationException("Value of the property `" + propertyName + "` was of the type " + value.GetType().Name + " which is not INotifyCollectionChanged");
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
            if (currentHelper != null)
            {
                currentHelper.Dispose();
            }

            BuildCollectionHelper();
        }

        public void Dispose()
        {
            // unsubscribe from property changes
            objectWithProperty.PropertyChanged -= OnObjectWithPropertyPropertyChanged;

            if (currentHelper != null)
            {
                currentHelper.Dispose();
            }
        }
    }
}