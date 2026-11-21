# Requests REST de ITHUB_SampleAPI

Cada archivo prueba un endpoint. Actualiza las variables editables (`email`, `password`, IDs) antes de ejecutar. Los requests protegidos realizan un login dentro del mismo archivo y reutilizan `{{login.response.body.$.accessToken}}`; no pegues JWTs manualmente.

Abre la carpeta `tools/ITHUB_SampleAPI` en VS Code y ejecuta los archivos con REST Client o la extensión REST Book. La API local usa `http://localhost:5110`.

El administrador inicial se configura en `SeedData:Administrator`; el usuario de demo en `SeedData:User`. Cambia las variables de password de los archivos para que coincidan con las contraseñas locales que configuraste. Las variables de IDs son valores editables: consulta primero las respuestas de los endpoints correspondientes.

| Carpeta | Archivos |
| --- | --- |
| `Auth/` | login |
| `Users/` | consultar el usuario actual y crear usuario (requiere Admin) |
| `Tasks/` | listar, consultar, crear tarea, agregar actividad y cambiar estado |
| `Statuses/` | consultar catálogo de estados |
| `Health/` | liveness y readiness |
| `postman/` | colección v2.1 y environment local importables en Postman |

## Postman

Importa `postman/ITHUB_SampleAPI.postman_collection.json` y `postman/ITHUB_SampleAPI.postman_environment.json`; selecciona el environment local. Completa `email`, `password` y `adminPassword`, y ejecuta los requests `Login` y `Login as administrator` antes de los requests protegidos. La colección conserva tokens de usuario y administrador por separado. Los IDs de tarea, actividad, usuario asignado y estado son variables editables en el environment.
