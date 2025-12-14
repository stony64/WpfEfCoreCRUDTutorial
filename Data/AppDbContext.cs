using Microsoft.EntityFrameworkCore;
using WpfEfCoreCRUDTutorial.Models; // Domänen-Entitäten (z.B. Person)

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
    /// Über dieses DbSet werden alle Personen-Abfragen und Änderungen ausgeführt (Query, Add, Update, Remove),
    /// sodass der Rest der Anwendung nicht direkt mit SQL, sondern mit stark typisierten Objekten arbeitet.
    /// </summary>
    public DbSet<Person> People { get; set; } = null!;

    #endregion

    #region MODEL / SCHEMA CONFIGURATION

    /// <summary>
    /// Zentrales Modell-Mapping (Fluent API).
    /// Hier werden Tabellenname, Indizes und Längenbegrenzungen definiert,
    /// damit das Datenbankschema konsistent und bewusst gesteuert wird und nicht nur von Konventionen abhängt.
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
            //    Ein Index kann hier die Performance deutlich verbessern, ohne Eindeutigkeit zu erzwingen.
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
        });

        // GLOBAL CONFIG
        // Standardschema für alle Tabellen auf "dbo" festlegen.
        // In typischen MSSQL-Szenarien ist dbo das Default-Schema; hier wird es explizit gesetzt,
        // damit das Verhalten auch mit zukünftigen Änderungen (z.B. mehr Mandanten-Schemata) reproduzierbar bleibt.
        modelBuilder.HasDefaultSchema("dbo");

        // Basis-Implementierung aufrufen, falls EF Core intern noch Konfigurationen durchführen muss
        // (z.B. Konventionen oder Konfigurationen aus Basis-Klassen).
        base.OnModelCreating(modelBuilder);
    }

    #endregion
}
