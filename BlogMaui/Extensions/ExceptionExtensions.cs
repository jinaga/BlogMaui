using System.Text;

namespace BlogMaui;

public static class ExceptionExtensions
{
    public static string GetMessage(this Exception exception)
    {
        var builder = new StringBuilder();
        Exception? ex = exception;
        while (ex != null)
        {
            builder.AppendLine(ex.Message);
            ex = ex.InnerException;
        }
        return builder.ToString();
    }
}
