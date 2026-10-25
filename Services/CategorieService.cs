using MonBudget.Data;
using MonBudget.Models;

namespace MonBudget.Services;

public class CategorieService : ICategorieService
{
    // Couleurs du graphique, attribuées selon le rang de la catégorie
    private static readonly string[] Palette = { "#000066", "#4350B5", "#1AA0D8", "#43A047" };

    private readonly BudgetDatabase _base;
    private readonly IAuthService _auth;

    public CategorieService(BudgetDatabase baseDeDonnees, IAuthService auth)
    {
        _base = baseDeDonnees;
        _auth = auth;
    }

    public async Task<List<Categorie>> ListerAsync()
    {
        var bd = await _base.ConnexionAsync();
        var id = _auth.Courant?.Id ?? 0;

        return await bd.Table<Categorie>()
            .Where(c => c.UtilisateurId == 0 || c.UtilisateurId == id)
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    private async Task<List<Depense>> DepensesDuMoisAsync(int annee, int mois)
    {
        var utilisateur = _auth.Courant;
        if (utilisateur is null) return new();

        var bd = await _base.ConnexionAsync();
        var debut = new DateTime(annee, mois, 1);
        var fin = debut.AddMonths(1);
        var id = utilisateur.Id;

        return await bd.Table<Depense>()
            .Where(d => d.UtilisateurId == id && d.Date >= debut && d.Date < fin)
            .ToListAsync();
    }

    public async Task<List<Categorie>> ListerAvecTotauxAsync(int annee, int mois)
    {
        var categories = await ListerAsync();
        var depenses = await DepensesDuMoisAsync(annee, mois);

        foreach (var c in categories)
        {
            var deCetteCategorie = depenses.Where(d => d.CategorieId == c.Id).ToList();
            c.Montant = deCetteCategorie.Sum(d => d.Montant);
            c.NbDepenses = deCetteCategorie.Count;
        }
        return categories;
    }

    public async Task<List<Categorie>> TotauxDuMoisAsync(int annee, int mois)
    {
        var categories = await ListerAvecTotauxAsync(annee, mois);
        var avecDepenses = categories.Where(c => c.Montant > 0).OrderByDescending(c => c.Montant).ToList();
        var total = avecDepenses.Sum(c => c.Montant);
        if (total <= 0) return new();

        for (var i = 0; i < avecDepenses.Count; i++)
        {
            avecDepenses[i].Pourcentage = (int)Math.Round(avecDepenses[i].Montant / total * 100);
            avecDepenses[i].CouleurHex = Palette[i % Palette.Length];
        }
        return avecDepenses;
    }

    public async Task<Categorie?> ObtenirAsync(int id)
    {
        var bd = await _base.ConnexionAsync();
        return await bd.FindAsync<Categorie>(id);
    }

    public async Task<string?> AjouterAsync(string nom, string description, string icone, string couleurHex)
    {
        var utilisateur = _auth.Courant;
        if (utilisateur is null) return "Aucun utilisateur connecté.";

        nom = nom.Trim();
        if (nom.Length < 2) return "Le nom doit contenir au moins 2 caractères.";
        if (nom.Length > 30) return "Le nom ne peut pas dépasser 30 caractères.";
        if (string.IsNullOrWhiteSpace(icone)) return "Choisissez une icône.";
        if (string.IsNullOrWhiteSpace(couleurHex)) return "Choisissez une couleur.";

        var existantes = await ListerAsync();
        if (existantes.Any(c => string.Equals(c.Nom, nom, StringComparison.OrdinalIgnoreCase)))
            return "Une catégorie porte déjà ce nom.";

        var bd = await _base.ConnexionAsync();
        await bd.InsertAsync(new Categorie
        {
            Nom = nom,
            Description = description.Trim(),
            Icone = icone.Replace(".png", ""),
            CouleurHex = couleurHex,
            UtilisateurId = utilisateur.Id
        });
        return null;
    }

    public async Task<string?> SupprimerAsync(int id)
    {
        var utilisateur = _auth.Courant;
        if (utilisateur is null) return "Aucun utilisateur connecté.";

        var bd = await _base.ConnexionAsync();
        var categorie = await bd.FindAsync<Categorie>(id);
        if (categorie is null) return "Catégorie introuvable.";
        if (categorie.UtilisateurId != utilisateur.Id)
            return "Les catégories par défaut ne peuvent pas être supprimées.";

        var nb = await bd.Table<Depense>().Where(d => d.CategorieId == id).CountAsync();
        if (nb > 0) return "Cette catégorie contient des dépenses : supprimez-les d'abord.";

        await bd.DeleteAsync(categorie);
        return null;
    }
}
