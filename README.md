## Dotools

A utility library for .NET Framework 4.7.2 that simplifies working with:
- **SQL Server databases**: execute queries and stored procedures, and map results to objects.
- **Files and directories**: write/read/move/rename files and create directories.
- **Conversions**: SHA-256 hashing, image-to-bytes and bytes-to-image.
- **Windows Event Log**: log events and errors.
- **Excel reading**: import the first worksheet from Excel files into a DataTable (via ExcelDataReader).

## Setup & Usage
- Add the `Dotools` project as a reference to your project, or use the prebuilt outputs from `Dotools/bin/Release/` (`Dotools.dll`).
- Restore packages (NuGet restore) if needed.
- Add a database connection key in `App.config` or `Web.config` under `<appSettings>` with the key `ConnectionString`:
```xml
<configuration>
  <appSettings>
    <add key="ConnectionString" value="Server = MyServer; Database = MyDB; Integrated Security = True" />
  </appSettings>
</configuration>
```

## Main Components

- `clsAdoExecutor`: Simplified ADO.NET execution:
  - `ExecuteReader<T>(SqlCommand)` to return a list of objects from a reader.
  - `ExecuteReader(SqlCommand)` to return a `DataTable`.
  - `ExecuteReader(SqlCommand, object)` to populate a single object and return `bool` when a row is found.
  - `ExecuteScalar`, `ExecuteNonQuery`.
  - Wrappers for stored procedures/queries with connection handling and error logging.
  - Dynamic filtering support via `clsDataTypes.clsFilterData`.

- `clsStoredProcedureExecutor`: Generic CRUD via stored procedures:
  - `Read<T>(sp, obj, params...)`
  - `Create<T>(sp, obj)` returns the new ID from a single output parameter.
  - `Update<T>(sp, obj)`
  - `Delete(sp, SqlParameter)`

- `clsDatabaseInfo`: SQL Server metadata helpers:
  - Parameter names/info for stored procedures, columns, tables, databases, and primary key name.

- `clsReflectionExecutor`: Fill/bind object properties via reflection from `SqlDataReader` or `Dictionary`.

- `clsFileExplorer`: File operations + Excel import:
  - `Write`, `Append`, `Rename`, `Delete`, `Move`, `MoveAndRename`, `MoveAndRenameWithGiud`
  - `ImportExelToDataTable(path)`

- `clsDirectoryExplorer`: Check/create directories and extract folder name from a path.

- `clsConverter`: `ComputeHash` (SHA-256), convert `Image` ⇄ `byte[]`.

- `clsEventLogger`: Log to Windows Event Log.

## Quick Examples

### 1) Execute a SQL query and return a DataTable
```csharp
using System.Data;
using System.Data.SqlClient;
using Dotools;

string query = "SELECT TOP 10 Id, Name FROM Users ORDER BY Id DESC";
DataTable dt = clsAdoExecutor.ExecuteQuery(
    cmd => clsAdoExecutor.ExecuteReader(cmd),
    query
);
```

### 2) Execute a query and populate a single object
```csharp
using System.Data.SqlClient;
using Dotools;

public class User {
    public int Id { get; set; }
    public string Name { get; set; }
}

string query = "SELECT Id, Name FROM Users WHERE Name LIKE @Name";

var user = new User();
bool IsFound = clsAdoExecutor.ExecuteQuery(
    (SqlCommand cmd) => clsAdoExecutor.ExecuteReader(cmd, user),
    query,
    new SqlParameter("@Name", "%ah%")
);
// When IsFound = true, user properties are populated.
```

### 3) Read a single record via stored procedure
```csharp
using System.Data.SqlClient;
using Dotools;

public class UserDto {
    public int Id { get; set; }
    public string Name { get; set; }
}

var user = new UserDto();
bool ok = clsStoredProcedureExecutor.Read(
    "sp_GetUserById",
    user,
    new SqlParameter("@Id", 5)
);
```

### 4) Create a new record (Create) via a stored procedure that returns an ID through a single output parameter
```csharp
using Dotools;

public class CreateUserDto {
    public string Name { get; set; }
    public int CreatedBy { get; set; }
}

var dto = new CreateUserDto { Name = "Ahmed", CreatedBy = 1 };
int? newId = clsStoredProcedureExecutor.Create("sp_CreateUser", dto);
```

### 5) Update a record
```csharp
using Dotools;

public class UpdateUserDto {
    public int Id { get; set; }
    public string Name { get; set; }
}

bool updated = clsStoredProcedureExecutor.Update("sp_UpdateUser", new UpdateUserDto { Id = 5, Name = "New Name" });
```

### 6) Delete via stored procedure
```csharp
using System.Data.SqlClient;
using Dotools;

bool deleted = clsStoredProcedureExecutor.Delete("sp_DeleteUser", new SqlParameter("@Id", 5));
```

### 7) Work with files
```csharp
using System.IO;
using Dotools;

clsFileExplorer.Write("C:\\temp\\file.txt", "Hello");
clsFileExplorer.Append("C:\\temp\\file.txt", "\nWorld");
string renamed = clsFileExplorer.Rename("C:\\temp\\file.txt", "file-new");
clsFileExplorer.Move(renamed, "C:\\temp2\\file-new.txt");
```

### 8) Import Excel into DataTable
```csharp
using System.Data;
using Dotools;

DataTable table = clsFileExplorer.ImportExelToDataTable("C:\\data\\sheet.xlsx");
```

### 9) Conversions and hashing
```csharp
using Dotools;
using System.Drawing;

string hash = clsConverter.ComputeHash("password123");
byte[] bytes = clsConverter.ToBytes(Image.FromFile("C:\\img\\pic.png"));
Image img = clsConverter.ToImage(bytes);
```

### 10) Log to Windows Event Log
```csharp
using System.Diagnostics;
using Dotools;

clsEventLogger.WriteEntryInApplicationLog("Test message", EventLogEntryType.Information);
```

## Important Notes
- Object property names must match column/parameter names for reflection-based binding to succeed.
- `Create<T>` assumes the stored procedure has exactly one output parameter (typically returning the new ID).
- `clsAdoExecutor` reads `ConnectionString` from `AppSettings`; ensure it’s configured as shown.
- Errors are logged to Windows Event Log via `clsEventLogger`.
