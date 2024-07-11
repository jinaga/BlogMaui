namespace Jinaga.Maui.Binding;

public class NavigationTreeBuilder
{
    public NavigationTreeBuilder AddPage<T>() where T: INavigationLifecycleAware
    {
        throw new NotImplementedException();
    }

    public NavigationTreeBuilder AddPage<T>(Func<NavigationTreeBuilder, NavigationTreeBuilder> pages) where T: INavigationLifecycleAware
    {
        throw new NotImplementedException();
    }

    internal NavigationTree Build()
    {
        throw new NotImplementedException();
    }
}
