# Sistema de Préstamo de Equipos de Laboratorio

Aplicación desarrollada con **.NET 8**, compuesta por una API REST y un cliente web MVC para administrar equipos de laboratorio, categorías y préstamos.

> **Estado del repositorio:** actualmente contiene los proyectos `EquiposAPI` y `EquiposMVC`. 

Para hacer que funcione el programa se le da clic izq. a configuracion de **Proyecto o perfil de Inicio**
Luego se le da al boton **Varios proyectos de inicio**
En proyecto aparecera `EquiposAPI` y `EquiposMVC`en accion seleccione **Iniciar sin depurar**
Luego a aplicar y aceptar, se le da ctrl + shift + B y luego darle al boton Iniciar sin depurar (Deberan aparecer 2 ventanas de `EquiposAPI` y `EquiposMVC`)

---
## Descripción

El sistema permite gestionar el inventario de equipos de laboratorio y controlar el proceso de préstamo y devolución. La API expone los servicios REST y se encarga del acceso a SQL Server. El cliente MVC consume la API mediante solicitudes HTTP; no accede directamente a la base de datos.

## Tecnologías

| Tecnología | Uso |
|---|---|
| C# y .NET 8 | Desarrollo de la aplicación |
| ASP.NET Core Web API | Servicios REST |
| ASP.NET Core MVC | Interfaz web |
| Entity Framework Core | Acceso a datos y migraciones |
| SQL Server | Base de datos |
| JWT Bearer | Autenticación de la API |
| Swagger / OpenAPI | Documentación y prueba de endpoints |
| AutoMapper | Mapeo entre entidades y DTOs |
| Bootstrap | Estilos de la interfaz MVC |

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
│   ├── Controllers/          # Controladores MVC
│   ├── Infraestructura/      # Cliente HTTP, sesión y manejo de JWT
│   ├── Models/               # Modelos de las vistas
│   ├── Services/              # Servicios que consumen la API
│   ├── Views/                 # Vistas Razor
│   └── wwwroot/               # Archivos estáticos
├── equipos.sql                # Script SQL generado para el esquema
├── EquiposAPI.csproj
├── EquiposAPI.slnx
└── README.md
```

## Funcionalidades implementadas

### API (`EquiposAPI`)

- Inicio de sesión y generación de token JWT.
- Registro de usuarios, restringido al rol Administrador.
- Consulta de equipos con filtros por texto, categoría y estado.
- Consulta, creación, actualización y eliminación de equipos.
- Consulta, creación, actualización y eliminación de categorías.
- Consulta de préstamos y consulta de un préstamo por identificador.
- Registro de préstamos y devolución de equipos.
- Documentación de endpoints mediante Swagger en entorno de desarrollo.

### Cliente web (`EquiposMVC`)

- Inicio y cierre de sesión.
- Consumo de la API mediante un cliente HTTP configurado.
- Envío del token JWT en las solicitudes autenticadas.
- Listado de equipos con búsqueda, filtros y paginación.
- Formularios para crear, editar y eliminar equipos.
- Listado y mantenimiento de categorías.
- Consulta de préstamos, filtro de préstamos vencidos, registro de préstamos y devolución.
- Restricción de opciones de la interfaz según el rol del usuario.

---

## Roles y permisos

Los permisos se validan en la API y también se consideran en la interfaz MVC.

| Funcionalidad | Admin | Docente | Estudiante |
|---|:---:|:---:|:---:|
| Consultar equipos | Sí | Sí | Sí, solo disponibles |
| Crear, editar y eliminar equipos | Sí | No | No |
| Consultar categorías | Sí | No | No |
| Crear, editar y eliminar categorías | Sí | No | No |
| Consultar préstamos | Todos | Todos | Solo los propios |
| Registrar préstamos | Sí | Sí | No |
| Registrar devoluciones | Sí | Sí | No |
| Registrar usuarios | Sí | No | No |

---

## Requisitos previos

- Visual Studio 2022 con soporte para ASP.NET y desarrollo web, o .NET SDK 8.
- SQL Server (por ejemplo, SQL Server Express).
- SQL Server Management Studio (SSMS), si se utilizará el script SQL.
- Entity Framework Core CLI (`dotnet-ef`) para aplicar migraciones desde la terminal.

Verifica la instalación del SDK:

```bash
dotnet --version
```

---

## Configuración e instalación

### 1. Clonar el repositorio

```bash
git clone URL_DEL_REPOSITORIO
cd EquiposAPI
```

Reemplaza `URL_DEL_REPOSITORIO` por la dirección HTTPS o SSH de tu repositorio de GitHub.

### 2. Crear la base de datos

La aplicación utiliza SQL Server y la cadena de conexión se configura mediante User Secrets en el proyecto de la API.

Desde la carpeta donde se encuentra `EquiposAPI.csproj`, configura la conexión. Ajusta el servidor y la instancia según tu instalación:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\\SQLEXPRESS;Database=EquiposDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Para crear las tablas, se puede utilizar una de estas opciones:

**Opción A: aplicar las migraciones**

```bash
dotnet ef database update
```

**Opción B: utilizar el script SQL**

Abrir `equipos.sql` desde SQL Server Management Studio y ejecutarlo en la instancia correspondiente.

### 3. Configurar JWT y el usuario administrador inicial

La API necesita una clave JWT y los datos que utilizará para crear el primer usuario administrador cuando la tabla `Usuarios` esté vacía.

Ejecuta estos comandos desde la carpeta del proyecto `EquiposAPI`:

```bash
dotnet user-secrets set "Jwt:Key" "REEMPLAZAR_POR_UNA_CLAVE_SEGURA_DE_AL_MENOS_32_CARACTERES"
dotnet user-secrets set "Jwt:Issuer" "EquiposAPI"
dotnet user-secrets set "Jwt:Audience" "EquiposAPIUsuarios"

dotnet user-secrets set "AdminSeed:Username" "admin"
dotnet user-secrets set "AdminSeed:Email" "admin@equipos.local"
dotnet user-secrets set "AdminSeed:Password" "REEMPLAZAR_POR_UNA_CONTRASENA_SEGURA"
```

Utiliza valores propios para la clave y la contraseña. **No publiques contraseñas, claves JWT ni cadenas de conexión con credenciales en GitHub.**

El usuario administrador se crea automáticamente al iniciar la API únicamente si la tabla `Usuarios` no contiene registros.

### 4. Configurar la dirección de la API en MVC

El cliente MVC obtiene la dirección de la API desde:

`EquiposMVC/appsettings.json`

Configuración actual del proyecto:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7207/"
  }
}
```

Si la API utiliza otro puerto en tu equipo, actualiza `BaseUrl` para que coincida con la dirección HTTPS de la API.

---

## Ejecución

La solución se encuentra en `EquiposAPI.slnx` y contiene dos proyectos.

### Desde Visual Studio

1. Abrir `EquiposAPI.slnx`.
2. Compilar la solución con **Compilar > Compilar solución** o `Ctrl + Shift + B`.
3. Iniciar primero el proyecto `EquiposAPI` con **Iniciar sin depurar**.
4. Comprobar que Swagger se abra correctamente.
5. Mantener la API en ejecución.
6. Iniciar después el proyecto `EquiposMVC` con **Iniciar sin depurar**.
7. Iniciar sesión desde la pantalla del cliente MVC con un usuario registrado.

### Direcciones configuradas en el proyecto

| Proyecto | Dirección local |
|---|---|
| API / Swagger | `https://localhost:7207/swagger` |
| Cliente MVC | `https://localhost:7032` |

Los puertos corresponden a los perfiles HTTPS configurados en `launchSettings.json`; pueden variar si se modifican los perfiles de ejecución.

---

## Endpoints principales

Las rutas están definidas en los controladores de `EquiposAPI`.

| Método | Endpoint | Descripción | Acceso |
|---|---|---|---|
| POST | `/api/Auth/login` | Iniciar sesión y obtener JWT | Público |
| POST | `/api/Auth/register` | Registrar un usuario | Admin |
| GET | `/api/Equipos` | Listar equipos y aplicar filtros | Autenticado |
| GET | `/api/Equipos/{id}` | Consultar un equipo | Autenticado |
| POST | `/api/Equipos` | Crear un equipo | Admin |
| PUT | `/api/Equipos/{id}` | Actualizar un equipo | Admin |
| DELETE | `/api/Equipos/{id}` | Eliminar un equipo | Admin |
| GET | `/api/Categorias` | Listar categorías | Autenticado |
| GET | `/api/Categorias/{id}` | Consultar una categoría | Autenticado |
| POST | `/api/Categorias` | Crear una categoría | Admin |
| PUT | `/api/Categorias/{id}` | Actualizar una categoría | Admin |
| DELETE | `/api/Categorias/{id}` | Eliminar una categoría | Admin |
| GET | `/api/Prestamos` | Listar préstamos | Autenticado |
| GET | `/api/Prestamos/{id}` | Consultar un préstamo | Autenticado |
| POST | `/api/Prestamos` | Registrar un préstamo | Admin / Docente |
| PUT | `/api/Prestamos/{id}/devolucion` | Registrar una devolución | Admin / Docente |

### Filtros disponibles

- Equipos: `texto`, `categoriaId` y `estado`.
- Préstamos: `soloVencidos`.

Ejemplos:

```http
GET /api/Equipos?texto=laptop
GET /api/Equipos?categoriaId=1
GET /api/Equipos?estado=Disponible
GET /api/Prestamos?soloVencidos=true
```

Los endpoints protegidos requieren enviar el token obtenido en el login mediante el encabezado:

```http
Authorization: Bearer TOKEN_JWT
```

---
## Reglas de negocio
De acuerdo con las validaciones implementadas en los controladores:
- Solo se puede prestar un equipo cuyo estado sea `Disponible`.
- Al registrar un préstamo, el equipo cambia a estado `Prestado`.
- La fecha de vencimiento no puede ser anterior al día actual ni superar siete días desde la fecha del préstamo.
- Al registrar una devolución, se completa la fecha de devolución y el equipo vuelve a estado `Disponible`.
- No se permite eliminar una categoría que tenga equipos asociados.
- No se permite eliminar un equipo que tenga historial de préstamos.
- Se utiliza `RowVersion` para el control de concurrencia en la actualización de equipos.

### Estados de los equipos

| Estado | Descripción |
|---|---|
| `Disponible` | El equipo puede ser prestado |
| `Prestado` | El equipo tiene un préstamo activo |
| `Mantenimiento` | El equipo se encuentra en mantenimiento |
---
