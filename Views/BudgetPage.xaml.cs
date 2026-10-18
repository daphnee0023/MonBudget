using MonBudget.Services;

namespace MonBudget.Views;

public partial class BudgetPage : ContentPage
{
    private readonly BudgetService _service = BudgetService.Instance;

    public BudgetPage() => InitializeComponent();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        EntryBudget.Text = _service.Budget.ToString("0");
        RadioOui.IsChecked = _service.AlerteActive;
        MajAffichage(_service.Budget);
    }

    private void OnBudgetChanged(object? sender, TextChangedEventArgs e)
    {
        if (decimal.TryParse(e.NewTextValue, out var budget) && budget > 0)
            MajAffichage(budget);
    }

    // Plafond, seuil (80 %) et rythme journalier (31 jours en octobre)
    private void MajAffichage(decimal budget)
    {
        var seuil = budget * 0.8m;
        LabelPlafond.Text = BudgetService.Nombre(budget);
        LabelSeuil.Text = BudgetService.Fcfa(seuil);
        LabelSeuilBarre.Text = $"80% ({BudgetService.Fcfa(seuil)})";
        LabelMax.Text = BudgetService.Fcfa(budget);
        LabelRythme.Text = $"~ {BudgetService.Nombre(Math.Round(budget / 31))} FCFA / jour";
    }

    private async void OnEnregistrerClicked(object? sender, EventArgs e)
    {
        if (!decimal.TryParse(EntryBudget.Text, out var budget) || budget <= 0)
        {
            await DisplayAlertAsync("Budget invalide", "Saisissez un montant supérieur à 0.", "OK");
            return;
        }

        _service.Budget = budget;
        _service.AlerteActive = RadioOui.IsChecked;
        await DisplayAlertAsync("Budget", "Votre budget mensuel a été enregistré.", "OK");
    }
}
