# WpfEfCoreCRUDTutorial

# 1. WPF‑Projekt in Visual Studio anlegen (.NET 10)

In diesem Abschnitt erstellst du das Grundgerüst deiner Anwendung: ein leeres WPF‑Projekt auf Basis von .NET 10. Dieses Projekt wird später um EF Core, MVVM und eine SQL‑Server‑Datenbank erweitert.

1. Starte Visual Studio 2026.
2. Wähle im Startfenster „Neues Projekt erstellen“.
3. Suche nach dem Projekttyp **„WPF-App (.NET)“** und wähle ihn aus.
4. Vergib folgende Einstellungen:
   - **Projektname**: `WpfEfCoreCRUDTutorial`
   - **Speicherort**: Ein Ordner deiner Wahl
   - **Lösungsname**: (kann gleich dem Projektnamen sein)
5. Klicke auf „Weiter“ und wähle als **Ziel-Framework**: `.NET 10.0`.
6. Erstelle das Projekt und führe einen ersten Start aus (F5), um zu prüfen, dass das leere WPF‑Fenster angezeigt wird.

Zu diesem Zeitpunkt existiert nur ein Standard‑`MainWindow` ohne Datenbankanbindung oder MVVM‑Struktur. Im nächsten Schritt bereitest du die Ordnerstruktur vor, die später für eine saubere Schichtentrennung sorgt.

---

# 2. Ordnerstruktur für MVVM vorbereiten

Damit dein Projekt von Anfang an übersichtlich bleibt, legst du eine einfache, aber skalierbare Ordnerstruktur an. Diese Struktur trennt Domänenlogik, Datenzugriff, Services, ViewModels, Views und Commands.

Öffne im Projektmappen-Explorer dein WPF‑Projekt `WpfEfCoreCRUDTutorial` und erstelle unterhalb des Projekts folgende Ordner:

- `Data`
- `Models`
- `Services`
- `ViewModels`
- `Views` (optional; das vorhandene `MainWindow` kannst du später dorthin verschieben)
- `Commands`

Die Rollen der Ordner sind:

- **Models**  
  Enthält die Domänenklassen wie `Person`. Diese Klassen repräsentieren die Daten, die gespeichert und im UI angezeigt werden. [web:300]

- **Data**  
  Beinhaltet den Entity‑Framework‑Core‑Kontext (`AppDbContext`). Der DbContext kennt die Entitäten aus `Models` und übernimmt das Mapping zur Datenbank. [web:300]

- **Services**  
  Enthält Serviceklassen wie `PersonService`, die den DbContext kapseln und Methoden wie `GetAllAsync`, `CreateAsync`, `UpdateAsync` und `DeleteAsync` bereitstellen. ViewModels sprechen nur mit Services, nicht direkt mit EF Core. [web:300]

- **ViewModels**  
  Hier liegen MVVM‑ViewModels wie `PersonViewModel`. Sie stellen Properties und Commands für die Oberfläche bereit und enthalten UI‑nahe Logik. [web:354]

- **Views**  
  Hier liegen die XAML‑Views (`MainWindow.xaml` und weitere Fenster/Views), die per DataBinding an die ViewModels gebunden werden. [web:354]

- **Commands**  
  Enthält wiederverwendbare Command‑Implementierungen wie `AsyncRelayCommand` und das Interface `IAsyncCommand`, die von mehreren ViewModels genutzt werden können. [web:438]

Mit dieser Struktur ist das Projekt bereit für den nächsten Schritt: das Hinzufügen der benötigten NuGet‑Pakete für Entity Framework Core 10, den Generic Host und die Konfiguration über `appsettings.json`.

---

# 3. NuGet‑Pakete für EF Core 10 und Infrastruktur installieren

Damit deine WPF‑App mit einer SQL‑Server‑Datenbank sprechen und moderne .NET‑10‑Infrastruktur (Generic Host, DI, Konfiguration) nutzen kann, installierst du nun die passenden NuGet‑Pakete.

Öffne im Projektmappen-Explorer das Kontextmenü des Projekts `WpfEfCoreCRUDTutorial` und wähle:

- „**NuGet-Pakete verwalten…**“

Wechsle auf den Reiter **„Durchsuchen“** und installiere nacheinander folgende Pakete:

1. **Microsoft.EntityFrameworkCore.SqlServer**  
   Dieses Paket enthält den SQL‑Server‑Provider für Entity Framework Core 10 und bringt den EF‑Kern gleich mit. Damit kannst du via `UseSqlServer(...)` auf eine MSSQL‑Datenbank (z.B. LocalDB) zugreifen. [web:300]

2. **Microsoft.EntityFrameworkCore.Tools**  
   Dieses Paket benötigst du, wenn du Migrationen und `update-database` über die Package Manager Console oder die .NET‑CLI verwenden möchtest, um das Datenbankschema aus deinem Modell zu erzeugen und zu aktualisieren. [web:430]

3. **Microsoft.Extensions.Hosting**  
   Stellt den Generic Host bereit, den du in einer WPF‑App genauso nutzen kannst wie in ASP.NET Core oder Worker‑Services. Darüber konfigurierst du Dependency Injection, Logging und Konfiguration zentral in `App.xaml.cs`. [web:300]

4. **Microsoft.Extensions.Configuration.Json**  
   Ermöglicht das Einlesen von Konfiguration aus einer `appsettings.json`‑Datei, insbesondere des ConnectionStrings für EF Core, ohne ihn hart im Code zu hinterlegen. [web:410]

Optional, aber empfehlenswert für spätere Erweiterungen:

- **Microsoft.EntityFrameworkCore.Design**  
  Ergänzt Design‑Time‑Funktionalität (z.B. Scaffolding) und erleichtert manche EF‑Core‑Werkzeuge. [web:430]

- **CommunityToolkit.Mvvm**  
  Dieses Paket ist für das hier gezeigte, manuell aufgebaute MVVM nicht zwingend nötig, kann aber viel Boilerplate (INotifyPropertyChanged, Commands, DI‑Integration) abnehmen, wenn du das Projekt weiterentwickelst. [web:386]

Nach der Installation dieser Pakete ist dein WPF‑Projekt bereit, Konfigurationswerte aus JSON zu lesen, einen Generic Host mit DI und Logging zu verwenden und mit EF Core 10 gegen eine SQL‑Server‑Datenbank zu arbeiten. [web:300][web:410]

---

# 4. Konfiguration mit appsettings.json

In diesem Schritt lagerst du den ConnectionString und weitere Einstellungen in eine Konfigurationsdatei aus, damit sie ohne Rebuild angepasst werden können.

1. Lege im Projektstamm eine neue Datei mit dem Namen `appsettings.json` an (der vollständige Inhalt liegt im Repository).
2. Öffne die Eigenschaften der Datei und stelle Folgendes ein:
   - **Buildvorgang**: `Inhalt`  
   - **In Ausgabeverzeichnis kopieren**: `Kopieren, wenn neuer`  

Durch diese Einstellungen wird die Datei beim Build ins Ausgabeverzeichnis kopiert und kann zur Laufzeit von der Konfigurations‑API eingelesen werden. [web:410]  
Optional kannst du eine zusätzliche `appsettings.Development.json` anlegen und im Host als zweite Konfigurationsquelle registrieren, um Entwicklungs‑ und Produktionsumgebungen sauber zu trennen. [web:404]

---

# 5. Domänenmodell Person definieren (Models/Person.cs)

Nun definierst du das zentrale Domänenmodell der Anwendung.

1. Lege im Ordner `Models` die Datei `Person.cs` an (der vollständige Code liegt im Repository).
2. In dieser Datei wird die Entität `Person` mit ihren Eigenschaften definiert.

Die Eigenschaft `Id` ist als ganzzahliger Primärschlüssel vorgesehen; durch die EF‑Core‑Konventionen wird eine Eigenschaft mit dem Namen `Id` automatisch als Primärschlüssel erkannt. [web:300]  
Für die Eigenschaft `Name` wird über Datenanmerkungen sichergestellt, dass sie Pflicht ist und eine sinnvolle Längenbegrenzung besitzt, was sowohl das Datenbankschema als auch die Eingabevalidierung beeinflusst. [web:300]  
Die Eigenschaft `Email` ist optional, erhält aber eine maximale Länge und eine einfache Plausibilitätsprüfung für E‑Mail‑Adressen über passende Attribute. [web:304]  
Die Eigenschaft `CreatedAt` speichert den Erstellungszeitpunkt und wird beim Anlegen einer neuen Person zentral im Service mit der aktuellen UTC‑Zeit gesetzt. [web:345]

An dieser Stelle kannst du im Tutorial kurz hervorheben, dass `Person.cs` das Domänenmodell in C# beschreibt, während EF Core daraus später die Tabelle `People` mit passenden Spalten, Schlüsseln und Längenbegrenzungen in der Datenbank erzeugt. [web:300]

---

# 6. EF‑Core‑Kontext AppDbContext anlegen (Data/AppDbContext.cs)

Im nächsten Schritt definierst du den DbContext, der das Bindeglied zwischen Domänenmodell und Datenbank bildet.

1. Lege im Ordner `Data` die Datei `AppDbContext.cs` an (der vollständige Code liegt im Repository).
2. Die Klasse `AppDbContext` erbt von `DbContext` und erhält im Konstruktor ein `DbContextOptions<AppDbContext>`‑Objekt, damit sie über Dependency Injection konfiguriert werden kann. [web:300]
3. Über `DbSet<Person> People` wird festgelegt, dass es in der Datenbank eine Tabelle für Personen gibt.

In der überschriebenen Methode `OnModelCreating` konfigurierst du Details wie den Tabellennamen, Indizes und maximale Längen, passend zu den DataAnnotations im Modell. [web:300][web:353]  
Optional kannst du hier auch ein Standardschema wie `dbo` für SQL Server setzen, falls nötig. [web:300]

---

# 7. Generic Host und Dependency Injection konfigurieren (App.xaml / App.xaml.cs)

Damit EF Core, Services und ViewModels sauber über Dependency Injection bereitgestellt werden, richtest du den Generic Host in deiner WPF‑App ein.

1. Entferne in `App.xaml` das Attribut `StartupUri`, da der Start über den Generic Host erfolgt.
2. Öffne `App.xaml.cs` und überschreibe die Methode `OnStartup`.
3. Erzeuge innerhalb von `OnStartup` den Host über `Host.CreateApplicationBuilder()` und füge:
   - die Konfiguration aus `appsettings.json` hinzu,
   - die Logging‑Konfiguration,
   - die Registrierung von `AppDbContext`, `PersonService`, `PersonViewModel` und `MainWindow` im DI‑Container. [web:300][web:410]

Anschließend baust du den Host, erzeugst das `MainWindow` über `GetRequiredService<MainWindow>()` und zeigst es an. So erhält das Fenster sein ViewModel und alle abhängigen Services automatisch über Dependency Injection. [web:300]

---

# 8. Service‑Schicht PersonService implementieren (Services/PersonService.cs)

Die Service‑Schicht kapselt alle Datenzugriffe auf die Entität `Person` und hält EF Core von den ViewModels fern.

1. Lege im Ordner `Services` die Datei `PersonService.cs` an (der vollständige Code liegt im Repository).
2. Der Service erhält im Konstruktor einen `AppDbContext` und stellt asynchrone Methoden bereit, zum Beispiel:
   - `GetAllAsync()` lädt alle Personen, typischerweise sortiert nach Name.
   - `CreateAsync(Person person)` setzt `CreatedAt`, fügt die Person hinzu und speichert die Änderungen.
   - `UpdateAsync(Person person)` aktualisiert eine vorhandene Entität.
   - `DeleteAsync(Person person)` entfernt eine Person aus der Datenbank. [web:300][web:345]

ViewModels arbeiten ausschließlich mit dem `PersonService` und müssen keine EF‑Core‑APIs mehr kennen, was Struktur und Testbarkeit deutlich verbessert. [web:300]

---

# 9. MVVM‑Logik im PersonViewModel (ViewModels/PersonViewModel.cs)

Das `PersonViewModel` stellt alle Daten und Befehle für die Oberfläche bereit und bildet die Brücke zwischen UI und Service‑Schicht.

1. Lege im Ordner `ViewModels` die Datei `PersonViewModel.cs` an (der vollständige Code liegt im Repository).
2. Typische Bestandteile sind:
   - `ObservableCollection<Person> People` als Datenquelle für die Personenliste. [web:354]
   - `Person? SelectedPerson` als aktuell ausgewählte Person.
   - Eigenschaften wie `Name`, `Email` und `StatusMessage` für Eingaben und Rückmeldungen.
   - Befehle wie `LoadCommand`, `CreateCommand`, `UpdateCommand` und `DeleteCommand`, die jeweils die entsprechenden Methoden im `PersonService` aufrufen. [web:354]

Die Commands werden über ein zentrales Async‑Command‑Interface (`IAsyncCommand`) und die Implementierung `AsyncRelayCommand` bereitgestellt, die im Ordner `Commands` liegen und von mehreren ViewModels wiederverwendet werden können. [web:394][web:438]  
Durch DataBinding in der View werden Änderungen im ViewModel automatisch im UI reflektiert, und Benutzeraktionen lösen die zugehörigen Commands aus, ohne dass Code‑Behind‑Logik nötig ist. [web:354]

---

# 10. View MainWindow.xaml an das ViewModel binden

Zum Schluss verbindest du das `MainWindow` mit dem `PersonViewModel` und richtest die Bindings ein.

1. Öffne `MainWindow.xaml` und definiere:
   - TextBoxen für `Name` und `Email`, deren `Text`‑Eigenschaft an die gleichnamigen Properties des ViewModels gebunden ist.
   - Buttons für Laden, Anlegen, Aktualisieren und Löschen, deren `Command`‑Eigenschaft an die jeweiligen Commands gebunden ist.
   - Eine `ListBox` oder ein anderes Listen‑Steuerelement, deren `ItemsSource` an `People` und `SelectedItem` an `SelectedPerson` gebunden ist.
   - Ein Status‑Element (z.B. `StatusBar` oder `TextBlock`) mit Binding auf `StatusMessage`. [web:354]

2. Im Code‑Behind von `MainWindow.xaml.cs` wird nur noch der `DataContext` gesetzt, typischerweise über Dependency Injection beim Erzeugen des Fensters.  
So bleibt das Window „dumm“ im Sinne von MVVM und kennt nur sein ViewModel, aber keine Details über EF Core oder die Datenbank. [web:438]

---

# 11. Datenbank anlegen und Anwendung testen

Zum Anlegen und Aktualisieren des Datenbankschemas kannst du die EF‑Core‑Migrations nutzen.

1. Erzeuge in der Package Manager Console eine erste Migration, die das Modell in ein Datenbankschema überführt.
2. Spiele die Migration anschließend auf die konfigurierte Datenbank, um Tabellen und Beziehungen zu erstellen. [web:430]

Alternativ kann EF Core die Datenbank beim ersten Zugriff auch programmgesteuert erstellen, zum Beispiel über `Database.EnsureCreated()` im Kontext oder an einer zentralen Stelle beim Start der Anwendung. Diese Variante eignet sich vor allem für Prototypen oder Tests, während für produktive Szenarien in der Regel Migrations (`Database.Migrate()`) empfohlen werden, da sie Schemaänderungen versioniert nachverfolgen. [web:345][web:363]
