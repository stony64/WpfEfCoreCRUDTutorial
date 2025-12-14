// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using WpfEfCoreCRUDTutorial.Models; // Domänen-Entitäten (z.B. Person, Address)

namespace WpfEfCoreCRUDTutorial.Data;

/// <summary>
/// Zentrale EF-Core-Datenbank-Brücke (DbContext).
/// Stellt die Verbindung zur MSSQL-Datenbank her und mappt C#-Entitäten auf Tabellen.
/// In dieser Anwendung existiert genau ein DbContext (AppDbContext) als Einstiegspunkt für alle Datenzugriffe.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    #region DBSets

    /// <summary>
    /// Repräsentiert die Tabelle "People" in der Datenbank.
    /// CRUD auf <see cref="Person"/> wird über dieses DbSet ausgeführt (Query, Add, Update, Remove),
    /// sodass auf Applikationsebene mit stark typisierten Objekten gearbeitet werden kann.
    /// </summary>
    public DbSet<Person> People { get; set; } = null!;

    /// <summary>
    /// Repräsentiert die Tabelle "Addresses" in der Datenbank.
    /// Dient zur Speicherung der 1:n-abhängigen Adressen einer Person.
    /// </summary>
    public DbSet<Address> Addresses { get; set; } = null!;

    #endregion DBSets

    #region MODEL / SCHEMA CONFIGURATION

    /// <summary>
    /// Zentrales Modell-Mapping (Fluent API).
    /// Hier werden Tabellenname, Indizes, Längenbegrenzungen, Beziehungen
    /// und Löschverhalten (z.B. Cascade Delete) definiert,
    /// damit das Datenbankschema bewusst gesteuert wird und nicht nur von Konventionen abhängt.
    /// Wird einmalig beim ersten Zugriff auf den Kontext aufgebaut.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PERSON TABLE CONFIGURATION
        modelBuilder.Entity<Person>(entity =>
        {
            // 1. Tabellennamen explizit festlegen.
            //    Vorteil: Du bist unabhängig von EF-Pluralisierungsregeln und späteren Klassenumbenennungen.
            //    Die Datenbank bleibt stabil, auch wenn du z.B. Person -> Customer im Code änderst.
            entity.ToTable("People");

            // 2. Nicht-uniquer Index auf Name für schnellere Suchen/Sortierungen nach Name.
            //    Typischer Use-Case: Listendarstellung nach Name sortiert oder Filterfunktionen im UI.
            //    Der Index erzwingt keine Eindeutigkeit, doppelte Namen sind weiterhin erlaubt.
            entity
                .HasIndex(e => e.Name)
                .HasDatabaseName("IX_People_Name");

            // 3. String-Längen konsistent zur DataAnnotation [StringLength(100)] festlegen.
            //    Gründe:
            //    - Verhindert unnötig breite Spalten (z.B. NVARCHAR(MAX)).
            //    - Vermeidet Truncation-Probleme, weil DB- und Modellbegrenzung übereinstimmen.
            //    - Macht das Schema „selbstdokumentierend“: Maximal 100 Zeichen sind erlaubt.
            entity.Property(e => e.Name)
                  .HasMaxLength(100);

            entity.Property(e => e.Email)
                  .HasMaxLength(100);

            // 4. 1:n-Beziehung Person → Addresses.
            //    Eine Person hat viele Adressen, jede Adresse gehört genau zu einer Person.
            //    Die Beziehung wird hier explizit beschrieben, obwohl EF sie auch per Konvention
            //    aus Person.Addresses, Address.Person und Address.PersonId ableiten könnte.
            //    OnDelete(DeleteBehavior.Cascade) stellt sicher, dass beim Löschen einer Person
            //    alle zugehörigen Adressen automatisch mitentfernt werden.
            entity
                .HasMany(p => p.Addresses)          // Navigation von Person zur Collection Address
                .WithOne(a => a.Person)             // Navigation von Address zurück zur Person
                .HasForeignKey(a => a.PersonId)     // Fremdschlüssel in der Address-Tabelle
                .OnDelete(DeleteBehavior.Cascade);  // Cascade Delete für abhängige Adressen
        });

        // ADDRESS TABLE CONFIGURATION
        modelBuilder.Entity<Address>(entity =>
        {
            // 1. Tabellennamen explizit festlegen.
            entity.ToTable("Addresses");

            // 2. String-Längen konsistent zu den DataAnnotations setzen.
            entity.Property(a => a.Street)
                  .HasMaxLength(200);

            entity.Property(a => a.PostalCode)
                  .HasMaxLength(10);

            entity.Property(a => a.City)
                  .HasMaxLength(100);

            entity.Property(a => a.Country)
                  .HasMaxLength(100);

            // 3. Index auf PersonId für schnellere Joins/Abfragen nach Person.
            //    Typisch z.B. beim Laden aller Adressen zu einer bestimmten Person.
            entity
                .HasIndex(a => a.PersonId)
                .HasDatabaseName("IX_Addresses_PersonId");
        });

        // GLOBAL CONFIG
        // Standardschema für alle Tabellen auf "dbo" festlegen.
        // In typischen MSSQL-Szenarien ist dbo das Default-Schema; hier wird es explizit gesetzt,
        // damit das Verhalten auch mit zukünftigen Änderungen (z.B. mehreren Schemata) reproduzierbar bleibt.
        modelBuilder.HasDefaultSchema("dbo");

        // Basis-Implementierung aufrufen, falls EF Core intern noch Konfigurationen durchführen muss
        // (z.B. Konventionen oder Konfigurationen aus Basis-Klassen).
        base.OnModelCreating(modelBuilder);
    }

    #endregion MODEL / SCHEMA CONFIGURATION
}