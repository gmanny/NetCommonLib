using System.Collections.ObjectModel;

namespace WpfAppCommon.CollectionSegmenting;

/// <summary>
/// Describes item block. Which is a segment of the outer collection.
/// </summary>
/// <typeparam name="TItem">type of items in the collection</typeparam>
public interface IItemBlock<TItem>
{
    /// <summary>
    /// Gets the observable collection of items.
    /// </summary>
    ObservableCollection<TItem> ItemCollection { get; }
}