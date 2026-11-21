# ITHUB_SampleAPI

API de demostración en .NET 10, con JWT propio, usuarios locales, EF Core database-first y una base SQL Server administrada desde un proyecto SQL versionado.

## Soluciones y Visual Studio

- Abrir `ITHUB_SampleAPI.slnx` para trabajar con la API y sus pruebas en Visual Studio Community 2026.
- Abrir `database/ITHUB.SampleAPI.Database.slnx` por separado en Visual Studio Community 2022 con el componente **SQL Server Data Tools, SDK-style (Preview)** para editar el proyecto SQL desde Visual Studio. Esa solución solo contiene el proyecto de base de datos.
- El proyecto SQL se compila desde la línea de comandos con el .NET SDK independientemente de Visual Studio: `dotnet build .\database\ITHUB.SampleAPI.Database.sqlproj --configuration Release`.

## Publicar en Plesk para Windows

La API apunta a .NET 10. Plesk para Windows requiere Obsidian 18.0.75 o posterior y el componente **Microsoft ASP.NET Core hosting features** instalado por el administrador del servidor. Para publicar con el perfil Web Deploy, el servidor también debe tener Microsoft Web Deploy instalado y habilitado para el dominio.

1. En Plesk, habilitar Web Deploy en la configuración de hosting del dominio y descargar **Web Deploy Publishing Settings**.
2. En Visual Studio, abrir `src/ITHUB.SampleAPI.Api/ITHUB.SampleAPI.Api.csproj`, seleccionar **Publish > Import Profile** e importar el archivo descargado.
3. Preparar una configuración local de producción a partir de los User Secrets que ya usa tu API local:

   ```powershell
   .\Prepare-PleskPublish.ps1
   ```

   El comando crea `appsettings.Production.json` local, valida que la conexión apunte a la base de `.config/db-info.md` y deja desactivado el seed de cuentas porque Admin y User ya existen en la base remota. El archivo está excluido de Git y contiene credenciales de producción: no lo compartas ni lo subas al repositorio.

4. Publicar con el perfil `sample-api.ithub-webhost.net - Web Deploy.pubxml`. El perfil establece el entorno `Production`; ASP.NET Core carga `appsettings.Production.json` automáticamente. El `appsettings.json` local no se copia al paquete publicado y los User Secrets no se incluyen directamente: el comando prepara el archivo de producción que sí necesita el despliegue.
5. Probar `/api/health/live` y `/api/health/ready` en el dominio publicado.

El perfil tiene temporalmente `AllowUntrustedCertificate=true` porque el endpoint Web Deploy de Mochahost presenta un certificado que Visual Studio no valida. Cuando el proveedor instale un certificado confiable para ese endpoint, quitar esta opción. En una captura previa Plesk indicó que `sample-api.ithub-webhost.net` no resolvía; el DNS del dominio debe apuntar al servidor para que la URL pública responda.

## Requisitos

- .NET 10 SDK.
- SQL Server local accesible como `localhost` y permisos para crear la base `ITHUB_SampleAPI`.
- Acceso a NuGet durante la primera restauración de dependencias y SqlPackage.

Para usar los comandos de base de datos en un clon nuevo, copia `.config/db-info.example.md` a `.config/db-info.md` y coloca el nombre real de la base de destino en el archivo local. `db-info.md`, las credenciales y el perfil de publicación de Plesk se excluyen de Git.

## Crear o actualizar la base

Desde esta carpeta, ejecutar:

```powershell
.\Update-Database.ps1
```

El script restaura la herramienta local SqlPackage 170.5.96, compila `database/ITHUB.SampleAPI.Database.sqlproj` con Microsoft.Build.Sql 2.3.0 y publica el esquema directamente en `localhost`. Si `ITHUB_SampleAPI` no existe, SqlPackage la crea si la identidad de Windows tiene permiso `CREATE DATABASE`; si ya existe, actualiza su esquema. No es necesario abrir SSMS ni manipular manualmente el `.dacpac`.

### Generar el script SQL inicial para ejecución manual

Si el usuario de conexión no puede publicar el esquema, generar un script completo desde el modelo:

```powershell
.\Export-CreateDatabaseScript.ps1
```

El resultado queda en `database/deployment/create-sampleapi-schema.sql`; incluye tablas, claves, índices y los seis estados iniciales. Como la base de datos ya existe, el script solo crea objetos dentro de ella. Para ejecutarlo en SSMS, abre una ventana de consulta nueva, selecciona `ithubhos_SampleApiDb`, activa **Consulta > Modo SQLCMD** y ejecuta el archivo completo. El script empieza con `SET NOEXEC OFF` para recuperarse de un intento anterior que haya dejado ese modo activo. En Mensajes deben aparecer `Creando Tabla...` y `Actualización completada`. La cuenta que lo ejecute todavía necesita permisos `CREATE`/`ALTER`; el archivo no puede elevar los permisos del login.

Después de ejecutar correctamente ese script, guardar el modelo desplegado para que los siguientes cambios se generen como diferencias incrementales:

```powershell
.\Export-CreateDatabaseScript.ps1 -Mode MarkApplied
```

Cuando cambie el proyecto SQL, generar el siguiente script de actualización con:

```powershell
.\Export-CreateDatabaseScript.ps1 -Mode Update
```

Ejecuta `database/deployment/update-sampleapi-schema.sql` con SQLCMD Mode y, solo tras completarse correctamente, vuelve a ejecutar `-Mode MarkApplied`. El archivo baseline se guarda localmente bajo `.config/` y se ignora en Git; al trabajar desde otra máquina, transfiere su copia vigente de forma segura o crea un baseline equivalente antes de generar deltas.

Para mostrar el destino y las acciones previstas sin conectarse ni modificar nada:

```powershell
.\Update-Database.ps1 -WhatIf
```

La publicación bloquea cambios que puedan perder datos (`BlockOnPossibleDataLoss=True`) y no elimina objetos que no estén definidos en el proyecto (`DropObjectsNotInSource=False`). El reporte de publicación queda en `database/bin/Release/deployment-report.xml`.

### Otro SQL Server o producción

Configurar la variable `SAMPLEAPI_PRODUCTION_CONNECTION_STRING` en el entorno de ejecución o almacén de secretos, y ejecutar explícitamente:

```powershell
.\Update-Database.ps1 -Target Production -ConfirmProduction
```

El destino de base de datos esperado se lee de `.config/db-info.md` y debe coincidir con `Initial Catalog` en la cadena de conexión. La identidad requiere permisos para publicar el esquema (normalmente `db_owner` sobre una base existente y permiso para crear una base nueva). El servidor debe permitir conexiones desde el equipo/agente que ejecuta el comando. El valor de conexión no se imprime ni se guarda en el repositorio. Revisar especialmente el destino de producción antes de ejecutar.

Se puede usar otro servidor local o cambiar el catálogo local con `SAMPLEAPI_LOCAL_CONNECTION_STRING`; si no se define, se usa autenticación integrada contra `localhost` y `ITHUB_SampleAPI`.

## JWT para desarrollo

El secreto de firma no se guarda en `appsettings.json`. Configurarlo una vez con User Secrets; el ejemplo genera una llave aleatoria de 256 bits:

```powershell
$bytes = New-Object byte[] 32
$rng = [Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$key = [Convert]::ToBase64String($bytes)
dotnet user-secrets set --project .\src\ITHUB.SampleAPI.Api\ITHUB.SampleAPI.Api.csproj "Jwt:SigningKey" $key
dotnet user-secrets set --project .\src\ITHUB.SampleAPI.Api\ITHUB.SampleAPI.Api.csproj "SeedData:Administrator:Password" "<password-local-del-admin>"
dotnet user-secrets set --project .\src\ITHUB.SampleAPI.Api\ITHUB.SampleAPI.Api.csproj "SeedData:User:Password" "<password-local-del-user>"
```

El login local se realiza con email y password. Las contraseñas se guardan con `PasswordHasher<TUser>` y la API emite sus propios JWT; no consulta a la API legacy ni a otro servicio.

### Seed del administrador inicial

`SampleApiSeedData.SeedAsync()` se ejecuta automáticamente al iniciar la API. También completa el catálogo compartido de estados. Si aún no hay ningún usuario `Admin`, inserta el administrador definido en `SeedData:Administrator`; inserta el usuario definido en `SeedData:User` solo si su email todavía no existe. Las contraseñas se guardan hasheadas y los roles los fija el seed: `Admin` y `User`, respectivamente.

```powershell
dotnet user-secrets set --project .\src\ITHUB.SampleAPI.Api\ITHUB.SampleAPI.Api.csproj "SeedData:Administrator:Password" "replace-with-a-strong-local-password"
```

Si el email configurado para administrador ya pertenece a una cuenta `User` y todavía no existe ningún administrador, el seed eleva esa cuenta a `Admin` y conserva su hash de contraseña. Si ya existe un usuario Admin, el seed no altera cuentas ni contraseñas. Para el usuario seed, si el email ya existe, no cambia el rol ni la contraseña. El nodo `SeedData:User` de `appsettings.json` contiene los datos editables para el usuario de demostración; cambia su contraseña antes de usarla fuera del entorno local. El endpoint administrativo siempre asigna el rol `User`; el cliente no puede elegir el rol.

Para otros entornos, configurar `SeedData__Administrator__Email`, `SeedData__Administrator__DisplayName`, `SeedData__Administrator__Password`, `SeedData__User__Email`, `SeedData__User__DisplayName`, `SeedData__User__Password` y `Jwt__SigningKey` desde el almacén de secretos del entorno. En el primer arranque sin un usuario Admin, la contraseña del administrador es obligatoria y la API no inicia si falta.

## Demo inicial de tablero

- El usuario inicial del seed y los usuarios creados por un administrador reciben el rol `User`.
- El administrador puede crear usuarios y tareas asignadas a cuentas activas. Las respuestas de creación de usuario/login incluyen el `userId` para usarlo al asignar tareas.
- Un usuario solo consulta las tareas asignadas a su cuenta.
- El usuario asignado y el administrador pueden añadir actividades a una tarea.
- Un usuario puede cambiar el estado de sus propias actividades dentro de una tarea asignada; no puede modificar la tarea ni actividades de otras personas.
- El administrador puede cambiar el estado de cualquier actividad.
- Los estados de tareas y actividades se leen de un catálogo SQL compartido y las respuestas los exponen como objetos con `id`, `scope`, `code` y `name`. Estados iniciales: tareas `Assigned` (1), `InProgress` (2), `Completed` (3); actividades `Pending` (4), `InProgress` (5), `Completed` (6).
- Para cambiar el estado de una actividad, enviar el `id` de estado obtenido desde `GET /api/statuses?scope=Activity` en el campo `statusId`.

Endpoints principales:

| Método | Ruta | Permiso | Acción |
| --- | --- | --- | --- |
| `POST` | `/api/auth/login` | Anónimo | Iniciar sesión y emitir JWT |
| `POST` | `/api/users` | Admin | Crear una cuenta con rol `User` (no devuelve un JWT para esa cuenta) |
| `GET` | `/api/users/me` | Usuario/Admin | Consultar el perfil de la cuenta autenticada |
| `GET` | `/api/tasks` | Usuario/Admin | Listar tareas visibles para el rol |
| `GET` | `/api/tasks/{taskId}` | Usuario/Admin | Ver tarea asignada o, como admin, cualquier tarea |
| `POST` | `/api/tasks` | Admin | Crear y asignar una tarea |
| `POST` | `/api/tasks/{taskId}/activities` | Usuario asignado/Admin | Añadir actividad |
| `PATCH` | `/api/tasks/{taskId}/activities/{activityId}/status` | Autor asignado/Admin | Cambiar estado de actividad |
| `GET` | `/api/statuses?scope=Task` | Usuario/Admin | Consultar estados disponibles para tareas |
| `GET` | `/api/statuses?scope=Activity` | Usuario/Admin | Consultar estados disponibles para actividades |

La base contiene las tablas `Users`, `ApplicationLogs`, `Tasks`, `TaskActivities` y `Statuses` con sus claves foráneas e índices. `Tasks` y `TaskActivities` guardan `StatusId`; el repositorio carga la navegación del catálogo y Mapster la convierte a DTO. Los estados iniciales se versionan mediante el post-deployment SQL.

## Ejecutar la API

Primero crear/actualizar el esquema, luego configurar User Secrets y ejecutar:

```powershell
dotnet run --project .\src\ITHUB.SampleAPI.Api\ITHUB.SampleAPI.Api.csproj
```

- API HTTP: `http://localhost:5110`
- Swagger en Development: `http://localhost:5110/swagger`
- Readiness: `http://localhost:5110/api/health/ready`
- Requests REST organizados por controller y endpoint: carpeta `http/` en la raíz del proyecto.
- Colección Postman lista para importar: [`http/ITHUB_SampleAPI_Postman.zip`](http/ITHUB_SampleAPI_Postman.zip).

El seed crea las cuentas configuradas en `SeedData` al iniciar la API. `POST /api/users`, reservado al administrador, crea un usuario `User` sin devolverle su token al administrador. `POST /api/auth/login` devuelve el JWT de la cuenta; `GET /api/users/me` entrega el perfil asociado a ese token. La consulta también registra un evento mediante el servicio de logs.

## Logging de negocio

Inyectar `ILogService` y llamar:

```csharp
await logService.LogAsync(
    "demo.action.completed",
    "The demo action completed.",
    properties: new { ItemId = 123 },
    cancellationToken: cancellationToken);
```

El servicio registra en `dbo.ApplicationLogs` fecha UTC, nivel, nombre del evento, mensaje, propiedades JSON, trace ID y correo del usuario autenticado cuando estén disponibles.

## Base de datos y modelo EF

Los objetos SQL versionados bajo `database/dbo/` son la definición del esquema. Las descripciones de tareas y actividades son `nvarchar(max)`. Las entidades y configuraciones explícitas EF Core bajo `src/ITHUB.SampleAPI.Domain/` y `src/ITHUB.SampleAPI.Persistence/Configuration/` mapean ese esquema; no se usan migraciones EF. `Update-Database.ps1` migra las antiguas columnas de texto `Status` al catálogo con IDs antes de publicar; aborta si encuentra un estado existente que no reconoce, para no perder esa información. Al añadir o cambiar objetos, actualizar el proyecto SQL y el mapeo EF en el mismo cambio.

## Compilar y probar

```powershell
dotnet build .\ITHUB_SampleAPI.slnx --configuration Release
dotnet test .\ITHUB_SampleAPI.slnx --configuration Release
```
