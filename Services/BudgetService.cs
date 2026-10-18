using System.Globalization;
using MonBudget.Data;
using MonBudget.Models;

namespace MonBudget.Services;

/// <summary>
/// Service en mémoire (temporaire). Sera remplacé par une couche SQLite
/// en gardant les mêmes méthodes publiques.
/// </summary>
public class BudgetService
{
    public static BudgetService Instance { get; } = new();

    private static readonly CultureInfo Fr = new("fr-FR");
    private const decimal DepensesAnterieures = 48000m; // dépenses déjà faites avant celles de la liste
    private readonly Dictionary<string, (string Nom, string Mot)> _comptes = new();

    // ---------- Comptes ----------
    public string? NomUtilisateur { get; private set; }

    public bool Inscrire(string nom, string contact, string motDePasse)
    {
        var cle = contact.Trim().ToLowerInvariant();
        if (_comptes.ContainsKey(cle)) return false;
        _comptes[cle] = (nom.Trim(), motDePasse);
        NomUtilisateur = nom.Trim();
        return true;
    }

    public bool Connecter(string contact, string motDePasse)
    {
        var cle = contact.Trim().ToLowerInvariant();
        if (!_comptes.TryGetValue(cle, out var compte) || compte.Mot != motDePasse) return false;
        NomUtilisateur = compte.Nom;
        return true;
    }

    public bool AucunCompte => _comptes.Count == 0;

    // ---------- Budget ----------
    public decimal Budget { get; set; } = 150000m;
    public bool AlerteActive { get; set; } = true;
    public decimal Seuil => Budget * 0.8m;

    public List<Depense> Depenses { get; } = DonneesExemple.DernieresDepenses;

    public decimal TotalDepense => DepensesAnterieures + Depenses.Sum(d => d.Montant);
    public decimal Restant => Budget - TotalDepense;
    public bool SeuilAtteint => AlerteActive && TotalDepense >= Seuil;

    public double PartDepensee =>
        Budget <= 0 ? 0 : Math.Clamp((double)(TotalDepense / Budget), 0, 1);

    public void AjouterDepense(Depense depense) => Depenses.Insert(0, depense);

    // ---------- Format ----------
    public static string Fcfa(decimal valeur) => valeur.ToString("N0", Fr) + " FCFA";
    public static string Nombre(decimal valeur) => valeur.ToString("N0", Fr);
}