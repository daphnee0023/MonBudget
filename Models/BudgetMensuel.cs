using SQLite;

namespace MonBudget.Models;

public class BudgetMensuel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed] public int UtilisateurId { get; set; }
    public int Annee { get; set; }
    public int Mois { get; set; }

    public decimal Montant { get; set; }
    public bool AlerteActive { get; set; } = true;
}
