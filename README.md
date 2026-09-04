# Zona Virtual — Prueba Técnica

## Plataforma de pagos

Aplicación web desarrollada como prueba técnica para el cargo de
**Desarrollador Full Stack en Zona Virtual S.A.**

La solución permite gestionar pagos mediante dos perfiles:

- **Pagador:** consulta sus pagos y registra nuevas transacciones.
- **Comercio:** consulta los pagos recibidos, filtra transacciones y
  actualiza aquellas que aún no han sido aprobadas.

### Tecnologías principales

- **Backend:** .NET 8 / ASP.NET Core Web API
- **ORM:** Entity Framework Core 8
- **Base de datos:** SQL Server
- **Frontend:** Angular 17
- **Autenticación:** JWT Bearer
- **Hash de contraseñas:** PBKDF2 + SHA-256
 
```
zona-virtual/
├── backend/ZonaVirtual.Api/     API REST (.NET 8)
└── frontend/zona-virtual-web/   Aplicación Angular
```
 
---
 
## Tabla de contenido
 
- [Stack técnico](#stack-técnico)
- [Arquitectura del backend](#arquitectura-del-backend)
- [Modelo de datos](#modelo-de-datos)
- [Requisitos previos](#requisitos-previos)
- [Cómo levantar el backend](#cómo-levantar-el-backend)
- [Cómo levantar el frontend](#cómo-levantar-el-frontend)
- [Flujo funcional de la aplicación](#flujo-funcional-de-la-aplicación)
- [Mapeo de requisitos de la prueba](#mapeo-de-requisitos-de-la-prueba)
- [Solución de problemas comunes](#solución-de-problemas-comunes)
- [Seguridad](#seguridad)
---
 
## Stack técnico
 
| Capa | Tecnología |
|---|---|
| Backend | .NET 8, ASP.NET Core Web API, Entity Framework Core 8 |
| Base de datos | SQL Server |
| Autenticación | JWT (Bearer) + PBKDF2 para hash de contraseñas |
| Documentación de API | Swagger / OpenAPI |
| Frontend | Angular 17 (standalone components, signals) |
| Estilos | CSS puro por componente (sin librerías de UI externas) |
 
---
## Características principales

### Pagador

- Registro y autenticación.
- Consulta de pagos realizados.
- Registro de nuevas transacciones.
- Selección del comercio destinatario.
- Validación de código de transacción único.

### Comercio

- Autenticación mediante JWT.
- Consulta de pagos recibidos.
- Filtros por fecha, código de pago y cliente.
- Cálculo del total de las transacciones filtradas.
- Actualización de transacciones no aprobadas.

### Seguridad

- Autenticación basada en JWT.
- Autorización por roles.
- Contraseñas protegidas mediante PBKDF2 + salt.
- Separación entre identidad de negocio y credenciales.
- Protección de endpoints mediante `[Authorize]`. 
---
## Arquitectura del backend
 
El proyecto está organizado en capas, separando responsabilidades:
 
```
ZonaVirtual.Api/
├── Controllers/     → capa de entrada HTTP: reciben el request, delegan al servicio, devuelven la respuesta
├── Services/        → lógica de negocio (auth, transacciones, generación de datos de prueba)
├── Data/            → AppDbContext (EF Core)
├── Models/          → entidades de dominio
├── Dtos/            → contratos de entrada/salida, para no exponer las entidades directamente
└── Database/        → schema.sql, alternativa a las migraciones de EF Core
```
 
Un controller nunca contiene lógica de negocio — solo valida el modelo, llama al servicio correspondiente y traduce el resultado (o la excepción) a un código HTTP. Toda la lógica de validación (Trans_codigo único, no editar transacciones aprobadas, etc.) vive en `Services`.
 
## Modelo de datos
 
**Modelo relacional, normalizado en 3FN:**
 
| Tabla | Descripción |
|---|---|
| `Comercios` | `ComercioCodigo`, `ComercioNombre`, `ComercioNit`, `ComercioDireccion` |
| `UsuariosPagadores` | `UsuarioIdentificacion`, `UsuarioNombre`, `UsuarioEmail` |
| `Transacciones` | `TransCodigo` (único), `TransMedioPago`, `TransEstado`, `TransTotal`, `TransFecha`, `TransConcepto` + FK a `Comercio` y a `UsuarioPagador` |
| `Cuentas` | credenciales de acceso (`Username` + `PasswordHash`), enlazadas 1-a-1 a un pagador **o** a un comercio, nunca a ambos |
 
**Decisión de diseño clave:** se separó la identidad de negocio (`Comercios` / `UsuariosPagadores` — lo que genera el endpoint de datos de prueba) de las credenciales de acceso (`Cuentas`). Esto permite que un registro exista en la base sin tener todavía usuario/contraseña, que es justo el escenario que describe el Punto 2 del enunciado.
 
**Índices:**
- Únicos: `Comercios.ComercioCodigo`, `Comercios.ComercioNit`, `UsuariosPagadores.UsuarioIdentificacion`, `UsuariosPagadores.UsuarioEmail`, `Transacciones.TransCodigo`, `Cuentas.(Perfil, Username)`.
- De consulta: `Transacciones.TransFecha`, `Transacciones.TransEstado`, `Transacciones.ComercioId`, `Transacciones.UsuarioPagadorId`.
---
 
## Requisitos previos
 
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local, Express, o Docker)
- [Node.js 18+](https://nodejs.org) y npm
- [Angular CLI](https://angular.dev/tools/cli): `npm install -g @angular/cli`
- Git
---
 
## Cómo levantar el backend
 
### 1. Restaurar paquetes NuGet
 
```bash
cd backend/ZonaVirtual.Api
dotnet restore
```
 
> **Si tu máquina solo tiene configurada la fuente "Microsoft Visual Studio Offline Packages"** (típico en instalaciones nuevas de Visual Studio), el restore va a fallar con errores `NU1101`. Agrega la fuente oficial de NuGet:
> ```bash
> dotnet nuget add source https://api.nuget.org/v3/index.json -n "nuget.org"
> ```
 
### 2. Crear la base de datos
 
**Opción A — EF Core migrations (recomendada):**
```bash
dotnet tool install --global dotnet-ef   # si no lo tienes
dotnet ef migrations add InitialCreate
dotnet ef database update
```
 
**Opción B — script SQL manual:** ejecuta `backend/ZonaVirtual.Api/Database/schema.sql` en SQL Server Management Studio / Azure Data Studio.
 
### 3. Configurar la cadena de conexión
 
En `appsettings.json`, ajusta `ConnectionStrings:DefaultConnection` según tu instalación de SQL Server:
 
- **Con Windows Authentication** (recomendado para desarrollo local):
```json
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=ZonaVirtualDB;Trusted_Connection=True;TrustServerCertificate=True;"
```
En caso de utilizar una instancia diferente de SQL Server, ajustar el valor de Server según la configuración local.

- **Con usuario/clave de SQL Server** (requiere modo de autenticación mixto habilitado):
```json
  "DefaultConnection": "Server=localhost;Database=ZonaVirtualDB;User Id=zv_user;Password=CambiaEstaClave123!;TrustServerCertificate=True;"
```
 
### 4. Ejecutar la API
 
```bash
dotnet run
```
 
o, para que recompile automáticamente con cada cambio durante el desarrollo:
 
```bash
dotnet watch run
```
 
Con el `Properties/launchSettings.json` incluido, la API queda disponible en `https://localhost:7099` (HTTPS) y `http://localhost:5000` (HTTP), en modo `Development`, y abre Swagger automáticamente en `https://localhost:7099/swagger`.
 
### 5. Generar datos de prueba (Punto 1)
 
Desde Swagger (o Postman):
 
```
POST /api/Prueba/GenerarDatos
{ "cantidadComercios": 5, "cantidadUsuarios": 10, "cantidadTransacciones": 25 }
```
 
---
 
## Cómo levantar el frontend
 
```bash
cd frontend/zona-virtual-web
npm install
ng serve
```
 
Abre `http://localhost:4200`.
 
Antes de esto, confirma que `src/environments/environment.ts` apunte al puerto real donde quedó corriendo tu API:
 
```ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7099/api'
};
```
 
---
 
## Flujo funcional de la aplicación
 
1. **Pantalla de selección de perfil** — el usuario elige si es "Pagador" o "Comercio". Sin este paso no se puede avanzar (Punto 2).
2. **Login** — pide correo y contraseña. Si el usuario no tiene cuenta, un enlace lo lleva al registro.
3. **Registro** — pide identificación (o NIT, si es comercio), nombre, correo y contraseña. Si esa identificación/NIT ya existe en la base (por ejemplo, generada por el Punto 1), la cuenta queda vinculada a ese registro; si no existe, se crea desde cero. Al terminar, vuelve a la pantalla de login para que el usuario ingrese con las credenciales recién creadas — tal como lo pide el enunciado.
4. **Dashboard de pagador** — lista los pagos realizados y permite registrar uno nuevo a cualquier comercio existente, validando que el `Trans_codigo` no se repita.
5. **Dashboard de comercio** — lista los pagos recibidos, con filtros por fecha, código de pago y nombre del cliente; muestra el total de las transacciones filtradas; permite editar una transacción solo si no está en estado "Aprobada".
---
 
## Mapeo de requisitos de la prueba
 
| Punto | Requisito | Dónde está implementado |
|---|---|---|
| 1 | Generar y guardar datos de prueba (comercios, usuarios, transacciones), sin repetir `Trans_codigo` | `POST /api/Prueba/GenerarDatos` → `DatosPruebaService.GenerarAsync` |
| 2 | Selección de perfil, registro de usuario/contraseña si no existe, login | Pantallas `seleccion` → `registrar` → `login` en `LoginComponent`; `POST /api/Auth/registro`, `POST /api/Auth/login` |
| 3 | Pagador ve sus pagos y puede pagar a cualquier comercio, sin repetir `Trans_codigo` | `PagadorDashboardComponent`; `GET/POST /api/Transacciones/pagador` |
| 4 | Comercio ve pagos recibidos, filtra por fecha/código/usuario, ve el total | `ComercioDashboardComponent`; `GET /api/Transacciones/comercio` |
| 5 | Guardar transacción | `POST /api/Transacciones/pagador` |
| 6 | Modificar transacción solo si no está aprobada | `PUT /api/Transacciones/comercio/{id}`; validación en `TransaccionService.ActualizarAsync` |
 
Las tablas de `Trans_medio_pago` (Tarjeta de Crédito / PSE / Gana / Caja) y `Trans_estado` (Aprobada / Rechazada / Pendiente / Rechazada SR) se traducen en toda la interfaz — nunca se muestran los códigos numéricos crudos al usuario.
 
---
 
## Solución de problemas comunes
 
Estos son los tropiezos más frecuentes al levantar el proyecto por primera vez en Windows, y cómo se resuelven:
 
**`error NU1101: No se encuentra el paquete...`**
Tu única fuente de NuGet es la caché offline de Visual Studio. Agrega nuget.org:
```bash
dotnet nuget add source https://api.nuget.org/v3/index.json -n "nuget.org"
```
 
**`CultureNotFoundException: Only the invariant culture is supported`**
El `.csproj` no debe tener `<InvariantGlobalization>true</InvariantGlobalization>` — ese modo rompe la conexión de Entity Framework a SQL Server. Verifica que no esté presente (o esté en `false`) en `ZonaVirtual.Api.csproj`.
 
**La API levanta en `http://localhost:5000` y en modo `Production` (no aparece Swagger)**
Falta o está mal ubicado `Properties/launchSettings.json`. Debe estar en `backend/ZonaVirtual.Api/Properties/launchSettings.json`, con `ASPNETCORE_ENVIRONMENT=Development` y `applicationUrl` incluyendo el puerto HTTPS.
 
**`Login failed for user 'zv_user'` (SQL error 18456)**
Tu instancia de SQL Server no tiene habilitada la autenticación mixta, o el login no se creó. La forma más simple para desarrollo local es usar `Trusted_Connection=True` (Windows Authentication) en la cadena de conexión, en vez del usuario `zv_user`.
 
**`ng: no se reconoce como un comando`**
Angular CLI no quedó en el PATH. Instálalo global (`npm install -g @angular/cli`, abriendo una terminal nueva después) o usa `npx ng serve` en su lugar, que no depende del PATH.
 
**Cambié un archivo `.cs` y el error sigue igual**
A diferencia de Angular, .NET **no recompila solo** con `dotnet run`. Hay que detener el proceso (`Ctrl+C`) y volver a correrlo, o usar `dotnet watch run` desde el principio para que recompile automáticamente.
 
---
 
## Seguridad
 
- Las contraseñas se almacenan con **PBKDF2 + salt** (100.000 iteraciones, SHA-256), implementado sin dependencias externas.
- La API usa **JWT Bearer** con claims de perfil (`Pagador`/`Comercio`) y de referencia (Id del pagador o del comercio autenticado). Los endpoints de transacciones están protegidos con `[Authorize(Roles = "Pagador")]` / `[Authorize(Roles = "Comercio")]`, así que un comercio nunca puede ver o modificar transacciones de otro comercio, ni un pagador ver pagos de otro pagador.
- CORS está habilitado únicamente para `http://localhost:4200` (configurable en `appsettings.json` → `Cors:AllowedOrigins`).
---
 
**Autor:** Adriana Vásquez — desarrollado como prueba técnica para Zona Virtual S.A.
