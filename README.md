# WpfEfCoreCRUDTutorial

Einfaches WPF-CRUD-Tutorial mit Entity Framework Core und MSSQL-Datenbank und einer 1:n-Beziehung.

## Voraussetzungen
- Visual Studio 2022 oder neuer (im Beispiel: 2026)
- .NET 8.0 SDK oder neuer (im Beispiel: 10.0)
- SQL Server bzw. LocalDB

## Projektbeschreibung

Dieses Projekt zeigt eine einfache WPF-Anwendung mit CRUD-Operationen (Create, Read, Update, Delete) auf einer SQL-Server-Datenbank unter Verwendung von Entity Framework Core.  
Im Domänenmodell wird eine 1:n-Beziehung zwischen `Person` und `Address` umgesetzt, die sowohl in den Entitäten als auch in der UI (Master-Detail-Ansicht) abgebildet ist.

## Technologien

- WPF (.NET Desktop)
- Entity Framework Core (Code First, SQL Server Provider)
- .NET Generic Host mit Dependency Injection und Konfiguration über `appsettings.json`
- MVVM-Architektur mit getrennten Ebenen für Models, ViewModels, Services und Views

## Projektstruktur

- `Data` – `AppDbContext`, EF-Core-Konfiguration, 1:n-Mapping Person → Adressen  
- `Models` – Domänenklassen `Person` und `Address` (inkl. Navigations-Eigenschaften und DataAnnotations)  
- `Services` – `PersonService` für Datenzugriff und Geschäftslogik (CRUD-Methoden)  
- `ViewModels` – `PersonViewModel`, `AddressViewModel`, `MainViewModel` für MVVM-Bindings  
- `Views` – `MainWindow` (Personenliste) und `PersonAddressDetailsWindow` (Adressdetails, Master-Detail)  
- `Commands` – Implementierungen für asynchrone Commands (z.B. `AsyncRelayCommand`)

## Funktionsumfang

- Anlegen, Bearbeiten, Löschen und Anzeigen von Personen  
- Anlegen, Bearbeiten, Löschen und Anzeigen von Adressen zu einer ausgewählten Person (Detailansicht)  
- Automatisches Nachladen der zugehörigen Adressen beim Wechsel der ausgewählten Person (UI-gebundene 1:n-Beziehung)

## Einrichtung

1. ConnectionString in `appsettings.json` anpassen (SQL Server, Datenbankname, Authentifizierung).  
2. Über die Package Manager Console Migrationen ausführen:  
   - `Add-Migration InitialCreate`  
   - `Update-Database`  

Dadurch werden die Tabellen `People` und `Addresses` mit Fremdschlüsselbeziehung erzeugt.

## Starten der Anwendung

Nach erfolgreicher Migration kann das Projekt in Visual Studio gestartet werden.  
Im `MainWindow` werden Personen verwaltet, über das `PersonAddressDetailsWindow` lassen sich Adressen zur gewählten Person bearbeiten und die 1:n-Beziehung in der Praxis testen.
