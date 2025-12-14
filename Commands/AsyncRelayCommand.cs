using System.Windows.Input;

namespace WpfEfCoreCRUDTutorial.Commands;

/// <summary>
/// Einfaches Async-Command-Interface,
/// damit Buttons async-Methoden aufrufen können, ohne dass der Aufrufer
/// wissen muss, ob die Logik synchron oder asynchron ist.
/// </summary>
public interface IAsyncCommand : ICommand
{
    /// <summary>
    /// Asynchrone Variante von Execute.
    /// Wird vom ViewModel direkt aufgerufen, während WPF selbst nur Execute(object) kennt.
    /// </summary>
    Task ExecuteAsync(object? parameter = null);
}

/// <summary>
/// Async-Command-Implementierung mit optionaler CanExecute-Logik und Parameter-Unterstützung.
/// Kapselt eine asynchrone Methode (Func<object?, Task>) …
/// für WPF zur Verfügung. Zusätzlich wird verhindert, dass der Benutzer
/// das gleiche Command mehrfach parallel auslöst (z.B. mehrfaches Klicken).
/// </summary>
public class AsyncRelayCommand : IAsyncCommand
{
    /// <summary>
    /// Delegat auf die auszuführende asynchrone Methode (z.B. LoadAsync im ViewModel).
    /// Der Parameter kann bei Bedarf genutzt werden, häufig wird er aber ignoriert.
    /// </summary>
    private readonly Func<object?, Task> _execute;

    /// <summary>
    /// Optionale CanExecute-Logik.
    /// Wenn nicht gesetzt, entscheidet nur der Ausführungsstatus (_isExecuting).
    /// </summary>
    private readonly Func<object?, bool>? _canExecute;

    /// <summary>
    /// Flag, ob das Command gerade ausgeführt wird.
    /// Dient dazu, CanExecute zu steuern und Mehrfachausführung zu verhindern.
    /// </summary>
    private bool _isExecuting;

    /// <summary>
    /// Erzeugt ein Async-Command ohne zusätzliche CanExecute-Logik.
    /// </summary>
    /// <param name="execute">Asynchrone Aktion, die ausgeführt werden soll.</param>
    public AsyncRelayCommand(Func<Task> execute)
        : this(_ => execute())
    {
    }

    /// <summary>
    /// Erzeugt ein Async-Command mit optionaler CanExecute-Logik und Parameter-Unterstützung.
    /// </summary>
    /// <param name="execute">Asynchrone Aktion, die ausgeführt werden soll.</param>
    /// <param name="canExecute">
    /// Optionale Bedingung, ob das Command aktuell ausgeführt werden darf
    /// (z.B. SelectedPerson != null).
    /// </param>
    public AsyncRelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Wird von WPF (z.B. Buttons) regelmäßig abgefragt, um zu entscheiden,
    /// ob das Command aktuell ausgeführt werden darf (Button enabled/disabled).
    /// Solange _isExecuting true ist, wird CanExecute false zurückgeben.
    /// Zusätzlich kann eine eigene CanExecute-Logik hinterlegt werden.
    /// </summary>
    public bool CanExecute(object? parameter)
    {
        if (_isExecuting)
        {
            return false;
        }

        return _canExecute?.Invoke(parameter) ?? true;
    }

    /// <summary>
    /// Einstiegspunkt für WPF: ruft intern die asynchrone Ausführung auf.
    /// Da ICommand.Execute kein Task zurückgeben kann, wird hier „fire and forget“
    /// verwendet und die eigentliche Arbeit an ExecuteAsync delegiert.
    /// </summary>
    public async void Execute(object? parameter)
        => await ExecuteAsync(parameter);

    /// <summary>
    /// Führt die hinterlegte asynchrone Aktion aus und steuert den _isExecuting-Status.
    /// Während der Ausführung wird CanExecute auf false gesetzt, um z.B. Buttons zu deaktivieren.
    /// Nach Abschluss wird CanExecute wieder auf true gesetzt.
    /// </summary>
    public async Task ExecuteAsync(object? parameter = null)
    {
        // Schutz gegen parallele Aufrufe (z.B. Doppelklick auf Button).
        if (!CanExecute(parameter))
        {
            return;
        }

        try
        {
            _isExecuting = true;
            RaiseCanExecuteChanged(); // UI erfährt, dass das Command vorübergehend nicht mehr ausführbar ist.
            await _execute(parameter); // die eigentliche Logik (z.B. LoadAsync) aus dem ViewModel ausführen.
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged(); // UI erfährt, dass das Command wieder ausführbar ist.
        }
    }

    /// <summary>
    /// Standardereignis aus ICommand.
    /// WPF hängt sich hieran, um Änderungen am CanExecute-Status mitzubekommen
    /// und z.B. Buttons zu aktivieren/deaktivieren.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Hilfsmethode zum Auslösen von CanExecuteChanged.
    /// Sollte immer dann aufgerufen werden, wenn sich der Ausführbarkeitszustand
    /// des Commands ändert (hier: bei Start/Ende der Ausführung).
    /// </summary>
    private void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}