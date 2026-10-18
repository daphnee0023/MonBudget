using MonBudget.Services;

namespace MonBudget.Views;

public partial class ConnexionPage : ContentPage
{
    public ConnexionPage() => InitializeComponent();

    private void OnAfficherMotDePasseTapped(object? sender, TappedEventArgs e)
    {
        EntryMotDePasse.IsPassword = !EntryMotDePasse.IsPassword;
        IconeOeil.Source = EntryMotDePasse.IsPassword ? "ic_eye_gris" : "ic_eye_off_gris";
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

        if (BudgetService.Instance.AucunCompte)
        {
            await DisplayAlertAsync("Aucun compte", "Créez d'abord un compte pour vous connecter.", "OK");
            return;
        }

        if (!BudgetService.Instance.Connecter(identifiant, motDePasse))
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
