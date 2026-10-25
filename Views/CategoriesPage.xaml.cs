using MonBudget.Models;
using MonBudget.Services;

namespace MonBudget.Views;

public partial class CategoriesPage : ContentPage
{
    private readonly ICategorieService _categories;

    public CategoriesPage(ICategorieService categories)
    {
        InitializeComponent();
        _categories = categories;
    }

    // Rechargé à chaque retour sur l'écran : nouvelle catégorie, nouvelle dépense…
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var aujourdhui = DateTime.Today;
        var liste = await _categories.ListerAvecTotauxAsync(aujourdhui.Year, aujourdhui.Month);

        BindableLayout.SetItemsSource(ListeCategories, liste);
        LabelNbCategories.Text = $"{liste.Count} Catégories actives";
        LabelTotalMois.Text = $"{Format.Fcfa(liste.Sum(c => c.Montant))} dépensés ce mois-ci";
    }

    private async void OnCategorieTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: Categorie categorie })
            await Shell.Current.GoToAsync($"CategorieDetail?id={categorie.Id}");
    }

    private async void OnToutesDepensesClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("CategorieDetail?id=0");

    private async void OnNouvelleCategorieClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("NouvelleCategorie");
}
