using MonBudget.Models;

namespace MonBudget.Data;

// Données temporaires pour tester le front-end.
// Seront remplacées par SQLite + ViewModels.
public static class DonneesExemple
{
    public static List<Depense> DernieresDepenses => new()
    {
        new() { Categorie = "Alimentation", Icone = "ic_utensils_marine", DateTexte = "Aujourd'hui", Montant = 5000,  Note = "Repas midi" },
        new() { Categorie = "Transport",    Icone = "ic_bus_marine", DateTexte = "Hier",        Montant = 2000,  Note = "Trajet campus" },
        new() { Categorie = "Logement",     Icone = "ic_home_marine", DateTexte = "Lundi",       Montant = 30000, Note = "Charges loyer" },
    };

    public static List<Categorie> Categories => new()
    {
        new() { Nom = "Alimentation", Icone = "ic_utensils_marine", Description = "Courses, resto & repas",      Montant = 34000, Pourcentage = 40, CouleurHex = "#000066" },
        new() { Nom = "Transport",    Icone = "ic_bus_marine", Description = "Bus, taxi, carburant",         Montant = 21250, Pourcentage = 25, CouleurHex = "#4350B5" },
        new() { Nom = "Logement",     Icone = "ic_home_marine", Description = "Loyer, charges & électricité", Montant = 17000, Pourcentage = 20, CouleurHex = "#1AA0D8" },
        new() { Nom = "Loisirs",      Icone = "ic_gamepad_marine", Description = "Cinéma, sorties, jeux",        Montant = 12750, Pourcentage = 15, CouleurHex = "#43A047" },
        new() { Nom = "Santé",        Icone = "ic_pill_marine", Description = "Pharmacie, consultations" },
        new() { Nom = "Éducation",    Icone = "ic_book_marine", Description = "Livres, formations, scolarité" },
        new() { Nom = "Autres",       Icone = "ic_more_marine", Description = "Dépenses diverses" },
    };

    public static List<Categorie> CategoriesAvecDepenses => Categories.Where(c => c.Montant > 0).ToList();
}
