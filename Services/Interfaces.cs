using MonBudget.Models;

namespace MonBudget.Services;

public interface IAuthService
{
    Utilisateur? Courant { get; }

    /// <summary>Crée un compte. Retourne null si tout va bien, sinon le message d'erreur.</summary>
    Task<string?> InscrireAsync(string nom, string contact, string motDePasse);

    Task<bool> ConnecterAsync(string contact, string motDePasse, bool resterConnecte);

    /// <summary>Reconnecte l'utilisateur si « Se souvenir de moi » avait été coché.</summary>
    Task<bool> RestaurerSessionAsync();

    void Deconnecter();
}

public interface ICategorieService
{
    /// <summary>Catégories par défaut + celles créées par l'utilisateur connecté.</summary>
    Task<List<Categorie>> ListerAsync();

    /// <summary>Toutes les catégories visibles, avec le montant et le nombre de dépenses du mois.</summary>
    Task<List<Categorie>> ListerAvecTotauxAsync(int annee, int mois);

    /// <summary>Catégories ayant des dépenses ce mois-là, triées par montant, avec pourcentage et couleur de rang.</summary>
    Task<List<Categorie>> TotauxDuMoisAsync(int annee, int mois);

    Task<Categorie?> ObtenirAsync(int id);

    /// <summary>Crée une catégorie. Retourne null si tout va bien, sinon le message d'erreur.</summary>
    Task<string?> AjouterAsync(string nom, string description, string icone, string couleurHex);

    /// <summary>Supprime une catégorie personnalisée sans dépenses. Retourne null si OK, sinon l'erreur.</summary>
    Task<string?> SupprimerAsync(int id);
}

public interface IDepenseService
{
    Task AjouterAsync(int categorieId, decimal montant, DateTime date, string note);
    Task SupprimerAsync(int id);

    Task<List<Depense>> RecentesAsync(int nombre);

    /// <summary>Dépenses (toutes périodes), éventuellement d'une seule catégorie, les plus récentes d'abord.</summary>
    Task<List<Depense>> ListerAsync(int? categorieId, int limite = 200);

    Task<List<Depense>> ListerDuMoisAsync(int annee, int mois);
    Task<decimal> TotalDuMoisAsync(int annee, int mois);

    Task<StatsMois> StatsDuMoisAsync(int annee, int mois);

    /// <summary>Totaux des N derniers mois, en terminant par le mois donné.</summary>
    Task<List<TotalMois>> EvolutionAsync(int annee, int mois, int nombreMois);
}

public interface IBudgetService
{
    /// <summary>Budget, dépenses, restant et seuil d'alerte du mois en cours.</summary>
    Task<ResumeBudget> ResumeAsync();

    /// <summary>Idem pour un mois donné.</summary>
    Task<ResumeBudget> ResumeAsync(int annee, int mois);

    Task EnregistrerAsync(decimal montant, bool alerteActive);
}

public interface INotificationService
{
    /// <summary>Affiche une notification système. Retourne false si elle n'a pas pu être envoyée.</summary>
    Task<bool> NotifierAsync(string titre, string message);
}

public interface IExportService
{
    /// <summary>Génère le rapport PDF du mois et retourne le chemin du fichier.</summary>
    Task<string> GenererRapportPdfAsync(int annee, int mois);

    /// <summary>Ouvre le PDF (Windows) ou le partage (Android).</summary>
    Task PartagerAsync(string chemin);
}
