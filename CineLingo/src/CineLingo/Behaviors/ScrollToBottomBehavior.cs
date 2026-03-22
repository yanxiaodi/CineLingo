using System.Collections.Specialized;

namespace CineLingo.Behaviors;

/// <summary>Automatically scrolls a CollectionView to the last item when new items are added.</summary>
public class ScrollToBottomBehavior : Behavior<CollectionView>
{
    private CollectionView? _collectionView;

    protected override void OnAttachedTo(CollectionView bindable)
    {
        base.OnAttachedTo(bindable);
        _collectionView = bindable;
        _collectionView.PropertyChanged += OnPropertyChanged;
        AttachCollectionChangedHandler(_collectionView.ItemsSource as INotifyCollectionChanged);
    }

    protected override void OnDetachingFrom(CollectionView bindable)
    {
        base.OnDetachingFrom(bindable);
        if (_collectionView?.ItemsSource is INotifyCollectionChanged collection)
            collection.CollectionChanged -= OnCollectionChanged;
        _collectionView = null;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0 && _collectionView is not null)
        {
            var lastItem = e.NewItems[e.NewItems.Count - 1];
            _collectionView.ScrollTo(lastItem, position: ScrollToPosition.End, animate: true);
        }
    }

    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CollectionView.ItemsSource))
            AttachCollectionChangedHandler(_collectionView?.ItemsSource as INotifyCollectionChanged);
    }

    private void AttachCollectionChangedHandler(INotifyCollectionChanged? collection)
    {
        if (collection is not null)
            collection.CollectionChanged += OnCollectionChanged;
    }
}
