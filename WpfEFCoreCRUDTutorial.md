# WpfEfCoreCRUDTutorial

Einfaches WPF-CRUD-Tutorial mit Entity Framework Core und SQL-Server-Datenbank sowie einer 1:n-Beziehung (Person → Adressen). Das Beispiel orientiert sich an aktuellen .NET-Praktiken mit Generic Host, Dependency Injection und Konfiguration über appsettings.json.

## 1. WPF-Projekt in Visual Studio anlegen (.NET ≥ 8, Beispiel .NET 10)

Erstelle ein neues WPF-Projekt auf Basis eines aktuellen .NET-Frameworks (mindestens .NET 8.0; im Beispiel wird .NET 10.0 verwendet). Starte Visual Studio, wähle „WPF-App (.NET)“, nenne das Projekt `WpfEfCoreCRUDTutorial`, setze das Ziel-Framework auf `.NET 10.0` (oder dein gewünschtes aktuelles Framework) und prüfe mit F5, dass ein leeres Fenster startet.

## 2. Ordnerstruktur für MVVM vorbereiten

Lege im Projekt die Ordner `Data`, `Models`, `Services`, `ViewModels`, `Views` und `Commands` an. In `Models` liegen Domänenklassen wie `Person` und `Address`, in `Data` der `AppDbContext`, in `Services` z.B. der `PersonService`, in `ViewModels` u.a. `PersonViewModel`, `AddressViewModel` und `MainViewModel`, in `Views` die XAML-Fenster (`MainWindow`, `PersonAddressDetailsWindow`) und in `Commands` wiederverwendbare Command-Implementierungen wie `AsyncRelayCommand`. Diese Struktur folgt dem MVVM-Muster und trennt UI, Logik und Datenzugriff klar voneinander.

## 3. NuGet-Pakete für EF Core und Infrastruktur installieren

Über „NuGet-Pakete verwalten…“ installierst du mindestens:
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.Extensions.Hosting`
- `Microsoft.Extensions.Configuration.Json`

Optional kommen u.a. `Microsoft.EntityFrameworkCore.Design` und `CommunityToolkit.Mvvm` hinzu. Damit kann die WPF-App Konfiguration aus JSON lesen, einen Generic Host nutzen und per EF Core auf SQL Server zugreifen; der CommunityToolkit stellt u.a. Implementierungen für `INotifyPropertyChanged` und `AsyncRelayCommand` bereit.

## 4. Konfiguration mit appsettings.json

Lege im Projektstamm eine `appsettings.json` an, in der du den ConnectionString und ggf. weitere Einstellungen hinterlegst, zum Beispiel im Abschnitt `ConnectionStrings`. Stelle bei den Dateieigenschaften sicher, dass der Buildvorgang auf „Inhalt“ und „In Ausgabeverzeichnis kopieren“ auf „Kopieren, wenn neuer“ steht. So kann der Generic Host die Konfiguration zur Laufzeit laden und EF Core den ConnectionString verwenden.

## 5. Domänenmodell Person (Models/Person.cs)

Definiere im Ordner `Models` die Entität `Person` mit einem Primärschlüssel (`Id`), fachlichen Eigenschaften (`Name`, `Email`, `CreatedAt`) und einer Navigation Collection `ICollection<Address> Addresses` für die 1:n-Beziehung. Attribute für Pflichtfelder und maximale Längen sorgen dafür, dass C#-Modell und Datenbankschema zusammenpassen. Das Feld `CreatedAt` kann entweder im `PersonService` beim Anlegen gesetzt oder über eine Datenbank-Default-Konfiguration in `OnModelCreating` definiert werden. EF Core erzeugt später daraus standardmäßig die Tabelle `People`.

## 6. Domänenmodell Address für die 1:n-Beziehung (Models/Address.cs)

Lege im selben Ordner die Entität `Address` an, z.B. mit `Id`, `Street`, `PostalCode`, `City`, `Country`, dem Fremdschlüssel `PersonId` und der Navigation `Person`. Zusammengenommen bilden `Person.Addresses` und `Address.Person` eine 1:n-Beziehung: Eine Person kann mehrere Adressen haben, jede Adresse gehört genau zu einer Person. Auch hier können DataAnnotations (Pflichtfelder, maximale Längen) verwendet werden, um Modell und Datenbank eng zu koppeln.

## 7. EF-Core-Kontext AppDbContext und 1:n-Mapping (Data/AppDbContext.cs)

Im Ordner `Data` definierst du den `AppDbContext`, der von `DbContext` erbt und über Dependency Injection konfigurierte Optionen erhält. Du fügst `DbSet<Person> People` und `DbSet<Address> Addresses` hinzu und konfigurierst in `OnModelCreating` unter anderem:
- Tabellennamen (`People`, `Addresses`)
- maximale Längen und Indizes (z.B. Index auf `People.Name` und auf `Addresses.PersonId`)
- die 1:n-Beziehung mit einer Fluent-API-Konfiguration: eine Person hat viele Adressen, eine Adresse gehört zu genau einer Person (`HasMany(p => p.Addresses).WithOne(a => a.Person).HasForeignKey(a => a.PersonId)`)
- das Löschverhalten, z.B. `OnDelete(DeleteBehavior.Cascade)`, damit das Löschen einer Person automatisch alle zugehörigen Adressen entfernt
- optional ein Standardschema wie `dbo`.

Damit ist die Beziehung in der Datenbank eindeutig definiert, und EF Core kennt das gewünschte Cascade-Delete-Verhalten.

## 8. Generic Host und Dependency Injection (App.xaml / App.xaml.cs)

In `App.xaml` entfernst du `StartupUri`, damit die App über den Generic Host startet, anstatt ein Fenster direkt per XAML zu öffnen. In `App.xaml.cs` überschreibst du `OnStartup`, erstellst mit `Host.CreateApplicationBuilder()` den Host, fügst die Konfiguration aus `appsettings.json` hinzu, richtest Logging ein und registrierst im DI-Container:
- `AppDbContext` mit `UseSqlServer(...)` und dem ConnectionString aus der Konfiguration
- Services wie `PersonService`
- ViewModels (`PersonViewModel`, `AddressViewModel`, `MainViewModel`) – typischerweise als Singleton, damit sie während der Anwendungslaufzeit ihren Zustand behalten
- `MainWindow` (Singleton) und `PersonAddressDetailsWindow` (Transient)

Danach baust du den Host, holst dir `MainWindow` aus dem ServiceProvider, initialisierst das `MainViewModel` (z.B. mit einer asynchronen `InitializeAsync`-Methode) und zeigst das Fenster an. Für EF-Migrationen kannst du entweder eine klassische `CreateHostBuilder`-Methode oder eine `IDesignTimeDbContextFactory<AppDbContext>` bereitstellen, damit die Design-Time-Werkzeuge den DbContext korrekt konfigurieren können.

## 9. Service-Schicht PersonService (Services/PersonService.cs)

`PersonService` kapselt alle Datenzugriffe auf Personen und erhält im Konstruktor einen `AppDbContext`. Er stellt asynchrone Methoden bereit wie:
- `GetAllAsync()` zum Laden aller Personen,
- `CreateAsync(Person person)` zum Anlegen (inkl. Setzen von `CreatedAt`, falls nicht per Datenbank-Default),
- `UpdateAsync(Person person)` zum Aktualisieren,
- `DeleteAsync(Person person)` zum Löschen (inkl. abhängiger Adressen per konfiguriertem Cascade Delete).

ViewModels sprechen nur über diesen Service mit der Datenbank und kennen keine EF-APIs direkt. Beim Laden entscheidest du, ob Adressen explizit mitgeladen werden (`Include(p => p.Addresses)`) oder ob sie separat im `AddressViewModel` abgefragt werden; für eine klare Trennung der Verantwortlichkeiten wird typischerweise nur die Personenliste geladen und Adressen separat nachgeladen.

## 10. Async-Commands für MVVM (Commands)

Im Ordner `Commands` definierst du ein Interface `IAsyncCommand` (Erweiterung von `ICommand` mit `ExecuteAsync`) und eine Implementierung `AsyncRelayCommand`, die asynchrone Operationen kapselt, `CanExecute` berücksichtigt und Fehler handhaben kann. Wichtig ist, im ViewModel nicht mit `async void`, sondern mit asynchronen Methoden zu arbeiten, die vom Command aufgerufen werden, sodass Fehler abgefangen und z.B. in einer `StatusMessage` angezeigt werden können. Diese Commands nutzt du später in `PersonViewModel` und `AddressViewModel` für alle asynchronen Aktionen (Laden, Speichern, Löschen), ohne die UI zu blockieren.

## 11. PersonViewModel für CRUD auf Personen (ViewModels/PersonViewModel.cs)

`PersonViewModel` stellt die Master-Sicht auf Personen bereit und enthält:
- `ObservableCollection<Person> People` als Liste für das UI,
- `Person? SelectedPerson`,
- Eingabe-Properties wie `Name` und `Email`,
- eine `StatusMessage` für Rückmeldungen,
- Async-Commands wie `LoadCommand`, `CreateCommand`, `UpdateCommand`, `DeleteCommand`, die den `PersonService` aufrufen und `People` bzw. `SelectedPerson` aktualisieren.

So bildet `PersonViewModel` das Bindeglied zwischen UI und Service-Schicht und folgt strikt dem MVVM-Muster. Optional kannst du Validierung ergänzen, z.B. über DataAnnotations in den Modellen in Kombination mit `INotifyDataErrorInfo` im ViewModel, sodass Eingabefehler direkt im UI angezeigt werden.

## 12. AddressViewModel für die 1:n-Detailansicht (ViewModels/AddressViewModel.cs)

`AddressViewModel` repräsentiert die Detailseite für Adressen zur ausgewählten Person. Typische Inhalte sind:
- `ObservableCollection<Address> Addresses` und `Address? SelectedAddress`,
- Formular-Properties wie `Street`, `PostalCode`, `City`, `Country`,
- eine interne Referenz auf die aktuelle Person (z.B. PersonId), die über `SetCurrentPersonAsync(Person? person)` gesetzt wird,
- eine Anzeige-Property wie `CurrentPersonName`, die im Detailfenster den Namen der aktuellen Person zeigt,
- Async-Commands `CreateAddressCommand`, `UpdateAddressCommand`, `DeleteAddressCommand`,
- eine Methode `SetCurrentPersonAsync(Person? person)`, die die aktuelle Person-ID und `CurrentPersonName` setzt und `Addresses` per Datenbankabfrage auf die Adressen dieser Person aktualisiert.

Damit bildet das ViewModel die „n“-Seite der 1:n-Beziehung ab und ist Grundlage für die Adressbearbeitung im Detailfenster. Wenn das ViewModel als Singleton registriert ist, bleibt sein Zustand auch beim Schließen und erneuten Öffnen des Detailfensters erhalten.

## 13. MainViewModel als Koordinator (ViewModels/MainViewModel.cs)

`MainViewModel` kapselt das Zusammenspiel zwischen Personen- und Adress-Bereich. Es erhält `PersonViewModel` und `AddressViewModel` per Konstruktor-Injection und:
- stellt beide als Properties bereit,
- registriert sich auf `PersonViewModel.PropertyChanged`, um bei Änderungen von `SelectedPerson` automatisch `AddressViewModel.SetCurrentPersonAsync(...)` aufzurufen,
- bietet eine `InitializeAsync`-Methode, die beim Start einmalig die Personenliste lädt.

Damit wird bei einem Personenwechsel automatisch die passende Adressliste nachgezogen; die 1:n-Beziehung ist nicht nur im Datenmodell, sondern auch in der UI-Logik konsistent abgebildet.

## 14. MainWindow.xaml als Master-View

`MainWindow` verwendet `MainViewModel` als DataContext. In der XAML:
- bindest du Eingabefelder für `Name` und `Email` an `PersonViewModel.Name` bzw. `.Email`,
- Buttons für Neu/Laden/Update/Löschen an die Commands im `PersonViewModel`,
- eine ListBox mit `ItemsSource="{Binding PersonViewModel.People}"` und `SelectedItem="{Binding PersonViewModel.SelectedPerson}"`,
- eine Statuszeile an `PersonViewModel.StatusMessage`.

Zusätzlich gibt es einen Button „Adressdetails“, der ein `PersonAddressDetailsWindow` öffnet, sobald `SelectedPerson` nicht null ist. Die Bezeichner bleiben damit konsistent zum Domänenmodell „Person“ und „Address“.

## 15. PersonAddressDetailsWindow.xaml als Detail-View der 1:n-Beziehung

`PersonAddressDetailsWindow` bindet an `AddressViewModel`, das per Konstruktor-Injection gesetzt wird. In der XAML:
- zeigt ein Kopfbereich oben z.B. „Adressen für: {CurrentPersonName}“ an,
- darunter stehen Labels und TextBoxen für `Street`, `PostalCode`, `City`, `Country`,
- daneben Buttons „Neu“, „Speichern“, „Löschen“ mit Bindings auf die Adress-Commands,
- darunter eine ListBox mit `ItemsSource="{Binding Addresses}"` und `SelectedItem="{Binding SelectedAddress}"`,
- unten eine Statuszeile für meldende Texte (`StatusMessage`).

Dieses Fenster bildet die Adressseite der 1:n-Beziehung visuell ab und erlaubt CRUD-Operationen für die Adressen der aktuell gewählten Person.

## 16. Zusammenspiel von MainWindow und PersonAddressDetailsWindow

Beim Klick auf „Adressdetails“ im `MainWindow`:
- prüfst du, ob eine `SelectedPerson` vorhanden ist,
- holst dir aus DI das (als Singleton registrierte) `AddressViewModel`,
- rufst `SetCurrentPersonAsync(SelectedPerson)` auf, damit die Adressen der gewählten Person geladen und `CurrentPersonName` gesetzt werden,
- erzeugst per DI ein neues `PersonAddressDetailsWindow` (Transient), setzt dessen `Owner` und rufst `Show()` oder `ShowDialog()` auf.

Die technische Registrierung von ViewModel und Fenster im DI-Container erfolgt, wie in Abschnitt 8 beschrieben, im `App.xaml.cs`.

## 17. Datenbank mit Migrationen anlegen

Über die Package Manager Console führst du aus:
- `Add-Migration InitialCreate` zum Erzeugen der ersten Migration basierend auf `AppDbContext` und den Entitäten `Person` und `Address`,
- `Update-Database` zum Anwenden dieser Migration auf die konfigurierte Datenbank.

Dadurch entstehen Tabellen für `People` und `Addresses` inklusive Fremdschlüssel und 1:n-Beziehung. Wird die Datenbank später gelöscht, kannst du sie jederzeit über `Update-Database` neu aufbauen. Falls du eine `IDesignTimeDbContextFactory<AppDbContext>` verwendest, sorgt sie dafür, dass die Migrationen den DbContext mit der gleichen Konfiguration wie zur Laufzeit erstellen können.

## 18. Design-Time-DbContext für Migrationen (Data/DesignTimeDbContextFactory.cs)

Da diese WPF-Anwendung den DbContext zur Laufzeit über den Generic Host und Dependency Injection konfiguriert, benötigen die EF-Core-Tools (z.B. `Add-Migration`, `Update-Database`) eine spezielle Design-Time-Fabrik, um den `AppDbContext` auch ohne laufenden Host instanziieren zu können. Ohne diese Fabrik erscheint typischerweise ein Fehler wie „Unable to create a 'DbContext' of type 'AppDbContext' … Unable to resolve service for type 'DbContextOptions<AppDbContext>' …“.

Lege im Ordner `Data` eine Klasse `DesignTimeDbContextFactory` an, die `IDesignTimeDbContextFactory<AppDbContext>` implementiert und den DbContext mit dem ConnectionString aus `appsettings.json` erstellt. So können die EF-Core-Tools den `AppDbContext` zur Design-Time korrekt erzeugen, ohne auf den WPF-Einstiegspunkt oder den Generic Host angewiesen zu sein.

Typischer Ablauf nach dem Hinzufügen der Factory:
- Projekt neu bauen
- vorhandene Migrationen (z.B. `InitialCreate`) mit `Update-Database` anwenden
- neue Migrationen wie gewohnt mit `Add-Migration` erzeugen

## 19. Anwendung starten und 1:n-Beziehung testen

Starte die Anwendung, lege im `MainWindow` einige Personen an und wähle einen Eintrag aus. Öffne das `PersonAddressDetailsWindow`, füge Adressen hinzu, bearbeite oder lösche sie und prüfe, dass:
- jede Person ihre eigenen Adressen hat,
- beim Wechsel der ausgewählten Person im `MainWindow` und erneutem Öffnen des Detailfensters die passenden Adressen geladen werden.

Damit ist das Tutorial um eine vollständige 1:n-Beziehung Person → Adressen erweitert, sowohl im Datenmodell (EF Core) als auch in der MVVM-Struktur und der WPF-Oberfläche. Der konkrete Beispielcode zu allen Klassen und XAML-Dateien liegt im zugehörigen Repository und kann dort vollständig eingesehen werden.
