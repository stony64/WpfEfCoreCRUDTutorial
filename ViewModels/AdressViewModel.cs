// View/Models/AddressViewModel.cs
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfEfCoreCRUDTutorial.Commands;
using WpfEfCoreCRUDTutorial.Models;
using WpfEfCoreCRUDTutorial.Services;

namespace WpfEfCoreCRUDTutorial.ViewModels;

/// <summary>
/// ViewModel für die Adress-Verwaltung (Detail-Teil im Master-Detail-Szenario).
/// Verantwortlichkeiten:
/// - Verwaltet alle Adressen zur aktuell ausgewählten Person
/// - Stellt Eingabefelder und Commands für CRUD-Operationen auf Address bereit
/// - Wird vom MainViewModel über SetCurrentPersonAsync gesteuert
///
/// WICHTIG:
/// Dieses ViewModel kennt keine EF-Core-Details.
/// Es arbeitet ausschließlich mit dem PersonService und domänenspezifischen Modellen (Address).
/// </summary>
public class AddressViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Service zum Laden, Anlegen, Aktualisieren und Löschen
    /// von Personen und deren Adressen.
    /// Die eigentliche Datenzugriffslogik liegt nicht im ViewModel.
    /// </summary>
    private readonly PersonService _personService;

    /// <summary>
    /// Id der aktuell ausgewählten Person, für die Adressen verwaltet werden.
    /// Wert 0 bedeutet: Es ist keine Person ausgewählt.
    /// Der Wert wird über SetCurrentPersonAsync gesetzt.
    /// </summary>
    private int _currentPersonId;

    /// <summary>
    /// Anzeigename der aktuellen Person für das Adress-Detailfenster.
    /// Wird im Header des PersonAddressDetailsWindow angezeigt.
    /// </summary>
    private string _currentPersonName = string.Empty;

    /// <summary>
    /// Öffentliche Property für den Namen der aktuellen Person.
    /// Bindet im Detailfenster an einen TextBlock („Adressen für: {Name}“).
    /// </summary>
    public string CurrentPersonName
    {
        get => _currentPersonName;
        set
        {
            _currentPersonName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Konstruktor: erhält den PersonService über Dependency Injection
    /// und initialisiert die Commands.
    /// </summary>
    /// <param name="personService">Service für Datenzugriffe auf Person/Address.</param>
    public AddressViewModel(PersonService personService)
    {
        _personService = personService;

        // Commands mit den asynchronen Methoden dieses ViewModels verbinden.
        // Die AsyncRelayCommands sorgen dafür, dass die Methoden asynchron ausgeführt werden
        // und währenddessen die zugehörigen Buttons deaktiviert sind.
        LoadAddressesCommand = new AsyncRelayCommand(LoadAddressesAsync);
        CreateAddressCommand = new AsyncRelayCommand(CreateAddressAsync);
        UpdateAddressCommand = new AsyncRelayCommand(UpdateAddressAsync);
        DeleteAddressCommand = new AsyncRelayCommand(DeleteAddressAsync);
    }

    #region Properties (für Bindings)

    /// <summary>
    /// Interne Sammlung der aktuell geladenen Adressen zur gewählten Person.
    /// </summary>
    private ObservableCollection<Address> _addresses = new();

    /// <summary>
    /// Alle Adressen der aktuell ausgewählten Person.
    /// Wird z.B. an eine ListBox oder ein DataGrid gebunden.
    ///
    /// Beispiel in XAML:
    ///   &lt;ListBox ItemsSource="{Binding Addresses}"
    ///             SelectedItem="{Binding SelectedAddress}" /&gt;
    /// </summary>
    public ObservableCollection<Address> Addresses
    {
        get => _addresses;
        set
        {
            _addresses = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Interne Referenz auf die aktuell ausgewählte Adresse.
    /// </summary>
    private Address? _selectedAddress;

    /// <summary>
    /// Aktuell ausgewählte Adresse.
    /// Wird an SelectedItem der Adress-ListBox gebunden.
    /// Beim Setzen werden die Eingabefelder (Street, PostalCode, City, Country)
    /// und der Status synchronisiert, damit der Benutzer sieht, welche Adresse er bearbeitet.
    /// </summary>
    public Address? SelectedAddress
    {
        get => _selectedAddress;
        set
        {
            _selectedAddress = value;
            OnPropertyChanged();

            if (value != null)
            {
                // Felder mit den Werten der ausgewählten Adresse füllen.
                Street = value.Street;
                PostalCode = value.PostalCode ?? string.Empty;
                City = value.City;
                Country = value.Country ?? string.Empty;
                StatusMessage = $"Adresse ausgewählt: {value.Street}, {value.City}.";
            }
            else
            {
                // Wenn keine Adresse ausgewählt ist, Eingabefelder zurücksetzen.
                Street = string.Empty;
                PostalCode = string.Empty;
                City = string.Empty;
                Country = string.Empty;
                StatusMessage = "Bitte Adresse auswählen oder neu anlegen.";
            }
        }
    }

    /// <summary>
    /// Puffer für Straße/Hausnummer.
    /// </summary>
    private string _street = string.Empty;

    /// <summary>
    /// Straße/Hausnummer-Eingabefeld für die UI.
    /// Wird an die entsprechende TextBox gebunden.
    /// </summary>
    public string Street
    {
        get => _street;
        set
        {
            _street = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Puffer für die optionale Postleitzahl.
    /// </summary>
    private string _postalCode = string.Empty;

    /// <summary>
    /// Postleitzahl-Eingabefeld für die UI.
    /// Kann leer sein; im Modell wird dies dann als null gespeichert.
    /// </summary>
    public string PostalCode
    {
        get => _postalCode;
        set
        {
            _postalCode = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Puffer für den Ort.
    /// </summary>
    private string _city = string.Empty;

    /// <summary>
    /// Ort/Stadt-Eingabefeld für die UI.
    /// Wird an die entsprechende TextBox gebunden.
    /// </summary>
    public string City
    {
        get => _city;
        set
        {
            _city = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Puffer für das optionale Land.
    /// </summary>
    private string _country = string.Empty;

    /// <summary>
    /// Land-Eingabefeld für die UI.
    /// Kann leer sein; im Modell wird dies dann als null gespeichert.
    /// </summary>
    public string Country
    {
        get => _country;
        set
        {
            _country = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Interner Status-Text, der z.B. in einer separaten StatusBar für Adressen angezeigt wird.
    /// </summary>
    private string _statusMessage = "Bereit (Adressen)";

    /// <summary>
    /// Statuszeile für adressbezogene Meldungen (Laden, Fehler, Erfolg).
    ///
    /// Beispiel in XAML:
    ///   &lt;StatusBarItem Content="{Binding StatusMessage}" /&gt;
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
    /// Lädt alle Adressen zur aktuellen Person.
    /// </summary>
    public IAsyncCommand LoadAddressesCommand { get; }

    /// <summary>
    /// Legt eine neue Adresse zur aktuellen Person an.
    /// </summary>
    public IAsyncCommand CreateAddressCommand { get; }

    /// <summary>
    /// Speichert Änderungen an der ausgewählten Adresse.
    /// </summary>
    public IAsyncCommand UpdateAddressCommand { get; }

    /// <summary>
    /// Löscht die ausgewählte Adresse.
    /// </summary>
    public IAsyncCommand DeleteAddressCommand { get; }

    #endregion Commands

    #region Öffentliche API für MainViewModel

    /// <summary>
    /// Wird vom aufrufenden MainViewModel gesetzt,
    /// wenn der Benutzer im PersonViewModel eine andere Person auswählt.
    ///
    /// Ablauf:
    /// - Setzt die interne _currentPersonId
    /// - Setzt CurrentPersonName für die Anzeige im Detailfenster
    /// - Lädt die zugehörigen Adressen über LoadAddressesAsync
    /// - oder leert die Listen, wenn person == null ist
    /// </summary>
    /// <param name="person">Die aktuell ausgewählte Person oder null.</param>
    public async Task SetCurrentPersonAsync(Person? person)
    {
        if (person is null)
        {
            _currentPersonId = 0;
            CurrentPersonName = string.Empty;
            Addresses = new ObservableCollection<Address>();
            SelectedAddress = null;
            StatusMessage = "Keine Person ausgewählt – keine Adressen.";
            return;
        }

        _currentPersonId = person.Id;
        CurrentPersonName = person.Name;
        await LoadAddressesAsync();
    }

    #endregion Öffentliche API für MainViewModel

    #region Command-Methoden (Business-Logik für Adress-Buttons)

    /// <summary>
    /// Lädt alle Adressen zur aktuellen Person.
    /// Wird typischerweise aufgerufen, wenn sich die ausgewählte Person ändert
    /// oder wenn der Benutzer die Adressen explizit neu laden möchte.
    /// </summary>
    private async Task LoadAddressesAsync()
    {
        if (_currentPersonId == 0)
        {
            Addresses = new ObservableCollection<Address>();
            StatusMessage = "Keine Person ausgewählt – keine Adressen zu laden.";
            return;
        }

        try
        {
            var addresses = await _personService.GetAddressesForPersonAsync(_currentPersonId);
            Addresses = new ObservableCollection<Address>(addresses);
            StatusMessage = $"{Addresses.Count} Adressen geladen.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler beim Laden der Adressen: {ex.Message}";
        }
    }

    /// <summary>
    /// Erstellt eine neue Adresse für die aktuell ausgewählte Person.
    /// Verwendet die Eingabefelder Street, PostalCode, City, Country.
    /// </summary>
    private async Task CreateAddressAsync()
    {
        if (_currentPersonId == 0)
        {
            StatusMessage = "Bitte zuerst eine Person auswählen.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Street) || string.IsNullOrWhiteSpace(City))
        {
            StatusMessage = "Straße und Ort sind Pflichtfelder.";
            return;
        }

        var address = new Address
        {
            PersonId = _currentPersonId,
            Street = Street.Trim(),
            PostalCode = string.IsNullOrWhiteSpace(PostalCode) ? null : PostalCode.Trim(),
            City = City.Trim(),
            Country = string.IsNullOrWhiteSpace(Country) ? null : Country.Trim()
        };

        try
        {
            await _personService.AddAddressAsync(address);
            await LoadAddressesAsync();

            StatusMessage = $"Adresse hinzugefügt: {address.Street}, {address.City}.";
            Street = string.Empty;
            PostalCode = string.Empty;
            City = string.Empty;
            Country = string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler beim Anlegen der Adresse: {ex.Message}";
        }
    }

    /// <summary>
    /// Aktualisiert die ausgewählte Adresse mit den aktuellen Eingabefeld-Werten.
    /// </summary>
    private async Task UpdateAddressAsync()
    {
        if (SelectedAddress is null)
        {
            StatusMessage = "Bitte eine Adresse auswählen.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Street) || string.IsNullOrWhiteSpace(City))
        {
            StatusMessage = "Straße und Ort sind Pflichtfelder.";
            return;
        }

        SelectedAddress.Street = Street.Trim();
        SelectedAddress.PostalCode = string.IsNullOrWhiteSpace(PostalCode) ? null : PostalCode.Trim();
        SelectedAddress.City = City.Trim();
        SelectedAddress.Country = string.IsNullOrWhiteSpace(Country) ? null : Country.Trim();

        var streetBefore = SelectedAddress.Street;
        var cityBefore = SelectedAddress.City;

        try
        {
            await _personService.UpdateAddressAsync(SelectedAddress);
            await LoadAddressesAsync();

            StatusMessage = $"Adresse aktualisiert: {streetBefore}, {cityBefore}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler beim Aktualisieren der Adresse: {ex.Message}";
        }
    }

    /// <summary>
    /// Löscht die aktuell ausgewählte Adresse.
    /// </summary>
    private async Task DeleteAddressAsync()
    {
        if (SelectedAddress is null)
        {
            StatusMessage = "Bitte eine Adresse auswählen.";
            return;
        }

        var street = SelectedAddress.Street;
        var city = SelectedAddress.City;

        try
        {
            await _personService.DeleteAddressAsync(SelectedAddress);
            await LoadAddressesAsync();

            StatusMessage = $"Adresse gelöscht: {street}, {city}.";
            Street = string.Empty;
            PostalCode = string.Empty;
            City = string.Empty;
            Country = string.Empty;
            SelectedAddress = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fehler beim Löschen der Adresse: {ex.Message}";
        }
    }

    #endregion Command-Methoden (Business-Logik für Adress-Buttons)

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