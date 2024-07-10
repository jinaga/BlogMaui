using System.Reflection;

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

    public static Page? GetPreviousPage(this NavigatedToEventArgs args)
    {
        // The previous page is an internal property of the NavigationEventArgs called PreviousPage.
        // Use reflection to access it.
        var propertyInfo = args.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(property => property.Name == "PreviousPage")
            .FirstOrDefault();
        if (propertyInfo?.GetValue(args) is Page page)
        {
            return page;
        }
        return null;
    }

    public static Page? GetDestinationPage(this NavigatedFromEventArgs args)
    {
        // The destination page is an internal property of the NavigationEventArgs called DestinationPage.
        // Use reflection to access it.
        var propertyInfo = args.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(property => property.Name == "DestinationPage")
            .FirstOrDefault();
        if (propertyInfo?.GetValue(args) is Page page)
        {
            return page;
        }
        return null;
    }
}