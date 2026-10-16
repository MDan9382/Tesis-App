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
