using System.Globalization;

namespace MonBudget.Models;

public class Categorie
{
    public string Nom { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icone { get; set; } = "";
    public decimal Montant { get; set; }
    public int Pourcentage { get; set; }
    public string CouleurHex { get; set; } = "#000066";

    public Color Couleur => Color.FromArgb(CouleurHex);
    public string PourcentageTexte => $"{Pourcentage}%";
    public string MontantTexte =>
        string.Format(new CultureInfo("fr-FR"), "{0:N0} FCFA", Montant);
}
