using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfEfCoreCRUDTutorial.ViewModels;

/// <summary>
/// Zentrales ViewModel für das MainWindow.
/// Kapselt das Zusammenspiel von:
/// - PersonViewModel (Master: Personenliste)
/// - AddressViewModel (Detail: Adressen zur ausgewählten Person)
/// und sorgt dafür, dass bei Personenwechsel automatisch die passenden Adressen geladen werden.
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// ViewModel für die Personen (Master).
    /// </summary>
    public PersonViewModel PersonViewModel { get; }

    /// <summary>
    /// ViewModel für die Adressen (Detail).
    /// </summary>
    public AddressViewModel AddressViewModel { get; }

    /// <summary>
    /// Konstruktor: erhält PersonViewModel und AddressViewModel via Dependency Injection.
    /// Registriert sich auf PropertyChanged von PersonViewModel, um bei Änderungen der SelectedPerson
    /// das AddressViewModel zu informieren.
    /// </summary>
    /// <param name="personViewModel">ViewModel für Personen (Master).</param>
    /// <param name="addressViewModel">ViewModel für Adressen (Detail).</param>
    public MainViewModel(PersonViewModel personViewModel, AddressViewModel addressViewModel)
    {
        PersonViewModel = personViewModel;
        AddressViewModel = addressViewModel;

        // Wenn sich die SelectedPerson im PersonViewModel ändert,
        // soll das AddressViewModel automatisch auf die neue Person umschalten
        // und deren Adressen laden.
        PersonViewModel.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName == nameof(PersonViewModel.SelectedPerson))
            {
                await AddressViewModel.SetCurrentPersonAsync(PersonViewModel.SelectedPerson);
            }
        };
    }

    /// <summary>
    /// Kann vom MainWindow beim Start aufgerufen werden,
    /// um initial die Personenliste (und indirekt die Adressen) zu laden.
    /// Zentraler Initialisierungspunkt für die Gesamt-UI.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Lädt alle Personen; die SelectedPerson wird dabei i.d.R. zunächst null sein.
        await PersonViewModel.LoadCommand.ExecuteAsync(null);

        // Optional: Wenn nach dem Laden automatisch die erste Person ausgewählt werden soll,
        // kannst du hier z.B. folgendes ergänzen:
        // if (PersonViewModel.People.Any())
        //     PersonViewModel.SelectedPerson = PersonViewModel.People.First();
        //
        // Dadurch würde der PropertyChanged-Handler oben automatisch
        // AddressViewModel.SetCurrentPersonAsync(...) aufrufen.
    }

    #region INotifyPropertyChanged (für zukünftige Erweiterungen)

    /// <summary>
    /// Aktuell nicht zwingend benötigt, aber vorbereitet,
    /// falls MainViewModel später eigene bindbare Properties erhält.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion INotifyPropertyChanged (für zukünftige Erweiterungen)
}