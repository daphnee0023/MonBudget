namespace MonBudget.Models;

/// <summary>Chiffres clés d'un mois.</summary>
public class StatsMois
{
    public int Nombre { get; set; }
    public decimal Total { get; set; }
    public decimal MoyenneParJour { get; set; }
    public decimal PlusGrosseMontant { get; set; }
    public string PlusGrosseCategorie { get; set; } = "";
}
