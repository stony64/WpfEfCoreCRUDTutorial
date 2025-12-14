// Models/Adress.cs
using System.ComponentModel.DataAnnotations;

namespace WpfEfCoreCRUDTutorial.Models;

/// <summary>
/// Domänen-Entität „Address“ für eine 1:n-Beziehung zu Person.
/// Eine Person kann mehrere Adressen haben, jede Adresse gehört genau zu einer Person.
/// Repräsentiert einen Datensatz in der Tabelle "Addresses".
/// </summary>
public class Address
{
    /// <summary>
    /// Primärschlüssel der Adresse.
    /// Wie bei Person erkennt EF Core die Property "Id" per Konvention als Key
    /// und legt in SQL Server standardmäßig eine Identity-Spalte an.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Fremdschlüssel auf die zugehörige Person.
    /// - Verbindet die Adresse mit genau einer Person (1:n-Beziehung).
    /// - EF Core nutzt diese Property, um die FK-Spalte "PersonId" in der Tabelle "Addresses" zu erzeugen.
    /// - In der DbContext-Konfiguration wird auf Basis von PersonId und Person eine
    ///   Fremdschlüsselbeziehung "Addresses.PersonId → People.Id" erzeugt,
    ///   sodass die Datenbank Referenzintegrität sicherstellt.
    /// </summary>
    public int PersonId { get; set; }

    /// <summary>
    /// Navigation Property zur verknüpften Person.
    /// - Erlaubt das Navigieren von Address → Person im Objektmodell.
    /// - In Kombination mit Person.Addresses entsteht die vollständige 1:n-Beziehung.
    /// - Wird beim Laden der Entität durch EF Core befüllt; die non-nullable-Deklaration
    ///   (mit null!-Initialisierung) drückt aus, dass diese Navigation zur Laufzeit
    ///   immer gesetzt sein soll, obwohl der Wert beim Konstruktoraufruf noch null ist.
    /// </summary>
    public Person Person { get; set; } = null!;

    /// <summary>
    /// Straßen- und Hausnummernangabe.
    /// - Pflichtfeld, damit Adressen nicht „leer“ in der Datenbank stehen.
    /// - Maximal 200 Zeichen, um die Spaltenbreite sinnvoll zu begrenzen.
    /// </summary>
    [Required(ErrorMessage = "Straße ist erforderlich")]
    [StringLength(200, ErrorMessage = "Straße: maximal 200 Zeichen")]
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// Postleitzahl.
    /// - Keine komplexe Fachlogik hinterlegt, aber auf 10 Zeichen begrenzt,
    ///   damit das Feld für internationale Formate ausreichend groß ist.
    /// </summary>
    [StringLength(10, ErrorMessage = "PLZ: maximal 10 Zeichen")]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Ort / Stadt.
    /// - Pflichtfeld, da eine Adresse ohne Ort in der Regel nicht sinnvoll ist.
    /// - Maximal 100 Zeichen, analog zu Person.Name.
    /// </summary>
    [Required(ErrorMessage = "Ort ist erforderlich")]
    [StringLength(100, ErrorMessage = "Ort: maximal 100 Zeichen")]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Land.
    /// - Optionales Feld mit maximal 100 Zeichen.
    /// - Ermöglicht einfache internationale Adressen,
    ///   ohne separate Country-Tabelle zu benötigen.
    /// </summary>
    [StringLength(100, ErrorMessage = "Land: maximal 100 Zeichen")]
    public string? Country { get; set; }

    /// <summary>
    /// Zeitstempel der Erstellung (UTC).
    /// - Wird analog zu Person.CreatedAt in der Service-Schicht (z.B. AddressService)
    ///   beim Anlegen mit DateTime.UtcNow gesetzt.
    /// - Non-nullable, damit jede Adresse einen nachvollziehbaren Erstellungszeitpunkt besitzt.
    /// - Alternativ kann der Wert in OnModelCreating über einen Datenbank-Default
    ///   (z.B. GETUTCDATE()) konfiguriert werden, wenn die Verantwortung beim Server liegen soll.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}