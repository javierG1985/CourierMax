# CourierMax API

REST API para la gestión del ciclo completo de envíos de la empresa courier **CourierMax**.

---

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Sin base de datos externa — usa **SQLite** (archivo local generado automáticamente)

---

## Ejecutar el proyecto

```bash
# 1. Clonar el repositorio
git clone https://github.com/javierG1985/CourierMax.git
cd CourierMax/Ceiba.CourierMax

# 2. Restaurar dependencias
dotnet restore

# 3. Ejecutar la API
dotnet run --project src/Ceiba.CourierMax.API
```

La API arranca en `http://localhost:5262`
Documentación interactiva (Swagger UI): `http://localhost:5262/swagger`

> Al iniciar, la API crea automáticamente el esquema SQLite (`couriermax.db`) y siembra los datos de referencia (conductores, vehículos y usuario admin).

---

## Ejecutar los tests

```bash
dotnet test tests/Ceiba.CourierMax.Tests
```

**70 tests en total:**
- 62 unitarios (Domain + Application)
- 8 de integración HTTP end-to-end (SQLite `:memory:`)

### Organización de los tests

```
tests/Ceiba.CourierMax.Tests/
├── Unit/
│   ├── Domain/
│   │   ├── TrackingCodeTests.cs          → formato y generación del código de rastreo
│   │   ├── PhoneNumberTests.cs           → validación de número telefónico colombiano
│   │   ├── PackageDimensionsTests.cs     → cálculo de volumen m³
│   │   ├── BusinessDayServiceTests.cs    → días hábiles y festivos 2026
│   │   ├── ShipmentTests.cs              → máquina de estados del envío
│   │   └── VehicleCapacityTests.cs       → validación de capacidad del vehículo
│   └── Application/
│       ├── FareCalculatorServiceTests.cs  → cálculo de tarifas con recargos
│       └── LoginUseCaseTests.cs           → autenticación: credenciales válidas e inválidas
└── Integration/
    ├── CourierMaxWebFactory.cs            → factory con cliente pre-autenticado
    └── ShipmentsIntegrationTests.cs       → flujos HTTP completos contra SQLite :memory:
```

### Tests unitarios

No levantan la API ni tocan la base de datos. Instancian las clases directamente con sus dependencias controladas.

`LoginUseCaseTests` usa un `FakeUserService` interno que devuelve un usuario predefinido o `null` según el caso. El hash BCrypt se genera en el setup del propio test, garantizando que la verificación sea real y no simulada.

| Test | Qué verifica |
|---|---|
| `Login_WithValidCredentials` | Devuelve Username y Role correctos |
| `Login_WithUnknownUsername` | Lanza `DomainException` con mensaje genérico |
| `Login_WithWrongPassword` | Lanza `DomainException` con el mismo mensaje genérico |

> Ambos casos de error devuelven `"Credenciales inválidas."` de forma intencional — previene ataques de enumeración de usuarios (no se revela si el usuario existe).

### Tests de integración

Levantan la API completa en memoria con `WebApplicationFactory<Program>`, sustituyen SQLite de archivo por **SQLite `:memory:`** y ejecutan requests HTTP reales.

```
CourierMaxWebFactory.InitializeAsync()
  1. Abre la conexión SQLite :memory:
  2. Crea el schema y siembra datos (conductores, vehículos, usuario admin)
  3. Crea un HttpClient con HandleCookies = true
  4. Hace POST /api/auth/login → la cookie access_token queda en el cliente
  5. Expone ese cliente como AuthenticatedClient

ShipmentsIntegrationTests
  → usa factory.AuthenticatedClient en todos sus tests
  → cada request incluye automáticamente la cookie de sesión
```

El login ocurre **una sola vez por clase de test** (por ser `IClassFixture`), no antes de cada método.

**Consideraciones al escribir nuevos tests de integración:**
- Usar siempre `factory.AuthenticatedClient`, nunca `factory.CreateClient()` (sin cookie).
- Para verificar que un endpoint rechaza sin autenticación, usar `factory.CreateClient()` y confirmar `401`.
- Los tests comparten la misma base de datos `:memory:` — los datos creados en un test son visibles en los siguientes.

---

## Arquitectura

### Clean Architecture

```
src/
├── Ceiba.CourierMax.Domain        → Entidades, Value Objects, servicios de dominio, contratos
├── Ceiba.CourierMax.Application   → Casos de uso, DTOs, validators, mappers, contratos de servicios
├── Ceiba.CourierMax.Persistence   → EF Core + SQLite, repositorios, seed de datos
└── Ceiba.CourierMax.API           → Controladores, middleware, configuración DI, JWT

tests/
└── Ceiba.CourierMax.Tests         → Tests unitarios e integración (xUnit)
```

**Flujo de dependencias:**
```
API → Application → Domain
Persistence → Domain  (implementa interfaces de repositorio)
```

### Registro de servicios (DI)

Cada capa expone su propio método de extensión en la raíz de su proyecto:

| Archivo | Capa | Registra |
|---|---|---|
| `ApplicationServiceExtensions.cs` | Application | Use cases, validators, AutoMapper |
| `PersistenceServiceExtensions.cs` | Persistence | DbContext, repositorios |
| `DependencyInjectionService.cs` | API | Cookie policy, JWT, Swagger, FluentValidation pipeline |

`Program.cs` los encadena en orden:
```csharp
builder.Services
    .AddPersistence(builder.Configuration)
    .AddApplicationServices()
    .AddWebApi(builder.Configuration);
```

### Decisiones de diseño

| Decisión | Elección | Justificación |
|---|---|---|
| **Persistencia** | SQLite + EF Core 10 | Sin infraestructura externa; datos persisten entre reinicios |
| **Patrón de casos de uso** | Use Cases por funcionalidad (sin MediatR) | Cada caso de uso es una clase con una responsabilidad; claridad sin sobreingeniería |
| **Validación** | FluentValidation | Validators en Application, activados automáticamente en el pipeline de ASP.NET |
| **Mapeo** | AutoMapper (`MappingProfile`) | Reemplaza mapper estático; `IMapper` inyectable y testeable |
| **Formato de respuesta** | `ModelResponse` propio | Envoltorio uniforme `{ statusCode, success, message, data }` en todos los endpoints |
| **Errores no controlados** | `ModelResponse` propio | `ExceptionHandlingMiddleware` captura excepciones y devuelve el mismo envoltorio uniforme |
| **Token de sesión** | JWT en cookie HttpOnly | Previene XSS (JS no puede leer la cookie) + SameSite=Strict previene CSRF |
| **Inyección en controladores** | Constructor vacío + `[FromServices]` | Cada acción declara exactamente lo que necesita; sin estado en el controlador |
| **Enums** | Strings en JSON | Legibilidad (`"Express"` en lugar de `1`) |
| **Value Objects** | C# `record` | Inmutabilidad + igualdad por valor nativos |
| **Festivos** | Hardcoded 2026 | Lista provista explícitamente en los requisitos |
| **Dimensiones** | `ComplexProperty` EF Core 8+ | Almacenamiento inline sin tabla separada |
| **Documentación** | Swagger UI | Integrado con esquema de cookie para pruebas interactivas |

### Capas en detalle

**Domain** — núcleo sin dependencias externas:
- `Shipment` como agregado raíz con máquina de estados interna (`CREADO → ASIGNADO → EN_TRANSITO → ENTREGADO / CANCELADO`)
- Value Objects: `TrackingCode`, `PhoneNumber`, `Address`, `PackageDimensions`
- `BusinessDayService` con festivos colombianos 2026
- `CityRouteService` con tarifas por par de ciudades

**Application** — orquestación de reglas:
- 7 interfaces de casos de uso + sus implementaciones
- `FareCalculatorService` implementa el cálculo de tarifas con recargos por tipo de paquete
- `MappingProfile` (AutoMapper): `Shipment → ShipmentResponse`, `StatusHistory → StatusHistoryResponse`
- 5 validators FluentValidation: `LoginRequest`, `CreateShipmentRequest`, `AssignShipmentRequest`, `ChangeStatusRequest`, `CancelShipmentRequest`

**Persistence** — infraestructura de datos:
- Repositorios con `Include` explícito para evitar N+1
- `DataSeeder` idempotente con IDs fijos para los 3 conductores y vehículos
- Value Converters para mapear Value Objects a columnas simples en SQLite

**API** — punto de entrada HTTP:
- Controladores con constructor vacío y `[FromServices]` por método
- `JwtTokenService`: genera y revoca el JWT escribiendo/eliminando la cookie HttpOnly
- `ExceptionHandlingMiddleware`: captura `DomainException` (422), `KeyNotFoundException` (404) y excepciones genéricas (500)
- `DependencyInjectionService`: política global de cookies, JWT Bearer, Swagger con esquema de cookie

---

## Autenticación y autorización

Todos los endpoints de negocio están protegidos con `[Authorize(Roles = "Admin")]`. El token se transporta en una **cookie HttpOnly**, no en el header `Authorization`.

### Flujo

1. El cliente hace `POST /api/auth/login` con usuario y contraseña.
2. La API valida las credenciales contra SQLite (contraseña hasheada con BCrypt).
3. Si son válidas, genera un JWT firmado con HMAC-SHA256 y lo devuelve en una **cookie HttpOnly**.
4. El browser adjunta la cookie automáticamente en cada request posterior.
5. El middleware de ASP.NET lee el JWT desde la cookie, lo valida y establece el `ClaimsPrincipal`.

### Por qué cookie HttpOnly en lugar de localStorage

| Almacenamiento | XSS | CSRF |
|---|---|---|
| `localStorage` + header `Bearer` | **Vulnerable** — cualquier script puede leer el token | Seguro |
| Cookie `HttpOnly` + `SameSite=Strict` | **Seguro** — JavaScript no puede acceder a la cookie | **Seguro** — el browser no la envía en requests cross-site |

### Atributos de seguridad

| Atributo | Protege contra |
|---|---|
| `HttpOnly` | XSS — JavaScript no puede leer ni robar el token |
| `Secure` | Sniffing — solo viaja sobre HTTPS |
| `SameSite=Strict` | CSRF — no se envía en requests de otros dominios |

### Credenciales pre-cargadas

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `Admin123!` | Admin |

### Endpoints de autenticación

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `POST` | `/api/auth/login` | Público | Valida credenciales y establece la cookie |
| `POST` | `/api/auth/logout` | Requerida | Elimina la cookie de sesión |

**Login — request:**
```json
{
  "username": "admin",
  "password": "Admin123!"
}
```

**Login — respuesta `200 OK`:**
```json
{
  "statusCode": 200,
  "success": true,
  "message": "Login exitoso",
  "data": {
    "username": "admin",
    "role": "Admin"
  }
}
```

### Cómo probarlo en Swagger

1. Ir a `http://localhost:5262/swagger`
2. Ejecutar `POST /api/auth/login` con las credenciales de arriba
3. La cookie se almacena automáticamente en el browser
4. Todos los demás endpoints funcionan desde ese momento sin pasos adicionales

> Si un endpoint se llama sin sesión activa, la API responde `401 Unauthorized`.

### Configuración JWT

```json
"Jwt": {
  "SecretKey": "CourierMax2026$SuperSecretKey#MustBe32Chars!!",
  "Issuer": "CourierMax",
  "Audience": "CourierMaxUsers",
  "ExpirationHours": 8
}
```

> En producción, `SecretKey` debe gestionarse como variable de entorno o secreto de bóveda (Azure Key Vault, AWS Secrets Manager, etc.), nunca en el repositorio.

---

## Formato de respuesta

Todos los endpoints — incluyendo errores de validación, excepciones de dominio y errores internos — devuelven siempre el mismo envoltorio `ModelResponse`. El front nunca necesita manejar formatos distintos según el tipo de error.

```json
{
  "statusCode": 201,
  "success": true,
  "message": "Envío creado exitosamente.",
  "data": { ... }
}
```

| Campo | Tipo | Descripción |
|---|---|---|
| `statusCode` | `int` | Código HTTP de la operación |
| `success` | `bool` | `true` si `statusCode` está entre 200 y 299 |
| `message` | `string` | Descripción del resultado o del error |
| `data` | `object \| null` | Payload de la respuesta (`null` en errores) |

---

## Manejo de errores

Todos los códigos de estado usan el mismo formato `ModelResponse`:

| Código | Situación | `success` |
|---|---|---|
| `200` / `201` | Operación exitosa | `true` |
| `400` | Validación fallida (FluentValidation) | `false` |
| `401` | Sin autenticación o token expirado | `false` |
| `404` | Recurso no encontrado | `false` |
| `422` | Regla de negocio violada | `false` |
| `500` | Error interno inesperado | `false` |

**Ejemplo 400 — validación:**
```json
{
  "statusCode": 400,
  "success": false,
  "message": "El nombre del remitente es obligatorio.; El peso debe ser mayor a 0.",
  "data": null
}
```

**Ejemplo 422 — regla de negocio:**
```json
{
  "statusCode": 422,
  "success": false,
  "message": "El vehículo ABC-123 excede su capacidad de peso. Disponible: 50 kg, requerido: 80 kg.",
  "data": null
}
```

**Ejemplo 404 — recurso no encontrado:**
```json
{
  "statusCode": 404,
  "success": false,
  "message": "Envío CM12345678 no encontrado.",
  "data": null
}
```

**Ejemplo 500 — error inesperado:**
```json
{
  "statusCode": 500,
  "success": false,
  "message": "Ocurrió un error inesperado. Intente más tarde.",
  "data": null
}
```

---

## Endpoints disponibles

### Envíos

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/shipments` | Crear envío (tarifa calculada automáticamente) |
| `GET` | `/api/shipments` | Listar todos los envíos |
| `GET` | `/api/shipments/{id}` | Obtener envío por ID |
| `GET` | `/api/shipments/tracking/{code}` | Rastrear por código `CM########` |
| `POST` | `/api/shipments/{id}/assign` | Asignar a conductor (valida capacidad) |
| `PUT` | `/api/shipments/{id}/transit` | Marcar en tránsito |
| `PUT` | `/api/shipments/{id}/deliver` | Marcar como entregado |
| `PUT` | `/api/shipments/{id}/cancel` | Cancelar con motivo obligatorio |

### Conductores

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/drivers` | Listar conductores con sus vehículos |
| `GET` | `/api/drivers/metrics` | Métricas de eficiencia de todos los conductores |
| `GET` | `/api/drivers/{id}/metrics` | Métricas de un conductor específico |

### Reportes

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/reports/delayed?from=&to=` | Envíos atrasados (superaron SLA) en rango de fechas |

---

## Ejemplos de uso (curl)

> Todos los ejemplos asumen que la API corre en `http://localhost:5262` y que la cookie `access_token` ya está establecida (tras hacer login).

### 1. Login

```bash
curl -c cookies.txt -X POST http://localhost:5262/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{ "username": "admin", "password": "Admin123!" }'
```

### 2. Crear un envío

```bash
curl -b cookies.txt -X POST http://localhost:5262/api/shipments \
  -H "Content-Type: application/json" \
  -d '{
    "senderName": "Carlos Martínez",
    "senderPhone": "3001234567",
    "senderAddress": "Calle 72 #10-34, Bogotá",
    "recipientName": "Laura Gómez",
    "recipientPhone": "3159876543",
    "recipientAddress": "Carrera 43A #5A-113, Medellín",
    "weightKg": 5,
    "lengthCm": 30,
    "widthCm": 20,
    "heightCm": 15,
    "packageType": "Fragil",
    "serviceType": "Express",
    "originCity": "Bogota",
    "destinationCity": "Medellin"
  }'
```

**Respuesta 201:**
```json
{
  "statusCode": 201,
  "success": true,
  "message": "Envío creado exitosamente.",
  "data": {
    "id": "3fa85f64-...",
    "trackingCode": "CM24488879",
    "fare": 40950,
    "status": "CREADO"
  }
}
```

### 3. Rastrear envío

```bash
curl -b cookies.txt http://localhost:5262/api/shipments/tracking/CM24488879
```

### 4. Asignar a conductor

```bash
curl -b cookies.txt -X POST http://localhost:5262/api/shipments/{id}/assign \
  -H "Content-Type: application/json" \
  -d '{
    "driverId": "22222222-0000-0000-0000-000000000001",
    "assignedBy": "operador1"
  }'
```

### 5. Marcar en tránsito

```bash
curl -b cookies.txt -X PUT http://localhost:5262/api/shipments/{id}/transit \
  -H "Content-Type: application/json" \
  -d '{ "changedBy": "conductor1" }'
```

### 6. Entregar

```bash
curl -b cookies.txt -X PUT http://localhost:5262/api/shipments/{id}/deliver \
  -H "Content-Type: application/json" \
  -d '{ "changedBy": "conductor1" }'
```

### 7. Cancelar

```bash
curl -b cookies.txt -X PUT http://localhost:5262/api/shipments/{id}/cancel \
  -H "Content-Type: application/json" \
  -d '{
    "reason": "Cliente solicitó cancelación por cambio de dirección",
    "cancelledBy": "operador1"
  }'
```

### 8. Métricas de conductores

```bash
curl -b cookies.txt http://localhost:5262/api/drivers/metrics
```

### 9. Envíos atrasados

```bash
curl -b cookies.txt "http://localhost:5262/api/reports/delayed?from=2026-01-01&to=2026-12-31"
```

---

## Datos de referencia (pre-cargados)

### Conductores y vehículos

| ID Conductor | Nombre | Placa | Cap. Peso | Cap. Volumen |
|---|---|---|---|---|
| `22222222-0000-0000-0000-000000000001` | Juan Pérez | ABC-123 | 500 kg | 10 m³ |
| `22222222-0000-0000-0000-000000000002` | María López | DEF-456 | 300 kg | 6 m³ |
| `22222222-0000-0000-0000-000000000003` | Carlos Ruiz | GHI-789 | 800 kg | 15 m³ |

### Ciudades válidas

`Bogota` · `Medellin` · `Cali` · `Barranquilla`

### Tipos de servicio

| Valor | SLA | Tarifa base |
|---|---|---|
| `Estandar` | 5 días hábiles | $8,000 |
| `Express` | 2 días hábiles | $15,000 |
| `MismoDia` | 0 días hábiles | $25,000 |

### Tipos de paquete

| Valor | Recargo |
|---|---|
| `Documento` | Sin recargo |
| `Paquete` | Sin recargo |
| `Fragil` | +30% |
| `Perecedero` | +25% |

### Estados del envío

```
CREADO → ASIGNADO → EN_TRANSITO → ENTREGADO
                 ↘              ↘
               CANCELADO      CANCELADO
```
