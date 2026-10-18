using System.Globalization;

namespace MonBudget.Models;

public class Depense
{
    public string Categorie { get; set; } = "";
    public string Icone { get; set; } = "";
    public string DateTexte { get; set; } = "";
    public decimal Montant { get; set; }
    public string Note { get; set; } = "";

    public string MontantTexte =>
        string.Format(new CultureInfo("fr-FR"), "-{0:N0} FCFA", Montant);
}
