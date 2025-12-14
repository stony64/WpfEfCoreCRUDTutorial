using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfEfCoreCRUDTutorial.Commands;

/// <summary>
/// Einfaches Async-Command-Interface,
/// damit Buttons async-Methoden aufrufen können.
/// </summary>
public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync(object? parameter = null);
}

/// <summary>
/// Einfache Async-Command-Implementierung.
/// Kapselt eine asynchrone Methode und verhindert parallele Mehrfachausführung.
/// </summary>
public class AsyncRelayCommand : IAsyncCommand
{
    private readonly Func<Task> _execute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<Task> execute)
    {
        _execute = execute;
    }

    public bool CanExecute(object? parameter) => !_isExecuting;

    public async void Execute(object? parameter)
        => await ExecuteAsync(parameter);

    public async Task ExecuteAsync(object? parameter = null)
    {
        if (_isExecuting)
        {
            return;
        }

        try
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();
            await _execute();
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public event EventHandler? CanExecuteChanged;

    private void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
