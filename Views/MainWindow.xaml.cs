using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using WpfEfCoreCRUDTutorial.ViewModels;

namespace WpfEfCoreCRUDTutorial;

/// <summary>
/// Hauptfenster der Anwendung (View in MVVM).
/// - Enthält keine Business- oder Datenzugriffslogik
/// - Erhält sein MainViewModel über Dependency Injection
/// - Bindet Personen- und Adressbereich über das kombinierte MainViewModel
/// Zusätzlich kann von hier aus ein Detailfenster (UserDetailsWindow) geöffnet werden,
/// in dem Adressen zur ausgewählten Person bearbeitet werden.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Referenz auf das zentrale MainViewModel.
    /// Wird für DataBinding (DataContext) und für Logik wie das Öffnen des Detailfensters verwendet.
    /// </summary>
    private readonly MainViewModel _mainViewModel;

    /// <summary>
    /// Konstruktor des MainWindow.
    /// Das <see cref="MainViewModel"/> wird vom DI-Container injiziert.
    /// Dadurch muss das Fenster nicht wissen, wie das ViewModel erzeugt oder verkabelt wird.
    /// </summary>
    /// <param name="viewModel">
    /// Zentrales ViewModel, das sowohl das PersonViewModel (Master)
    /// als auch das AddressViewModel (Detail) enthält und das Zusammenspiel koordiniert.
    /// </param>
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        // ViewModel zwischenspeichern, um später darauf zugreifen zu können
        // (z.B. im Button-Click-Handler für das Öffnen des Detailfensters).
        _mainViewModel = viewModel;

        // ViewModel als DataContext setzen:
        // Alle Bindings in MainWindow.xaml (z.B. PersonViewModel.*, AddressViewModel.*)
        // beziehen sich ab jetzt auf dieses Objekt.
        DataContext = viewModel;

        // Hinweis:
        // Die Initialisierung (Laden der Personen etc.) erfolgt aktuell in App.xaml.cs
        // über mainViewModel.InitializeAsync().
        // Alternativ könnte dieser Aufruf auch hier erfolgen, um Logik näher an die View zu bringen,
        // für dieses Tutorial wird die Startlogik jedoch bewusst im Application-Bootstrapper gebündelt.
    }

    /// <summary>
    /// Event-Handler für den "User-Details"-Button.
    /// Öffnet ein separates Fenster (UserDetailsWindow), in dem die Adressen
    /// zur aktuell ausgewählten Person angezeigt und bearbeitet werden können.
    /// Implementiert Variante b): explizite Laufzeit-Prüfung auf null für App.Services.
    /// </summary>
    private void UserDetailsButton_Click(object sender, RoutedEventArgs e)
    {
        // Sicherstellen, dass eine Person ausgewählt ist.
        // Ohne ausgewählte Person ergibt die Adressbearbeitung keinen Sinn.
        if (_mainViewModel.PersonViewModel.SelectedPerson is null)
        {
            MessageBox.Show(
                "Bitte zuerst eine Person auswählen.",
                "Hinweis",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        // Laufzeit-Prüfung: Wurde der DI-Container (Services) korrekt initialisiert?
        // Das passiert normalerweise in App.OnStartup. Falls nicht, wird hier sauber abgebrochen.
        if (App.Services is null)
        {
            MessageBox.Show(
                "Der DI-Container (App.Services) ist nicht initialisiert. " +
                "Stellen Sie sicher, dass der Generic Host in App.xaml.cs korrekt aufgebaut wurde.",
                "Initialisierungsfehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        // AddressViewModel aus dem DI-Container holen,
        // damit es dieselben Services (PersonService, DbContext) verwendet
        // und korrekt in den Lebenszyklus eingebunden ist.
        var addressViewModel = App.Services.GetRequiredService<AddressViewModel>();

        // Die aktuell ausgewählte Person in das neue AddressViewModel „übergeben“,
        // sodass dort sofort die richtigen Adressen geladen werden.
        _ = addressViewModel.SetCurrentPersonAsync(_mainViewModel.PersonViewModel.SelectedPerson);

        // Detailfenster über DI erzeugen, damit der Konstruktor das AddressViewModel
        // automatisch injizieren kann (UserDetailsWindow(AddressViewModel vm)).
        var detailsWindow = App.Services.GetRequiredService<UserDetailsWindow>();

        // Owner setzen, damit das Detailfenster an das Hauptfenster „angedockt“ ist
        // (z.B. für gemeinsame Minimierung/Maximierung und Z-Order).
        detailsWindow.Owner = this;

        // Fenster anzeigen. Je nach Anforderung:
        // - Show()       → nicht-modales Fenster, Benutzer kann weiterhin im Hauptfenster arbeiten.
        // - ShowDialog() → modales Fenster, blockiert das Hauptfenster bis zum Schließen.
        detailsWindow.Show();
    }
}