# ElectronicsStoreAss3 | SWE30003

ASP.NET MVC application using SQLite.

## Setup

Ensure you have .NET 9 installed. Including the ASP.NET runtime.

1. Restore dependencies

```bash
dotnet restore
```

2. Run application

```
dotnet watch run
```

3. Visit site at http://localhost:5299

## Database

Application uses SQLite.

- Main file: Database.db
- File is automatically created on first run if it does not exist. (`db.Database.EnsureCreated();`

**Tip**: Use [DB Browser for SLite](https://sqlitebrowser.org/) or an extension like [SQLite Viewer](https://marketplace.visualstudio.com/items?itemName=qwtel.sqlite-viewer) in VSCode to view/modify the data inside the .db file via GUI.
