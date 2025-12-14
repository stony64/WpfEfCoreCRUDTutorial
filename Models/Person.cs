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
    /// EF Core erkennt "Id" automatisch als Key und erzeugt in MSSQL eine Identity-Spalte (IDENTITY(1,1)).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name der Person.
    /// - Pflichtfeld (Required)
    /// - Mindestlänge 2 Zeichen
    /// - Maximale Länge 100 Zeichen (wird sowohl im Modell als auch in der DB berücksichtigt).
    /// </summary>
    [Required(ErrorMessage = "Name ist erforderlich")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name: 2-100 Zeichen")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// E-Mail-Adresse der Person (optional).
    /// - Maximal 100 Zeichen
    /// - [EmailAddress] sorgt für grundlegende Formatvalidierung (z.B. in WPF/Forms über DataAnnotations).
    /// </summary>
    [StringLength(100)]
    [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse")]
    public string? Email { get; set; }

    /// <summary>
    /// Erstellungszeitpunkt (UTC) des Datensatzes.
    /// - Der Wert wird beim Anlegen der Entität in der Service-Schicht (PersonService) mit DateTime.UtcNow gesetzt.
    /// - In der Datenbank ist das Feld als NOT NULL definiert.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
