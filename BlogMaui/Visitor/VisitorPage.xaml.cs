namespace BlogMaui.Visitor;

public partial class VisitorPage : ContentPage
{
	public VisitorPage(VisitorViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}