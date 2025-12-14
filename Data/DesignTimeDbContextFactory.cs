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
    public AppDbContext CreateDbContext(string[] args)
    {
        // Basisverzeichnis ist das aktuelle Arbeitsverzeichnis der Tools (Projektordner).
        var basePath = Directory.GetCurrentDirectory();

        // Konfiguration (inkl. ConnectionStrings) aus appsettings.json laden.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
