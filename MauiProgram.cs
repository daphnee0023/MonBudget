using Microsoft.Extensions.Logging;
using MonBudget.Data;
using MonBudget.Services;
using MonBudget.Views;

namespace MonBudget;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // --- Données et services (un seul exemplaire pour toute l'application) ---
        builder.Services.AddSingleton<BudgetDatabase>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<ICategorieService, CategorieService>();
        builder.Services.AddSingleton<IDepenseService, DepenseService>();
        builder.Services.AddSingleton<IBudgetService, BudgetService>();
        builder.Services.AddSingleton<INotificationService, NotificationService>();
        builder.Services.AddSingleton<IExportService, ExportService>();

        // --- Pages (créées par l'injection de dépendances) ---
        builder.Services.AddTransient<SplashPage>();
        builder.Services.AddTransient<ConnexionPage>();
        builder.Services.AddTransient<InscriptionPage>();
        builder.Services.AddTransient<AccueilPage>();
        builder.Services.AddTransient<NouvelleDepensePage>();
        builder.Services.AddTransient<CategoriesPage>();
        builder.Services.AddTransient<CategorieDetailPage>();
        builder.Services.AddTransient<NouvelleCategoriePage>();
        builder.Services.AddTransient<StatistiquesPage>();
        builder.Services.AddTransient<BudgetPage>();
        builder.Services.AddTransient<ProfilPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
