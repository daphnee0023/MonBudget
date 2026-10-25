using MonBudget.Data;
using MonBudget.Models;

namespace MonBudget.Services;

public class DepenseService : IDepenseService
{
    private readonly BudgetDatabase _base;
    private readonly IAuthService _auth;

    public DepenseService(BudgetDatabase baseDeDonnees, IAuthService auth)
    {
        _base = baseDeDonnees;
        _auth = auth;
    }

    // Complète chaque dépense avec le nom et l'icône de sa catégorie
    private async Task EnrichirAsync(List<Depense> depenses)
    {
        var bd = await _base.ConnexionAsync();
        var categories = (await bd.Table<Categorie>().ToListAsync()).ToDictionary(c => c.Id);

        foreach (var d in depenses)
        {
            if (!categories.TryGetValue(d.CategorieId, out var c)) continue;
            d.Categorie = c.Nom;
            d.Icone = c.Icone;
        }
    }

    public async Task AjouterAsync(int categorieId, decimal montant, DateTime date, string note)
    {
        var utilisateur = _auth.Courant ?? throw new InvalidOperationException("Aucun utilisateur connecté.");
        var bd = await _base.ConnexionAsync();

        await bd.InsertAsync(new Depense
        {
            UtilisateurId = utilisateur.Id,
            CategorieId = categorieId,
            Montant = montant,
            Date = date.Date,
            Note = note
        });
    }

    public async Task SupprimerAsync(int id)
    {
        var utilisateur = _auth.Courant;
        if (utilisateur is null) return;

        var bd = await _base.ConnexionAsync();
        var depense = await bd.FindAsync<Depense>(id);
        if (depense is not null && depense.UtilisateurId == utilisateur.Id)
            await bd.DeleteAsync(depense);
    }

    public async Task<List<Depense>> RecentesAsync(int nombre)
    {
        var utilisateur = _auth.Courant;
        if (utilisateur is null) return new();

        var bd = await _base.ConnexionAsync();
        var id = utilisateur.Id;

        var depenses = await bd.Table<Depense>()
            .Where(d => d.UtilisateurId == id)
            .OrderByDescending(d => d.Date)
            .ThenByDescending(d => d.Id)
            .Take(nombre)
            .ToListAsync();

        await EnrichirAsync(depenses);
        return depenses;
    }

    public async Task<List<Depense>> ListerAsync(int? categorieId, int limite = 200)
    {
        var utilisateur = _auth.Courant;
        if (utilisateur is null) return new();

        var bd = await _base.ConnexionAsync();
        var id = utilisateur.Id;

        var toutes = await bd.Table<Depense>()
            .Where(d => d.UtilisateurId == id)
            .OrderByDescending(d => d.Date)
            .ThenByDescending(d => d.Id)
            .ToListAsync();

        var depenses = toutes
            .Where(d => categorieId is null || d.CategorieId == categorieId)
            .Take(limite)
            .ToList();

        await EnrichirAsync(depenses);
        return depenses;
    }

    public async Task<List<Depense>> ListerDuMoisAsync(int annee, int mois)
    {
        var utilisateur = _auth.Courant;
        if (utilisateur is null) return new();

        var bd = await _base.ConnexionAsync();
        var debut = new DateTime(annee, mois, 1);
        var fin = debut.AddMonths(1);
        var id = utilisateur.Id;

        var depenses = await bd.Table<Depense>()
            .Where(d => d.UtilisateurId == id && d.Date >= debut && d.Date < fin)
            .OrderByDescending(d => d.Date)
            .ThenByDescending(d => d.Id)
            .ToListAsync();

        await EnrichirAsync(depenses);
        return depenses;
    }

    public async Task<decimal> TotalDuMoisAsync(int annee, int mois)
        => (await ListerDuMoisAsync(annee, mois)).Sum(d => d.Montant);

    public async Task<StatsMois> StatsDuMoisAsync(int annee, int mois)
    {
        var depenses = await ListerDuMoisAsync(annee, mois);
        var stats = new StatsMois { Nombre = depenses.Count, Total = depenses.Sum(d => d.Montant) };

        // Jours écoulés : tout le mois s'il est terminé, sinon jusqu'à aujourd'hui
        var debut = new DateTime(annee, mois, 1);
        var aujourdhui = DateTime.Today;
        int jours =
            debut > aujourdhui ? 1 :
            (aujourdhui.Year == annee && aujourdhui.Month == mois) ? aujourdhui.Day :
            DateTime.DaysInMonth(annee, mois);

        stats.MoyenneParJour = Math.Round(stats.Total / jours);

        var plusGrosse = depenses.OrderByDescending(d => d.Montant).FirstOrDefault();
        if (plusGrosse is not null)
        {
            stats.PlusGrosseMontant = plusGrosse.Montant;
            stats.PlusGrosseCategorie = plusGrosse.Categorie;
        }
        return stats;
    }

    public async Task<List<TotalMois>> EvolutionAsync(int annee, int mois, int nombreMois)
    {
        var resultat = new List<TotalMois>();
        var utilisateur = _auth.Courant;
        var dernier = new DateTime(annee, mois, 1);

        for (var i = nombreMois - 1; i >= 0; i--)
        {
            var m = dernier.AddMonths(-i);
            resultat.Add(new TotalMois { Annee = m.Year, Mois = m.Month });
        }
        if (utilisateur is null) return resultat;

        var bd = await _base.ConnexionAsync();
        var debut = dernier.AddMonths(-(nombreMois - 1));
        var fin = dernier.AddMonths(1);
        var id = utilisateur.Id;

        var depenses = await bd.Table<Depense>()
            .Where(d => d.UtilisateurId == id && d.Date >= debut && d.Date < fin)
            .ToListAsync();

        foreach (var t in resultat)
            t.Total = depenses.Where(d => d.Date.Year == t.Annee && d.Date.Month == t.Mois).Sum(d => d.Montant);

        return resultat;
    }
}
