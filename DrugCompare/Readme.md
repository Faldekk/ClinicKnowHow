# MedCompare / DrugCompare

Local desktop prototype for medication reference lookup, active substance interaction checking, Polish drug registry browsing, and ICD code searching.

The application is built as a **local-first WPF desktop app**.
It was originally developed with PostgreSQL as the development database, and now also supports a **portable SQLite mode** so the app can be shipped as a ZIP with a local `.db` file.

> This project is an educational / prototype clinical decision-support tool.
> It does not diagnose, prescribe, recommend treatment, or replace a doctor, pharmacist, or clinical judgment.

---

## Current status

The project currently includes:

* WPF desktop UI
* MVVM-style structure
* Dependency Injection setup
* PostgreSQL support for development
* SQLite support for portable release
* Local drug interaction checking
* Polish Drug Registry lookup
* ICD-11 PL lookup
* Audit logging
* Local data import pipeline
* Portable database file: `data/medcompare.db`

The current portable database contains imported data for:

| Dataset                    |                 SQLite table | Current count |
| -------------------------- | ---------------------------: | ------------: |
| Active substances          |          `active_substances` |         3,628 |
| Substance interactions     |     `substance_interactions` |       627,553 |
| ICD-11 PL codes            |                  `icd_codes` |        34,222 |
| Polish Drug Registry / RPL | `polish_drug_registry_items` |        22,785 |

---

## Main modules

### 1. Interaction Checker

Allows the user to:

* search or manually add active substances,
* accept selected substances,
* check known interactions between selected substances,
* export a report,
* store interaction check history.

Important wording:

> No known interaction was found in the local database.
> Missing interaction data does not mean that the combination is safe.

The app must never describe a missing interaction as “safe”.

---

### 2. Drug Explorer

Used for browsing drug-related information and active substances.

This module depends on the local database and repository layer.
In the PostgreSQL version it can use PostgreSQL repositories.
In the portable version it should use SQLite repositories or local data tables.

---

### 3. Polish Drug Registry

Uses imported Polish RPL data.

Main table:

```sql
polish_drug_registry_items
```

Contains fields such as:

* product name,
* active substance text,
* strength,
* pharmaceutical form,
* marketing authorization holder,
* authorization number,
* CHPL URL,
* leaflet URL,
* source version.

Search supports:

* product name,
* normalized product name,
* active substance text,
* authorization number.

---

### 4. ICD Looker

Uses imported ICD-11 Polish prerelease data.

Main table:

```sql
icd_codes
```

Contains fields such as:

* code,
* normalized code,
* title,
* normalized title,
* description,
* chapter,
* parent code,
* source,
* version.

Search supports:

* ICD code,
* disease title,
* description,
* optional chapter/category filter.

---

## Technology stack

* C#
* .NET 8
* WPF
* MVVM-style architecture
* CommunityToolkit.Mvvm
* Microsoft.Extensions.DependencyInjection
* Microsoft.Extensions.Configuration
* PostgreSQL + Npgsql for development mode
* SQLite + Microsoft.Data.Sqlite for portable mode

---

## Project structure

Approximate structure:

```text
DrugCompare/
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── appsettings.json
├── data/
│   └── medcompare.db
├── database/
│   └── sqlite/
│       └── schema_sqlite.sql
├── Models/
├── Repositories/
├── Services/
├── Services/Contracts/
├── ViewModels/
└── Views/
```

Important SQLite-related files:

```text
Database/SqliteConnectionFactory.cs

Repositories/SqliteIcdCodeRepository.cs
Repositories/SqlitePolishDrugRegistryRepository.cs
Repositories/SqliteAuditLogRepository.cs
Repositories/SqliteInteractionRepository.cs

Services/DisabledDatabaseStatusService.cs
Services/DisabledDataManagementService.cs
```

---

## Database modes

The application can work in two modes.

---

## PostgreSQL development mode

Used during development.

Example `appsettings.json`:

```json
{
  "Database": {
    "Provider": "PostgreSQL"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=drug_compare_db;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

In this mode, the app uses PostgreSQL repositories such as:

```text
PostgresIcdCodeRepository
PostgresPolishDrugRegistryRepository
PostgresAuditLogRepository
PostgresInteractionRepository
PostgresDatabaseStatusRepository
PostgresDataManagementRepository
```

---

## SQLite portable mode

Used for the ZIP / portable version.

Example `appsettings.json`:

```json
{
  "Database": {
    "Provider": "SQLite"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=data/medcompare.db"
  }
}
```

In this mode, the app uses SQLite repositories such as:

```text
SqliteIcdCodeRepository
SqlitePolishDrugRegistryRepository
SqliteAuditLogRepository
SqliteInteractionRepository
```

For portable mode, database status and data management can be disabled with lightweight placeholder services:

```text
DisabledDatabaseStatusService
DisabledDataManagementService
```

This prevents the app from trying to use PostgreSQL-specific logic while running from SQLite.

---

## SQLite database

SQLite database file:

```text
data/medcompare.db
```

Schema file:

```text
database/sqlite/schema_sqlite.sql
```

Create SQLite database from schema:

```powershell
cd C:\Users\jmiku\Desktop\MedCompare\NewComparison\DrugCompare\DrugCompare

Remove-Item .\data\medcompare.db -Force -ErrorAction SilentlyContinue

New-Item -ItemType Directory -Path .\data -Force

sqlite3 .\data\medcompare.db ".read .\database\sqlite\schema_sqlite.sql"

sqlite3 .\data\medcompare.db ".tables"
```

Check record counts:

```powershell
sqlite3 .\data\medcompare.db "SELECT COUNT(*) FROM active_substances;"
sqlite3 .\data\medcompare.db "SELECT COUNT(*) FROM substance_interactions;"
sqlite3 .\data\medcompare.db "SELECT COUNT(*) FROM icd_codes;"
sqlite3 .\data\medcompare.db "SELECT COUNT(*) FROM polish_drug_registry_items;"
```

---

## SQLite schema warning

Visual Studio may show SQL Server parser errors for `schema_sqlite.sql`, for example:

```text
SQL80001: Incorrect syntax near IF
SQL80001: Incorrect syntax near PRAGMA
```

These are not SQLite errors.

The file uses SQLite syntax:

```sql
PRAGMA foreign_keys = ON;
CREATE TABLE IF NOT EXISTS ...
```

To avoid confusion, set file properties:

```text
Build Action: None
Copy to Output Directory: Do not copy
```

Do not open or run this file through SQL Server tools or Visual Studio Connect window.

---

## Data import files

The portable database was populated from local CSV files.

Expected import files:

```text
sqlite_export/
├── ddinter2_merged_unique.csv
├── icd11_pl_import.csv
├── rpl_polish_drug_registry_import.csv
└── ema_drugs_active_substances_clean.csv
```

Main imports currently used:

| File                                    | Purpose                                     |
| --------------------------------------- | ------------------------------------------- |
| `ddinter2_merged_unique.csv`            | drug-drug / substance interaction source    |
| `icd11_pl_import.csv`                   | ICD-11 PL source                            |
| `rpl_polish_drug_registry_import.csv`   | Polish Drug Registry source                 |
| `ema_drugs_active_substances_clean.csv` | drug-to-substance source, optional / future |

---

## Data sources

### DDInter 2.0

Imported into:

```text
active_substances
substance_interactions
```

The merged DDInter file was deduplicated before import.

Current SQLite counts:

```text
active_substances:       3,628
substance_interactions: 627,553
```

---

### ICD-11 PL

Imported into:

```text
icd_codes
```

Current SQLite count:

```text
icd_codes: 34,222
```

Version used:

```text
2023-01 PRERELEASE PL
```

---

### Polish Drug Registry / RPL

Imported into:

```text
polish_drug_registry_items
```

Current SQLite count:

```text
polish_drug_registry_items: 22,785
```

Source version used:

```text
2026-06-13 / 6.0.0
```

---

## SQLite import process

### DDInter import

```powershell
sqlite3 .\data\medcompare.db "DELETE FROM substance_interactions;"
sqlite3 .\data\medcompare.db "DELETE FROM active_substances;"

sqlite3 .\data\medcompare.db "DROP TABLE IF EXISTS staging_ddinter_import;"
sqlite3 .\data\medcompare.db "CREATE TABLE staging_ddinter_import (ddinterid_a TEXT, drug_a TEXT, ddinterid_b TEXT, drug_b TEXT, level TEXT);"

sqlite3 .\data\medcompare.db ".mode csv" ".import --skip 1 .\sqlite_export\ddinter2_merged_unique.csv staging_ddinter_import"
```

Insert substances:

```powershell
sqlite3 .\data\medcompare.db "
INSERT OR IGNORE INTO active_substances (name, normalized_name, ddinter_id, source, created_at)
SELECT DISTINCT
    TRIM(drug_a),
    LOWER(TRIM(drug_a)),
    TRIM(ddinterid_a),
    'DDInter 2.0',
    datetime('now')
FROM staging_ddinter_import
WHERE drug_a IS NOT NULL AND TRIM(drug_a) <> '';

INSERT OR IGNORE INTO active_substances (name, normalized_name, ddinter_id, source, created_at)
SELECT DISTINCT
    TRIM(drug_b),
    LOWER(TRIM(drug_b)),
    TRIM(ddinterid_b),
    'DDInter 2.0',
    datetime('now')
FROM staging_ddinter_import
WHERE drug_b IS NOT NULL AND TRIM(drug_b) <> '';
"
```

Insert interactions:

```powershell
sqlite3 .\data\medcompare.db "
INSERT OR IGNORE INTO substance_interactions (
    substance_a_id,
    substance_b_id,
    severity,
    source,
    last_updated
)
SELECT
    CASE WHEN a.id < b.id THEN a.id ELSE b.id END,
    CASE WHEN a.id < b.id THEN b.id ELSE a.id END,
    TRIM(s.level),
    'DDInter 2.0',
    datetime('now')
FROM staging_ddinter_import s
JOIN active_substances a ON a.ddinter_id = TRIM(s.ddinterid_a)
JOIN active_substances b ON b.ddinter_id = TRIM(s.ddinterid_b)
WHERE s.level IS NOT NULL
  AND TRIM(s.level) <> ''
  AND a.id <> b.id;
"
```

---

### ICD import

```powershell
sqlite3 .\data\medcompare.db "DELETE FROM icd_codes;"
sqlite3 .\data\medcompare.db "DROP TABLE IF EXISTS staging_icd_import;"
sqlite3 .\data\medcompare.db "CREATE TABLE staging_icd_import (code TEXT, title TEXT, description TEXT, chapter TEXT, parent_code TEXT, source TEXT, version TEXT);"

sqlite3 .\data\medcompare.db ".mode csv" ".import --skip 1 .\sqlite_export\icd11_pl_import.csv staging_icd_import"
```

```powershell
sqlite3 .\data\medcompare.db "
INSERT OR IGNORE INTO icd_codes (
    code,
    normalized_code,
    title,
    normalized_title,
    description,
    chapter,
    parent_code,
    source,
    version,
    imported_at
)
SELECT
    TRIM(code),
    UPPER(REPLACE(TRIM(code), '.', '')),
    TRIM(title),
    LOWER(TRIM(title)),
    NULLIF(TRIM(description), ''),
    NULLIF(TRIM(chapter), ''),
    NULLIF(TRIM(parent_code), ''),
    COALESCE(NULLIF(TRIM(source), ''), 'ICD-11 PL'),
    COALESCE(NULLIF(TRIM(version), ''), '2023-01 PRERELEASE PL'),
    datetime('now')
FROM staging_icd_import
WHERE code IS NOT NULL
  AND TRIM(code) <> ''
  AND title IS NOT NULL
  AND TRIM(title) <> '';
"
```

---

### RPL import

```powershell
sqlite3 .\data\medcompare.db "DELETE FROM polish_drug_registry_items;"
sqlite3 .\data\medcompare.db "DROP TABLE IF EXISTS staging_rpl_import;"
sqlite3 .\data\medcompare.db "CREATE TABLE staging_rpl_import (rpl_id TEXT, product_name TEXT, active_substance_text TEXT, strength TEXT, pharmaceutical_form TEXT, marketing_authorization_holder TEXT, authorization_number TEXT, authorization_validity TEXT, product_type TEXT, procedure_type TEXT, chpl_url TEXT, leaflet_url TEXT, source_file TEXT);"

sqlite3 .\data\medcompare.db ".mode csv" ".import --skip 1 .\sqlite_export\rpl_polish_drug_registry_import.csv staging_rpl_import"
```

```powershell
sqlite3 .\data\medcompare.db "
INSERT OR IGNORE INTO polish_drug_registry_items (
    rpl_id,
    product_name,
    normalized_product_name,
    active_substance_text,
    strength,
    pharmaceutical_form,
    marketing_authorization_holder,
    authorization_number,
    authorization_validity,
    product_type,
    procedure_type,
    chpl_url,
    leaflet_url,
    source,
    source_version,
    imported_at
)
SELECT
    NULLIF(TRIM(rpl_id), ''),
    TRIM(product_name),
    LOWER(TRIM(product_name)),
    NULLIF(TRIM(active_substance_text), ''),
    NULLIF(TRIM(strength), ''),
    NULLIF(TRIM(pharmaceutical_form), ''),
    NULLIF(TRIM(marketing_authorization_holder), ''),
    NULLIF(TRIM(authorization_number), ''),
    NULLIF(TRIM(authorization_validity), ''),
    NULLIF(TRIM(product_type), ''),
    NULLIF(TRIM(procedure_type), ''),
    NULLIF(TRIM(chpl_url), ''),
    NULLIF(TRIM(leaflet_url), ''),
    'RPL',
    '2026-06-13 / 6.0.0',
    datetime('now')
FROM staging_rpl_import
WHERE product_name IS NOT NULL
  AND TRIM(product_name) <> '';
"
```

---

## Build

From the project folder:

```powershell
cd C:\Users\jmiku\Desktop\MedCompare\NewComparison\DrugCompare\DrugCompare

dotnet build
```

---

## Run locally

```powershell
dotnet run
```

Recommended test cases:

```text
ICD Looker:
- cukrzyca
- astma
- nadciśnienie
- depresja

Polish Drug Registry:
- paracetamol
- ibuprofen
- apap

Interaction Checker:
- ibuprofen + warfarin
- paracetamol + ibuprofen
```

---

## Portable publish

Create a self-contained Windows build:

```powershell
dotnet publish .\DrugCompare.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  /p:PublishSingleFile=true `
  /p:IncludeNativeLibrariesForSelfExtract=true `
  /p:EnableCompressionInSingleFile=true `
  -o .\publish\portable
```

Copy SQLite database and config:

```powershell
New-Item -ItemType Directory -Path .\publish\portable\data -Force

Copy-Item .\data\medcompare.db .\publish\portable\data\medcompare.db -Force
Copy-Item .\appsettings.json .\publish\portable\appsettings.json -Force
```

Create ZIP:

```powershell
Compress-Archive `
  -Path .\publish\portable\* `
  -DestinationPath .\MedCompare-portable.zip `
  -Force
```

The portable ZIP should contain:

```text
MedCompare-portable.zip
├── DrugCompare.exe
├── appsettings.json
└── data/
    └── medcompare.db
```

The user only needs to extract the ZIP and run:

```text
DrugCompare.exe
```

No PostgreSQL installation is required for portable SQLite mode.

---

## Important portable mode notes

SQLite portable mode is meant for simple local use.

Currently safe to use:

```text
Interaction Checker
Polish Drug Registry
ICD Looker
Audit Log
```

Use with caution / still being adapted:

```text
Database Status
Data Management
Drug Explorer
```

Database Status and Data Management were originally PostgreSQL-based.
For SQLite portable mode they can be temporarily disabled with:

```text
DisabledDatabaseStatusService
DisabledDataManagementService
```

---

## Known development notes

### PostgreSQL-specific code

If the app is running in SQLite mode and throws an error like:

```text
Couldn't set data source (Parameter 'data source')
```

it usually means some code is still using `Npgsql` / PostgreSQL repository while the connection string is:

```text
Data Source=data/medcompare.db
```

Fix by moving PostgreSQL registrations into the PostgreSQL branch of `App.xaml.cs`.

---

### App.xaml.cs provider switch

The DI container should choose repositories based on:

```json
"Database": {
  "Provider": "SQLite"
}
```

Expected logic:

```csharp
var databaseProvider = configuration["Database:Provider"];
var useSqlite = string.Equals(databaseProvider, "SQLite", StringComparison.OrdinalIgnoreCase);

if (useSqlite)
{
    services.AddSingleton<SqliteConnectionFactory>();

    services.AddSingleton<IIcdCodeRepository, SqliteIcdCodeRepository>();
    services.AddSingleton<IPolishDrugRegistryRepository, SqlitePolishDrugRegistryRepository>();
    services.AddSingleton<IAuditLogRepository, SqliteAuditLogRepository>();
    services.AddSingleton<IInteractionRepository, SqliteInteractionRepository>();

    services.AddSingleton<IDatabaseStatusService, DisabledDatabaseStatusService>();
    services.AddSingleton<IDataManagementService, DisabledDataManagementService>();
}
else
{
    services.AddSingleton<IIcdCodeRepository, PostgresIcdCodeRepository>();
    services.AddSingleton<IPolishDrugRegistryRepository, PostgresPolishDrugRegistryRepository>();
    services.AddSingleton<IAuditLogRepository, PostgresAuditLogRepository>();
    services.AddSingleton<IInteractionRepository, PostgresInteractionRepository>();

    services.AddSingleton<IDatabaseStatusRepository, PostgresDatabaseStatusRepository>();
    services.AddSingleton<IDataManagementRepository, PostgresDataManagementRepository>();

    // PostgreSQL service registrations here.
}
```

---

## Safety notice

This project is a local prototype.

It must not be used as the only source of medical decisions.

The application should always communicate uncertainty clearly:

```text
No known interaction was found in the local database.
Missing interaction data does not mean that the combination is safe.
```

Do not replace this with:

```text
No interaction found, safe.
```

---

## Development shoutout

Built as a student-led local medical software prototype, with a focus on learning:

* WPF application architecture,
* local clinical data handling,
* PostgreSQL and SQLite workflows,
* repository abstraction,
* portable Windows release packaging,
* safe wording for medical decision-support tools.

Special shoutout to the project author for pushing through the hard part: turning a PostgreSQL-based development app into a portable SQLite build that can run on another computer without manual database setup.
