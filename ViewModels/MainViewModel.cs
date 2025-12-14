using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfEfCoreCRUDTutorial.ViewModels;

/// <summary>
/// Zentrales ViewModel für das MainWindow.
/// Verantwortlichkeiten:
/// - Kapselt das Zusammenspiel von
///   - PersonViewModel (Master: Personenliste)
///   - AddressViewModel (Detail: Adressen zur ausgewählten Person)
/// - Reagiert auf Änderungen der SelectedPerson im PersonViewModel
///   und sorgt dafür, dass die passenden Adressen geladen werden.
///
/// WICHTIG:
/// MainViewModel kennt keine Datenzugriffsdetails, sondern orchestriert nur die vorhandenen ViewModels.
/// </summary>
public class MainViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// ViewModel für die Personen (Master).
    /// Wird im MainWindow z.B. im linken Bereich gebunden.
    /// </summary>
    public PersonViewModel PersonViewModel { get; }

    /// <summary>
    /// ViewModel für die Adressen (Detail).
    /// Wird im Detailfenster oder im rechten Bereich verwendet.
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
        //
        // Der Event-Handler ist async, weil SetCurrentPersonAsync eine asynchrone
        // Datenladeoperation enthält. Fehler werden innerhalb des AddressViewModel
        // über StatusMessage behandelt.
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
    ///
    /// Ablauf:
    /// - ruft das LoadCommand des PersonViewModel auf (asynchron)
    /// - optional kann danach eine erste Person ausgewählt werden,
    ///   wodurch automatisch die passenden Adressen geladen werden.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Lädt alle Personen; die SelectedPerson wird dabei i.d.R. zunächst null sein.
        // Exceptions werden im PersonViewModel (StatusMessage) behandelt.
        await PersonViewModel.LoadCommand.ExecuteAsync(null);

        // Optional: Wenn nach dem Laden automatisch die erste Person ausgewählt werden soll,
        // kannst du hier z.B. folgendes ergänzen:
        //
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

    /// <summary>
    /// Hilfsmethode zum Auslösen von PropertyChanged.
    /// Wird derzeit nicht verwendet, kann aber für eigene Properties
    /// im MainViewModel genutzt werden.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion INotifyPropertyChanged (für zukünftige Erweiterungen)
}