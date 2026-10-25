using System.Globalization;

namespace MonBudget.Models;

/// <summary>Total des dépenses d'un mois (pour le graphique d'évolution).</summary>
public class TotalMois
{
    public int Annee { get; set; }
    public int Mois { get; set; }
    public decimal Total { get; set; }

    public string Libelle
    {
        get
        {
            var texte = new DateTime(Annee, Mois, 1).ToString("MMM", new CultureInfo("fr-FR")).TrimEnd('.');
            return texte.Length == 0 ? texte : char.ToUpper(texte[0]) + texte[1..];
        }
    }
}
