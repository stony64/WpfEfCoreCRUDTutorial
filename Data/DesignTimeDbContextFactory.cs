// Data/DesignTimeDbContextFactory.cs
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WpfEfCoreCRUDTutorial.Data;

/// <summary>
/// Design-Time-Factory für AppDbContext.
/// Wird nur von den EF-Core-Tools (Add-Migration, Update-Database) verwendet,
/// um zur Entwurfszeit einen DbContext mit den richtigen Optionen zu erzeugen.
/// Die Laufzeit-Konfiguration erfolgt weiterhin über den Generic Host in App.xaml.cs.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>
    /// Wird von den EF-Core-Tools aufgerufen, um zur Design-Time
    /// einen AppDbContext zu erzeugen.
    /// Wichtig: Diese Methode wird NICHT zur Laufzeit der WPF-Anwendung verwendet,
    /// sondern ausschließlich bei Befehlen wie Add-Migration oder Update-Database.
    /// </summary>
    /// <param name="args">
    /// Von den Tools übergebene Argumente (in diesem Beispiel nicht genutzt).
    /// </param>
    /// <returns>Neu erzeugter AppDbContext mit SQL-Server-Konfiguration.</returns>
    public AppDbContext CreateDbContext(string[] args)
    {
        // Basisverzeichnis ist das aktuelle Arbeitsverzeichnis der Tools (Projektordner).
        // Von hier aus wird appsettings.json gesucht.
        var basePath = Directory.GetCurrentDirectory();

        // Konfiguration (inkl. ConnectionStrings) aus appsettings.json laden.
        // Diese Konfiguration ist unabhängig vom Generic Host in App.xaml.cs.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // ConnectionString mit dem Namen "DefaultConnection" auslesen.
        // Der Name muss mit dem Eintrag in appsettings.json übereinstimmen.
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // DbContextOptions für AppDbContext mit SQL Server konfigurieren.
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        // AppDbContext mit den vorbereiteten Optionen zurückgeben.
        // EF Core nutzt diesen Kontext für Migrationen und Update-Database.
        return new AppDbContext(optionsBuilder.Options);
    }
}