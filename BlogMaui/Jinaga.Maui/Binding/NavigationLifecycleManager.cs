namespace Jinaga.Maui.Binding;

public class NavigationLifecycleManager : INavigationLifecycleManager
{
    private NavigationTree tree;

    internal NavigationLifecycleManager(NavigationTree tree)
    {
        this.tree = tree;
    }

    public void OnAppearing(INavigationLifecycleAware viewModel)
    {
        viewModel.Load();
    }

    public void OnDisappearing(INavigationLifecycleAware viewModel)
    {
        viewModel.Unload();
    }
}