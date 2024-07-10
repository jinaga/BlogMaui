namespace Jinaga.Maui;

public static class PageExtensions
{
    public static bool PageIsInStack(this Page page)
    {
        Element element = page;
        while (element.Parent != null)
        {
            if (element.Parent is Shell shell)
            {
                return shell.Navigation.NavigationStack.Contains(page) ||
                       shell.Navigation.ModalStack.Contains(page);
            }
            element = element.Parent;
        }
        return false;
    }
}