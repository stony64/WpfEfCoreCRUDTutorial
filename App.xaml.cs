using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;
using WpfEfCoreCRUDTutorial.Data;
using WpfEfCoreCRUDTutorial.Services;
using WpfEfCoreCRUDTutorial.ViewModels;

namespace WpfEfCoreCRUDTutorial;

/// <summary>
/// WPF Application Bootstrapper (.NET 10).
/// Verantwortlich für:
/// - Aufbau des Generic Hosts (DI, Logging, Configuration)
/// - Registrierung von DbContext, Services, ViewModels und Fenstern
/// - Erzeugung und Start des MainWindow über DI
/// Durch die Verwendung des Generic Host verhält sich die WPF-App
/// infrastrukturell ähnlich wie eine ASP.NET-Core-Anwendung.
/// </summary>
public partial class App : Application
{
    #region GLOBAL SERVICES

    /// <summary>
    /// Globaler DI-Container.
    /// Ermöglicht bei Bedarf Zugriff auf registrierte Services
    /// außerhalb von Konstruktor-Injection (z.B. in speziellen Hilfsklassen oder Dialogen).
    /// Sollte sparsam verwendet werden, ist aber für ein Tutorial praktisch.
    /// </summary>
    public static IServiceProvider? Services { get; private set; }

    #endregion GLOBAL SERVICES

    #region WPF STARTUP

    /// <summary>
    /// Einstiegspunkt der WPF-Anwendung (entspricht Program.Main bei Konsolenanwendungen).
    /// Hier wird der Generic Host konfiguriert, alle Abhängigkeiten registriert
    /// und anschließend das MainWindow über den DI-Container gestartet.
    /// </summary>
    /// <param name="e">Startup-Argumente (z.B. Command-Line-Args).</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            #region 1. HOST BUILDER (Generic Host)

            // Erzeugt einen HostBuilder, der DI, Logging und Konfiguration verwaltet.
            // Dieser Ansatz entspricht dem modernen .NET-Hosting-Modell
            // und ermöglicht es, Konfiguration und Dienste an einer zentralen Stelle zu bündeln.
            var builder = Host.CreateApplicationBuilder();

            #endregion 1. HOST BUILDER (Generic Host)

            #region 2. CONFIGURATION LADEN

            // Lädt Einstellungen aus appsettings.json (z.B. ConnectionString, Logging-Konfiguration).
            // optional: false → Datei MUSS vorhanden sein; ansonsten wird beim Start eine Exception ausgelöst,
            // was frühzeitig auf fehlende Konfiguration hinweist.
            builder.Configuration.AddJsonFile(
                "appsettings.json",
                optional: false,
                reloadOnChange: true); // Änderungen zur Laufzeit nachladen (v.a. im Development nützlich)

            #endregion 2. CONFIGURATION LADEN

            #region 3. LOGGING

            // Einfaches Console-Logging mit Mindestlevel Warning.
            // Ziel: Nur wichtige Meldungen im „Produktiv“-Betrieb loggen und die Ausgabe übersichtlich halten.
            builder.Services.AddLogging(logging =>
                logging.AddConsole()
                       .SetMinimumLevel(LogLevel.Warning));

            #endregion 3. LOGGING

            #region 4. EF CORE + RETRY POLICY

            // Registrierung des AppDbContext für EF Core.
            // UseSqlServer: Anbindung an MSSQL über den ConnectionString "DefaultConnection" aus appsettings.json.
            // EnableRetryOnFailure: Automatische Wiederholungsversuche bei transienten DB-Fehlern
            // (z.B. kurze Netzwerkunterbrechungen, Timeout), um die Robustheit zu erhöhen.
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,                         // max. 3 Wiederholungsversuche
                        maxRetryDelay: TimeSpan.FromSeconds(10), // max. 10 Sekunden Verzögerung
                        errorNumbersToAdd: null)));              // Standard-Fehlercodes verwenden

            #endregion 4. EF CORE + RETRY POLICY

            #region 5. APPLICATION SERVICES

            // Service-Schicht für Person- und Adress-Operationen (CRUD auf Person/Address über EF Core).
            // Scoped: Pro Scope (hier typischerweise pro WPF-App-Lebensdauer) eine Instanz,
            // passend zur Lebensdauer des DbContext.
            builder.Services.AddScoped<PersonService>();

            #endregion 5. APPLICATION SERVICES

            #region 6. VIEWMODELS REGISTRIEREN

            // ViewModel für Personen (Master).
            // Singleton: Eine Instanz pro App-Lebensdauer, sodass Auswahl/Zustand
            // im gesamten UI konsistent bleibt.
            builder.Services.AddSingleton<PersonViewModel>();

            // ViewModel für Adressen (Detail).
            // Ebenfalls Singleton, damit MainWindow und UserDetailsWindow
            // dasselbe AddressViewModel teilen und die aktuell ausgewählte Person
            // samt Adressen übergeben werden kann.
            builder.Services.AddSingleton<AddressViewModel>();

            // Zentrales MainViewModel, das PersonViewModel und AddressViewModel zusammenspielt.
            // Singleton, da es den globalen UI-Zustand koordiniert.
            builder.Services.AddSingleton<MainViewModel>();

            #endregion 6. VIEWMODELS REGISTRIEREN

            #region 7. WINDOWS REGISTRIEREN

            // Registrierung des MainWindow selbst.
            // Der DI-Container injiziert automatisch das benötigte MainViewModel in den Konstruktor.
            // Singleton, weil es genau ein Hauptfenster geben soll.
            builder.Services.AddSingleton<MainWindow>();

            // Registrierung des Detailfensters für Benutzerdaten (Adressen).
            // Der DI-Container injiziert automatisch das AddressViewModel in UserDetailsWindow.
            // Ebenfalls Singleton, damit immer dieselbe Instanz mit demselben AddressViewModel
            // verwendet wird, solange die App läuft.
            builder.Services.AddTransient<PersonAddressDetailsWindow>();

            #endregion 7. WINDOWS REGISTRIEREN

            #region 8. HOST STARTEN + MAINWINDOW ERZEUGEN

            // Host (inkl. DI-Container) aufbauen.
            var host = builder.Build();
            Services = host.Services;

            // MainWindow aus dem DI-Container beziehen.
            // Vorteil: Alle Abhängigkeiten (MainViewModel, weitere ViewModels, Services, DbContext)
            // werden automatisch aufgelöst und sind an einer zentralen Stelle konfiguriert.
            var mainWindow = Services.GetRequiredService<MainWindow>();

            // Zentrales MainViewModel aus dem DataContext holen
            // (wird im MainWindow-Konstruktor gesetzt) und initialisieren.
            if (mainWindow.DataContext is MainViewModel mainViewModel)
            {
                // Initiale Daten laden (Personenliste, ggf. erste Auswahl).
                _ = mainViewModel.InitializeAsync();
            }

            // WPF-Fenster anzeigen → ab hier übernimmt der WPF-Dispatcher die Steuerung des UI-Threads.
            mainWindow.Show();

            #endregion 8. HOST STARTEN + MAINWINDOW ERZEUGEN
        }
        catch (Exception ex)
        {
            #region 9. GRACEFUL ERROR HANDLING

            // Typische Fehlerquellen:
            // - appsettings.json fehlt oder ist syntaktisch fehlerhaft
            // - Datenbank nicht erreichbar / falscher ConnectionString
            // - DI-Konfiguration fehlerhaft (z.B. fehlende Registrierung)
            // Durch eine MessageBox erhält der Benutzer eine verständliche Fehlermeldung,
            // statt dass die Anwendung kommentarlos abstürzt.
            MessageBox.Show(
                $"Startup fehlgeschlagen: {ex.Message}",
                "Kritischer Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // Sauber beenden, falls die Anwendung nicht korrekt initialisiert werden konnte.
            Shutdown();

            #endregion 9. GRACEFUL ERROR HANDLING
        }
    }

    #endregion WPF STARTUP
}