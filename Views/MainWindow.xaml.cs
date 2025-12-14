using System.Windows;
using WpfEfCoreCRUDTutorial.ViewModels;

namespace WpfEfCoreCRUDTutorial;

/// <summary>
/// Hauptfenster der Anwendung (View in MVVM).
/// Enthält keine Business- oder Datenzugriffslogik,
/// sondern erhält sein ViewModel über Dependency Injection und arbeitet ausschließlich mit DataBinding.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Konstruktor des MainWindow.
    /// Das <see cref="PersonViewModel"/> wird vom DI-Container injiziert,
    /// sodass das Fenster selbst nicht wissen muss, wie das ViewModel erzeugt oder verkabelt wird.
    /// </summary>
    /// <param name="viewModel">
    /// ViewModel, das alle anzuzeigenden Daten (People, SelectedPerson, Name, Email)
    /// und die Commands (Load/Create/Update/Delete) bereitstellt.
    /// </param>
    public MainWindow(PersonViewModel viewModel)
    {
        InitializeComponent();

        // ViewModel als DataContext setzen:
        // Alle Bindings in MainWindow.xaml (z.B. TextBox.Text, Button.Command, ListBox.ItemsSource)
        // beziehen sich ab jetzt auf dieses Objekt.
        DataContext = viewModel;

        // Optional: direkt beim Start Personen laden.
        // Hier wird explizit das asynchrone Load-Command angestoßen, damit die Liste
        // bereits gefüllt ist, wenn das Fenster angezeigt wird.
        // Die Async-Command-Implementierung sorgt dafür, dass der UI-Thread nicht blockiert.
        _ = viewModel.LoadCommand.ExecuteAsync(null);
    }
}
