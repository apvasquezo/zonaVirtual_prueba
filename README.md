# Zona Virtual — Prueba Técnica (EV-ZP-004)

Proyecto completo: API en **.NET 8** (Web API + EF Core + SQL Server) y frontend en **Angular 17**.

```
zona-virtual/
├── backend/ZonaVirtual.Api/     API REST (.NET 8)
└── frontend/zona-virtual-web/   Aplicación Angular
```

## Arquitectura y modelo de datos

**Backend en capas:**
- `Controllers` → capa de entrada HTTP (solo reciben request/response, sin lógica de negocio).
- `Services` → lógica de negocio (auth, transacciones, generación de datos de prueba).
- `Data` → `AppDbContext` (EF Core).
- `Models` → entidades de dominio.
- `Dtos` → contratos de entrada/salida, para no exponer las entidades directamente.

**Modelo relacional (normalizado, 3FN):**

| Tabla | Descripción |
|---|---|
| `Comercios` | comercio_codigo, comercio_nombre, comercio_nit, comercio_direccion |
| `UsuariosPagadores` | usuario_identificacion, usuario_nombre, usuario_email |
| `Transacciones` | Trans_codigo (único), Trans_medio_pago, Trans_estado, Trans_total, Trans_fecha, Trans_concepto + FK a Comercio y a UsuarioPagador |
| `Cuentas` | credenciales de acceso (username + hash de contraseña), enlazadas 1-a-1 a un pagador **o** a un comercio, nunca ambos |

Se separó la identidad de negocio (`Comercios` / `UsuariosPagadores`, que es lo que genera el endpoint de datos de prueba) de las credenciales de acceso (`Cuentas`), porque así lo pide el Punto 2: los registros pueden existir sin tener todavía usuario/contraseña, y el login se resuelve verificando primero si la persona existe y luego si ya tiene cuenta.

Índices creados: `TransCodigo` (único), `TransFecha`, `TransEstado`, `ComercioId`, `UsuarioPagadorId`, y únicos en `ComercioCodigo`, `ComercioNit`, `UsuarioIdentificacion`, `UsuarioEmail`.

## Cómo levantar el backend

**Requisitos:** .NET 8 SDK, SQL Server (local, Docker o Express) con acceso a internet para restaurar NuGet.

1. Crear la base de datos:
   - **Opción A (recomendada, EF Core migrations):**
     ```bash
     cd backend/ZonaVirtual.Api
     dotnet tool install --global dotnet-ef   # si no lo tienes
     dotnet ef migrations add InitialCreate
     dotnet ef database update
     ```
   - **Opción B (script SQL manual):** ejecuta `backend/ZonaVirtual.Api/Database/schema.sql` en SQL Server Management Studio / Azure Data Studio.

2. Ajustar la cadena de conexión y la clave de JWT en `appsettings.json` si es necesario (usuario `zv_user`, clave `CambiaEstaClave123!` por defecto — cámbiala).

3. Ejecutar la API:
   ```bash
   cd backend/ZonaVirtual.Api
   dotnet restore
   dotnet run
   ```
   Swagger queda disponible en `https://localhost:7099/swagger` (revisa el puerto real que imprime la consola y ajústalo en `frontend/zona-virtual-web/src/environments/environment.ts`).

4. Generar datos de prueba (Punto 1): desde Swagger o Postman,
   ```
   POST /api/Prueba/GenerarDatos
   { "cantidadComercios": 5, "cantidadUsuarios": 10, "cantidadTransacciones": 25 }
   ```

## Cómo levantar el frontend

**Requisitos:** Node.js 18+ y Angular CLI (`npm i -g @angular/cli`).

```bash
cd frontend/zona-virtual-web
npm install
ng serve
```

Abre `http://localhost:4200`. El proyecto ya compiló limpio (`ng build`) durante el desarrollo, así que `npm install` + `ng serve` debe levantar sin pasos adicionales — solo confirma que `environment.ts` apunte al puerto real de tu API.

## Flujo funcional implementado

- **Punto 1:** `POST /api/Prueba/GenerarDatos` genera comercios, usuarios pagadores y transacciones aleatorias, validando que `Trans_codigo` no se repita.
- **Punto 2:** en el login de Angular, el usuario elige perfil (pagador/comercio), escribe su identificación/NIT; si no tiene cuenta se le pide crear usuario y contraseña (`POST /api/auth/registro`); si ya la tiene, inicia sesión (`POST /api/auth/login`) y recibe un JWT.
- **Punto 3:** el pagador ve sus pagos (`GET /api/transacciones/pagador`) y puede registrar uno nuevo a cualquier comercio existente (`POST /api/transacciones/pagador`), validando que `Trans_codigo` no se repita.
- **Punto 4:** el comercio ve los pagos recibidos (`GET /api/transacciones/comercio`), con filtros por fecha, código y nombre de cliente, y el total de las transacciones filtradas.
- **Punto 6:** el comercio puede modificar una transacción (`PUT /api/transacciones/comercio/{id}`) solo si su estado no es `Aprobada (1)`.
- Tablas de `Trans_medio_pago` y `Trans_estado` se muestran traducidas en toda la interfaz (Tarjeta de Crédito, PSE, Gana, Caja / Aprobada, Rechazada, Pendiente, Rechazada SR).

## Notas

- La autenticación usa JWT (Bearer) con claims de perfil y de referencia (Id de pagador o de comercio), y los endpoints de transacciones están protegidos con `[Authorize(Roles = "Pagador")]` / `[Authorize(Roles = "Comercio")]`.
- Las contraseñas se guardan con PBKDF2 + salt (sin dependencias externas).
- CORS está habilitado para `http://localhost:4200`.
