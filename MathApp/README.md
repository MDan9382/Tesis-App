# MathApp.Api — capa de acceso a datos

Proyecto ASP.NET Core Web API (.NET 8) con MySQL a través de **MySqlConnector**.

```
MathApp.Api/
├── Models/            Una clase por tabla (una instancia = una fila)
├── Data/
│   ├── IDbConnectionFactory.cs / MySqlConnectionFactory.cs   Abre conexiones (con pool)
│   ├── DaoBase.cs                Consultas parametrizadas reutilizables
│   ├── DataReaderExtensions.cs   Lectura de columnas y NULL
│   ├── DataAccessServiceCollectionExtensions.cs   AddAccesoDatos(...) para DI
│   └── Daos/          Un DAO (interfaz + implementación) por tabla
├── Controllers/       Endpoints REST (ver tabla abajo)
├── Dtos/              Cuerpos de petición y respuestas compuestas
├── Servicios/         UsuarioActual: el usuario del token de Firebase
├── Infraestructura/   Autenticación Firebase, manejo de errores, health check
└── Program.cs         DAOs, CORS, /health, puerto de Railway, cabeceras del proxy
Dockerfile, railway.json   Despliegue en Railway
```

## Configurar la conexión

En desarrollo, sin subir la contraseña a GitHub:

```bash
cd MathApp.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:MathApp" "Server=localhost;Database=tesis;User ID=root;Password=...;CharSet=utf8mb4;"
```

Para que el frontend local pueda llamar a la API, `appsettings.Development.json` ya permite
`http://localhost:5173` y `http://localhost:3000` (CORS).

## Desplegar en Railway

1. Sube la carpeta `MathApp/` (la que tiene el `Dockerfile`) a un repositorio de GitHub.
2. En Railway: **New Project → Deploy from GitHub repo** y elige el repositorio.
   Railway detecta el `Dockerfile` y `railway.json` automáticamente.
3. En el mismo proyecto: **+ New → Database → MySQL**.
4. Crea las tablas: en el servicio MySQL abre la pestaña **Connect**, copia los datos de
   conexión pública y conéctate con MySQL Workbench. Ejecuta en este orden:
   `Db_Tesis_R.sql`, `sugerencias_bd.sql` y `cambios_pruebas_y_orden.sql`.
5. En el servicio de la API, pestaña **Variables**, agrega:

   | Variable | Valor |
   |---|---|
   | `ConnectionStrings__MathApp` | `Server=${{MySQL.MYSQLHOST}};Port=${{MySQL.MYSQLPORT}};Database=${{MySQL.MYSQLDATABASE}};User ID=${{MySQL.MYSQLUSER}};Password=${{MySQL.MYSQLPASSWORD}};CharSet=utf8mb4;` |
   | `Cors__Origenes` | URL del frontend, p. ej. `https://mathapp-front.up.railway.app` |

   `${{MySQL.…}}` son variables de referencia: Railway las sustituye por los datos reales
   de tu servicio MySQL (si lo renombraste, cambia `MySQL` por ese nombre) y usa la red
   privada, sin exponer la base de datos.
6. **Settings → Networking → Generate Domain** para obtener la URL pública (con HTTPS).
7. Abre `https://<tu-dominio>/health`: debe responder `Healthy`. Si responde `Unhealthy`,
   la API no puede conectarse a MySQL (revisa la variable del paso 5).

Cada `git push` a la rama principal vuelve a desplegar. Railway espera a que `/health`
responda bien antes de cambiar el tráfico a la versión nueva, así un despliegue roto no
tumba el que ya funcionaba. Los logs de la API se ven en la pestaña **Deployments**.

## Autenticación

El frontend inicia sesión con Firebase y manda el ID token en cada petición:

```js
const token = await auth.currentUser.getIdToken(); // Firebase lo renueva solo
const res = await fetch(`${API}/api/yo`, {
  headers: { Authorization: `Bearer ${token}` },
});
```

La API valida el token contra el proyecto `tesis-mate-app` (configurado en
`appsettings.json` → `Firebase:ProjectId`). Toda la API exige token excepto `/health`.
El usuario se identifica **solo** por el token: los cuerpos de las peticiones nunca
llevan `usuarioId`. La primera llamada a `GET /api/yo` registra al usuario en la tabla
`Usuarios`.

Para probar en Swagger: inicia sesión en el frontend, copia el token
(`await auth.currentUser.getIdToken()` en la consola del navegador) y pégalo en el
botón **Authorize**.

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/health` | Estado de la API y MySQL (sin token) |
| GET | `/api/yo` | Usuario actual (lo registra la primera vez) |
| PUT | `/api/yo/nombre` | Cambiar nombre `{ "nombre": "Ana" }` |
| GET | `/api/yo/perfil` | Planes activos, lecciones completadas y resultados |
| GET | `/api/yo/planes?soloActivos=true` | Planes del usuario |
| POST / DELETE | `/api/yo/planes/{planId}` | Activar / desactivar un plan |
| GET | `/api/planes` | Planes de estudio |
| GET | `/api/planes/{id}` | Un plan |
| GET | `/api/planes/{id}/temas` | Temas del plan en orden de ruta |
| GET | `/api/temas/{id}` | Un tema |
| GET | `/api/temas/{id}/lecciones` | Ruta del tema: lecciones en orden con `completada` |
| GET | `/api/temas/{id}/pruebas?esPrueba=` | Pruebas del tema |
| GET | `/api/lecciones/buscar?q=texto` | Búsqueda por nombre (mín. 2 letras) |
| GET | `/api/lecciones/{id}` | Lección con contenido |
| GET | `/api/lecciones/{id}/ejercicios` | Ejercicios de la lección |
| POST | `/api/lecciones/{id}/completar` | Marcar lección como completada |
| GET | `/api/ejercicios/{id}` | Un ejercicio |
| POST | `/api/ejercicios/{id}/resultados` | Guardar intento `{ "puntaje": 80 }` (0–100) |
| GET | `/api/ejercicios/{id}/resultados` | Mis intentos y mejor puntaje |
| GET | `/api/pruebas/{id}` | Una prueba |
| POST | `/api/pruebas/{id}/resultados` | Guardar intento `{ "puntaje": 80 }` (0–100) |
| GET | `/api/pruebas/{id}/resultados` | Mis intentos y mejor puntaje |

Respuestas de error: 401 sin token o token inválido, 404 si el recurso no existe,
400 si los datos no son válidos (detalle en formato ProblemDetails).

## Notas

- Todas las consultas usan parámetros (`@nombre`): protegidas contra inyección SQL.
- Los errores de MySQL se traducen solos a respuestas JSON: borrar algo con registros
  dependientes → 409, registro duplicado → 409, referencia a un Id inexistente → 400,
  cualquier otro → 500 (registrado en el log, sin mostrar detalles internos al cliente).
- `UP_Activo` en NULL se interpreta como `false` (inactivo).
- Temas y lecciones se devuelven en el orden de la ruta (`Orden`, y el Id como desempate).
- Las pruebas (`PruebaDao`) y sus resultados (`ResultadoPruebaDao`) funcionan igual que
  los ejercicios: cada intento es una fila nueva en `Resultados_Pruebas`.
