using MonBudget.Services;

namespace MonBudget.Views;

public partial class BudgetPage : ContentPage
{
    private readonly IBudgetService _budget;

    public BudgetPage(IBudgetService budget)
    {
        InitializeComponent();
        _budget = budget;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var resume = await _budget.ResumeAsync();
        EntryBudget.Text = resume.Budget.ToString("0");
        RadioOui.IsChecked = resume.AlerteActive;
        RadioNon.IsChecked = !resume.AlerteActive;
        MajAffichage(resume.Budget);
    }

    private void OnBudgetChanged(object? sender, TextChangedEventArgs e)
    {
        if (decimal.TryParse(e.NewTextValue, out var budget) && budget > 0)
            MajAffichage(budget);
    }

    // Plafond, seuil (80 %) et rythme journalier selon le nombre de jours du mois
    private void MajAffichage(decimal budget)
    {
        var seuil = budget * 0.8m;
        var jours = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);

        LabelPlafond.Text = Format.Nombre(budget);
        LabelSeuil.Text = Format.Fcfa(seuil);
        LabelSeuilBarre.Text = $"80% ({Format.Fcfa(seuil)})";
        LabelMax.Text = Format.Fcfa(budget);
        LabelRythme.Text = $"~ {Format.Nombre(Math.Round(budget / jours))} FCFA / jour";
    }

    private async void OnEnregistrerClicked(object? sender, EventArgs e)
    {
        if (!decimal.TryParse(EntryBudget.Text, out var montant) || montant <= 0)
        {
            await DisplayAlertAsync("Budget invalide", "Saisissez un montant supérieur à 0.", "OK");
            return;
        }

        await _budget.EnregistrerAsync(montant, RadioOui.IsChecked);
        await DisplayAlertAsync("Budget", "Votre budget mensuel a été enregistré.", "OK");
    }
}
