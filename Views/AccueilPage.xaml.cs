using MonBudget.Services;

namespace MonBudget.Views;

public partial class AccueilPage : ContentPage
{
    private readonly BudgetService _service = BudgetService.Instance;

    public AccueilPage() => InitializeComponent();

    // Rafraîchit les chiffres à chaque retour sur l'écran (après une nouvelle dépense, un nouveau budget…)
    protected override void OnAppearing()
    {
        base.OnAppearing();

        var prenom = _service.NomUtilisateur?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        LabelBonjour.Text = prenom is null ? "Bonjour" : $"Bonjour {prenom}";

        LabelBudget.Text = BudgetService.Nombre(_service.Budget);
        LabelDepense.Text = BudgetService.Fcfa(_service.TotalDepense);
        LabelRestant.Text = BudgetService.Fcfa(_service.Restant);

        var part = _service.PartDepensee;
        BarreBudget.Progress = part;
        LabelAlloue.Text = $"{Math.Round(part * 100)}% alloué";
        LabelMarge.Text = $"{Math.Round((1 - part) * 100)}% marge";

        BindableLayout.SetItemsSource(ListeDepenses, null);
        BindableLayout.SetItemsSource(ListeDepenses, _service.Depenses.Take(3).ToList());
    }

    private async void OnAjouterClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("NouvelleDepense");

    private async void OnVoirToutTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//Main/Depenses");
}
