namespace Jinaga.Maui.Binding;

public interface INavigationLifecycleManager
{
    void OnAppearing(INavigationLifecycleAware viewModel);
    void OnDisappearing(INavigationLifecycleAware viewModel);
}