using MonBudget.Models;
using MonBudget.Services;

namespace MonBudget.Views;

public partial class NouvelleDepensePage : ContentPage
{
    private static readonly Color Marine = Color.FromArgb("#000066");

    private readonly ICategorieService _categories;
    private readonly IDepenseService _depenses;
    private readonly IBudgetService _budget;
    private readonly INotificationService _notifications;

    private Categorie? _categorieChoisie;
    private ResumeBudget _resume = new();
    private bool _categoriesChargees;

    public NouvelleDepensePage(ICategorieService categories, IDepenseService depenses,
                               IBudgetService budget, INotificationService notifications)
    {
        InitializeComponent();
        _categories = categories;
        _depenses = depenses;
        _budget = budget;
        _notifications = notifications;

        DateDepense.Date = DateTime.Today;
        DateDepense.MaximumDate = DateTime.Today;   // pas de dépense dans le futur
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _resume = await _budget.ResumeAsync();
        LabelBudgetRestant.Text = Format.Fcfa(_resume.Restant);
        MajSoldeEstime();

        if (_categoriesChargees) return;
        BindableLayout.SetItemsSource(GrilleCategories, await _categories.ListerAsync());
        _categoriesChargees = true;

        // Présélectionne la première catégorie
        var premiere = GrilleCategories.Children.OfType<Border>().FirstOrDefault();
        if (premiere is not null) Selectionner(premiere);
    }

    private static decimal LireMontant(string? texte)
        => decimal.TryParse(texte, out var m) ? m : 0m;

    private void MajSoldeEstime()
        => LabelSoldeEstime.Text = Format.Fcfa(_resume.Restant - LireMontant(EntryMontant.Text));

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

    // Notification système si possible (Android), sinon alerte à l'écran
    private async Task AlerterAsync(string titre, string message)
    {
        var envoyee = await _notifications.NotifierAsync(titre, message);
        if (!envoyee) await DisplayAlertAsync(titre, message, "OK");
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
        await _depenses.AjouterAsync(_categorieChoisie.Id, montant, date, EditorNote.Text?.Trim() ?? "");

        // Une alerte n'est envoyée qu'au moment où cette dépense franchit un seuil
        var apres = await _budget.ResumeAsync();
        if (apres.AlerteDepassement && !_resume.AlerteDepassement)
        {
            await AlerterAsync("Budget dépassé",
                $"Vous avez dépensé {Format.Fcfa(apres.TotalDepense)} pour un budget de {Format.Fcfa(apres.Budget)}.");
        }
        else if (apres.SeuilAtteint && !_resume.SeuilAtteint)
        {
            await AlerterAsync("80% du budget atteint",
                $"Il vous reste {Format.Fcfa(apres.Restant)} sur {Format.Fcfa(apres.Budget)} ce mois-ci.");
        }

        await Shell.Current.GoToAsync("..");
    }

    private async void OnRetourClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");
}
