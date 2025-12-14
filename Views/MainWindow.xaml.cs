using System.Windows;
using WpfEfCoreCRUDTutorial.ViewModels;

namespace WpfEfCoreCRUDTutorial;

/// <summary>
/// Hauptfenster der Anwendung (View in MVVM).
/// Enthält keine Business- oder Datenzugriffslogik,
/// sondern erhält sein ViewModel über Dependency Injection.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Konstruktor des MainWindow.
    /// Das <see cref="PersonViewModel"/> wird vom DI-Container injiziert.
    /// </summary>
    /// <param name="viewModel">
    /// ViewModel, das alle anzuzeigenden Daten (People, SelectedPerson, Name, Email)
    /// und die Commands (Load/Create/Update/Delete) bereitstellt.
    /// </param>
    public MainWindow(PersonViewModel viewModel)
    {
        InitializeComponent();

        // ViewModel als DataContext setzen:
        // Alle Bindings in MainWindow.xaml beziehen sich ab jetzt auf dieses Objekt.
        DataContext = viewModel;

        // Optional: direkt beim Start Personen laden.
        // Die Async-Command-Implementierung kümmert sich um asynchrone Ausführung.
        _ = viewModel.LoadCommand.ExecuteAsync(null);
    }
}
