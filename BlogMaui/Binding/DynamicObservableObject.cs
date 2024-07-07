using System.ComponentModel;
using System.Dynamic;

namespace BlogMaui.Binding;

public class DynamicObservableObject : DynamicObject, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly object? innerObject;
    private readonly Dictionary<string, object?> properties = new();

    public DynamicObservableObject(object? innerObject)
    {
        this.innerObject = innerObject;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        var oldValue = Get(binder.Name);
        if (oldValue == value)
        {
            return true;
        }

        Set(binder.Name, value);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(binder.Name));
        return true;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        result = Get(binder.Name);
        return true;
    }

    private object? Get(string name)
    {
        if (innerObject != null)
        {
            var property = innerObject.GetType().GetProperty(name);
            if (property != null && property.CanRead)
            {
                return property.GetValue(innerObject);
            }
        }

        if (properties.TryGetValue(name, out var value))
        {
            return value;
        }
        else
        {
            return null;
        }
    }

    private void Set(string name, object? value)
    {
        if (innerObject != null)
        {
            var property = innerObject.GetType().GetProperty(name);
            if (property != null && property.CanWrite)
            {
                property.SetValue(innerObject, value);
                return;
            }
        }

        properties[name] = value;
    }
}