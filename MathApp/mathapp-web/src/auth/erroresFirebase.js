// Traduce los códigos de error de Firebase Authentication a mensajes en español.
const MENSAJES = {
  "auth/invalid-email": "El correo electrónico no es válido.",
  "auth/missing-email": "Escribe tu correo electrónico.",
  "auth/missing-password": "Escribe tu contraseña.",
  "auth/invalid-credential": "Correo o contraseña incorrectos.",
  "auth/wrong-password": "Correo o contraseña incorrectos.",
  "auth/user-not-found": "Correo o contraseña incorrectos.",
  "auth/user-disabled": "Esta cuenta está desactivada.",
  "auth/email-already-in-use": "Ya existe una cuenta con ese correo. Inicia sesión.",
  "auth/weak-password": "La contraseña debe tener al menos 6 caracteres.",
  "auth/password-does-not-meet-requirements": "La contraseña no cumple los requisitos de seguridad.",
  "auth/too-many-requests": "Demasiados intentos. Espera unos minutos e inténtalo de nuevo.",
  "auth/network-request-failed": "Sin conexión. Revisa tu internet e inténtalo de nuevo.",
  "auth/account-exists-with-different-credential":
    "Ya tienes una cuenta con ese correo usando otro método. Entra con ese método.",
  "auth/operation-not-allowed":
    "Este método de inicio de sesión no está activado en Firebase (Authentication → Sign-in method).",
  "auth/unauthorized-domain":
    "Este sitio no está autorizado en Firebase (Authentication → Settings → Authorized domains).",
  "auth/invalid-api-key": "La configuración de Firebase no es válida. Revisa el archivo .env.local.",
  "auth/api-key-not-valid.-please-pass-a-valid-api-key.":
    "La configuración de Firebase no es válida. Revisa el archivo .env.local.",
};

/** Errores que no hay que mostrar (el usuario cerró la ventana de Google, etc.). */
export const ERRORES_SILENCIOSOS = new Set([
  "auth/popup-closed-by-user",
  "auth/cancelled-popup-request",
  "auth/user-cancelled",
]);

export function mensajeDeError(error) {
  if (error?.name === "ErrorApi" || error?.estado !== undefined) return error.message; // errores del backend
  if (MENSAJES[error?.code]) return MENSAJES[error.code];
  console.error(error);
  return "Ocurrió un error. Inténtalo de nuevo.";
}
