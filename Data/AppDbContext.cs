using Microsoft.EntityFrameworkCore;
using WpfEfCoreCRUDTutorial.Models; // Domänen-Entitäten (z.B. Person)

namespace WpfEfCoreCRUDTutorial.Data;

/// <summary>
/// Zentrale EF Core Datenbank-Brücke (DbContext).
/// Stellt die Verbindung zur MSSQL-Datenbank her und mappt C#-Entitäten auf Tabellen.
/// In dieser Anwendung existiert genau ein DbContext (AppDbContext).
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    #region DBSets

    /// <summary>
    /// Repräsentiert die Tabelle "People" in der Datenbank.
    /// CRUD auf <see cref="Person"/> wird über dieses DbSet ausgeführt (Query, Add, Update, Remove).
    /// </summary>
    public DbSet<Person> People { get; set; } = null!;

    #endregion

    #region MODEL / SCHEMA CONFIGURATION

    /// <summary>
    /// Zentrales Modell-Mapping (Fluent API).
    /// Hier werden Tabellenname, Indizes und Längenbegrenzungen definiert.
    /// Wird einmalig beim ersten Zugriff auf den Kontext aufgebaut.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PERSON TABLE CONFIGURATION
        modelBuilder.Entity<Person>(entity =>
        {
            // 1. Tabellennamen explizit festlegen
            //    Verhindert unerwünschte Pluralisierung oder spätere Umbenennungsprobleme.
            entity.ToTable("People");

            // 2. Nicht-uniquer Index auf Name für schnellere Suchen/Sortierungen nach Name.
            entity
                .HasIndex(e => e.Name)
                .HasDatabaseName("IX_People_Name");

            // 3. String-Längen konsistent zur DataAnnotation [StringLength(100)]
            //    → Verhindert zu breite Spalten und potenzielle SQL-Truncation.
            entity.Property(e => e.Name)
                  .HasMaxLength(100);

            entity.Property(e => e.Email)
                  .HasMaxLength(100);
        });

        // GLOBAL CONFIG
        // Standard-Schema für alle Tabellen auf "dbo" festlegen.
        // In typischen MSSQL-Szenarien ist dbo das Standardschema.
        modelBuilder.HasDefaultSchema("dbo");

        // Basis-Implementierung aufrufen, falls EF Core intern noch Konfigurationen durchführen muss.
        base.OnModelCreating(modelBuilder);
    }

    #endregion
}
