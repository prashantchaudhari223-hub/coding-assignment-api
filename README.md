# PharmacyInventory API

Lightweight ASP.NET Core Web API for managing pharmacy medicines.

## Requirements
- .NET 8 SDK
- C# 12
- (Optional) Visual Studio 2022/2026 or `dotnet` CLI

## Build & run
From repository root:
- dotnet restore
- dotnet build
- dotnet run --project src/PharmacyInventory.API

Or open `PharmacyInventory.sln` in Visual Studio and run.

## Authentication
All endpoints in `MedicinesController` are protected by role-based authorization (`[Authorize(Roles = "Admin")]`). Include an Authorization header:
- `Authorization: Bearer <TOKEN>`

Note : Token is generated using /api/auth/login endpoint passing username as 'admin' and password as 'password'.

## Endpoints
- GET `/api/medicines?search={q}&page={n}&pageSize={m}`
  - Defaults: `page=1`, `pageSize=20`
  - `MaxPageSize = 100`
  - Response body includes HTTP code:
    - `{ "StatusCode": 200, "Data": { Items: [...], Total, Page, PageSize } }`

- GET `/api/medicines/{id}`
  - 200: `{ "StatusCode": 200, "Data": <MedicineDetailDto> }`

- POST `/api/medicines`
  - Body: `CreateMedicineDto` (validated with FluentValidation)
  - 201 Created (Location header) and body:
    - `{ "StatusCode": 201, "Data": <MedicineDetailDto> }`

## Notes
- Validation performed with `FluentValidation`.
- Service interactions via `IMedicineService`.
- `NotFoundException` may be thrown by services for missing resources.

## Example (curl)
curl -H "Authorization: Bearer <TOKEN>" \
  "https://localhost:5001/api/medicines?search=aspirin&page=1&pageSize=10"