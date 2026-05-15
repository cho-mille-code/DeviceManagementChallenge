# DeviceManagementChallenge

A .NET 10 Web API for managing device inventory backed by a MSSQL database.

**Base URL:** `http://localhost:5224`

---

## Endpoints

### POST /api/devices
Creates a new device. All fields are required. `serialNumber` must be a valid GUID, `primaryUser` must be a valid email address, `deviceType` must be `Laptop` or `Desktop`, and `status` must be `Active`, `Inactive`, or `Retired`.

Once created, `serialNumber`, `modelId`, `modelName`, and `manufacturer` are immutable and cannot be changed.

**Example — Laptop, Active**
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

**Example — Desktop, Inactive**
```
POST http://localhost:5224/api/devices
Content-Type: application/json
```
```json
{
    "serialNumber": "9b2e4c1d-8a3f-4e7b-b6d2-1f5a0c3e8d92",
    "modelId": "MDL-002",
    "modelName": "OptiPlex 7090",
    "manufacturer": "Dell",
    "primaryUser": "bob@lego.com",
    "operatingSystem": "Windows 10",
    "deviceType": "Desktop",
    "status": "Inactive"
}
```

**Example — Laptop, Retired**
```
POST http://localhost:5224/api/devices
Content-Type: application/json
```
```json
{
    "serialNumber": "c7f3a2b1-4d6e-4a8c-9e1f-2b3d5f7a9c0e",
    "modelId": "MDL-003",
    "modelName": "MacBook Pro",
    "manufacturer": "Apple",
    "primaryUser": "carol@lego.com",
    "operatingSystem": "macOS Sequoia",
    "deviceType": "Laptop",
    "status": "Retired"
}
```

---

### GET /api/devices/{serialNumber}
Returns a single device by its serial number.

**Example — fetch the ThinkPad created above**
```
GET http://localhost:5224/api/devices/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Example — fetch the OptiPlex**
```
GET http://localhost:5224/api/devices/9b2e4c1d-8a3f-4e7b-b6d2-1f5a0c3e8d92
```

**Example — unknown device (returns 404)**
```
GET http://localhost:5224/api/devices/00000000-0000-0000-0000-000000000000
```

---

### GET /api/devices?primaryUser={email}
Returns all devices assigned to a specific user. Returns an empty array if the user has no devices. `primaryUser` is required — omitting it returns 400.

**Example — all devices for alice**
```
GET http://localhost:5224/api/devices?primaryUser=alice@lego.com
```

**Example — all devices for bob**
```
GET http://localhost:5224/api/devices?primaryUser=bob@lego.com
```

**Example — missing query param (returns 400)**
```
GET http://localhost:5224/api/devices
```

---

### PUT /api/devices/{serialNumber}
Updates one or more of the mutable fields: `primaryUser`, `operatingSystem`, `deviceType`, `status`. All fields are optional — only include the ones you want to change.

**Example — update primary user and status**
```
PUT http://localhost:5224/api/devices/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json
```
```json
{
    "primaryUser": "newowner@lego.com",
    "status": "Inactive"
}
```

**Example — change OS only**
```
PUT http://localhost:5224/api/devices/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json
```
```json
{
    "operatingSystem": "Windows 11 Pro"
}
```

**Example — retire a device**
```
PUT http://localhost:5224/api/devices/9b2e4c1d-8a3f-4e7b-b6d2-1f5a0c3e8d92
Content-Type: application/json
```
```json
{
    "status": "Retired"
}
```

**Example — full update of all mutable fields**
```
PUT http://localhost:5224/api/devices/9b2e4c1d-8a3f-4e7b-b6d2-1f5a0c3e8d92
Content-Type: application/json
```
```json
{
    "primaryUser": "dave@lego.com",
    "operatingSystem": "Ubuntu 24.04",
    "deviceType": "Desktop",
    "status": "Active"
}
```

---

## Validation rules

| Field | Rule |
|---|---|
| `serialNumber` | Valid GUID, must not be all zeros |
| `primaryUser` | Valid email address |
| `deviceType` | `Laptop` or `Desktop` |
| `status` | `Active`, `Inactive`, or `Retired` |

## Running the database

The API uses MSSQL via LocalDB. To set up from scratch:

```bash
dotnet ef migrations add InitialCreate --project DeviceManagement
dotnet ef database update --project DeviceManagement
```
