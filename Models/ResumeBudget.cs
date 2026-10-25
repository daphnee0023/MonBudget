namespace MonBudget.Models;

/// <summary>Photographie du budget d'un mois (calculée, jamais stockée).</summary>
public class ResumeBudget
{
    public decimal Budget { get; set; }
    public decimal TotalDepense { get; set; }
    public bool AlerteActive { get; set; } = true;

    public decimal Restant => Budget - TotalDepense;
    public decimal Seuil => Budget * 0.8m;
    public bool Depasse => TotalDepense > Budget;

    public bool SeuilAtteint => AlerteActive && TotalDepense >= Seuil;
    public bool AlerteDepassement => AlerteActive && Depasse;

    public double PartDepensee =>
        Budget <= 0 ? 0 : Math.Clamp((double)(TotalDepense / Budget), 0, 1);
}
