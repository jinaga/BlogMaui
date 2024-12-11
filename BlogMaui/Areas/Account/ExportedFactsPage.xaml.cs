
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
    }
}