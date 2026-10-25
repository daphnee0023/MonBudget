using MonBudget.Models;

namespace MonBudget.Data;

/// <summary>Catégories créées automatiquement au premier lancement.</summary>
public static class DonneesExemple
{
    public static List<Categorie> Categories => new()
    {
        new() { Nom = "Alimentation", Description = "Courses, resto & repas",       Icone = "ic_utensils_marine", CouleurHex = "#000066" },
        new() { Nom = "Transport",    Description = "Bus, taxi, carburant",          Icone = "ic_bus_marine",      CouleurHex = "#4350B5" },
        new() { Nom = "Logement",     Description = "Loyer, charges & électricité",  Icone = "ic_home_marine",     CouleurHex = "#1AA0D8" },
        new() { Nom = "Loisirs",      Description = "Cinéma, sorties, jeux",         Icone = "ic_gamepad_marine",  CouleurHex = "#43A047" },
        new() { Nom = "Santé",        Description = "Pharmacie, consultations",      Icone = "ic_pill_marine",     CouleurHex = "#E57373" },
        new() { Nom = "Éducation",    Description = "Livres, formations, scolarité", Icone = "ic_book_marine",     CouleurHex = "#FFB300" },
        new() { Nom = "Autres",       Description = "Dépenses diverses",             Icone = "ic_more_marine",     CouleurHex = "#9AA0A6" },
    };
}
