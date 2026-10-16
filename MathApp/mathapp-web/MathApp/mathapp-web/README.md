# mathapp-web — frontend en React

React + Vite, con inicio de sesión de Firebase (Google y correo/contraseña).

## Ponerlo a funcionar

1. Instala las dependencias (necesitas Node.js 20 o superior):
   ```bash
   npm install
   ```
2. Copia `.env.example` como `.env.local` y llena los datos de Firebase:
   Consola de Firebase → ⚙ Configuración del proyecto → *Tus apps* → tu app web →
   `firebaseConfig`. Solo hacen falta `apiKey` y `appId`; el resto ya está puesto.
3. Pon en `VITE_API_URL` la dirección del backend (la que muestra Visual Studio al
   ejecutar la API, p. ej. `http://localhost:5000`).
4. Arranca:
   ```bash
   npm run dev
   ```
   y abre http://localhost:5173. El backend ya permite este origen (CORS) en desarrollo.

## Revisar en Firebase

- **Authentication → Sign-in method**: activa *Google* y *Correo electrónico/contraseña*.
- **Authentication → Settings → Authorized domains**: `localhost` ya viene. Cuando
  publiques el frontend agrega su dominio; si no, el botón de Google dará error.

## Publicar en Railway

El frontend se publica como un **segundo servicio** del mismo proyecto de Railway, desde
el mismo repositorio de GitHub. El `Dockerfile` compila la app y la sirve con Caddy.

1. En el proyecto: **+ New → GitHub Repo** y elige el mismo repositorio.
2. En el servicio nuevo, pestaña **Settings**:
   - **Root Directory**: `/mathapp-web`
   - **Config-as-code → Railway Config File**: `/mathapp-web/railway.json`
     (Railway no busca este archivo dentro de Root Directory; sin esto tomaría el
     `railway.json` del backend).
   - **Networking → Generate Domain**.
3. Pestaña **Variables**:

   | Variable | Valor |
   |---|---|
   | `VITE_FIREBASE_API_KEY` | el `apiKey` de Firebase |
   | `VITE_FIREBASE_APP_ID` | el `appId` de Firebase |
   | `VITE_FIREBASE_AUTH_DOMAIN` | `tesis-mate-app.firebaseapp.com` |
   | `VITE_FIREBASE_PROJECT_ID` | `tesis-mate-app` |
   | `VITE_API_URL` | `https://${{NOMBRE_DEL_BACKEND.RAILWAY_PUBLIC_DOMAIN}}` |

   Cambia `NOMBRE_DEL_BACKEND` por el nombre de tu servicio de la API en Railway.
   Estas variables se usan **al compilar**: si cambias alguna, Railway vuelve a
   compilar y publicar.
4. En el servicio del **backend**, agrega el dominio del frontend a `Cors__Origenes`:
   `https://<dominio-del-frontend>` (sin `/` al final). Para seguir probando en tu
   computadora: `https://<dominio-del-frontend>,http://localhost:5173`.
5. En **Firebase → Authentication → Settings → Authorized domains**, agrega el
   dominio del frontend (sin `https://`).

## Estructura

```
src/
├── firebase.js            Inicializa Firebase con las variables de .env.local
├── api/cliente.js         api("/ruta"): llama al backend con el token de Firebase
├── auth/
│   ├── AuthContext.jsx    Sesión: entrar, registrarse, recuperar contraseña, salir
│   ├── RutaProtegida.jsx  Manda a /entrar a quien no tiene sesión
│   └── erroresFirebase.js Mensajes de error en español
├── componentes/           Barra superior (Layout), pantallas de carga y error
├── paginas/
│   ├── Entrar.jsx         Inicio de sesión / crear cuenta / recuperar contraseña
│   └── Proximamente.jsx   Lugar temporal de los módulos que faltan
└── estilos/               Colores y componentes tomados de los prototipos
```

## Cómo usar la sesión en otras páginas

```jsx
import { useAuth } from "../auth/AuthContext";
import { api } from "../api/cliente";

const { usuario } = useAuth();                 // fila de la tabla Usuarios
const temas = await api("/api/planes/1/temas");
await api("/api/lecciones/3/completar", { metodo: "POST" });
```

`api()` agrega el token automáticamente y lanza un `ErrorApi` con un mensaje en
español si algo falla (`error.message`, `error.estado`).

## Nota sobre el botón de Google

El botón usa un ícono genérico. Si prefieres el botón oficial con el logotipo de
Google, descárgalo de la guía de marca de Google Identity ("Sign in with Google
branding guidelines") y reemplaza el `<svg>` en `Entrar.jsx`.
