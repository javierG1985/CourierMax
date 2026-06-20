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
git clone <URL_DEL_REPOSITORIO>
cd Ceiba.CourierMax

# 2. Restaurar dependencias
dotnet restore

# 3. Ejecutar la API
dotnet run --project src/Ceiba.CourierMax.API

# La API arranca en: http://localhost:5262
# Documentación interactiva (Swagger UI): http://localhost:5262/swagger
```

> Al iniciar, la API crea automáticamente el esquema SQLite (`couriermax.db`) y siembra los datos de referencia (vehículos y conductores).

---

## Ejecutar los tests

```bash
dotnet test tests/Ceiba.CourierMax.Tests
```

**70 tests en total:**
- 62 unitarios (Domain + Application)
- 8 de integración HTTP end-to-end (SQLite `:memory:`)

### Cómo están organizados los tests

```
tests/Ceiba.CourierMax.Tests/
├── Unit/
│   ├── Domain/
│   │   ├── TrackingCodeTests.cs         → formato y generación del código de rastreo
│   │   ├── PhoneNumberTests.cs          → validación de número telefónico colombiano
│   │   ├── PackageDimensionsTests.cs    → cálculo de volumen m³
│   │   ├── BusinessDayServiceTests.cs   → días hábiles y festivos 2026
│   │   ├── ShipmentTests.cs             → máquina de estados del envío
│   │   └── VehicleCapacityTests.cs      → validación de capacidad del vehículo
│   └── Application/
│       ├── FareCalculatorServiceTests.cs → cálculo de tarifas con recargos
│       └── LoginUseCaseTests.cs          → autenticación: credenciales válidas e inválidas
└── Integration/
    ├── CourierMaxWebFactory.cs           → factory con cliente pre-autenticado
    └── ShipmentsIntegrationTests.cs      → flujos HTTP completos contra SQLite :memory:
```

### Tests unitarios — cómo funcionan

Los tests unitarios **no levantan la API ni tocan la base de datos**. Instancian las clases directamente con sus dependencias controladas.

Para `LoginUseCaseTests` se usa un **`FakeUserRepository`** (clase privada del test) que devuelve un usuario predefinido o `null`, según el caso. El hash de BCrypt se genera en el propio setup del test con `BCrypt.HashPassword()`, garantizando que la verificación sea real y no simulada.

```
Test                                     Qué verifica
─────────────────────────────────────────────────────────────────
Login_WithValidCredentials               Username y Role en la respuesta
Login_WithUnknownUsername                Lanza DomainException (mensaje genérico)
Login_WithWrongPassword                  Lanza DomainException (mismo mensaje)
```

> Ambos casos de error retornan el mismo mensaje (`"Credenciales inválidas."`) de forma intencional para no revelar si el usuario existe o no — esto previene ataques de enumeración de usuarios.

### Tests de integración — cómo funcionan

Los tests de integración **levantan la API completa** en memoria usando `WebApplicationFactory<Program>`, sustituyen el SQLite de archivo por **SQLite `:memory:`** y ejecutan requests HTTP reales contra los controladores.

**El problema con la autenticación:** al agregar `[Authorize]` a todos los endpoints, los tests empezarían a recibir `401 Unauthorized`. La solución es que la factory haga login una sola vez durante la inicialización y comparta el cliente autenticado entre todos los tests.

**Cómo está implementado:**

```
CourierMaxWebFactory.InitializeAsync()
  1. Abre la conexión SQLite :memory:
  2. Crea el schema y siembra datos (vehículos, conductores, usuario admin)
  3. Crea un HttpClient con HandleCookies = true
  4. Hace POST /api/auth/login → la cookie access_token queda en el cliente
  5. Expone ese cliente como AuthenticatedClient

ShipmentsIntegrationTests
  → usa factory.AuthenticatedClient en todos sus tests
  → cada request incluye automáticamente la cookie de sesión
```

El login ocurre **una sola vez por clase de test** (por ser `IClassFixture`), no antes de cada método — lo que mantiene los tests rápidos.

### Consideraciones al escribir nuevos tests de integración

- Usar siempre `factory.AuthenticatedClient`, nunca `factory.CreateClient()` (ese cliente no tiene la cookie).
- Si se necesita verificar que un endpoint **rechaza** requests sin autenticación, crear un cliente separado con `factory.CreateClient()` y confirmar que responde `401`.
- Los tests comparten la misma base de datos `:memory:` durante toda la ejecución de la clase — los datos creados en un test son visibles en los siguientes. Diseñar los tests teniendo esto en cuenta (p. ej., no asumir que una lista está vacía).

---

## Arquitectura

### Clean Architecture (Arquitectura Limpia)

```
src/
├── Ceiba.CourierMax.Domain        → Entidades, Value Objects, servicios de dominio, interfaces
├── Ceiba.CourierMax.Application   → Casos de uso, DTOs, cálculo de tarifas, mappers
├── Ceiba.CourierMax.Persistence   → EF Core + SQLite, repositorios, seed de datos
└── Ceiba.CourierMax.API           → Controladores, middleware, configuración DI

tests/
└── Ceiba.CourierMax.Tests         → Tests unitarios e integración
```

**Flujo de dependencias:**
```
API → Application → Domain
Persistence → Domain  (implementa interfaces)
```

### Decisiones de diseño

| Decisión | Elección | Justificación |
|---|---|---|
| **Persistencia** | SQLite + EF Core | Sin infraestructura externa; datos persisten entre reinicios |
| **Patrón de casos de uso** | CQRS simple (sin MediatR) | Claridad sin sobreingeniería |
| **Errores** | ProblemDetails RFC 7807 | Estándar ASP.NET Core, formato consistente |
| **Enums** | Strings en JSON | Legibilidad en las peticiones (`"Express"` en lugar de `1`) |
| **Value Objects** | C# `record` | Inmutabilidad + igualdad por valor nativos del lenguaje |
| **Festivos** | Hardcoded 2026 | La lista fue provista explícitamente en los requisitos |
| **Dimensiones** | `ComplexProperty` EF Core 8+ | Almacenamiento inline sin tabla separada |
| **Decimales** | `HasColumnType("TEXT")` en SQLite | Evita pérdida de precisión en valores monetarios |
| **Documentación** | Scalar UI | Reemplaza Swagger UI con interfaz más moderna |

### Capas en detalle

**Domain** — núcleo sin dependencias externas:
- `Shipment` como agregado raíz con máquina de estados interna
- Value Objects: `TrackingCode`, `PhoneNumber`, `Address`, `PackageDimensions`
- `BusinessDayService` con festivos colombianos 2026 hardcoded
- `CityRouteService` con tarifas por par de ciudades (bidireccional)

**Application** — orquestación de reglas:
- `FareCalculatorService` implementa RF-04 completo
- 7 casos de uso cubriendo todo el ciclo de vida de un envío
- Reportes: envíos atrasados (RF-05) y métricas de conductores (RF-06)

**Persistence** — infraestructura:
- Repositorios con `Include` explícito para evitar N+1
- `DataSeeder` idempotente con IDs fijos para los 3 vehículos y conductores del enunciado
- Value Converters para mapear Value Objects a columnas simples

---

## Autenticación y autorización

### Descripción general

Todos los endpoints de negocio están protegidos con **JWT Bearer + cookie HttpOnly**. El rol requerido para acceder es `Admin`.

El flujo es:
1. El cliente hace `POST /api/auth/login` con usuario y contraseña.
2. La API valida las credenciales contra la tabla `Users` en SQLite (contraseña hasheada con **BCrypt**).
3. Si son válidas, genera un **JWT** firmado con HMAC-SHA256 y lo devuelve en una **cookie HttpOnly**.
4. El browser adjunta esa cookie automáticamente en cada request posterior — sin intervención del cliente.
5. El middleware de ASP.NET Core lee el JWT desde la cookie, lo valida y establece el `ClaimsPrincipal`.

### Por qué cookie HttpOnly en lugar de `localStorage`

| Almacenamiento | XSS | CSRF |
|---|---|---|
| `localStorage` + header `Bearer` | **Vulnerable** — cualquier script puede leer el token | Seguro |
| Cookie `HttpOnly` + `SameSite=Strict` | **Seguro** — JavaScript no puede acceder a la cookie | **Seguro** — el browser no la envía en requests cross-site |

Con `SameSite=Strict` no se necesita un token CSRF adicional: el browser rechaza automáticamente enviar la cookie si el request proviene de otro dominio.

### Atributos de seguridad de la cookie

```
Set-Cookie: access_token=<jwt>; HttpOnly; Secure; SameSite=Strict; Expires=...
```

| Atributo | Protege contra |
|---|---|
| `HttpOnly` | XSS — JavaScript no puede leer ni robar el token |
| `Secure` | Sniffing — solo viaja sobre HTTPS |
| `SameSite=Strict` | CSRF — no se envía en requests originados desde otros dominios |

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
  "username": "admin",
  "role": "Admin",
  "message": "Login exitoso."
}
```
> La cookie `access_token` queda establecida en el browser. No es necesario copiar ni pegar ningún token.

### Cómo probarlo en Swagger

1. Ir a `http://localhost:5262/swagger`
2. Ejecutar `POST /api/auth/login` con las credenciales de arriba
3. La cookie se almacena automáticamente en el browser
4. Todos los demás endpoints funcionan desde ese momento sin ningún paso adicional

Si un endpoint se llama sin sesión activa, la API responde `401 Unauthorized`.

### Configuración JWT (`appsettings.json`)

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

> Todos los ejemplos asumen que la API corre en `http://localhost:5262`

### 1. Crear un envío

```bash
curl -X POST http://localhost:5262/api/shipments \
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

**Respuesta esperada (201 Created):**
```json
{
  "id": "3fa85f64-...",
  "trackingCode": "CM24488879",
  "fare": 40950,
  "status": "CREADO",
  ...
}
```

### 2. Rastrear envío por código

```bash
curl http://localhost:5262/api/shipments/tracking/CM24488879
```

### 3. Asignar a conductor

```bash
# IDs de conductores disponibles:
# Juan Pérez:   22222222-0000-0000-0000-000000000001
# María López:  22222222-0000-0000-0000-000000000002
# Carlos Ruiz:  22222222-0000-0000-0000-000000000003

curl -X POST http://localhost:5262/api/shipments/{id}/assign \
  -H "Content-Type: application/json" \
  -d '{
    "driverId": "22222222-0000-0000-0000-000000000001",
    "assignedBy": "operador1"
  }'
```

### 4. Marcar en tránsito

```bash
curl -X PUT http://localhost:5262/api/shipments/{id}/transit \
  -H "Content-Type: application/json" \
  -d '{ "changedBy": "conductor1" }'
```

### 5. Entregar

```bash
curl -X PUT http://localhost:5262/api/shipments/{id}/deliver \
  -H "Content-Type: application/json" \
  -d '{ "changedBy": "conductor1" }'
```

### 6. Cancelar

```bash
curl -X PUT http://localhost:5262/api/shipments/{id}/cancel \
  -H "Content-Type: application/json" \
  -d '{
    "reason": "Cliente solicitó cancelación por cambio de dirección",
    "cancelledBy": "operador1"
  }'
```

### 7. Métricas de conductores

```bash
curl http://localhost:5262/api/drivers/metrics
```

### 8. Envíos atrasados

```bash
curl "http://localhost:5262/api/reports/delayed?from=2026-01-01&to=2026-12-31"
```

---

## Datos de referencia (pre-cargados)

### Conductores y vehículos

| ID Conductor | Nombre | Vehículo | Cap. Peso | Cap. Volumen |
|---|---|---|---|---|
| `22222222-0000-0000-0000-000000000001` | Juan Pérez | ABC-123 | 500 kg | 10 m³ |
| `22222222-0000-0000-0000-000000000002` | María López | DEF-456 | 300 kg | 6 m³ |
| `22222222-0000-0000-0000-000000000003` | Carlos Ruiz | GHI-789 | 800 kg | 15 m³ |

### Ciudades válidas

`Bogota`, `Medellin`, `Cali`, `Barranquilla`

### Tipos de servicio

| Valor | Descripción | SLA | Tarifa base |
|---|---|---|---|
| `Estandar` | Estándar | 5 días hábiles | $8,000 |
| `Express` | Express | 2 días hábiles | $15,000 |
| `MismoDia` | Mismo día | 0 días hábiles | $25,000 |

### Tipos de paquete

| Valor | Recargo |
|---|---|
| `Documento` | Sin recargo |
| `Paquete` | Sin recargo |
| `Fragil` | +30% |
| `Perecedero` | +25% |

---

## Manejo de errores

La API responde siempre con [ProblemDetails (RFC 7807)](https://datatracker.ietf.org/doc/html/rfc7807):

| Código | Situación |
|---|---|
| `201` | Recurso creado exitosamente |
| `200` | Operación exitosa |
| `400` | Parámetros inválidos (query params, formato incorrecto) |
| `404` | Recurso no encontrado |
| `422` | Regla de negocio violada (estado inválido, capacidad excedida, etc.) |
| `500` | Error interno inesperado |

```json
{
  "title": "Regla de negocio",
  "status": 422,
  "detail": "El vehículo ABC-123 excede su capacidad de peso. Disponible: 50 kg, requerido: 80 kg.",
  "instance": "/api/shipments/3fa85f64-.../assign"
}
```
