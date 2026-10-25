using System.Globalization;

namespace MonBudget;

/// <summary>Formatage des montants en FCFA (séparateur de milliers français).</summary>
public static class Format
{
    private static readonly CultureInfo Fr = new("fr-FR");

    public static string Nombre(decimal valeur) => valeur.ToString("N0", Fr);
    public static string Fcfa(decimal valeur) => Nombre(valeur) + " FCFA";
}
