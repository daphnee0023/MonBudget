using System.Security.Cryptography;
using System.Text;
using MonBudget.Data;
using MonBudget.Models;

namespace MonBudget.Services;

public class AuthService : IAuthService
{
    private const string CleSession = "session_utilisateur_id";
    private readonly BudgetDatabase _base;

    public AuthService(BudgetDatabase baseDeDonnees) => _base = baseDeDonnees;

    public Utilisateur? Courant { get; private set; }

    private static string Normaliser(string contact) => contact.Trim().ToLowerInvariant();

    private static string Hacher(string motDePasse, byte[] sel)
        => Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(motDePasse), sel, 100_000, HashAlgorithmName.SHA256, 32));

    public async Task<string?> InscrireAsync(string nom, string contact, string motDePasse)
    {
        var bd = await _base.ConnexionAsync();
        var identifiant = Normaliser(contact);

        var existant = await bd.Table<Utilisateur>().Where(u => u.Contact == identifiant).FirstOrDefaultAsync();
        if (existant is not null) return "Un compte existe déjà avec cet identifiant.";

        var sel = RandomNumberGenerator.GetBytes(16);
        var utilisateur = new Utilisateur
        {
            Nom = nom.Trim(),
            Contact = identifiant,
            Sel = Convert.ToBase64String(sel),
            MotDePasseHash = Hacher(motDePasse, sel)
        };
        await bd.InsertAsync(utilisateur);

        Courant = utilisateur;
        return null;
    }

    public async Task<bool> ConnecterAsync(string contact, string motDePasse, bool resterConnecte)
    {
        var bd = await _base.ConnexionAsync();
        var identifiant = Normaliser(contact);

        var utilisateur = await bd.Table<Utilisateur>().Where(u => u.Contact == identifiant).FirstOrDefaultAsync();
        if (utilisateur is null) return false;

        var attendu = Convert.FromBase64String(utilisateur.MotDePasseHash);
        var calcule = Convert.FromBase64String(Hacher(motDePasse, Convert.FromBase64String(utilisateur.Sel)));
        if (!CryptographicOperations.FixedTimeEquals(attendu, calcule)) return false;

        Courant = utilisateur;
        if (resterConnecte) Preferences.Set(CleSession, utilisateur.Id);
        else Preferences.Remove(CleSession);
        return true;
    }

    public async Task<bool> RestaurerSessionAsync()
    {
        if (Courant is not null) return true;

        var id = Preferences.Get(CleSession, 0);
        if (id == 0) return false;

        var bd = await _base.ConnexionAsync();
        Courant = await bd.FindAsync<Utilisateur>(id);
        return Courant is not null;
    }

    public void Deconnecter()
    {
        Courant = null;
        Preferences.Remove(CleSession);
    }
}
