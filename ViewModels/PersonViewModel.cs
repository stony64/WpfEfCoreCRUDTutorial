using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WpfEfCoreCRUDTutorial.Commands;
using WpfEfCoreCRUDTutorial.Models;
using WpfEfCoreCRUDTutorial.Services;

namespace WpfEfCoreCRUDTutorial.ViewModels;

/// <summary>
/// ViewModel für das MainWindow:
/// - Hält die angezeigten Personen
/// - Nimmt Eingaben aus TextBoxen entgegen (Name, Email)
/// - Bietet Commands für Laden, Erstellen, Aktualisieren, Löschen
/// </summary>
public class PersonViewModel : INotifyPropertyChanged
{
    private readonly PersonService _personService;

    /// <summary>
    /// Konstruktor: erhält den PersonService über Dependency Injection
    /// und initialisiert Commands.
    /// </summary>
    /// <param name="personService">Service für alle Person-bezogenen Datenzugriffe.</param>
    public PersonViewModel(PersonService personService)
    {
        _personService = personService;

        // Commands mit Methoden verbinden
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        CreateCommand = new AsyncRelayCommand(CreateAsync);
        UpdateCommand = new AsyncRelayCommand(UpdateAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
    }

    #region Properties (für Bindings)

    private ObservableCollection<Person> _people = new();

    /// <summary>
    /// Sammlung aller aktuell geladenen Personen.
    /// Wird an die ListBox gebunden (ItemsSource).
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

    private Person? _selectedPerson;

    /// <summary>
    /// Aktuell ausgewählte Person in der UI.
    /// Wird an SelectedItem der ListBox gebunden.
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
                Name = value.Name;
                Email = value.Email ?? string.Empty;
                StatusMessage = $"👆 Ausgewählt: {value.Name}";
            }
            else
            {
                Name = string.Empty;
                Email = string.Empty;
                StatusMessage = "📋 Bitte Person auswählen";
            }
        }
    }

    private string _name = string.Empty;

    /// <summary>
    /// Name-Eingabefeld für die UI.
    /// Wird an die Name-TextBox gebunden.
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

    private string? _email;

    /// <summary>
    /// Email-Eingabefeld für die UI.
    /// Wird an die Email-TextBox gebunden.
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

    private string _statusMessage = "Bereit";

    /// <summary>
    /// Statuszeile unten im Fenster (StatusBar).
    /// Zeigt z.B. Ladezustände, Fehler oder Erfolgsnachrichten an.
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

    #endregion

    #region Commands

    public IAsyncCommand LoadCommand { get; }

    public IAsyncCommand CreateCommand { get; }

    public IAsyncCommand UpdateCommand { get; }

    public IAsyncCommand DeleteCommand { get; }

    #endregion

    #region Command-Methoden

    private async Task LoadAsync()
    {
        var people = await _personService.GetAllAsync();
        People = new ObservableCollection<Person>(people);
        StatusMessage = $"📋 {People.Count} Personen geladen";
    }

    private async Task CreateAsync()
    {
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

        await _personService.CreateAsync(person);
        await LoadAsync();

        StatusMessage = $"➕ Neu erstellt: {person.Name} (ID: {person.Id})";
        Name = string.Empty;
        Email = string.Empty;
    }

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
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}
