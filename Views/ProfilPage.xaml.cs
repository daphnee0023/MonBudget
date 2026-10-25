using MonBudget.Services;

namespace MonBudget.Views;

public partial class ProfilPage : ContentPage
{
    private readonly IAuthService _auth;
    private readonly IBudgetService _budget;

    public ProfilPage(IAuthService auth, IBudgetService budget)
    {
        InitializeComponent();
        _auth = auth;
        _budget = budget;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var utilisateur = _auth.Courant;
        if (utilisateur is null)
        {
            await Shell.Current.GoToAsync("//Connexion");
            return;
        }

        LabelInitiales.Text = Initiales(utilisateur.Nom);
        LabelNom.Text = utilisateur.Nom;
        LabelContact.Text = utilisateur.Contact;
        LabelIdentifiant.Text = utilisateur.Contact;
        LabelMembre.Text = utilisateur.DateCreation.ToString("dd/MM/yyyy");

        var resume = await _budget.ResumeAsync();
        LabelBudget.Text = Format.Fcfa(resume.Budget);
        LabelDepense.Text = Format.Fcfa(resume.TotalDepense);
        LabelRestant.Text = Format.Fcfa(resume.Restant);
        LabelRestant.TextColor = Color.FromArgb(resume.Restant >= 0 ? "#43A047" : "#D32F2F");
    }

    // « Amadou Diallo » → « AD »
    private static string Initiales(string nom)
    {
        var mots = nom.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return mots.Length switch
        {
            0 => "?",
            1 => mots[0][..1].ToUpperInvariant(),
            _ => (mots[0][..1] + mots[1][..1]).ToUpperInvariant()
        };
    }

    private async void OnDeconnexionClicked(object? sender, EventArgs e)
    {
        var confirme = await DisplayAlertAsync("Déconnexion", "Voulez-vous vraiment vous déconnecter ?", "Se déconnecter", "Annuler");
        if (!confirme) return;

        _auth.Deconnecter();                       // oublie aussi « Se souvenir de moi »
        await Shell.Current.GoToAsync("//Connexion");
    }

    private async void OnRetourClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");
}
