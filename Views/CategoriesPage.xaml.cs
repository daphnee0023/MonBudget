using MonBudget.Data;

namespace MonBudget.Views;

public partial class CategoriesPage : ContentPage
{
    public CategoriesPage()
    {
        InitializeComponent();
        var categories = DonneesExemple.Categories;
        BindableLayout.SetItemsSource(ListeCategories, categories);
        LabelNbCategories.Text = $"{categories.Count} Catégories actives";
    }

    // TODO : ouvrir un formulaire d'ajout de catégorie
    private async void OnNouvelleCategorieClicked(object? sender, EventArgs e)
        => await DisplayAlertAsync("Nouvelle catégorie", "Formulaire à venir.", "OK");
}
