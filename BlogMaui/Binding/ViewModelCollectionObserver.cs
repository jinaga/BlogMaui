using System.Collections;
using Jinaga;

namespace BlogMaui.Binding;

internal class ViewModelCollectionObserver : IObserver
{
    private IObserver observer;
    private ArrayList items;

    public ViewModelCollectionObserver(IObserver observer, ArrayList items)
    {
        this.observer = observer;
        this.items = items;
    }

    public Task<bool> Cached => observer.Cached;

    public Task Loaded => observer.Loaded;

    public Task Refresh(CancellationToken? cancellationToken = null)
    {
        return observer.Refresh(cancellationToken);
    }

    public void Stop()
    {
        observer.Stop();
        items.Clear();
    }
}