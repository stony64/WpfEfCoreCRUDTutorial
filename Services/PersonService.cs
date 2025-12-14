using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WpfEfCoreCRUDTutorial.Data;
using WpfEfCoreCRUDTutorial.Models;

namespace WpfEfCoreCRUDTutorial.Services;

/// <summary>
/// Service-Schicht für alle CRUD-Operationen auf <see cref="Person"/>.
/// Kapselt den EF Core DbContext, damit ViewModels nicht direkt mit Datenbank-Details arbeiten müssen.
/// </summary>
public class PersonService
{
    /// <summary>
    /// EF Core DbContext-Instanz (per DI injiziert, z.B. Scoped Lifetime).
    /// </summary>
    private readonly AppDbContext _context;

    /// <summary>
    /// Konstruktor mit Dependency Injection.
    /// </summary>
    /// <param name="context">Anwendungs-DbContext für den Datenzugriff.</param>
    public PersonService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Liefert alle Personen sortiert nach Name.
    /// Async-Variante vermeidet UI-Blockaden in WPF.
    /// </summary>
    /// <returns>Liste aller Personen aus der Datenbank.</returns>
    public async Task<List<Person>> GetAllAsync()
    {
        return await _context.People
            .OrderBy(p => p.Name)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Erstellt eine neue Person und speichert sie in der Datenbank.
    /// - Setzt den CreatedAt-Zeitstempel zentral in der Service-Schicht (UTC).
    /// - Erwartet ein bereits validiertes <see cref="Person"/>-Objekt (z.B. durch ViewModel/Validation).
    /// </summary>
    /// <param name="person">Neue Person, die angelegt werden soll.</param>
    public async Task CreateAsync(Person person)
    {
        // CreatedAt immer zentral hier setzen, damit alle Aufrufer denselben Pfad nutzen.
        person.CreatedAt = DateTime.UtcNow;

        _context.People.Add(person);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Aktualisiert eine bestehende Person in der Datenbank.
    /// Die übergebene Instanz sollte aus dem aktuellen Kontext stammen (z.B. via GetAllAsync).
    /// </summary>
    /// <param name="person">Geänderte Person.</param>
    public async Task UpdateAsync(Person person)
    {
        // Für dein aktuelles Szenario reicht SaveChanges, weil die Instanz getrackt ist.
        // Bei detached Entities könnte man explizit markieren:
        // _context.Entry(person).State = EntityState.Modified;

        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Löscht eine bestehende Person aus der Datenbank.
    /// </summary>
    /// <param name="person">Zu löschende Person.</param>
    public async Task DeleteAsync(Person person)
    {
        _context.People.Remove(person);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }
}
