// Views/PersonAddressDetailsWindow.xaml.cs
using System.Windows;
using WpfEfCoreCRUDTutorial.ViewModels;

namespace WpfEfCoreCRUDTutorial;

/// <summary>
/// Detailfenster für Adressdaten.
/// - Zeigt alle Adressen zur aktuell ausgewählten Person an
/// - Ermöglicht das Anlegen, Bearbeiten und Löschen von Adressen
/// - Arbeitet ausschließlich mit dem AddressViewModel als DataContext
/// Die Auswahl der Person und das initiale Laden der Adressen
/// werden vor dem Öffnen im MainWindow/MainViewModel vorgenommen.
/// </summary>
public partial class PersonAddressDetailsWindow : Window
{
    /// <summary>
    /// Konstruktor des Detailfensters.
    /// Das <see cref="AddressViewModel"/> wird vom DI-Container injiziert,
    /// sodass das Fenster selbst nicht wissen muss, wie das ViewModel erzeugt wird.
    /// </summary>
    /// <param name="addressViewModel">
    /// ViewModel, das:
    /// - die Adressliste (Addresses, SelectedAddress)
    /// - die Eingabefelder (Street, PostalCode, City, Country)
    /// - sowie die Commands (Create/Update/Delete) bereitstellt.
    /// Die Zuordnung zur aktuell ausgewählten Person wird vor dem Öffnen
    /// im MainWindow vorgenommen (SetCurrentPersonAsync).
    /// </param>
    public PersonAddressDetailsWindow(AddressViewModel addressViewModel)
    {
        InitializeComponent();

        // AddressViewModel als DataContext setzen:
        // Alle Bindings in PersonAddressDetailsWindow.xaml (z.B. Addresses, Street, CreateAddressCommand)
        // beziehen sich ab jetzt auf dieses Objekt.
        DataContext = addressViewModel;

        // Hinweis:
        // Die Auswahl der Person und das initiale Laden der Adressen
        // wird im MainWindow erledigt, bevor das Fenster geöffnet wird.
        // Dadurch bleibt dieses Fenster frei von „globalem Kontext“ und kennt nur sein ViewModel.
    }
}