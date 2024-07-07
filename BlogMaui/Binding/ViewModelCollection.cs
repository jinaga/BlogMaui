using System.Collections;
using System.Collections.Specialized;
using Jinaga;
using Jinaga.Observers;

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
            HandleChanges(projection, snapshot =>
            {
                augmenter(snapshot, dynamicViewModel);
            });
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

    private void HandleChanges<TProjection>(TProjection projection, Action<TProjection> onChanged)
    {
        // The projection type must have a parameterless constructor.
        var constructor = typeof(TProjection).GetConstructor(Type.EmptyTypes);
        if (constructor == null)
        {
            throw new ArgumentException("The projection type must have a parameterless constructor.");
        }

        // Find the properties of the projection type.
        var properties = typeof(TProjection).GetProperties();

        // Create a snapshot of the projection.
        var snapshot = (TProjection)constructor.Invoke([]);

        // Set every property, replacing every IObservableCollection with a SnapshotObservableCollection.
        foreach (var property in properties)
        {
            if (property.PropertyType.IsGenericType &&
                property.PropertyType.GetGenericTypeDefinition() == typeof(IObservableCollection<>) &&
                property.PropertyType.GetGenericArguments().Length == 1)
            {
                var itemType = property.PropertyType.GetGenericArguments()[0];
                var snapshotType = typeof(SnapshotObservableCollection<>).MakeGenericType(itemType);
                var snapshotConstructor = snapshotType.GetConstructor([property.PropertyType, typeof(Action)]);
                if (snapshotConstructor == null)
                {
                    throw new ArgumentException("The snapshot type must have a constructor that takes an IObservableCollection and an Action.");
                }
                var snapshotCollection = snapshotConstructor.Invoke([property.GetValue(projection), onChanged]);
                property.SetValue(snapshot, snapshotCollection);
            }
            else
            {
                property.SetValue(snapshot, property.GetValue(projection));
            }
        }

        // Set the initial value.
        onChanged(snapshot);
    }

    private SnapshotObservableCollection<T> CreateSnapshotObservableCollection<T>(IObservableCollection<T> observedCollection, Action onChanged)
    {
        return new SnapshotObservableCollection<T>(observedCollection, onChanged);
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