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

Returns `201 Created` with the device.

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

