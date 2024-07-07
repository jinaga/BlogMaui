using System.Collections;
using Jinaga.Observers;

namespace BlogMaui.Binding;

public class SnapshotObservableCollection<T> : IObservableCollection<T>
{
    private readonly IObservableCollection<T> observedCollection;

    private List<T> items = new();

    public SnapshotObservableCollection(IObservableCollection<T> observedCollection, Action onChanged)
    {
        this.observedCollection = observedCollection;
        observedCollection.OnAdded(item =>
        {
            items.Add(item);
            onChanged();

            return () =>
            {
                items.Remove(item);
                onChanged();
            };
        });
    }

    public IEnumerator<T> GetEnumerator()
    {
        return items.GetEnumerator();
    }

    public void OnAdded(Func<T, Task<Func<Task>>> added)
    {
        observedCollection.OnAdded(added);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return items.GetEnumerator();
    }
}
