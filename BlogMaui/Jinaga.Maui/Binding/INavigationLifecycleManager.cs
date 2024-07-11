namespace Jinaga.Maui.Binding;

public interface INavigationLifecycleManager
{
    /// <summary>
    /// Call this method in the OnAppearing method of the view.
    /// </summary>
    /// <param name="viewModel">The view model to manage</param>
    void StartManaging(INavigationLifecycleAware viewModel);
    /// <summary>
    /// Call this method in the OnNavigatedFrom method of the view.
    /// </summary>
    /// <param name="viewModel">The view model to stop managing</param>
    void StopManaging(INavigationLifecycleAware viewModel);
}