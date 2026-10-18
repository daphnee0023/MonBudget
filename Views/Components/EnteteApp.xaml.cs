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
}
