using System.Collections;
using System.Collections.Specialized;
using Jinaga;

namespace BlogMaui.Binding;

public class ViewModelCollection : IEnumerable, INotifyCollectionChanged
{
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    private readonly ArrayList items = new();

    public IEnumerator GetEnumerator()
    {
        return items.GetEnumerator();
    }

    public IObserver Watch<TFact, TProjection, TViewModel>(
        JinagaClient jinagaClient,
        Specification<TFact, TProjection> specification,
        TFact given,
        Func<TProjection, TViewModel> constructor,
        Action<TProjection, dynamic> augmenter) where TFact : class
    {
        var observer = jinagaClient.Watch(specification, given, projection =>
        {
            var viewModel = constructor(projection);
            var dynamicViewModel = new DynamicObservableObject(viewModel);
            augmenter(projection, dynamicViewModel);
            items.Add(dynamicViewModel);
            NotifyAdded(dynamicViewModel);
            return () =>
            {
                items.Remove(dynamicViewModel);
                NotifyRemoved(dynamicViewModel);
            };
        });

        return new ViewModelCollectionObserver(observer, items);
    }

    private void NotifyAdded(object? item)
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
    }

    private void NotifyRemoved(object? item)
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
    }
}