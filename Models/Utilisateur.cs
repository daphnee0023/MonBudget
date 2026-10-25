using SQLite;

namespace MonBudget.Models;

public class Utilisateur
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Nom { get; set; } = "";

    [Indexed(Unique = true)]
    public string Contact { get; set; } = "";      // e-mail ou numéro de téléphone

    public string MotDePasseHash { get; set; } = "";
    public string Sel { get; set; } = "";
    public DateTime DateCreation { get; set; } = DateTime.Now;
}
