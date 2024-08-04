using System.Windows.Input;

namespace BlogMaui.Components;

public partial class XLabeledEntry : ContentView
{
    public XLabeledEntry()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty LabelTextProperty =
        BindableProperty.Create(nameof(LabelText), typeof(string), typeof(XLabeledEntry), default(string));

    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public static readonly BindableProperty EntryTextProperty =
        BindableProperty.Create(nameof(EntryText), typeof(string), typeof(XLabeledEntry), default(string), BindingMode.TwoWay);

    public string EntryText
    {
        get => (string)GetValue(EntryTextProperty);
        set => SetValue(EntryTextProperty, value);
    }

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(XLabeledEntry), default(string));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly BindableProperty ButtonCommandProperty =
        BindableProperty.Create(nameof(ButtonCommand), typeof(ICommand), typeof(XLabeledEntry), default(ICommand));

    public ICommand ButtonCommand
    {
        get => (ICommand)GetValue(ButtonCommandProperty);
        set => SetValue(ButtonCommandProperty, value);
    }

    public static readonly BindableProperty ButtonIsVisibleProperty =
        BindableProperty.Create(nameof(ButtonIsVisible), typeof(bool), typeof(XLabeledEntry), default(bool));

    public bool ButtonIsVisible
    {
        get => (bool)GetValue(ButtonIsVisibleProperty);
        set => SetValue(ButtonIsVisibleProperty, value);
    }
}