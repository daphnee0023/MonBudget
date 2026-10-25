using System.Globalization;
using System.Text;

namespace MonBudget.Services;

/// <summary>
/// Petit générateur de PDF sans dépendance (texte, rectangles, lignes).
/// Page A4 de 595 x 842 points. Les coordonnées Y se mesurent depuis le HAUT de la page.
/// </summary>
public class MiniPdf
{
    public const double LargeurPage = 595;
    public const double HauteurPage = 842;

    private readonly List<StringBuilder> _pages = new();
    private StringBuilder Courante => _pages[^1];

    public int NombrePages => _pages.Count;
    public void NouvellePage() => _pages.Add(new StringBuilder());

    private static string N(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Couleur((double r, double g, double b) c) => $"{N(c.r)} {N(c.g)} {N(c.b)}";

    public static (double r, double g, double b) Rgb(string hex)
    {
        hex = hex.TrimStart('#');
        return (Convert.ToInt32(hex[..2], 16) / 255.0,
                Convert.ToInt32(hex.Substring(2, 2), 16) / 255.0,
                Convert.ToInt32(hex.Substring(4, 2), 16) / 255.0);
    }

    public void Rectangle(double x, double yHaut, double largeur, double hauteur, (double r, double g, double b) couleur)
        => Courante.Append($"{Couleur(couleur)} rg {N(x)} {N(HauteurPage - yHaut - hauteur)} {N(largeur)} {N(hauteur)} re f\n");

    public void Ligne(double x1, double y1, double x2, double y2, (double r, double g, double b) couleur, double epaisseur = 0.5)
        => Courante.Append($"{Couleur(couleur)} RG {N(epaisseur)} w {N(x1)} {N(HauteurPage - y1)} m {N(x2)} {N(HauteurPage - y2)} l S\n");

    /// <summary>Écrit du texte. yBase = position de la ligne de base du texte.</summary>
    public void Texte(double x, double yBase, double taille, string texte, bool gras = false,
                      (double r, double g, double b)? couleur = null)
    {
        var c = couleur ?? (0.1, 0.1, 0.1);
        Courante.Append($"{Couleur(c)} rg BT /{(gras ? "F2" : "F1")} {N(taille)} Tf {N(x)} {N(HauteurPage - yBase)} Td ({Echapper(texte)}) Tj ET\n");
    }

    public void TexteDroite(double xDroite, double yBase, double taille, string texte, bool gras = false,
                            (double r, double g, double b)? couleur = null)
        => Texte(xDroite - LargeurTexte(texte, taille, gras), yBase, taille, texte, gras, couleur);

    /// <summary>Largeur approximative d'un texte en police Helvetica.</summary>
    public static double LargeurTexte(string texte, double taille, bool gras = false)
    {
        double total = 0;
        foreach (var c in Nettoyer(texte))
        {
            total += c switch
            {
                ' ' => 0.278,
                >= '0' and <= '9' => 0.556,
                >= 'A' and <= 'Z' => gras ? 0.72 : 0.68,
                'i' or 'l' or 'j' or '.' or ',' or ':' or ';' or '\'' or '!' or '|' => 0.25,
                'm' or 'w' => 0.8,
                _ => gras ? 0.58 : 0.52
            };
        }
        return total * taille;
    }

    // Remplace les caractères que la police standard ne sait pas afficher
    private static string Nettoyer(string texte)
    {
        var sb = new StringBuilder(texte.Length);
        foreach (var c in texte)
        {
            sb.Append(c switch
            {
                '\u202F' or '\u00A0' => ' ',
                '\u2019' or '\u2018' => '\'',
                '\u2013' or '\u2014' => '-',
                '\u2026' => '.',
                > '\u00FF' => '?',
                _ => c
            });
        }
        return sb.ToString();
    }

    private static string Echapper(string texte)
        => Nettoyer(texte).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    public byte[] Generer()
    {
        if (_pages.Count == 0) NouvellePage();

        var latin1 = Encoding.Latin1;
        using var flux = new MemoryStream();
        var positions = new List<long>();

        void Ecrire(string s) { var b = latin1.GetBytes(s); flux.Write(b, 0, b.Length); }
        void DebutObjet(int numero) { positions.Add(flux.Position); Ecrire($"{numero} 0 obj\n"); }

        Ecrire("%PDF-1.4\n");

        // 1 = catalogue, 2 = liste des pages, 3 et 4 = polices, puis (page, contenu) pour chaque page
        var n = _pages.Count;
        DebutObjet(1); Ecrire("<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
        var kids = string.Join(" ", Enumerable.Range(0, n).Select(i => $"{5 + 2 * i} 0 R"));
        DebutObjet(2); Ecrire($"<< /Type /Pages /Kids [{kids}] /Count {n} >>\nendobj\n");
        DebutObjet(3); Ecrire("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>\nendobj\n");
        DebutObjet(4); Ecrire("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>\nendobj\n");

        for (var i = 0; i < n; i++)
        {
            var contenu = latin1.GetBytes(_pages[i].ToString());

            DebutObjet(5 + 2 * i);
            Ecrire($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {6 + 2 * i} 0 R >>\nendobj\n");

            DebutObjet(6 + 2 * i);
            Ecrire($"<< /Length {contenu.Length} >>\nstream\n");
            flux.Write(contenu, 0, contenu.Length);
            Ecrire("\nendstream\nendobj\n");
        }

        var debutXref = flux.Position;
        var total = positions.Count + 1;
        Ecrire($"xref\n0 {total}\n0000000000 65535 f \n");
        foreach (var p in positions) Ecrire($"{p:D10} 00000 n \n");
        Ecrire($"trailer\n<< /Size {total} /Root 1 0 R >>\nstartxref\n{debutXref}\n%%EOF");

        return flux.ToArray();
    }
}
