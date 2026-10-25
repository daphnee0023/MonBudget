using Microsoft.Maui.Controls.Shapes;
using MonBudget.Services;

namespace MonBudget.Views;

public partial class NouvelleCategoriePage : ContentPage
{
    private static readonly string[] Icones =
    {
        "ic_utensils_marine.png", "ic_bus_marine.png", "ic_home_marine.png", "ic_gamepad_marine.png",
        "ic_pill_marine.png", "ic_book_marine.png", "ic_bag_marine.png", "ic_gift_marine.png",
        "ic_heart_marine.png", "ic_phone_marine.png", "ic_coffee_marine.png", "ic_briefcase_marine.png",
        "ic_music_marine.png", "ic_wifi_marine.png", "ic_zap_marine.png", "ic_more_marine.png"
    };

    private static readonly string[] Couleurs =
    {
        "#4350B5", "#1AA0D8", "#43A047", "#FFB300", "#E57373", "#9C27B0", "#FF7043", "#00897B"
    };

    private static readonly Color Marine = Color.FromArgb("#000066");

    private readonly ICategorieService _categories;
    private string _icone = Icones[^1];
    private string _couleur = Couleurs[0];

    public NouvelleCategoriePage(ICategorieService categories)
    {
        InitializeComponent();
        _categories = categories;

        BindableLayout.SetItemsSource(GrilleIcones, Icones);
        CreerPastillesCouleurs();

        // Sélection par défaut (une fois les éléments créés)
        Loaded += (_, _) =>
        {
            MarquerSelection(GrilleIcones, _icone);
            MarquerSelection(GrilleCouleurs, _couleur);
            MajApercu();
        };
    }

    private void CreerPastillesCouleurs()
    {
        foreach (var hex in Couleurs)
        {
            var pastille = new Border
            {
                WidthRequest = 44,
                HeightRequest = 44,
                Margin = new Thickness(0, 0, 12, 12),
                BackgroundColor = Color.FromArgb(hex),
                Stroke = Colors.Transparent,
                StrokeThickness = 3,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(22) },
                BindingContext = hex
            };
            var geste = new TapGestureRecognizer();
            geste.Tapped += OnCouleurTapped;
            pastille.GestureRecognizers.Add(geste);
            GrilleCouleurs.Children.Add(pastille);
        }
    }

    // Met un contour sur l'élément choisi, enlève celui des autres
    private static void MarquerSelection(Layout grille, string valeur)
    {
        foreach (var element in grille.Children.OfType<Border>())
            element.Stroke = (element.BindingContext as string) == valeur ? Marine : Colors.Transparent;
    }

    private void MajApercu()
    {
        ApercuIcone.Source = _icone;
        ApercuFond.BackgroundColor = Color.FromArgb(_couleur).WithAlpha(0.15f);
        ApercuNom.Text = string.IsNullOrWhiteSpace(EntryNom.Text) ? "Nom de la catégorie" : EntryNom.Text.Trim();
        ApercuDescription.Text = string.IsNullOrWhiteSpace(EntryDescription.Text) ? "Description" : EntryDescription.Text.Trim();
    }

    private void OnTexteChanged(object? sender, TextChangedEventArgs e) => MajApercu();

    private void OnIconeTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Border { BindingContext: string icone }) return;
        _icone = icone;
        MarquerSelection(GrilleIcones, _icone);
        MajApercu();
    }

    private void OnCouleurTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Border { BindingContext: string couleur }) return;
        _couleur = couleur;
        MarquerSelection(GrilleCouleurs, _couleur);
        MajApercu();
    }

    private async void OnCreerClicked(object? sender, EventArgs e)
    {
        var erreur = await _categories.AjouterAsync(
            EntryNom.Text ?? "", EntryDescription.Text ?? "", _icone, _couleur);

        if (erreur is not null)
        {
            await DisplayAlertAsync("Nouvelle catégorie", erreur, "OK");
            return;
        }
        await Shell.Current.GoToAsync("..");
    }

    private async void OnRetourClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");
}
