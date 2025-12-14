using System;
using System.Threading.Tasks;
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
/// Einfache Async-Command-Implementierung.
/// Kapselt eine asynchrone Methode (Func;Task;) und stellt sie als ICommand
/// für WPF zur Verfügung. Zusätzlich wird verhindert, dass der Benutzer
/// das gleiche Command mehrfach parallel auslöst (z.B. mehrfaches Klicken).
/// </summary>
public class AsyncRelayCommand : IAsyncCommand
{
    /// <summary>
    /// Delegat auf die auszuführende asynchrone Methode (z.B. LoadAsync im ViewModel).
    /// </summary>
    private readonly Func<Task> _execute;

    /// <summary>
    /// Flag, ob das Command gerade ausgeführt wird.
    /// Dient dazu, CanExecute zu steuern und Mehrfachausführung zu verhindern.
    /// </summary>
    private bool _isExecuting;

    /// <summary>
    /// Übergibt die auszuführende async-Methode beim Erzeugen des Commands.
    /// So kann jedes ViewModel seine eigene Logik injizieren, ohne
    /// eine eigene Command-Klasse zu schreiben.
    /// </summary>
    public AsyncRelayCommand(Func<Task> execute)
    {
        _execute = execute;
    }

    /// <summary>
    /// Wird von WPF (z.B. Buttons) regelmäßig abgefragt, um zu entscheiden,
    /// ob das Command aktuell ausgeführt werden darf (Button enabled/disabled).
    /// Solange _isExecuting true ist, wird CanExecute false zurückgeben.
    /// </summary>
    public bool CanExecute(object? parameter) => !_isExecuting;

    /// <summary>
    /// Einstiegspunkt für WPF: ruft intern die asynchrone Ausführung auf.
    /// Da ICommand.Execute kein Task zurückgeben kann, wird hier "fire and forget"
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
        if (_isExecuting)
        {
            return;
        }

        try
        {
            _isExecuting = true;
            RaiseCanExecuteChanged(); // UI erfährt, dass das Command vorübergehend nicht mehr ausführbar ist.
            await _execute();         // die eigentliche Logik (z.B. LoadAsync) aus dem ViewModel ausführen.
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