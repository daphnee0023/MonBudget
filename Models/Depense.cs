using SQLite;

namespace MonBudget.Models;

public class Depense
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed] public int UtilisateurId { get; set; }
    [Indexed] public int CategorieId { get; set; }

    public decimal Montant { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string Note { get; set; } = "";

    // --- Champs calculés pour l'affichage (non stockés en base) ---
    [Ignore] public string Categorie { get; set; } = "";
    [Ignore] public string Icone { get; set; } = "";

    [Ignore] public string IconeImage => Icone.EndsWith(".png") ? Icone : Icone + ".png";

    [Ignore]
    public string DateTexte =>
        Date.Date == DateTime.Today ? "Aujourd'hui" :
        Date.Date == DateTime.Today.AddDays(-1) ? "Hier" :
        Date.ToString("dd/MM/yyyy");

    [Ignore] public string MontantTexte => "-" + Format.Fcfa(Montant);
}
