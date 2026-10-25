using MonBudget.Data;
using MonBudget.Models;

namespace MonBudget.Services;

public class BudgetService : IBudgetService
{
    private const decimal BudgetParDefaut = 150000m;

    private readonly BudgetDatabase _base;
    private readonly IAuthService _auth;
    private readonly IDepenseService _depenses;

    public BudgetService(BudgetDatabase baseDeDonnees, IAuthService auth, IDepenseService depenses)
    {
        _base = baseDeDonnees;
        _auth = auth;
        _depenses = depenses;
    }

    private async Task<BudgetMensuel?> BudgetExactAsync(int utilisateurId, int annee, int mois)
    {
        var bd = await _base.ConnexionAsync();
        return await bd.Table<BudgetMensuel>()
            .Where(b => b.UtilisateurId == utilisateurId && b.Annee == annee && b.Mois == mois)
            .FirstOrDefaultAsync();
    }

    // Budget du mois, ou à défaut le dernier budget défini avant ce mois
    private async Task<BudgetMensuel?> BudgetPourAsync(int utilisateurId, int annee, int mois)
    {
        var exact = await BudgetExactAsync(utilisateurId, annee, mois);
        if (exact is not null) return exact;

        var bd = await _base.ConnexionAsync();
        var cle = annee * 100 + mois;
        var tous = await bd.Table<BudgetMensuel>().Where(b => b.UtilisateurId == utilisateurId).ToListAsync();

        return tous
            .Where(b => b.Annee * 100 + b.Mois < cle)
            .OrderByDescending(b => b.Annee * 100 + b.Mois)
            .FirstOrDefault();
    }

    public Task<ResumeBudget> ResumeAsync() => ResumeAsync(DateTime.Today.Year, DateTime.Today.Month);

    public async Task<ResumeBudget> ResumeAsync(int annee, int mois)
    {
        var utilisateur = _auth.Courant;
        if (utilisateur is null) return new ResumeBudget { Budget = BudgetParDefaut };

        var budget = await BudgetPourAsync(utilisateur.Id, annee, mois);
        var total = await _depenses.TotalDuMoisAsync(annee, mois);

        return new ResumeBudget
        {
            Budget = budget?.Montant ?? BudgetParDefaut,
            AlerteActive = budget?.AlerteActive ?? true,
            TotalDepense = total
        };
    }

    public async Task EnregistrerAsync(decimal montant, bool alerteActive)
    {
        var utilisateur = _auth.Courant ?? throw new InvalidOperationException("Aucun utilisateur connecté.");
        var maintenant = DateTime.Today;
        var bd = await _base.ConnexionAsync();

        var existant = await BudgetExactAsync(utilisateur.Id, maintenant.Year, maintenant.Month);
        if (existant is null)
        {
            await bd.InsertAsync(new BudgetMensuel
            {
                UtilisateurId = utilisateur.Id,
                Annee = maintenant.Year,
                Mois = maintenant.Month,
                Montant = montant,
                AlerteActive = alerteActive
            });
        }
        else
        {
            existant.Montant = montant;
            existant.AlerteActive = alerteActive;
            await bd.UpdateAsync(existant);
        }
    }
}
