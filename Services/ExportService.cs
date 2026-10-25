using System.Globalization;
using MonBudget.Models;

namespace MonBudget.Services;

public class ExportService : IExportService
{
    private static readonly CultureInfo Fr = new("fr-FR");

    private static readonly (double, double, double) Marine = MiniPdf.Rgb("#000066");
    private static readonly (double, double, double) Gris = MiniPdf.Rgb("#5F6368");
    private static readonly (double, double, double) GrisClair = MiniPdf.Rgb("#F1F2F8");
    private static readonly (double, double, double) Blanc = (1, 1, 1);

    private readonly IAuthService _auth;
    private readonly IBudgetService _budget;
    private readonly ICategorieService _categories;
    private readonly IDepenseService _depenses;

    public ExportService(IAuthService auth, IBudgetService budget, ICategorieService categories, IDepenseService depenses)
    {
        _auth = auth;
        _budget = budget;
        _categories = categories;
        _depenses = depenses;
    }

    private static string Majuscule(string texte)
        => texte.Length == 0 ? texte : char.ToUpper(texte[0]) + texte[1..];

    private static string Tronquer(string texte, int max)
        => texte.Length <= max ? texte : texte[..(max - 1)] + ".";

    public async Task<string> GenererRapportPdfAsync(int annee, int mois)
    {
        var resume = await _budget.ResumeAsync(annee, mois);
        var categories = await _categories.TotauxDuMoisAsync(annee, mois);
        var depenses = (await _depenses.ListerDuMoisAsync(annee, mois)).OrderBy(d => d.Date).ThenBy(d => d.Id).ToList();
        var nomUtilisateur = _auth.Courant?.Nom ?? "";
        var libelleMois = Majuscule(new DateTime(annee, mois, 1).ToString("MMMM yyyy", Fr));

        var pdf = new MiniPdf();
        double y = 0;

        void NouvellePage()
        {
            pdf.NouvellePage();
            pdf.Ligne(40, 818, 555, 818, GrisClair, 0.8);
            pdf.Texte(40, 832, 8, "MonBudget - Rapport mensuel", false, Gris);
            pdf.TexteDroite(555, 832, 8, $"Page {pdf.NombrePages}", false, Gris);
        }

        // ---------- Page 1 : en-tête ----------
        NouvellePage();
        pdf.Rectangle(0, 0, MiniPdf.LargeurPage, 96, Marine);
        pdf.Texte(40, 46, 24, "MonBudget", true, Blanc);
        pdf.Texte(40, 72, 12, $"Rapport mensuel - {libelleMois}", false, Blanc);
        pdf.TexteDroite(555, 46, 11, nomUtilisateur, true, Blanc);
        pdf.TexteDroite(555, 72, 9, $"Généré le {DateTime.Now:dd/MM/yyyy HH:mm}", false, Blanc);

        // ---------- Résumé ----------
        y = 126;
        var titres = new[] { "Budget", "Dépensé", "Restant", "Utilisation" };
        var valeurs = new[]
        {
            Format.Fcfa(resume.Budget),
            Format.Fcfa(resume.TotalDepense),
            Format.Fcfa(resume.Restant),
            $"{Math.Round(resume.PartDepensee * 100)} %"
        };
        const double largeurCarte = 121.25;
        for (var i = 0; i < 4; i++)
        {
            var x = 40 + i * (largeurCarte + 10);
            pdf.Rectangle(x, y, largeurCarte, 54, GrisClair);
            pdf.Texte(x + 10, y + 20, 9, titres[i], false, Gris);
            pdf.Texte(x + 10, y + 40, 12, valeurs[i], true, Marine);
        }
        y += 84;

        // ---------- Répartition par catégorie ----------
        pdf.Texte(40, y, 14, "Répartition par catégorie", true, Marine);
        y += 12;

        if (categories.Count == 0)
        {
            y += 14;
            pdf.Texte(40, y, 10, "Aucune dépense ce mois-ci.", false, Gris);
            y += 16;
        }
        else
        {
            y += 8;
            foreach (var c in categories)
            {
                if (y > 790) { NouvellePage(); y = 50; }
                pdf.Texte(40, y + 8, 10, Tronquer(c.Nom, 24), false);
                pdf.Rectangle(190, y, 160, 8, GrisClair);
                pdf.Rectangle(190, y, Math.Max(2, 160 * c.Pourcentage / 100.0), 8, MiniPdf.Rgb(c.CouleurHex));
                pdf.TexteDroite(470, y + 8, 10, c.MontantTexte, true, Marine);
                pdf.TexteDroite(555, y + 8, 10, c.PourcentageTexte, false, Gris);
                y += 20;
            }
        }
        y += 22;

        // ---------- Détail des dépenses ----------
        void EnteteTableau()
        {
            pdf.Rectangle(40, y - 12, 515, 20, Marine);
            pdf.Texte(46, y + 2, 9, "Date", true, Blanc);
            pdf.Texte(112, y + 2, 9, "Catégorie", true, Blanc);
            pdf.Texte(230, y + 2, 9, "Note", true, Blanc);
            pdf.TexteDroite(549, y + 2, 9, "Montant", true, Blanc);
            y += 22;
        }

        if (y > 740) { NouvellePage(); y = 50; }
        pdf.Texte(40, y, 14, "Détail des dépenses", true, Marine);
        y += 24;

        if (depenses.Count == 0)
        {
            pdf.Texte(40, y, 10, "Aucune dépense enregistrée ce mois-ci.", false, Gris);
        }
        else
        {
            EnteteTableau();
            var zebre = false;
            foreach (var d in depenses)
            {
                if (y > 790) { NouvellePage(); y = 60; EnteteTableau(); zebre = false; }
                if (zebre) pdf.Rectangle(40, y - 11, 515, 18, GrisClair);
                pdf.Texte(46, y + 1, 9, d.Date.ToString("dd/MM/yyyy"), false);
                pdf.Texte(112, y + 1, 9, Tronquer(d.Categorie, 18), false);
                pdf.Texte(230, y + 1, 9, Tronquer(d.Note, 44), false, Gris);
                pdf.TexteDroite(549, y + 1, 9, Format.Fcfa(d.Montant), true, Marine);
                y += 18;
                zebre = !zebre;
            }

            if (y > 790) { NouvellePage(); y = 60; }
            pdf.Ligne(40, y - 8, 555, y - 8, Marine, 1);
            pdf.Texte(46, y + 8, 10, $"Total ({depenses.Count} dépense{(depenses.Count > 1 ? "s" : "")})", true, Marine);
            pdf.TexteDroite(549, y + 8, 10, Format.Fcfa(depenses.Sum(d => d.Montant)), true, Marine);
        }

        // ---------- Enregistrement ----------
#if WINDOWS
        var dossier = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MonBudget");
#else
        var dossier = FileSystem.CacheDirectory;
#endif
        Directory.CreateDirectory(dossier);
        var chemin = Path.Combine(dossier, $"MonBudget_{annee}-{mois:00}.pdf");
        await File.WriteAllBytesAsync(chemin, pdf.Generer());
        return chemin;
    }

    public async Task PartagerAsync(string chemin)
    {
#if WINDOWS
        await Launcher.Default.OpenAsync(new OpenFileRequest("Rapport MonBudget", new ReadOnlyFile(chemin)));
#else
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Rapport MonBudget",
            File = new ShareFile(chemin)
        });
#endif
    }
}
