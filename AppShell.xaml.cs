namespace MonBudget;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        // Page ouverte par-dessus les onglets (bouton "+")
        Routing.RegisterRoute("NouvelleDepense", typeof(Views.NouvelleDepensePage));
    }
}
