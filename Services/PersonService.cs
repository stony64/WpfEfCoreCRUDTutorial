// Services/PersonService.cs
using Microsoft.EntityFrameworkCore;
using WpfEfCoreCRUDTutorial.Data;
using WpfEfCoreCRUDTutorial.Models;

namespace WpfEfCoreCRUDTutorial.Services;

/// <summary>
/// Service-Schicht für alle CRUD-Operationen auf <see cref="Person"/> und
/// den Umgang mit den zugehörigen Adressen (1:n-Beziehung).
/// Kapselt den EF-Core-DbContext, damit ViewModels nicht direkt mit
/// Datenbank-Details (DbContext, DbSet, SQL) arbeiten müssen.
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
    /// sodass dieser Service in Tests auch leicht mit einem InMemory-Kontext
    /// oder einer alternativen Implementierung ausgetauscht werden kann.
    /// </summary>
    /// <param name="context">Anwendungs-DbContext für den Datenzugriff.</param>
    public PersonService(AppDbContext context)
    {
        _context = context;
    }

    #region PERSONEN

    /// <summary>
    /// Liefert alle Personen sortiert nach Name.
    /// Optional können die zugehörigen Adressen direkt mitgeladen werden
    /// (Eager Loading), wenn das UI ein Master-Detail-Szenario benötigt.
    /// Async-Variante vermeidet UI-Blockaden in WPF.
    /// Für reine Anzeigezwecke werden die Entitäten als No-Tracking geladen,
    /// um den Change-Tracker zu entlasten.
    /// </summary>
    public async Task<List<Person>> GetAllAsync(bool includeAddresses = false)
    {
        IQueryable<Person> query = _context.People;

        if (includeAddresses)
        {
            query = query.Include(p => p.Addresses);
        }

        query = query.AsNoTracking();

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Lädt eine einzelne Person anhand der Id.
    /// Optional können auch die zugehörigen Adressen mitgeladen werden.
    /// Für reine Anzeigezwecke kann das Ergebnis ebenfalls ohne Tracking geladen werden.
    /// </summary>
    public async Task<Person?> GetByIdAsync(int id, bool includeAddresses = false, bool asNoTracking = true)
    {
        IQueryable<Person> query = _context.People;

        if (includeAddresses)
        {
            query = query.Include(p => p.Addresses);
        }

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query
            .FirstOrDefaultAsync(p => p.Id == id)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Erstellt eine neue Person und speichert sie in der Datenbank.
    /// - Setzt den CreatedAt-Zeitstempel zentral in der Service-Schicht (UTC),
    ///   damit nicht jedes ViewModel an diese Regel denken muss.
    /// - Erwartet ein bereits validiertes <see cref="Person"/>-Objekt (z.B. durch ViewModel/Validation).
    /// </summary>
    public async Task CreateAsync(Person person)
    {
        person.CreatedAt = DateTime.UtcNow;

        _context.People.Add(person);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Aktualisiert eine bestehende Person in der Datenbank.
    /// Die übergebene Instanz sollte in der Regel aus dem aktuellen Kontext stammen
    /// (z.B. via GetAllAsync oder GetByIdAsync ohne AsNoTracking geladen).
    /// Falls eine „entkoppelte“ Instanz übergeben wird, wird sie explizit angehängt.
    /// </summary>
    public async Task UpdateAsync(Person person)
    {
        if (_context.Entry(person).State == EntityState.Detached)
        {
            _context.Attach(person);
            _context.Entry(person).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Löscht eine bestehende Person aus der Datenbank.
    /// Durch das im AppDbContext konfigurierte DeleteBehavior.Cascade
    /// werden in der Datenbank automatisch alle zugehörigen Adressen mit gelöscht.
    /// </summary>
    public async Task DeleteAsync(Person person)
    {
        _context.People.Remove(person);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    #endregion PERSONEN

    #region ADRESSEN

    /// <summary>
    /// Fügt einer bestehenden Person eine neue Adresse hinzu.
    /// - Setzt CreatedAt zentral hier.
    /// - Erwartet, dass PersonId korrekt gesetzt ist (oder alternativ die Navigation Person),
    ///   damit die Adresse der richtigen Person zugeordnet werden kann.
    /// </summary>
    public async Task AddAddressAsync(Address address)
    {
        address.CreatedAt = DateTime.UtcNow;

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Lädt alle Adressen zu einer bestimmten Person.
    /// Diese Methode ist nützlich, wenn du im UI eine Detail-Ansicht für Adressen
    /// zur ausgewählten Person anzeigen möchtest.
    /// </summary>
    public async Task<List<Address>> GetAddressesForPersonAsync(int personId)
    {
        return await _context.Addresses
            .Where(a => a.PersonId == personId)
            .OrderBy(a => a.City).ThenBy(a => a.Street)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Aktualisiert eine bestehende Adresse.
    /// Die Instanz sollte idealerweise aus dem aktuellen Kontext stammen;
    /// falls sie detached ist, wird sie explizit angehängt.
    /// </summary>
    public async Task UpdateAddressAsync(Address address)
    {
        if (_context.Entry(address).State == EntityState.Detached)
        {
            _context.Attach(address);
            _context.Entry(address).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Löscht eine Adresse aus der Datenbank.
    /// Hat keinen Einfluss auf die verknüpfte Person;
    /// nur der Adressdatensatz wird entfernt.
    /// </summary>
    public async Task DeleteAddressAsync(Address address)
    {
        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    #endregion ADRESSEN
}