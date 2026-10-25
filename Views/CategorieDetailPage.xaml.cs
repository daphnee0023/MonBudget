using MonBudget.Models;
using MonBudget.Services;

namespace MonBudget.Views;

public partial class CategorieDetailPage : ContentPage, IQueryAttributable
{
    private readonly ICategorieService _categories;
    private readonly IDepenseService _depenses;
    private int _categorieId;          // 0 = toutes les dépenses

    public CategorieDetailPage(ICategorieService categories, IDepenseService depenses)
    {
        InitializeComponent();
        _categories = categories;
        _depenses = depenses;
    }

    // Reçoit « CategorieDetail?id=3 »
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var valeur) && int.TryParse(valeur?.ToString(), out var id))
            _categorieId = id;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ChargerAsync();
    }

    private async Task ChargerAsync()
    {
        var depenses = await _depenses.ListerAsync(_categorieId == 0 ? null : _categorieId);

        BindableLayout.SetItemsSource(ListeDepenses, depenses);
        LabelVide.IsVisible = depenses.Count == 0;
        LabelTotal.Text = Format.Fcfa(depenses.Sum(d => d.Montant));
        LabelNb.Text = $"{depenses.Count} dépense{(depenses.Count > 1 ? "s" : "")}";

        if (_categorieId == 0)
        {
            LabelTitre.Text = "Toutes les dépenses";
            BoutonSupprimerCategorie.IsVisible = false;
            return;
        }

        var categorie = await _categories.ObtenirAsync(_categorieId);
        LabelTitre.Text = categorie?.Nom ?? "Catégorie";
        // On ne propose la suppression que pour une catégorie personnalisée sans dépense
        BoutonSupprimerCategorie.IsVisible = categorie is { EstPersonnalisee: true } && depenses.Count == 0;
    }

    private async void OnSupprimerDepenseTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not BindableObject { BindingContext: Depense depense }) return;

        var confirme = await DisplayAlertAsync("Supprimer la dépense",
            $"Supprimer {depense.MontantTexte} ({depense.Categorie}) ?", "Supprimer", "Annuler");
        if (!confirme) return;

        await _depenses.SupprimerAsync(depense.Id);
        await ChargerAsync();
    }

    private async void OnSupprimerCategorieClicked(object? sender, EventArgs e)
    {
        var confirme = await DisplayAlertAsync("Supprimer la catégorie",
            "Voulez-vous vraiment supprimer cette catégorie ?", "Supprimer", "Annuler");
        if (!confirme) return;

        var erreur = await _categories.SupprimerAsync(_categorieId);
        if (erreur is not null)
        {
            await DisplayAlertAsync("Suppression impossible", erreur, "OK");
            return;
        }
        await Shell.Current.GoToAsync("..");
    }

    private async void OnRetourClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");
}
