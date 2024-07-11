namespace Jinaga.Maui.Binding;

public class NavigationTreeBuilder
{
    public NavigationTreeBuilder AddPage<T>() where T: INavigationLifecycleAware
    {
        return this;
    }

    public NavigationTreeBuilder AddPage<T>(Func<NavigationTreeBuilder, NavigationTreeBuilder> pages) where T: INavigationLifecycleAware
    {
        return this;
    }

    internal NavigationTree Build()
    {
        return new NavigationTree();
    }
}
