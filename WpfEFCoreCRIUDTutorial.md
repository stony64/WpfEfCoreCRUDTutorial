# WpfEfCoreCRUDTutorial

# 1. WPF‑Projekt in Visual Studio anlegen (.NET 10)

Erstelle ein neues WPF‑Projekt auf Basis von .NET 10, das später mit EF Core, MVVM und einer SQL‑Server‑Datenbank mit 1:n‑Beziehung (Person → Adressen) arbeitet. Starte Visual Studio, wähle „WPF-App (.NET)“, nenne das Projekt `WpfEfCoreCRUDTutorial`, setze das Ziel‑Framework auf `.NET 10.0` und prüfe mit F5, dass ein leeres Fenster startet.

---

# 2. Ordnerstruktur für MVVM vorbereiten

Lege im Projekt die Ordner `Data`, `Models`, `Services`, `ViewModels`, `Views` und `Commands` an.  
In `Models` liegen Domänenklassen wie `Person` und `Address`, in `Data` der `AppDbContext`, in `Services` z.B. der `PersonService`, in `ViewModels` u.a. `PersonViewModel`, `AddressViewModel` und `MainViewModel`, in `Views` die XAML‑Fenster (`MainWindow`, `UserDetailsWindow`) und in `Commands` wiederverwendbare Command‑Implementierungen wie `AsyncRelayCommand`.

---

# 3. NuGet‑Pakete für EF Core 10 und Infrastruktur installieren

Über „NuGet-Pakete verwalten…“ installierst du mindestens:
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.Extensions.Hosting`
- `Microsoft.Extensions.Configuration.Json`

Optional kommen u.a. `Microsoft.EntityFrameworkCore.Design` und `CommunityToolkit.Mvvm` hinzu. Damit kann die WPF‑App Konfiguration aus JSON lesen, einen Generic Host nutzen und per EF Core auf SQL Server zugreifen.

---

# 4. Konfiguration mit appsettings.json

Lege im Projektstamm eine `appsettings.json` an, in der du den ConnectionString und ggf. weitere Einstellungen hinterlegst. Stelle bei den Dateieigenschaften sicher, dass der Buildvorgang auf „Inhalt“ und „In Ausgabeverzeichnis kopieren“ auf „Kopieren, wenn neuer“ steht. So kann der Generic Host die Konfiguration zur Laufzeit laden.

---

# 5. Domänenmodell Person (Models/Person.cs)

Definiere im Ordner `Models` die Entität `Person` mit Schlüssel (`Id`), fachlichen Eigenschaften (`Name`, `Email`, `CreatedAt`) und einer Navigation Collection `ICollection<Address> Addresses` für die 1:n‑Beziehung. Attribute für Pflichtfelder und maximale Längen sorgen dafür, dass C#‑Modell und Datenbankschema zusammenpassen. EF Core erzeugt später daraus die Tabelle `People`.

---

# 6. Domänenmodell Address für die 1:n‑Beziehung (Models/Address.cs)

Lege im selben Ordner die Entität `Address` an, z.B. mit `Id`, `Street`, `PostalCode`, `City`, `Country`, dem Fremdschlüssel `PersonId` und der Navigation `Person`. Zusammengenommen bilden `Person.Addresses` und `Address.Person` eine 1:n‑Beziehung: Eine Person kann mehrere Adressen haben, jede Adresse gehört genau zu einer Person.

---

# 7. EF‑Core‑Kontext AppDbContext und 1:n‑Mapping (Data/AppDbContext.cs)

Im Ordner `Data` definierst du den `AppDbContext`, der von `DbContext` erbt und über DI konfigurierte Optionen erhält. Du fügst `DbSet<Person> People` und `DbSet<Address> Addresses` hinzu und konfigurierst in `OnModelCreating` u.a.:
- Tabellennamen (`People`, `Addresses`)
- maximale Längen und Indizes (z.B. Index auf `People.Name` und auf `Addresses.PersonId`)
- die 1:n‑Beziehung mit einer Fluent‑API‑Konfiguration („eine Person hat viele Adressen, eine Adresse gehört zu genau einer Person“)
- optional ein Standardschema wie `dbo`.

---

# 8. Generic Host und Dependency Injection (App.xaml / App.xaml.cs)

In `App.xaml` entfernst du `StartupUri`, damit die App über den Generic Host startet. In `App.xaml.cs` überschreibst du `OnStartup`, erstellst mit `Host.CreateApplicationBuilder()` den Host, fügst die Konfiguration aus `appsettings.json` hinzu, richtest Logging ein und registrierst im DI‑Container:
- `AppDbContext` mit `UseSqlServer(...)`
- Services wie `PersonService`
- ViewModels (`PersonViewModel`, `AddressViewModel`, `MainViewModel`) – typischerweise als Singleton
- `MainWindow` (Singleton) und `UserDetailsWindow` (Transient)

Danach baust du den Host, holst dir `MainWindow` aus dem ServiceProvider, initialisierst das `MainViewModel` (z.B. mit `InitializeAsync`) und zeigst das Fenster an.

---

# 9. Service‑Schicht PersonService (Services/PersonService.cs)

`PersonService` kapselt alle Datenzugriffe auf Personen und erhält im Konstruktor einen `AppDbContext`. Er stellt asynchrone Methoden bereit wie:
- `GetAllAsync()` zum Laden aller Personen,
- `CreateAsync(Person person)` zum Anlegen (inkl. Setzen von `CreatedAt`),
- `UpdateAsync(Person person)` zum Aktualisieren,
- `DeleteAsync(Person person)` zum Löschen (inkl. abhängiger Adressen per Cascade Delete).

ViewModels sprechen nur über diesen Service mit der Datenbank und kennen keine EF‑APIs direkt.

---

# 10. Async‑Commands für MVVM (Commands)

Im Ordner `Commands` definierst du ein Interface `IAsyncCommand` (Erweiterung von `ICommand` mit `ExecuteAsync`) und eine Implementierung `AsyncRelayCommand`, die asynchrone Operationen kapselt, `CanExecute` berücksichtigt und Fehler handhaben kann. Diese Commands nutzt du später in `PersonViewModel` und `AddressViewModel` für alle asynchronen Aktionen (Laden, Speichern, Löschen).

---

# 11. PersonViewModel für CRUD auf Personen (ViewModels/PersonViewModel.cs)

`PersonViewModel` stellt die Master‑Sicht auf Personen bereit und enthält:
- `ObservableCollection<Person> People` als Liste für das UI,
- `Person? SelectedPerson`,
- Eingabe‑Properties wie `Name` und `Email`,
- eine `StatusMessage` für Rückmeldungen,
- Async‑Commands wie `LoadCommand`, `CreateCommand`, `UpdateCommand`, `DeleteCommand`, die den `PersonService` aufrufen und `People` bzw. `SelectedPerson` aktualisieren.

So bildet `PersonViewModel` das Bindeglied zwischen UI und Service‑Schicht und folgt strikt dem MVVM‑Muster.

---

# 12. AddressViewModel für die 1:n‑Detailansicht (ViewModels/AddressViewModel.cs)

`AddressViewModel` repräsentiert die Detailseite für Adressen zur ausgewählten Person. Typische Inhalte sind:
- `ObservableCollection<Address> Addresses` und `Address? SelectedAddress`,
- Formular‑Properties wie `Street`, `PostalCode`, `City`, `Country`,
- eine Property für die aktuelle Person (z.B. `CurrentPerson`),
- Async‑Commands `CreateAddressCommand`, `UpdateAddressCommand`, `DeleteAddressCommand`,
- eine Methode `SetCurrentPersonAsync(Person? person)`, die `CurrentPerson` setzt und `Addresses` per Datenbankabfrage auf die Adressen dieser Person aktualisiert.

Damit bildet das ViewModel die „n“-Seite der 1:n‑Beziehung ab und ist Grundlage für die Adressbearbeitung im Detailfenster.

---

# 13. MainViewModel als Koordinator (ViewModels/MainViewModel.cs)

`MainViewModel` kapselt das Zusammenspiel zwischen Personen‑ und Adress‑Bereich. Es erhält `PersonViewModel` und `AddressViewModel` per Konstruktor‑Injection und:
- stellt beide als Properties bereit,
- registriert sich auf `PersonViewModel.PropertyChanged`, um bei Änderungen von `SelectedPerson` automatisch `AddressViewModel.SetCurrentPersonAsync(...)` aufzurufen,
- bietet eine `InitializeAsync`‑Methode, die beim Start einmalig die Personenliste lädt.

Damit wird bei einem Personenwechsel automatisch die passende Adressliste nachgezogen; die 1:n‑Beziehung ist nicht nur im Datenmodell, sondern auch in der UI‑Logik konsistent abgebildet.

---

# 14. MainWindow.xaml als Master‑View

`MainWindow` verwendet `MainViewModel` als DataContext. In der XAML:
- bindest du Eingabefelder für `Name` und `Email` an `PersonViewModel.Name` bzw. `.Email`,
- Buttons für Neu/Laden/Update/Löschen an die Commands im `PersonViewModel`,
- eine ListBox mit `ItemsSource="{Binding PersonViewModel.People}"` und `SelectedItem="{Binding PersonViewModel.SelectedPerson}"`,
- eine Statuszeile an `PersonViewModel.StatusMessage`.

Zusätzlich gibt es einen Button „User‑Details“, der ein `UserDetailsWindow` öffnet, sobald `SelectedPerson` nicht null ist.

---

# 15. UserDetailsWindow.xaml als Detail‑View der 1:n‑Beziehung

`UserDetailsWindow` bindet an `AddressViewModel`, das per Konstruktor‑Injection gesetzt wird. In der XAML:
- stehen oben Labels und TextBoxen für `Street`, `PostalCode`, `City`, `Country`,
- daneben Buttons „Neu“, „Speichern“, „Löschen“ mit Bindings auf die Adress‑Commands,
- darunter eine ListBox mit `ItemsSource="{Binding Addresses}"` und `SelectedItem="{Binding SelectedAddress}"`,
- unten eine Statuszeile für meldende Texte.

Dieses Fenster bildet die Adressseite der 1:n‑Beziehung visuell ab und erlaubt CRUD‑Operationen für die Adressen der aktuell gewählten Person.

---

# 16. Zusammenspiel von MainWindow und UserDetailsWindow

Beim Klick auf „User‑Details“ im `MainWindow`:
- prüfst du, ob eine `SelectedPerson` vorhanden ist,
- holst dir aus DI das (als Singleton registrierte) `AddressViewModel`,
- rufst `SetCurrentPersonAsync(SelectedPerson)` auf,
- erzeugst per DI ein neues `UserDetailsWindow` (Transient), setzt dessen `Owner` und rufst `Show()` oder `ShowDialog()` auf.

Weil Main‑ und Detailfenster dasselbe `AddressViewModel` teilen, erscheinen im `UserDetailsWindow` automatisch die Adressen der aktuell ausgewählten Person, und alle Änderungen bleiben konsistent.

---

# 17. Datenbank mit Migrationen anlegen

Über die Package Manager Console führst du aus:
- `Add-Migration InitialCreate` zum Erzeugen der ersten Migration basierend auf `AppDbContext` und den Entitäten `Person` und `Address`,
- `Update-Database` zum Anwenden dieser Migration auf die konfigurierte Datenbank.

Dadurch entstehen Tabellen für `People` und `Addresses` inklusive Fremdschlüssel und 1:n‑Beziehung. Wird die Datenbank später gelöscht, kannst du sie jederzeit über `Update-Database` neu aufbauen.

---

# 18. Anwendung starten und 1:n‑Beziehung testen

Starte die Anwendung, lege im `MainWindow` einige Personen an und wähle einen Eintrag aus. Öffne das `UserDetailsWindow`, füge Adressen hinzu, bearbeite oder lösche sie und prüfe, dass:
- jede Person ihre eigenen Adressen hat,
- beim Wechsel der ausgewählten Person im `MainWindow` und erneutem Öffnen des Detailfensters die passenden Adressen geladen werden.

Damit ist das Tutorial um eine vollständige 1:n‑Beziehung Person → Adressen erweitert, sowohl im Datenmodell (EF Core) als auch in der MVVM‑Struktur und der WPF‑Oberfläche. Der konkrete Beispielcode zu allen Klassen und XAML‑Dateien liegt im zugehörigen Repository und kann dort vollständig eingesehen werden.
