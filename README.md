# Device Management API

A .NET 10 / C# 12 Web API for managing device inventory, backed by SQL Server via EF Core.

**Local base URL:** `http://localhost:5224`  
**Interactive docs:** `http://localhost:5224/scalar/v1` (development only)

---

## Project structure

```
DeviceManagement/
  Controllers/   # HTTP routing and response shaping
  Models/        # Domain types: Device, UpdateDeviceRequest, DeviceType, Status
  Persistence/   # IDeviceRepository, SqlDeviceRepository, DeviceDbContext
  Validations/   # DeviceValidator, UpdateDeviceRequestValidator, StrictEnumJsonConverter

DeviceManagement.Tests/
  DeviceTests.cs              # Validator unit tests (7)
  DevicesControllerTests.cs   # Controller unit tests (13)
  ApiIntegrationTests.cs      # Full-pipeline integration tests (11)
  InMemoryDeviceRepository.cs # Test double — no database required
  DeviceApiFactory.cs         # WebApplicationFactory wiring
```

---

## Design decisions

- **Records + `required` + `init`** — `Device` is immutable after construction. Missing a field is a compile error and a JSON 400, not a runtime null.
- **`UpdateDeviceRequest`** — separate type containing only the 4 mutable fields. The type system enforces immutability; no runtime guard needed.
- **Two-layer validation** — `StrictEnumJsonConverter` rejects invalid enum values at deserialization with a clear message. `DeviceValidator` enforces business rules (non-empty fields, email format) via FluentValidation.
- **`IDeviceRepository`** — the interface is the seam between domain and persistence. Swapping SQL for any other backend is a single change in `Program.cs`.
- **`IReadOnlyList<T>`** — query results are materialised snapshots, not lazy cursors.
- **`AsNoTracking()` + `with`** — updates load the existing record untracked, produce a new record via `with`, then attach it explicitly. Required because `init` properties cannot be mutated in place.
- **Enums stored as strings** — `HasConversion<string>()` keeps the database human-readable without the source code.
- **`IsUnicode(false)`** — all string columns are `varchar`; device identifiers and email addresses are ASCII.
- **Index on `PrimaryUser`** — the `GET ?primaryUser=` query path has a covering index.

---

## Endpoints

### POST /api/devices

Creates a new device. All 8 fields are required.

```
POST http://localhost:5224/api/devices
Content-Type: application/json
```
```json
{
    "serialNumber": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "modelId": "MDL-001",
    "modelName": "ThinkPad X1",
    "manufacturer": "Lenovo",
    "primaryUser": "alice@lego.com",
    "operatingSystem": "Windows 11",
    "deviceType": "Laptop",
    "status": "Active"
}
```

Returns `201 Created` with the device and a `Location` header pointing to its GET URL.

---

### GET /api/devices/{serialNumber}

Returns a single device by GUID. Returns `404` if not found.

```
GET http://localhost:5224/api/devices/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

---

### GET /api/devices?primaryUser={email}

Returns all devices assigned to a user. Returns an empty array if none. `primaryUser` is required — omitting it returns `400`.

```
GET http://localhost:5224/api/devices?primaryUser=alice@spartan.dk
```

---

### PUT /api/devices/{serialNumber}

Updates one or more mutable fields: `primaryUser`, `operatingSystem`, `deviceType`, `status`. All fields are optional — only send the ones to change. Returns `404` if the serial number does not exist.

```
PUT http://localhost:5224/api/devices/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json
```
```json
{
    "primaryUser": "bob@spartan.dk",
    "status": "Inactive"
}
```

---

## Validation rules

| Field | Rule |
|---|---|
| `serialNumber` | Valid non-empty GUID |
| `modelId` | Non-empty string |
| `modelName` | Non-empty string |
| `manufacturer` | Non-empty string |
| `primaryUser` | Valid email address |
| `operatingSystem` | Non-empty string |
| `deviceType` | `Laptop` or `Desktop` (case-sensitive) |
| `status` | `Active`, `Inactive`, or `Retired` (case-sensitive) |

Invalid enum values return a 400 with the accepted values listed:
```json
{
  "status": 400,
  "errors": {
    "deviceType": ["'Tablet' is not a valid DeviceType. Accepted values are: Laptop, Desktop."]
  }
}
```

---

## Running locally

**Database setup (first time):**
```bash
dotnet ef migrations add InitialCreate --project DeviceManagement
dotnet ef database update --project DeviceManagement
```

**Run the API:**
```bash
dotnet run --project DeviceManagement
```

**Run all tests (no database required):**
```bash
dotnet test
```

---

## Docker

```bash
docker build -t device-management .
docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="..." device-management
```

---

## Azure

The application targets Azure Container Apps. In non-development environments it reads the connection string from Azure Key Vault using `DefaultAzureCredential` (Managed Identity — no credentials in code or config).

Infrastructure is defined in `infra/main.bicep`: Container Registry, Azure SQL, Key Vault, Managed Identity, and Container Apps with HTTP-based autoscaling (1–5 replicas).

CI/CD runs via GitHub Actions (`.github/workflows/ci.yml`): builds and tests on every push and PR; builds and pushes a Docker image to ACR on merges to `main`.
