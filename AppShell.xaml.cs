namespace MonBudget;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages ouvertes par-dessus les onglets
        Routing.RegisterRoute("NouvelleDepense", typeof(Views.NouvelleDepensePage));
        Routing.RegisterRoute("Profil", typeof(Views.ProfilPage));
        Routing.RegisterRoute("NouvelleCategorie", typeof(Views.NouvelleCategoriePage));
        Routing.RegisterRoute("CategorieDetail", typeof(Views.CategorieDetailPage));
    }
}
