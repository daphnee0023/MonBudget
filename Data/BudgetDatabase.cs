using SQLite;
using MonBudget.Models;

namespace MonBudget.Data;

/// <summary>
/// Ouvre la base SQLite (fichier monbudget.db3), crée les tables et les
/// catégories par défaut au premier lancement.
/// </summary>
public class BudgetDatabase
{
    private SQLiteAsyncConnection? _connexion;
    private readonly SemaphoreSlim _verrou = new(1, 1);

    public static string Chemin => Path.Combine(FileSystem.AppDataDirectory, "monbudget.db3");

    public async Task<SQLiteAsyncConnection> ConnexionAsync()
    {
        if (_connexion is not null) return _connexion;

        await _verrou.WaitAsync();
        try
        {
            if (_connexion is not null) return _connexion;

            var connexion = new SQLiteAsyncConnection(Chemin,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            await connexion.CreateTableAsync<Utilisateur>();
            await connexion.CreateTableAsync<Categorie>();
            await connexion.CreateTableAsync<Depense>();
            await connexion.CreateTableAsync<BudgetMensuel>();

            // Mise à jour d'une ancienne base : la colonne UtilisateurId a été ajoutée après coup
            // (valeur vide pour les catégories existantes = catégories par défaut).
            await connexion.ExecuteAsync("UPDATE Categorie SET UtilisateurId = 0 WHERE UtilisateurId IS NULL");

            if (await connexion.Table<Categorie>().CountAsync() == 0)
                await connexion.InsertAllAsync(DonneesExemple.Categories);

            _connexion = connexion;
            return connexion;
        }
        finally
        {
            _verrou.Release();
        }
    }
}
