namespace BlogMaui;
public static class ParameterExtensions
{
    public static T GetParameter<T>(this IDictionary<string, object> query, string name)
    {
        if (!query.TryGetValue(name, out var parameter))
        {
            throw new ArgumentException($"Please pass a {name} parameter");
        }
        if (parameter is not T value)
        {
            throw new ArgumentException($"The {name} parameter must be a {typeof(T).Name}");
        }

        return value;
    }
}
