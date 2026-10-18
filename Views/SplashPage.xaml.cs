namespace MonBudget.Views;

public partial class SplashPage : ContentPage
{
    public SplashPage() => InitializeComponent();

    private async void OnCommencerClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//Connexion");
}
