using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using WpfEfCoreCRUDTutorial.Data;
using WpfEfCoreCRUDTutorial.Services;
using WpfEfCoreCRUDTutorial.ViewModels;

namespace WpfEfCoreCRUDTutorial;

/// <summary>
/// WPF Application Bootstrapper (.NET 9).
/// Verantwortlich für:
/// - Aufbau des Generic Hosts (DI, Logging, Configuration)
/// - Registrierung von DbContext, Services und ViewModels
/// - Erzeugung und Start des MainWindow über DI
/// </summary>
public partial class App : Application
{
    #region GLOBAL SERVICES

    /// <summary>
    /// Globaler DI-Container.
    /// Ermöglicht bei Bedarf Zugriff auf registrierte Services
    /// außerhalb von Konstruktor-Injection.
    /// </summary>
    public static IServiceProvider? Services { get; private set; }

    #endregion

    #region WPF STARTUP

    /// <summary>
    /// Einstiegspunkt der WPF-Anwendung (entspricht Program.Main bei Konsolenanwendungen).
    /// Hier wird der Generic Host konfiguriert und das MainWindow gestartet.
    /// </summary>
    /// <param name="e">Startup-Argumente (z.B. Command-Line-Args).</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            #region 1. HOST BUILDER (.NET 9 Generic Host)

            // Erzeugt einen HostBuilder, der DI, Logging und Konfiguration verwaltet.
            var builder = Host.CreateApplicationBuilder();

            #endregion

            #region 2. CONFIGURATION LADEN

            // Lädt Einstellungen aus appsettings.json (z.B. ConnectionString, Logging).
            // optional: false → Datei MUSS vorhanden sein, sonst Fehler.
            builder.Configuration.AddJsonFile(
                "appsettings.json",
                optional: false,
                reloadOnChange: true); // Änderungen zur Laufzeit nachladen (v.a. im Development nützlich)

            #endregion

            #region 3. PRODUCTION LOGGING

            // Einfaches Console-Logging mit Mindestlevel Warning.
            // Ziel: Nur wichtige Meldungen im Produktivbetrieb loggen.
            builder.Services.AddLogging(logging =>
                logging.AddConsole()
                       .SetMinimumLevel(LogLevel.Warning));

            #endregion

            #region 4. EF CORE + RETRY POLICY

            // Registrierung des AppDbContext für EF Core.
            // UseSqlServer: Verbindung zur MSSQL-Datenbank über ConnectionString "DefaultConnection".
            // EnableRetryOnFailure: Automatische Wiederholungsversuche bei transienten DB-Fehlern.
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,                         // max. 3 Wiederholungsversuche
                        maxRetryDelay: TimeSpan.FromSeconds(10), // max. 10 Sekunden Verzögerung
                        errorNumbersToAdd: null)));              // Standard-Fehlercodes verwenden

            #endregion

            #region 4b. APPLICATION SERVICES & VIEWMODELS

            // Service-Schicht für Person-Operationen (CRUD auf Person über EF Core).
            builder.Services.AddScoped<PersonService>();

            // ViewModel für das MainWindow (enthält UI-Logik und Bindings).
            builder.Services.AddTransient<PersonViewModel>();

            // Registrierung des MainWindow selbst.
            // Der DI-Container injiziert automatisch das benötigte PersonViewModel in den Konstruktor.
            builder.Services.AddTransient<MainWindow>();

            #endregion

            #region 5. HOST STARTEN + MAINWINDOW ERZEUGEN

            // Host (inkl. DI-Container) aufbauen.
            var host = builder.Build();
            Services = host.Services;

            // MainWindow aus dem DI-Container beziehen.
            // Vorteil: Alle Abhängigkeiten (ViewModel, Services, DbContext) werden automatisch aufgelöst.
            var mainWindow = Services.GetRequiredService<MainWindow>();

            // WPF-Fenster anzeigen → ab hier übernimmt der WPF-Dispatcher den UI-Thread.
            mainWindow.Show();

            #endregion
        }
        catch (Exception ex)
        {
            #region 6. GRACEFUL ERROR HANDLING

            // Typische Fehler:
            // - appsettings.json fehlt oder ist fehlerhaft
            // - Datenbank nicht erreichbar / falscher ConnectionString
            // - DI-Konfiguration fehlerhaft
            MessageBox.Show(
                $"Startup fehlgeschlagen: {ex.Message}",
                "Kritischer Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // Sauber beenden, falls die Anwendung nicht korrekt initialisiert werden konnte.
            Shutdown();

            #endregion
        }
    }

    #endregion
}
