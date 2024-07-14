using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Jinaga.Maui;

public abstract class PageViewModel : INotifyPropertyChanged
{
    protected readonly JinagaClient jinagaClient;
    protected readonly ILogger logger;

    protected PageViewModel(JinagaClient jinagaClient, ILogger logger)
    {
        this.jinagaClient = jinagaClient;
        this.logger = logger;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool loading = false;
    private string error = "";

    public bool Loading
    {
        get => loading;
        protected set
        {
            if (value != loading)
            {
                loading = value;
                OnPropertyChanged(nameof(Loading));
            }
        }
    }

    public string Error
    {
        get => error;
        protected set
        {
            if (value != error)
            {
                error = value;
                OnPropertyChanged(nameof(Error));
            }
        }
    }

    public void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected async void Monitor(IObserver observer)
    {
        try
        {
            Loading = true;
            Error = "";

            bool wasInCache = await observer.Cached;
            if (!wasInCache)
            {
                await Task.WhenAll(
                    observer.Loaded,
                    jinagaClient.Push());
            }
        }
        catch (Exception x)
        {
            Error = x.Message;
            logger.LogError(x, "Error while loading");
        }
        finally
        {
            Loading = false;
        }
    }
}
