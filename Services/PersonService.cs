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
/// Kapselt den EF-Core-DbContext, damit ViewModels nicht direkt mit Datenbank-Details
/// (DbContext, DbSet, SQL) arbeiten müssen, sondern nur fachliche Methoden konsumieren.
/// </summary>
public class PersonService
{
    /// <summary>
    /// EF-Core-DbContext-Instanz (per DI injiziert, z.B. Scoped Lifetime).
    /// Der Service selbst enthält keine ConnectionStrings und kennt nur den Kontext.
    /// </summary>
    private readonly AppDbContext _context;

    /// <summary>
    /// Konstruktor mit Dependency Injection.
    /// Der DI-Container erzeugt den AppDbContext und übergibt ihn hier,
    /// sodass dieser Service in Tests auch leicht mit einem InMemory-Kontext ausgetauscht werden kann.
    /// </summary>
    /// <param name="context">Anwendungs-DbContext für den Datenzugriff.</param>
    public PersonService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Liefert alle Personen sortiert nach Name.
    /// Async-Variante vermeidet UI-Blockaden in WPF, weil der Datenbankzugriff nicht
    /// im UI-Thread erfolgt und der Aufrufer "await" verwenden kann.
    /// </summary>
    /// <returns>Liste aller Personen aus der Datenbank.</returns>
    public async Task<List<Person>> GetAllAsync()
    {
        return await _context.People
            .OrderBy(p => p.Name)           // Sortierung zentral hier, damit alle Aufrufer konsistent dieselbe Reihenfolge erhalten.
            .ToListAsync()                  // Datenbankzugriff als asynchrone Operation.
            .ConfigureAwait(false);         // Verhindert Deadlocks in bestimmten Synchronisierungskontexten, ist in WPF aber vor allem „best practice“.
    }

    /// <summary>
    /// Erstellt eine neue Person und speichert sie in der Datenbank.
    /// - Setzt den CreatedAt-Zeitstempel zentral in der Service-Schicht (UTC),
    ///   damit nicht jedes ViewModel an diese Regel denken muss.
    /// - Erwartet ein bereits validiertes <see cref="Person"/>-Objekt (z.B. durch ViewModel/Validation).
    /// </summary>
    /// <param name="person">Neue Person, die angelegt werden soll.</param>
    public async Task CreateAsync(Person person)
    {
        // CreatedAt immer zentral hier setzen:
        // So ist garantiert, dass jeder Datensatz einen konsistent ermittelten Zeitstempel bekommt
        // und diese Logik nicht mehrfach im UI kopiert werden muss.
        person.CreatedAt = DateTime.UtcNow;

        _context.People.Add(person);        // Entity dem Change-Tracker hinzufügen (State = Added).
        await _context.SaveChangesAsync()   // Eine Transaktion für alle ausstehenden Änderungen ausführen.
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Aktualisiert eine bestehende Person in der Datenbank.
    /// Die übergebene Instanz sollte in der Regel aus dem aktuellen Kontext stammen
    /// (z.B. via GetAllAsync geladen), damit sie bereits getrackt ist.
    /// </summary>
    /// <param name="person">Geänderte Person.</param>
    public async Task UpdateAsync(Person person)
    {
        // In deinem Szenario ist die Person-Instanz typischerweise bereits „tracked“,
        // weil sie über den DbContext geladen wurde und im ViewModel weitergereicht wird.
        // Deshalb reicht SaveChangesAsync, EF erkennt die geänderten Properties automatisch.
        //
        // Würdest du eine „entkoppelte“ (detached) Instanz übergeben, müsstest du sie explizit anhängen:
        // _context.Entry(person).State = EntityState.Modified;

        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Löscht eine bestehende Person aus der Datenbank.
    /// </summary>
    /// <param name="person">Zu löschende Person.</param>
    public async Task DeleteAsync(Person person)
    {
        // Remove markiert die Entität im Change-Tracker als Deleted.
        // Die tatsächliche Löschung in der Datenbank erfolgt erst bei SaveChangesAsync.
        _context.People.Remove(person);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }
}
