using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfEfCoreCRUDTutorial.Commands;
using WpfEfCoreCRUDTutorial.Models;
using WpfEfCoreCRUDTutorial.Services;

namespace WpfEfCoreCRUDTutorial.ViewModels;

/// <summary>
/// ViewModel für die Personenverwaltung (Master-Teil im Master-Detail-Szenario).
/// - Hält die Liste aller Personen
/// - Nimmt Eingaben für Name/Email entgegen
/// - Bietet Commands für Laden, Erstellen, Aktualisieren, Löschen
/// Die Adressen zur ausgewählten Person werden im AddressViewModel verwaltet und
/// über das MainViewModel synchronisiert.
/// </summary>
public class PersonViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Fachlicher Zugriffspunkt auf die Personen-Datenbankoperationen.
    /// Das ViewModel kennt nur den Service und muss keine EF-Core-Details verwenden.
    /// </summary>
    private readonly PersonService _personService;

    /// <summary>
    /// Konstruktor: erhält den PersonService über Dependency Injection
    /// und initialisiert die Commands, die später an Buttons gebunden werden.
    /// </summary>
    /// <param name="personService">Service für alle Person-bezogenen Datenzugriffe.</param>
    public PersonViewModel(PersonService personService)
    {
        _personService = personService;

        // Commands mit den asynchronen Methoden dieses ViewModels verbinden.
        // Dadurch bleibt die UI-Logik im ViewModel und die Buttons rufen nur Commands auf.
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        CreateCommand = new AsyncRelayCommand(CreateAsync);
        UpdateCommand = new AsyncRelayCommand(UpdateAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
    }

    #region Properties (für Bindings)

    /// <summary>
    /// Interne Sammlung der aktuell geladenen Personen.
    /// </summary>
    private ObservableCollection<Person> _people = new();

    /// <summary>
    /// Sammlung aller aktuell geladenen Personen.
    /// Wird in der View an die ListBox (ItemsSource) gebunden,
    /// sodass Änderungen an der Collection automatisch im UI reflektiert werden.
    /// </summary>
    public ObservableCollection<Person> People
    {
        get => _people;
        set
        {
            _people = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Interne Referenz auf die aktuell ausgewählte Person.
    /// </summary>
    private Person? _selectedPerson;

    /// <summary>
    /// Aktuell ausgewählte Person in der UI.
    /// Wird an SelectedItem der Personen-ListBox gebunden.
    /// Beim Setzen werden die Eingabefelder (Name/Email) und die Statuszeile synchronisiert,
    /// sodass der Benutzer sofort sieht, welche Person gerade bearbeitet wird.
    /// Das AddressViewModel wird über das MainViewModel informiert (dort via PropertyChanged-Subscription),
    /// nicht direkt von hier aus.
    /// </summary>
    public Person? SelectedPerson
    {
        get => _selectedPerson;
        set
        {
            _selectedPerson = value;
            OnPropertyChanged();

            if (value != null)
            {
                // Beim Auswählen einer Person die Eingabefelder mit deren Werten füllen.
                Name = value.Name;
                Email = value.Email ?? string.Empty;
                StatusMessage = $"👆 Ausgewählt: {value.Name}";
            }
            else
            {
                // Wenn nichts ausgewählt ist, Eingabefelder und Status zurücksetzen.
                Name = string.Empty;
                Email = string.Empty;
                StatusMessage = "📋 Bitte Person auswählen";
            }

            // WICHTIG:
            // Die Synchronisation der Adressen (AddressViewModel.SetCurrentPersonAsync)
            // passiert im MainViewModel, das sich auf PropertyChanged von PersonViewModel registriert.
        }
    }

    /// <summary>
    /// Puffer für den Namen, den der Benutzer eingibt.
    /// </summary>
    private string _name = string.Empty;

    /// <summary>
    /// Name-Eingabefeld für die UI.
    /// Wird an die Name-TextBox gebunden.
    /// Änderungen im Textfeld werden direkt im ViewModel gespeichert
    /// und später beim Erstellen/Aktualisieren verwendet.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Puffer für die optionale E-Mail-Adresse.
    /// </summary>
    private string? _email;

    /// <summary>
    /// Email-Eingabefeld für die UI.
    /// Wird an die Email-TextBox gebunden.
    /// Darf leer sein; im Modell wird dies dann als null gespeichert.
    /// </summary>
    public string? Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Interner Status-Text, der in der StatusBar angezeigt wird.
    /// </summary>
    private string _statusMessage = "Bereit (Personen)";

    /// <summary>
    /// Statuszeile unten im Fenster (StatusBar).
    /// Zeigt z.B. Ladezustände, Fehler oder Erfolgsnachrichten an,
    /// damit der Benutzer Feedback zu seiner Aktion bekommt.
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    #endregion Properties (für Bindings)

    #region Commands

    /// <summary>
    /// Lädt alle Personen neu aus der Datenbank.
    /// Wird typischerweise an einen „Laden“-Button gebunden.
    /// </summary>
    public IAsyncCommand LoadCommand { get; }

    /// <summary>
    /// Legt eine neue Person an.
    /// Verwendet die Eingaben aus Name/Email.
    /// </summary>
    public IAsyncCommand CreateCommand { get; }

    /// <summary>
    /// Speichert Änderungen an der aktuell ausgewählten Person.
    /// Nutzt ebenfalls die Eingabefelder Name/Email.
    /// </summary>
    public IAsyncCommand UpdateCommand { get; }

    /// <summary>
    /// Löscht die aktuell ausgewählte Person.
    /// </summary>
    public IAsyncCommand DeleteCommand { get; }

    #endregion Commands

    #region Command-Methoden

    /// <summary>
    /// Lädt alle Personen neu und aktualisiert die ObservableCollection.
    /// Die bisherige Collection wird komplett ersetzt, damit das UI eine saubere Aktualisierung erhält.
    /// </summary>
    private async Task LoadAsync()
    {
        // Adressen können bei Bedarf mitgeladen werden (includeAddresses = true),
        // hier reicht aber in der Regel das Laden der Personen,
        // weil das AddressViewModel seine Daten separat lädt.
        var people = await _personService.GetAllAsync(includeAddresses: false);
        People = new ObservableCollection<Person>(people);
        StatusMessage = $"📋 {People.Count} Personen geladen";
    }

    /// <summary>
    /// Erstellt eine neue Person anhand der Eingabefelder Name/Email.
    /// Führt eine einfache Validierung im ViewModel durch,
    /// bevor der Service aufgerufen wird.
    /// </summary>
    private async Task CreateAsync()
    {
        // Minimale Validierung der Benutzereingaben.
        if (string.IsNullOrWhiteSpace(Name) || Name.Length < 2)
        {
            StatusMessage = "⚠ Name erforderlich (mindestens 2 Zeichen)";
            return;
        }

        var person = new Person
        {
            Name = Name.Trim(),
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim()
        };

        // Persistieren über den Service; CreatedAt wird im Service gesetzt.
        await _personService.CreateAsync(person);
        await LoadAsync(); // Liste nach dem Anlegen neu laden.

        StatusMessage = $"➕ Neu erstellt: {person.Name} (ID: {person.Id})";
        Name = string.Empty;
        Email = string.Empty;
    }

    /// <summary>
    /// Aktualisiert die ausgewählte Person mit den aktuellen Eingabefeld-Werten.
    /// Die Änderungen werden in das ausgewählte Objekt zurückgeschrieben
    /// und dann über den Service gespeichert.
    /// </summary>
    private async Task UpdateAsync()
    {
        if (SelectedPerson is null)
        {
            StatusMessage = "⚠ Bitte eine Person auswählen";
            return;
        }

        SelectedPerson.Name = Name.Trim();
        SelectedPerson.Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim();

        await _personService.UpdateAsync(SelectedPerson);
        await LoadAsync();

        StatusMessage = $"✏️ Aktualisiert: {SelectedPerson.Name}";
    }

    /// <summary>
    /// Löscht die aktuell ausgewählte Person.
    /// Nach dem Löschen wird die Liste neu geladen und die Eingabefelder werden geleert.
    /// </summary>
    private async Task DeleteAsync()
    {
        if (SelectedPerson is null)
        {
            StatusMessage = "⚠ Bitte eine Person auswählen";
            return;
        }

        await _personService.DeleteAsync(SelectedPerson);
        await LoadAsync();

        StatusMessage = $"🗑️ Gelöscht: {SelectedPerson.Name}";
        Name = string.Empty;
        Email = string.Empty;
        SelectedPerson = null;
    }

    #endregion Command-Methoden

    #region INotifyPropertyChanged

    /// <summary>
    /// Wird von WPF ausgewertet, um UI-Updates bei Property-Änderungen auszulösen.
    /// Jede Änderung an einer gebundenen Property sollte OnPropertyChanged aufrufen.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Hilfsmethode zum Auslösen von PropertyChanged.
    /// Der CallerMemberName-Parameter übernimmt automatisch den Property-Namen,
    /// sodass beim Aufruf kein String-Literal nötig ist (vermeidet Tippfehler bei Refactorings).
    /// </summary>
    /// <param name="propertyName">Name der geänderten Property (optional, wird i.d.R. automatisch gesetzt).</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion INotifyPropertyChanged
}