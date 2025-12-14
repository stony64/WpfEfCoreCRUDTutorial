using System;
using System.ComponentModel.DataAnnotations;

namespace WpfEfCoreCRUDTutorial.Models;

/// <summary>
/// Domänen-Entität „Person“ für CRUD-Operationen mit EF Core.
/// Repräsentiert eine einzelne Person in der Anwendung und in der Tabelle "People".
/// </summary>
public class Person
{
    /// <summary>
    /// Primärschlüssel der Entität.
    /// EF Core erkennt die Property "Id" per Konvention automatisch als Key
    /// und legt in SQL Server standardmäßig eine Identity-Spalte (IDENTITY(1,1)) an,
    /// sodass der Wert beim Einfügen vom Datenbankserver generiert wird.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name der Person.
    /// - Pflichtfeld (Required), damit weder UI noch Datenbank leere Namen akzeptieren.
    /// - Mindestlänge 2 Zeichen zur einfachen Plausibilitätsprüfung.
    /// - Maximale Länge 100 Zeichen, was sowohl für die Spaltenbreite in der Datenbank
    ///   als auch für die Validierung im UI verwendet wird.
    /// Die Kombination aus Required + StringLength sorgt dafür, dass Fachregeln an einer
    /// zentralen Stelle (dem Modell) definiert werden und nicht mehrfach im UI wiederholt werden müssen.
    /// </summary>
    [Required(ErrorMessage = "Name ist erforderlich")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name: 2-100 Zeichen")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// E-Mail-Adresse der Person (optional).
    /// - Maximal 100 Zeichen, damit die Datenbankspalte bewusst begrenzt ist und keine überlangen Werte zulässt.
    /// - [EmailAddress] liefert eine einfache Formatprüfung (z.B. "@" enthalten),
    ///   die von UI-Frameworks oder manueller Validierung ausgewertet werden kann.
    /// Durch die Nullable-Deklaration (string?) ist im Code klar erkennbar,
    /// dass dieses Feld wirklich weggelassen werden darf.
    /// </summary>
    [StringLength(100)]
    [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse")]
    public string? Email { get; set; }

    /// <summary>
    /// Erstellungszeitpunkt (UTC) des Datensatzes.
    /// - Wird nicht von EF automatisch gefüllt, sondern explizit in der Service-Schicht
    ///   (z.B. im PersonService bei CreateAsync) mit DateTime.UtcNow gesetzt.
    /// - UTC-Zeit vermeidet Probleme mit Zeitzonen und Sommerzeit, besonders bei späteren Auswertungen.
    /// - Als non-nullable DateTime deklariert, damit jede Person zuverlässig einen Erstellungszeitpunkt besitzt.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
