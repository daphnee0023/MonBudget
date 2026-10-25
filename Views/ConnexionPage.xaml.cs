using MonBudget.Services;

namespace MonBudget.Views;

public partial class ConnexionPage : ContentPage
{
    private readonly IAuthService _auth;

    public ConnexionPage(IAuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    private void OnAfficherMotDePasseTapped(object? sender, TappedEventArgs e)
    {
        EntryMotDePasse.IsPassword = !EntryMotDePasse.IsPassword;
        IconeOeil.Source = EntryMotDePasse.IsPassword ? "ic_eye_gris.png" : "ic_eye_off_gris.png";
    }

    private async void OnConnexionClicked(object? sender, EventArgs e)
    {
        var identifiant = EntryIdentifiant.Text?.Trim() ?? "";
        var motDePasse = EntryMotDePasse.Text ?? "";

        if (identifiant.Length == 0 || motDePasse.Length == 0)
        {
            await DisplayAlertAsync("Champs requis", "Saisissez votre téléphone ou e-mail et votre mot de passe.", "OK");
            return;
        }

        if (!await _auth.ConnecterAsync(identifiant, motDePasse, CheckSouvenir.IsChecked))
        {
            await DisplayAlertAsync("Connexion impossible", "Identifiant ou mot de passe incorrect.", "OK");
            return;
        }

        EntryMotDePasse.Text = string.Empty;
        await Shell.Current.GoToAsync("//Main/Accueil");
    }

    private async void OnCreerCompteClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//Inscription");

    private async void OnRetourClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//Splash");
}
