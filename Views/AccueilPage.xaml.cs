using MonBudget.Services;

namespace MonBudget.Views;

public partial class AccueilPage : ContentPage
{
    private readonly IAuthService _auth;
    private readonly IBudgetService _budget;
    private readonly IDepenseService _depenses;

    public AccueilPage(IAuthService auth, IBudgetService budget, IDepenseService depenses)
    {
        InitializeComponent();
        _auth = auth;
        _budget = budget;
        _depenses = depenses;
    }

    // Rafraîchit les chiffres à chaque retour sur l'écran (nouvelle dépense, nouveau budget…)
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var prenom = _auth.Courant?.Nom.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        LabelBonjour.Text = prenom is null ? "Bonjour" : $"Bonjour {prenom}";

        var resume = await _budget.ResumeAsync();
        LabelBudget.Text = Format.Nombre(resume.Budget);
        LabelDepense.Text = Format.Fcfa(resume.TotalDepense);
        LabelRestant.Text = Format.Fcfa(resume.Restant);
        BarreBudget.Progress = resume.PartDepensee;
        LabelAlloue.Text = $"{Math.Round(resume.PartDepensee * 100)}% alloué";
        LabelMarge.Text = $"{Math.Round((1 - resume.PartDepensee) * 100)}% marge";

        var recentes = await _depenses.RecentesAsync(3);
        BindableLayout.SetItemsSource(ListeDepenses, recentes);
        LabelVide.IsVisible = recentes.Count == 0;
    }

    private async void OnAjouterClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("NouvelleDepense");

    private async void OnVoirToutTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//Main/Depenses");
}
