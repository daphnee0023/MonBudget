using MonBudget.Data;

namespace MonBudget.Views;

public partial class StatistiquesPage : ContentPage
{
    public StatistiquesPage()
    {
        InitializeComponent();
        BindableLayout.SetItemsSource(LegendeCategories, DonneesExemple.CategoriesAvecDepenses);
    }

    
    private async void OnExporterClicked(object? sender, EventArgs e)
        => await DisplayAlertAsync("Export", "Export CSV / PDF à venir.", "OK");
}
