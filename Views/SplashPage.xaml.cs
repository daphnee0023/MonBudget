using MonBudget.Services;

namespace MonBudget.Views;

public partial class SplashPage : ContentPage
{
    private readonly IAuthService _auth;

    public SplashPage(IAuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    // Si « Se souvenir de moi » avait été coché, on va directement à l'accueil
    private async void OnCommencerClicked(object? sender, EventArgs e)
    {
        var route = await _auth.RestaurerSessionAsync() ? "//Main/Accueil" : "//Connexion";
        await Shell.Current.GoToAsync(route);
    }
}
