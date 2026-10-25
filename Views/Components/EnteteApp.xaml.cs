namespace MonBudget.Views.Components;

public partial class EnteteApp : ContentView
{
    public static readonly BindableProperty TitreProperty =
        BindableProperty.Create(nameof(Titre), typeof(string), typeof(EnteteApp), string.Empty);

    public string Titre
    {
        get => (string)GetValue(TitreProperty);
        set => SetValue(TitreProperty, value);
    }

    public EnteteApp() => InitializeComponent();

    // Un appui sur le rond de profil ouvre l'écran Profil
    private async void OnProfilTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("Profil");
}
