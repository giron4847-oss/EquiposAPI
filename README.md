# Sistema de Préstamo de Equipos de Laboratorio

Aplicación desarrollada con **.NET 8** para administrar equipos de laboratorio, categorías y préstamos. Se compone de tres soluciones que comparten una sola base de datos SQL Server (`EquiposDb`):

| Proyecto | Descripción | Dirección local |
|---|---|---|
| `EquiposAPI` | Web API REST (EF Core código primero, JWT, Swagger) | https://localhost:7207/swagger |
| `EquiposMVC` | Cliente MVC de operación; consume la API, no accede a la base | https://localhost:7032 |
| `EquiposReportes` | Reportes de solo lectura (base de datos primero), solo rol Admin | https://localhost:7295 |

> Toda escritura pasa por la API. `EquiposReportes` solo lee, con un usuario SQL que tiene únicamente `db_datareader`.

---

## Contenido

- [Tecnologías](#tecnologías)
- [Estructura del repositorio](#estructura-del-repositorio)
- [Funcionalidades](#funcionalidades)
- [Roles y permisos](#roles-y-permisos)
- [Requisitos previos](#requisitos-previos)
- [Configuración e instalación](#configuración-e-instalación)
- [Ejecución](#ejecución)
- [Endpoints principales](#endpoints-principales)
- [Reglas de negocio](#reglas-de-negocio)
- [Pruebas documentadas (12 casos)](#pruebas-documentadas-12-casos)
- [Notas](#notas)

---

## Tecnologías

| Tecnología | Uso |
|---|---|
| C# y .NET 8 | Desarrollo de la aplicación |
| ASP.NET Core Web API | Servicios REST |
| ASP.NET Core MVC | Interfaz web (operación y reportes) |
| Entity Framework Core 8 | Código primero con migraciones (API) y base de datos primero con `Scaffold-DbContext` (Reportes) |
| SQL Server | Base de datos |
| JWT Bearer | Autenticación de la API |
| Swagger / OpenAPI | Documentación y prueba de endpoints |
| AutoMapper | Mapeo entre entidades y DTOs |
| Bootstrap | Estilos de la interfaz |

---

## Estructura del repositorio

```text
EquiposAPI/
├── Controllers/              # Controladores de la API
├── Data/                     # Contexto de Entity Framework
├── Dtos/                     # Objetos de transferencia de datos
├── Mappings/                 # Configuración de AutoMapper
├── Migrations/               # Migraciones de la base de datos
├── Models/                   # Entidades y enumeraciones
├── EquiposMVC/
│   ├── Controllers/          # Un controlador por recurso
│   ├── Infraestructura/      # ApiClient, handler del JWT, filtros y atributos
│   ├── Models/               # Modelos de las vistas
│   ├── Services/             # Un servicio por recurso (equipos, categorías, préstamos, autenticación)
│   └── Views/                # Vistas Razor
├── EquiposReportes/
│   ├── Controllers/          # Login y reportes
│   ├── Data/                 # Contexto y entidades generadas con Scaffold-DbContext
│   ├── Infrastructure/       # Filtro de acceso solo Admin
│   ├── Models/               # ViewModels de los reportes
│   ├── Services/             # Servicio de autenticación contra la API
│   ├── Views/                # Vistas Razor
│   └── crear_usuario_reportes.sql   # Crea el usuario SQL de solo lectura
├── docs/evidencias/          # Capturas de los 12 casos de prueba
├── equipos.sql               # Script SQL generado a partir de las migraciones
├── EquiposAPI.csproj
├── EquiposAPI.slnx
└── README.md
```

---

## Funcionalidades

### API (`EquiposAPI`)
- Inicio de sesión y generación de token JWT (con Id y rol como claims, expira en 2 horas).
- Registro de usuarios, solo para Admin (Username repetido devuelve 409).
- Equipos: listado con filtros (`texto`, `categoriaId`, `estado`), detalle, crear, actualizar y eliminar.
- Categorías: listado, detalle, crear, actualizar y eliminar.
- Préstamos: listado según el rol del token, detalle, registro y devolución.
- Concurrencia optimista con `RowVersion` en los equipos.
- Documentación con Swagger.

### Cliente web (`EquiposMVC`)
- Inicio y cierre de sesión; token, rol y usuario guardados en `Session`.
- El token se envía en cada solicitud mediante un `DelegatingHandler`.
- Menú que depende del rol.
- Equipos con filtros (texto, categoría y estado) y paginación de 10 registros.
- CRUD de equipos y categorías, y gestión de préstamos y devoluciones.
- Manejo de errores HTTP: 401 (limpia sesión y vuelve al login), 403 (vista de acceso denegado), 409 (mensaje de la API dentro del formulario sin perder lo escrito) y 5xx (mensaje genérico).
- La edición de equipos envía el `RowVersion` de vuelta a la API.

### Reportes (`EquiposReportes`)
- Inicio de sesión contra `/api/auth/login`; solo se permite el rol Admin.
- Cuatro reportes con LINQ, `AsNoTracking()` y ViewModels:
  1. Inventario por categoría (cantidad por estado).
  2. Préstamos vencidos (con usuario y días de atraso).
  3. Top 5 de equipos más prestados.
  4. Historial por usuario.
- Filtros por rango de fechas y categoría.

---

## Roles y permisos

Los permisos se validan en la API y también se reflejan en la interfaz MVC.

| Funcionalidad | Admin | Docente | Estudiante |
|---|:---:|:---:|:---:|
| CRUD de categorías y equipos | Sí | No | No |
| Consultar categorías (para filtros) | Sí | Sí | Sí |
| Registrar usuarios | Sí | No | No |
| Registrar préstamos y devoluciones | Sí | Sí | No |
| Ver equipos disponibles | Sí | Sí | Sí |
| Ver préstamos | Todos | Todos | Solo los propios |
| Reportes | Sí | No | No |

---

## Requisitos previos

- .NET SDK 8.0 (`dotnet --version`).
- Visual Studio 2022 con soporte para ASP.NET, o la CLI de .NET.
- SQL Server (por ejemplo SQL Server Express, instancia `.\SQLEXPRESS`) con autenticación en **modo mixto** (necesario para el usuario `reportes`).
- SQL Server Management Studio (SSMS), si se usará el script SQL.
- `dotnet-ef` si se aplican las migraciones desde la terminal.

---

## Configuración e instalación

### 1. Clonar el repositorio

```bash
git clone URL_DEL_REPOSITORIO
cd EquiposAPI
```

### 2. Crear la base de datos

Configurar la cadena de conexión de la API (desde la carpeta de `EquiposAPI.csproj`):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\\SQLEXPRESS;Database=EquiposDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Crear las tablas con una de estas opciones:

- **Opción A (migraciones):** `dotnet ef database update`
- **Opción B (script):** abrir `equipos.sql` en SSMS y ejecutarlo sobre la instancia.

La base se crea con cuatro categorías semilla (Laptops, Proyectores, Kits IoT y Tarjetas Arduino).

### 3. Configurar JWT y el administrador inicial (User Secrets de la API)

```bash
dotnet user-secrets set "Jwt:Key" "REEMPLAZAR_POR_UNA_CLAVE_DE_AL_MENOS_32_CARACTERES"
dotnet user-secrets set "Jwt:Issuer" "EquiposAPI"
dotnet user-secrets set "Jwt:Audience" "EquiposAPIUsuarios"

dotnet user-secrets set "AdminSeed:Username" "admin"
dotnet user-secrets set "AdminSeed:Email" "admin@equipos.local"
dotnet user-secrets set "AdminSeed:Password" "REEMPLAZAR_POR_UNA_CONTRASENA_SEGURA"
```

El administrador se crea automáticamente al iniciar la API solo si la tabla `Usuarios` está vacía. **No se publican contraseñas ni claves en el repositorio.**

### 4. Crear el usuario de solo lectura para reportes

Abrir `EquiposReportes/crear_usuario_reportes.sql` en SSMS, cambiar la contraseña y ejecutarlo. Crea el login y el usuario `reportes` con el rol `db_datareader` únicamente.

Luego, desde la carpeta de `EquiposReportes.csproj`:

```bash
dotnet user-secrets set "ConnectionStrings:ReportesConnection" "Server=.\\SQLEXPRESS;Database=EquiposDb;User Id=reportes;Password=CONTRASENA_DEL_USUARIO_REPORTES;TrustServerCertificate=True;"
```

### 5. Dirección de la API en MVC y Reportes

Ambos proyectos leen la URL de la API de `appsettings.json` (`ApiSettings:BaseUrl`, por defecto `https://localhost:7207/`). Si la API usa otro puerto, actualizar ese valor.

---

## Ejecución

1. Compilar la solución (`Ctrl + Shift + B`).
2. Iniciar **`EquiposAPI`** con *Iniciar sin depurar* y comprobar que Swagger abra.
3. Iniciar **`EquiposMVC`** y/o **`EquiposReportes`** con *Iniciar sin depurar*, con la API en ejecución.
4. Iniciar sesión con el administrador definido en `AdminSeed`.

Si el navegador pide confiar en el certificado de desarrollo: `dotnet dev-certs https --trust`.

Los usuarios Docente y Estudiante se crean con `POST /api/Auth/register` autenticado como Admin.

---

## Endpoints principales

| Método | Endpoint | Descripción | Acceso |
|---|---|---|---|
| POST | `/api/Auth/login` | Iniciar sesión y obtener JWT | Público |
| POST | `/api/Auth/register` | Registrar un usuario | Admin |
| GET | `/api/Categorias` | Listar categorías | Autenticado |
| GET | `/api/Categorias/{id}` | Consultar una categoría | Autenticado |
| POST / PUT / DELETE | `/api/Categorias` | Mantenimiento de categorías | Admin |
| GET | `/api/Equipos?texto=&categoriaId=&estado=` | Listar equipos con filtros | Autenticado |
| GET | `/api/Equipos/{id}` | Detalle de un equipo | Autenticado |
| POST / PUT / DELETE | `/api/Equipos` | Mantenimiento de equipos | Admin |
| POST | `/api/Prestamos` | Registrar un préstamo | Admin / Docente |
| PUT | `/api/Prestamos/{id}/devolucion` | Registrar una devolución | Admin / Docente |
| GET | `/api/Prestamos?soloVencidos=true` | Listar préstamos según el rol del token | Autenticado |

Los endpoints protegidos requieren el encabezado `Authorization: Bearer TOKEN_JWT`.

---

## Reglas de negocio

- Solo se puede prestar un equipo `Disponible`; si no, la API responde **409**.
- Al registrar un préstamo, el equipo pasa a `Prestado` en el mismo `SaveChanges` (transaccional).
- La fecha de vencimiento no puede ser anterior a hoy ni mayor a 7 días.
- Al registrar una devolución se completa `FechaDevolucion` y el equipo vuelve a `Disponible`.
- No se puede eliminar un equipo con historial de préstamos (**409**) ni una categoría con equipos.
- `RowVersion` controla la concurrencia al actualizar equipos (la segunda edición recibe **409**).
- Todas las relaciones usan `DeleteBehavior.Restrict`.

---

## Pruebas documentadas (12 casos)

Las capturas están en `docs/evidencias/`. Los casos 1 al 11 se probaron con Swagger o con el cliente MVC; el caso 12 compara los reportes con consultas en SSMS.

| N° | Caso | Esperado | Obtenido | Evidencia |
|---|---|---|---|---|
| 1 | Login correcto devuelve token con rol | 200 con token y rol | 200, `rol: Admin` | 
| 2 | Login con contraseña incorrecta | 401 | 401 "Usuario o contraseña incorrectos." | 
| 3 | Endpoint protegido sin token | 401 | 401 | 
| 4 | Estudiante intenta crear un equipo | 403 | 403 | 
| 5 | Registrar usuario con Username repetido | 409 | 409 "El nombre de usuario ya existe." | 
| 6 | Prestar un equipo ya Prestado | 409 | 409 "El equipo no está disponible (estado actual: Prestado)." | 
| 7 | FechaVencimiento a 15 días | Rechazada | Mensaje "no puede ser mayor a 7 días" | 
| 8 | Eliminar equipo con historial | 409 | 409 "No se puede eliminar un equipo con historial de préstamos." | 
| 9 | Dos ediciones simultáneas del mismo equipo | La segunda devuelve 409 | Mensaje de conflicto en el formulario, sin perder lo escrito |
| 10 | Estudiante solo ve sus préstamos | Solo los propios | Cada estudiante ve únicamente su préstamo | 
| 11 | Token expirado en MVC redirige al login | Login con aviso de sesión expirada | Aviso "Tu sesión expiró..." | 
| 12 | Los cuatro reportes coinciden con SSMS | Datos coherentes | Inventario, Vencidos, Top 5 e Historial coinciden con las consultas | 

---

## Notas

- Las credenciales y claves se guardan en User Secrets y nunca en el repositorio.
- `EquiposMVC` y `EquiposReportes` no escriben en la base de datos: toda escritura pasa por la API, y Reportes usa un usuario `db_datareader`.
- Las capturas deben corresponder al funcionamiento real de la aplicación y no mostrar contraseñas ni tokens.
