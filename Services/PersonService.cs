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
    /// </summary>
    /// <param name="includeAddresses">
    /// true = Adressen werden per Include mitgeladen (Person + Addresses),
    /// false = es werden nur Person-Daten geladen (Addresses können später nachgeladen werden).
    /// </param>
    /// <returns>Liste aller Personen aus der Datenbank.</returns>
    public async Task<List<Person>> GetAllAsync(bool includeAddresses = false)
    {
        IQueryable<Person> query = _context.People;

        if (includeAddresses)
        {
            // Eager Loading der Adressen, damit im UI ohne weitere DB-Abfragen
            // auf Person.Addresses zugegriffen werden kann.
            query = query.Include(p => p.Addresses);
        }

        return await query
            .OrderBy(p => p.Name)           // Sortierung zentral hier, damit alle Aufrufer konsistent dieselbe Reihenfolge erhalten.
            .ToListAsync()                  // Datenbankzugriff als asynchrone Operation.
            .ConfigureAwait(false);         // Verhindert Deadlocks in bestimmten Synchronisierungskontexten, ist in WPF aber vor allem „best practice“.
    }

    /// <summary>
    /// Lädt eine einzelne Person anhand der Id.
    /// Optional können auch die zugehörigen Adressen mitgeladen werden.
    /// </summary>
    /// <param name="id">Primärschlüssel der Person.</param>
    /// <param name="includeAddresses">
    /// true = Adressen werden mitgeladen, false = nur Person-Stammdaten.
    /// </param>
    public async Task<Person?> GetByIdAsync(int id, bool includeAddresses = false)
    {
        IQueryable<Person> query = _context.People;

        if (includeAddresses)
        {
            query = query.Include(p => p.Addresses);
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
    /// (z.B. via GetAllAsync oder GetByIdAsync geladen), damit sie bereits getrackt ist.
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
    /// Durch das in AppDbContext konfigurierte DeleteBehavior.Cascade
    /// werden automatisch alle zugehörigen Adressen mit gelöscht.
    /// </summary>
    /// <param name="person">Zu löschende Person.</param>
    public async Task DeleteAsync(Person person)
    {
        // Remove markiert die Entität im Change-Tracker als Deleted.
        // Die tatsächliche Löschung in der Datenbank erfolgt erst bei SaveChangesAsync.
        _context.People.Remove(person);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    #endregion PERSONEN

    #region ADRESSEN

    /// <summary>
    /// Fügt einer bestehenden Person eine neue Adresse hinzu.
    /// - Setzt CreatedAt zentral hier.
    /// - Erwartet, dass PersonId korrekt gesetzt ist oder Person referenziert wird.
    /// </summary>
    /// <param name="address">Neue Adresse, die der Person zugeordnet werden soll.</param>
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
    /// <param name="personId">Primärschlüssel der Person.</param>
    public async Task<List<Address>> GetAddressesForPersonAsync(int personId)
    {
        return await _context.Addresses
            .Where(a => a.PersonId == personId)
            .OrderBy(a => a.City).ThenBy(a => a.Street) // einfache, nachvollziehbare Sortierung
            .ToListAsync()
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Aktualisiert eine bestehende Adresse.
    /// Die Instanz sollte idealerweise aus dem aktuellen Kontext stammen,
    /// damit nur SaveChangesAsync notwendig ist.
    /// </summary>
    /// <param name="address">Geänderte Adresse.</param>
    public async Task UpdateAddressAsync(Address address)
    {
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Löscht eine Adresse aus der Datenbank.
    /// Hat keinen Einfluss auf die verknüpfte Person;
    /// nur der Adressdatensatz wird entfernt.
    /// </summary>
    /// <param name="address">Zu löschende Adresse.</param>
    public async Task DeleteAddressAsync(Address address)
    {
        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    #endregion ADRESSEN
}