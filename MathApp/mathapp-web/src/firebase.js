import { initializeApp } from "firebase/app";
import { getAuth } from "firebase/auth";

const config = {
  apiKey: import.meta.env.VITE_FIREBASE_API_KEY,
  authDomain: import.meta.env.VITE_FIREBASE_AUTH_DOMAIN,
  projectId: import.meta.env.VITE_FIREBASE_PROJECT_ID,
  appId: import.meta.env.VITE_FIREBASE_APP_ID,
};

export const firebaseConfigurado = Boolean(config.apiKey && config.appId);
if (!firebaseConfigurado) {
  console.warn("Falta la configuración de Firebase: copia .env.example como .env.local y llénalo.");
}

const app = initializeApp(config);

export const auth = getAuth(app);
auth.languageCode = "es"; // correos de Firebase (recuperar contraseña) en español
