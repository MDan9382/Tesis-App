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
├── Infraestructura/   Manejo global de errores y health check de MySQL
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

## Uso en un controlador

```csharp
[ApiController]
[Route("api/lecciones")]
public class LeccionesController(ILeccionDao lecciones) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Leccion>> Obtener(int id, CancellationToken ct) =>
        await lecciones.ObtenerPorIdAsync(id, ct) is { } l ? l : NotFound();

    [HttpGet("buscar")]
    public Task<List<LeccionResumen>> Buscar(string q, CancellationToken ct) =>
        lecciones.BuscarPorNombreAsync(q, ct: ct);
}
```

## Notas

- Todas las consultas usan parámetros (`@nombre`): protegidas contra inyección SQL.
- Los errores de MySQL se traducen solos a respuestas JSON: borrar algo con registros
  dependientes → 409, registro duplicado → 409, referencia a un Id inexistente → 400,
  cualquier otro → 500 (registrado en el log, sin mostrar detalles internos al cliente).
- `UP_Activo` en NULL se interpreta como `false` (inactivo).
- Temas y lecciones se devuelven en el orden de la ruta (`Orden`, y el Id como desempate).
- Las pruebas (`PruebaDao`) y sus resultados (`ResultadoPruebaDao`) funcionan igual que
  los ejercicios: cada intento es una fila nueva en `Resultados_Pruebas`.
