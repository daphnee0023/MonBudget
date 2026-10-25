using SQLite;

namespace MonBudget.Models;

public class Categorie
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Nom { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icone { get; set; } = "";          // nom de l'icône, sans extension
    public string CouleurHex { get; set; } = "#000066";

    /// <summary>0 = catégorie par défaut (commune), sinon identifiant de l'utilisateur qui l'a créée.</summary>
    public int UtilisateurId { get; set; }

    // --- Champs calculés pour l'affichage (non stockés en base) ---
    [Ignore] public decimal Montant { get; set; }
    [Ignore] public int Pourcentage { get; set; }
    [Ignore] public int NbDepenses { get; set; }

    [Ignore] public bool EstPersonnalisee => UtilisateurId != 0;
    [Ignore] public Color Couleur => Color.FromArgb(CouleurHex);
    [Ignore] public Color CouleurDouce => Color.FromArgb(CouleurHex).WithAlpha(0.15f);
    [Ignore] public string IconeImage => Icone.EndsWith(".png") ? Icone : Icone + ".png";
    [Ignore] public string PourcentageTexte => $"{Pourcentage}%";
    [Ignore] public string MontantTexte => Format.Fcfa(Montant);

    [Ignore]
    public string ResumeTexte => NbDepenses == 0
        ? "Aucune dépense"
        : $"{NbDepenses} dépense{(NbDepenses > 1 ? "s" : "")}";
}
