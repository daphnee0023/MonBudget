using MonBudget.Data;
using MonBudget.Models;
using MonBudget.Services;

namespace MonBudget.Views;

public partial class NouvelleDepensePage : ContentPage
{
    private static readonly Color Marine = Color.FromArgb("#000066");
    private readonly BudgetService _service = BudgetService.Instance;
    private Categorie? _categorieChoisie;

    public NouvelleDepensePage()
    {
        InitializeComponent();
        DateDepense.Date = DateTime.Today;
        DateDepense.MaximumDate = DateTime.Today;   // pas de dépense dans le futur
        BindableLayout.SetItemsSource(GrilleCategories, DonneesExemple.Categories);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LabelBudgetRestant.Text = BudgetService.Fcfa(_service.Restant);
        MajSoldeEstime();

        // Présélectionne la première catégorie (Alimentation)
        var premiere = GrilleCategories.Children.OfType<Border>().FirstOrDefault();
        if (premiere is not null && _categorieChoisie is null) Selectionner(premiere);
    }

    private static decimal LireMontant(string? texte)
        => decimal.TryParse(texte, out var m) ? m : 0m;

    private void MajSoldeEstime()
        => LabelSoldeEstime.Text = BudgetService.Fcfa(_service.Restant - LireMontant(EntryMontant.Text));

    private void OnMontantChanged(object? sender, TextChangedEventArgs e) => MajSoldeEstime();

    private void OnMontantRapideTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string valeur && decimal.TryParse(valeur, out var ajout))
            EntryMontant.Text = (LireMontant(EntryMontant.Text) + ajout).ToString("0");
    }

    private void OnCategorieTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Border tuile) Selectionner(tuile);
    }

    private void Selectionner(Border choisie)
    {
        foreach (var tuile in GrilleCategories.Children.OfType<Border>())
        {
            tuile.BackgroundColor = Colors.White;
            ColorerTextes(tuile, Marine);
        }
        choisie.BackgroundColor = Marine;
        ColorerTextes(choisie, Colors.White);

        if (choisie.BindingContext is Categorie c)
        {
            _categorieChoisie = c;
            LabelCategorieChoisie.Text = $"{c.Nom} sélectionnée";
        }
    }

    private static void ColorerTextes(Border tuile, Color couleur)
    {
        if (tuile.Content is VerticalStackLayout pile)
            foreach (var label in pile.Children.OfType<Label>())
                label.TextColor = couleur;
    }

    private async void OnEnregistrerClicked(object? sender, EventArgs e)
    {
        var montant = LireMontant(EntryMontant.Text);

        if (montant <= 0)
        {
            await DisplayAlertAsync("Montant invalide", "Le montant doit être supérieur à 0.", "OK");
            return;
        }
        if (_categorieChoisie is null)
        {
            await DisplayAlertAsync("Catégorie requise", "Choisissez une catégorie.", "OK");
            return;
        }

        var date = DateDepense.Date ?? DateTime.Today;
        var dateTexte = date.Date == DateTime.Today ? "Aujourd'hui"
                      : date.Date == DateTime.Today.AddDays(-1) ? "Hier"
                      : date.ToString("dd/MM/yyyy");

        _service.AjouterDepense(new Depense
        {
            Categorie = _categorieChoisie.Nom,
            Icone = _categorieChoisie.Icone,
            DateTexte = dateTexte,
            Montant = montant,
            Note = EditorNote.Text?.Trim() ?? ""
        });

        if (_service.SeuilAtteint)
            await DisplayAlertAsync("Alerte budget",
                $"Vous avez atteint 80% de votre budget mensuel ({BudgetService.Fcfa(_service.Seuil)}).", "OK");

        await Shell.Current.GoToAsync("..");
    }

    private async void OnRetourClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");
}
