using MonBudget.Services;

namespace MonBudget.Views;

public partial class InscriptionPage : ContentPage
{
    private static readonly Color Vert = Color.FromArgb("#43A047");
    private static readonly Color Gris = Color.FromArgb("#5F6368");
    private static readonly Color Rouge = Color.FromArgb("#D32F2F");

    private readonly IAuthService _auth;

    public InscriptionPage(IAuthService auth)
    {
        InitializeComponent();
        _auth = auth;
    }

    private void OnAfficherMotDePasseTapped(object? sender, TappedEventArgs e)
    {
        EntryMotDePasse.IsPassword = !EntryMotDePasse.IsPassword;
        IconeOeil.Source = EntryMotDePasse.IsPassword ? "ic_eye_gris.png" : "ic_eye_off_gris.png";
    }

    private static bool AUnChiffreOuSymbole(string mdp)
        => mdp.Any(c => char.IsDigit(c) || char.IsSymbol(c) || char.IsPunctuation(c));

    // Règles du mot de passe, mises à jour pendant la saisie
    private void OnMotDePasseChanged(object? sender, TextChangedEventArgs e)
    {
        var mdp = e.NewTextValue ?? "";
        MajRegle(IconeLongueur, RegleLongueur, "Au moins 8 caractères", mdp.Length >= 8);
        MajRegle(IconeChiffre, RegleChiffre, "Au moins un chiffre ou symbole", AUnChiffreOuSymbole(mdp));
        MajRegle(IconeMajuscule, RegleMajuscule, "Une lettre majuscule", mdp.Any(char.IsUpper));
        MajCorrespondance();
    }

    private void OnConfirmationChanged(object? sender, TextChangedEventArgs e) => MajCorrespondance();

    private void MajCorrespondance()
    {
        var confirmation = EntryConfirmation.Text ?? "";
        if (confirmation.Length == 0) { LabelCorrespondance.IsVisible = false; return; }

        var identiques = confirmation == (EntryMotDePasse.Text ?? "");
        LabelCorrespondance.IsVisible = true;
        LabelCorrespondance.Text = identiques ? "✓ Les mots de passe correspondent" : "✗ Ne correspondent pas";
        LabelCorrespondance.TextColor = identiques ? Vert : Rouge;
    }

    private static void MajRegle(Image icone, Label label, string texte, bool valide)
    {
        icone.Source = valide ? "ic_check_circle_vert.png" : "ic_circle_gris.png";
        label.Text = texte;
        label.TextColor = valide ? Vert : Gris;
    }

    private static bool ContactValide(string contact)
        => (contact.Contains('@') && contact.Contains('.'))
           || (contact.Length == 9 && contact.All(char.IsDigit));   // numéro camerounais 6XXXXXXXX

    private async void OnCreerCompteClicked(object? sender, EventArgs e)
    {
        var nom = EntryNom.Text?.Trim() ?? "";
        var contact = EntryContact.Text?.Trim() ?? "";
        var mdp = EntryMotDePasse.Text ?? "";
        var confirmation = EntryConfirmation.Text ?? "";

        string? erreur =
            nom.Length < 2 ? "Saisissez votre nom et prénom." :
            !ContactValide(contact) ? "Saisissez un e-mail valide ou un numéro à 9 chiffres (ex : 6XXXXXXXX)." :
            mdp.Length < 8 ? "Le mot de passe doit contenir au moins 8 caractères." :
            !AUnChiffreOuSymbole(mdp) ? "Le mot de passe doit contenir un chiffre ou un symbole." :
            !mdp.Any(char.IsUpper) ? "Le mot de passe doit contenir une majuscule." :
            mdp != confirmation ? "Les mots de passe ne correspondent pas." :
            !CheckCGU.IsChecked ? "Vous devez accepter les Conditions Générales." :
            null;

        if (erreur is null)
            erreur = await _auth.InscrireAsync(nom, contact, mdp);   // null = compte créé

        if (erreur is not null)
        {
            await DisplayAlertAsync("Inscription", erreur, "OK");
            return;
        }

        await Shell.Current.GoToAsync("//Main/Accueil");
    }

    private async void OnSeConnecterTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//Connexion");

    private async void OnRetourClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//Connexion");
}
