using System.Globalization;
using Microsoft.Maui.Controls.Shapes;
using MonBudget.Models;
using MonBudget.Services;

namespace MonBudget.Views;

public partial class StatistiquesPage : ContentPage
{
    // Longueur d'un tour d'anneau, exprimée en épaisseurs de trait (voir le XAML)
    private const double Circonference = 14.31;
    private static readonly CultureInfo Fr = new("fr-FR");

    private readonly ICategorieService _categories;
    private readonly IBudgetService _budget;
    private readonly IDepenseService _depenses;
    private readonly IExportService _export;

    private int _annee = DateTime.Today.Year;
    private int _mois = DateTime.Today.Month;

    public StatistiquesPage(ICategorieService categories, IBudgetService budget,
                            IDepenseService depenses, IExportService export)
    {
        InitializeComponent();
        _categories = categories;
        _budget = budget;
        _depenses = depenses;
        _export = export;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ChargerAsync();
    }

    private async Task ChargerAsync()
    {
        var libelle = new DateTime(_annee, _mois, 1).ToString("MMMM yyyy", Fr);
        LabelMois.Text = char.ToUpper(libelle[0]) + libelle[1..];

        // ----- Répartition par catégorie -----
        var totaux = await _categories.TotauxDuMoisAsync(_annee, _mois);
        var total = totaux.Sum(c => c.Montant);
        var principales = totaux.Take(4).ToList();

        LabelTotal.Text = Format.Nombre(total);
        LabelAucuneDepense.IsVisible = totaux.Count == 0;
        BindableLayout.SetItemsSource(LegendeCategories, principales);
        MajAnneau(principales, total);

        if (totaux.Count > 0)
        {
            LabelPosteMajeur.Text = totaux[0].Nom;
            LabelPartMajeur.Text = $"{totaux[0].Pourcentage}% des dépenses";
        }
        else
        {
            LabelPosteMajeur.Text = "—";
            LabelPartMajeur.Text = "Aucune dépense";
        }

        // ----- Rythme mensuel -----
        var resume = await _budget.ResumeAsync(_annee, _mois);
        var maitrise = resume.TotalDepense < resume.Seuil;
        LabelRythmeTitre.Text = resume.Depasse ? "Budget dépassé" : maitrise ? "Bien maîtrisé" : "À surveiller";
        LabelRythmeTitre.TextColor = Color.FromArgb(maitrise ? "#43A047" : "#D32F2F");
        LabelRythmeDetail.Text = resume.Depasse ? "Au-dessus du budget" : maitrise ? "Conforme aux prévisions" : "Seuil de 80% atteint";

        // ----- Chiffres clés -----
        var stats = await _depenses.StatsDuMoisAsync(_annee, _mois);
        LabelNbDepenses.Text = stats.Nombre.ToString();
        LabelMoyenne.Text = Format.Nombre(stats.MoyenneParJour);
        LabelPlusGrosse.Text = stats.Nombre == 0 ? "—" : Format.Nombre(stats.PlusGrosseMontant);
        LabelPlusGrosseCategorie.Text = stats.PlusGrosseCategorie;

        // ----- Évolution -----
        DessinerEvolution(await _depenses.EvolutionAsync(_annee, _mois, 6));
    }

    private void MajAnneau(List<Categorie> categories, decimal total)
    {
        var arcs = new[] { Arc1, Arc2, Arc3, Arc4 };
        double cumul = 0;

        for (var i = 0; i < arcs.Length; i++)
        {
            if (i >= categories.Count || total <= 0)
            {
                arcs[i].IsVisible = false;
                continue;
            }

            var longueur = (double)(categories[i].Montant / total) * Circonference;
            arcs[i].IsVisible = true;
            arcs[i].StrokeDashArray = new DoubleCollection { longueur, Circonference - longueur };
            arcs[i].StrokeDashOffset = -cumul;
            cumul += longueur;
        }
    }

    // Histogramme dessiné avec de simples rectangles (pas de bibliothèque de graphiques)
    private void DessinerEvolution(List<TotalMois> mois)
    {
        GrapheEvolution.Children.Clear();
        GrapheEvolution.ColumnDefinitions.Clear();

        var maximum = mois.Max(m => m.Total);
        if (maximum <= 0) maximum = 1;

        for (var i = 0; i < mois.Count; i++)
        {
            var m = mois[i];
            var selectionne = m.Annee == _annee && m.Mois == _mois;
            var hauteur = Math.Max(4, (double)(m.Total / maximum) * 105);

            GrapheEvolution.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            var valeur = m.Total >= 1000
                ? Math.Round(m.Total / 1000, 1).ToString("0.#", Fr) + " k"
                : m.Total.ToString("0");

            var colonne = new VerticalStackLayout
            {
                Spacing = 4,
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.Center
            };
            colonne.Children.Add(new Label
            {
                Text = m.Total > 0 ? valeur : "",
                FontSize = 10,
                TextColor = Color.FromArgb("#5F6368"),
                HorizontalTextAlignment = TextAlignment.Center
            });
            colonne.Children.Add(new Border
            {
                WidthRequest = 28,
                HeightRequest = hauteur,
                HorizontalOptions = LayoutOptions.Center,
                Stroke = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(6) },
                BackgroundColor = Color.FromArgb(selectionne ? "#000066" : "#4350B5")
            });
            colonne.Children.Add(new Label
            {
                Text = m.Libelle,
                FontSize = 11,
                FontAttributes = selectionne ? FontAttributes.Bold : FontAttributes.None,
                TextColor = Color.FromArgb(selectionne ? "#000066" : "#5F6368"),
                HorizontalTextAlignment = TextAlignment.Center
            });

            Grid.SetColumn(colonne, i);
            GrapheEvolution.Children.Add(colonne);
        }
    }

    private async void OnMoisPrecedentTapped(object? sender, TappedEventArgs e)
    {
        var precedent = new DateTime(_annee, _mois, 1).AddMonths(-1);
        _annee = precedent.Year;
        _mois = precedent.Month;
        await ChargerAsync();
    }

    private async void OnMoisSuivantTapped(object? sender, TappedEventArgs e)
    {
        var courant = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var suivant = new DateTime(_annee, _mois, 1).AddMonths(1);
        if (suivant > courant) return;           // pas de mois futur

        _annee = suivant.Year;
        _mois = suivant.Month;
        await ChargerAsync();
    }

    private async void OnExporterClicked(object? sender, EventArgs e)
    {
        BoutonExport.IsEnabled = false;
        try
        {
            var chemin = await _export.GenererRapportPdfAsync(_annee, _mois);
            await _export.PartagerAsync(chemin);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Export impossible", ex.Message, "OK");
        }
        finally
        {
            BoutonExport.IsEnabled = true;
        }
    }
}
