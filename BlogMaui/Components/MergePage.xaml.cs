namespace BlogMaui.Components;

public partial class MergePage : ContentPage
{
    public MergePage(MergeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
