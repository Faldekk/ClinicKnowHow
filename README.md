# MedCompare

MedCompare is a desktop application built with C# and WPF. Its main goal is to provide local access to medication-related information, active substances, drug interaction checking, Polish drug registry data, and ICD code lookup.

This project was created as an educational/prototype application. I wanted to build something more practical than a simple CRUD app — something that works with real medical datasets, runs locally, and shows how desktop development, databases, and basic clinical decision-support concepts can be combined in one project.

The application does not use cloud services or external APIs during normal use. The data is stored locally in a database.

---

## What the application does

MedCompare currently includes several main modules:

* checking interactions between active substances,
* searching drugs and active substances,
* browsing Polish drug registry data,
* searching ICD codes,
* viewing history and audit logs,
* running in local portable mode with SQLite.

---

## Main modules

### Interaction Checker

This is the main module of the application. It allows the user to search for active substances, manually add them, accept them for analysis, and then check whether known interactions exist in the local database.

The application displays:

* the first substance,
* the second substance,
* interaction severity,
* data source,
* a short result message.

An important part of this module is safe wording. The application does not say that a combination is “safe” when no interaction is found. It only means that no matching interaction record was found in the local database.

---

### Drug Explorer

This module is used for browsing drug-related information and linked active substances.

It is still being developed together with the drug database and data import process. The goal is to make it easy to check which active substances are connected with a specific medicinal product.

---

### Polish Drug Registry

This module is based on data from the Polish Register of Medicinal Products.

It allows searching medicinal products by:

* product name,
* active substance,
* authorization number,
* normalized product name.

The database includes information such as:

* product name,
* active substances,
* strength,
* pharmaceutical form,
* marketing authorization holder,
* authorization number,
* Summary of Product Characteristics URL,
* patient leaflet URL.

---

### ICD Looker

This module allows searching ICD codes. The current version uses ICD-11 data in Polish.

The user can search by:

* ICD code,
* disease name,
* description,
* category/chapter.

Example searches:

```text
diabetes
asthma
hypertension
depression
```

Polish terms can also be used, for example:

```text
cukrzyca
astma
nadciśnienie
depresja
```

---

## Data

The application uses local data imported into a SQLite database.

The current portable database contains:

| Dataset                      |                        Table | Record count |
| ---------------------------- | ---------------------------: | -----------: |
| Active substances            |          `active_substances` |        3,628 |
| Substance interactions       |     `substance_interactions` |      627,553 |
| ICD-11 PL codes              |                  `icd_codes` |       34,222 |
| Polish drug registry records | `polish_drug_registry_items` |       22,785 |

The data was imported from prepared CSV files. The project was originally developed with PostgreSQL, but later I added SQLite support so the application can run on another computer without requiring PostgreSQL installation or manual database setup.

---

## Database modes

The application can work in two database modes.

---

## PostgreSQL development mode

PostgreSQL was used as the main development database. It was useful for importing larger datasets, testing queries, and working with the data structure during development.

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

---

## SQLite portable mode

SQLite mode was added to make the app easier to share and run on another machine.

In this mode, the application uses a local database file:

```text
data/medcompare.db
```

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

With this setup, the app can be distributed as a ZIP file together with the SQLite database.

---

## Technologies used

The project uses:

* C#,
* .NET 8,
* WPF,
* MVVM-style architecture,
* CommunityToolkit.Mvvm,
* Dependency Injection,
* PostgreSQL,
* SQLite,
* Npgsql,
* Microsoft.Data.Sqlite.

---

## Project structure

Main folders:

```text
DrugCompare/
├── Models/
├── Repositories/
├── Services/
├── Services/Contracts/
├── ViewModels/
├── Views/
├── Database/
├── database/sqlite/
├── data/
└── appsettings.json
```

Important parts:

```text
Models/                       data models
Repositories/                 database access layer
Services/                     application logic
Services/Contracts/           service interfaces
ViewModels/                   view logic
Views/                        WPF views
Database/SqliteConnectionFactory.cs
database/sqlite/schema_sqlite.sql
data/medcompare.db
```

---

## Repository layer

The application has separate repository implementations for PostgreSQL and SQLite.

Examples of PostgreSQL repositories:

```text
PostgresIcdCodeRepository
PostgresPolishDrugRegistryRepository
PostgresAuditLogRepository
PostgresInteractionRepository
```

Examples of SQLite repositories:

```text
SqliteIcdCodeRepository
SqlitePolishDrugRegistryRepository
SqliteAuditLogRepository
SqliteInteractionRepository
```

In `App.xaml.cs`, the application reads:

```json
"Provider": "SQLite"
```

and chooses the correct repository implementations based on the selected provider.

---

## Running locally

From the project folder:

```powershell
dotnet build
dotnet run
```

For SQLite mode, the application needs:

```text
data/medcompare.db
```

and a correctly configured `appsettings.json`.

---

## Creating the SQLite database

The SQLite database is created from:

```text
database/sqlite/schema_sqlite.sql
```

Commands:

```powershell
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

## Publishing a portable release

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

Copy the database and configuration:

```powershell
New-Item -ItemType Directory -Path .\publish\portable\data -Force

Copy-Item .\data\medcompare.db .\publish\portable\data\medcompare.db -Force
Copy-Item .\appsettings.json .\publish\portable\appsettings.json -Force
```

The final ZIP should look like this:

```text
MedCompare-portable.zip
├── DrugCompare.exe
├── appsettings.json
└── data/
    └── medcompare.db
```

After extracting the ZIP, the application can be started by running:

```text
DrugCompare.exe
```

No PostgreSQL installation is required in SQLite portable mode.

---

## Current project status

The project is still a prototype, but the main local features are already implemented.

Currently implemented:

* WPF desktop application,
* multiple application modules,
* local medical data import,
* SQLite portable database,
* interaction checking,
* ICD code searching,
* Polish drug registry searching,
* basic audit logging,
* portable release preparation.

Still planned / in progress:

* improving Drug Explorer,
* improving Database Status for SQLite,
* improving Data Management for SQLite,
* automating data imports,
* improving the UI,
* improving user-facing messages,
* creating a cleaner installer or release package.

---

## Medical safety notice

MedCompare is not a certified medical device yet :) (I really want this application to help doctors and interns as me myself is a member of a medical scientific club, in which I recently has presented the threats of blindly trusting technology)

This application is an educational prototype and should not be used as the only source for medical decisions.

If no interaction is found in the local database, it does not mean that a given combination of drugs or substances is safe. It only means that the application did not find a matching record in the currently available local data.

Medical decisions should always be consulted with a doctor, pharmacist, or another qualified healthcare professional.

---

## Author

This project was created as an educational desktop application and a practical experiment with local medical information systems.

The goal was to combine:

* desktop application development,
* local databases,
* real medication-related datasets,
* simple interaction checking,
* a portable Windows release that can run on another computer without manual database setup.
