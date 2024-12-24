using Microsoft.Maui.Controls;

namespace BlogMaui.Areas.Account
{
    public partial class ExportedFactsPage : ContentPage
    {
        public ExportedFactsPage(string exportedFacts)
        {
            InitializeComponent();
            ExportedFactsLabel.Text = exportedFacts;
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
        {
            Shell.Current.Navigation.PopAsync();
        }
    }
}