using System.Collections;
using System.Collections.Specialized;

namespace CineLingo.Behaviors;
public class ScrollToBottomBehavior : Behavior<ListView>
{
    private ListView? _listView;

    protected override void OnAttachedTo(ListView bindable)
    {
        base.OnAttachedTo(bindable);
        _listView = bindable;
        _listView.PropertyChanged += OnListViewPropertyChanged;
        AttachCollectionChangedHandler(_listView.ItemsSource as INotifyCollectionChanged);
    }

    protected override void OnDetachingFrom(ListView bindable)
    {
        base.OnDetachingFrom(bindable);
        if (_listView?.ItemsSource is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= OnCollectionChanged;
        }
        _listView = null;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && _listView?.ItemsSource is IList items)
        {
            _listView.ScrollTo(items[^1], ScrollToPosition.End, true);
        }
    }

    private void OnListViewPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ListView.ItemsSource))
        {
            AttachCollectionChangedHandler(_listView?.ItemsSource as INotifyCollectionChanged);
        }
    }

    private void AttachCollectionChangedHandler(INotifyCollectionChanged? collection)
    {
        if (collection != null)
        {
            collection.CollectionChanged += OnCollectionChanged;
        }
    }
}
