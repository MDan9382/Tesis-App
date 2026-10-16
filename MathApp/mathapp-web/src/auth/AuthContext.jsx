import { createContext, useCallback, useContext, useEffect, useRef, useState } from "react";
import {
  GoogleAuthProvider,
  createUserWithEmailAndPassword,
  onAuthStateChanged,
  sendPasswordResetEmail,
  signInWithEmailAndPassword,
  signInWithPopup,
  signInWithRedirect,
  signOut,
  updateProfile,
} from "firebase/auth";
import { auth } from "../firebase";
import { api } from "../api/cliente";

const AuthContext = createContext(null);

/**
 * Estado de la sesión:
 *  - cuentaFirebase: undefined mientras Firebase revisa si hay sesión guardada,
 *    null si no hay sesión, o el usuario de Firebase.
 *  - usuario: la fila de la tabla Usuarios que devuelve GET /api/yo.
 */
export function AuthProvider({ children }) {
  const [cuentaFirebase, setCuentaFirebase] = useState(undefined);
  const [usuario, setUsuario] = useState(null);
  const [errorUsuario, setErrorUsuario] = useState(null);
  const registrando = useRef(false); // durante el registro, el usuario lo carga registrarConCorreo

  const cargarUsuario = useCallback(async () => {
    setErrorUsuario(null);
    try {
      // La primera vez que alguien entra, esta llamada lo registra en la base de datos.
      setUsuario(await api("/api/yo"));
    } catch (e) {
      setErrorUsuario(e);
    }
  }, []);

  useEffect(() => {
    return onAuthStateChanged(auth, async (cuenta) => {
      setUsuario(null);
      setCuentaFirebase(cuenta);
      if (cuenta && !registrando.current) await cargarUsuario();
    });
  }, [cargarUsuario]);

  const entrarConGoogle = useCallback(async () => {
    const proveedor = new GoogleAuthProvider();
    proveedor.setCustomParameters({ prompt: "select_account" });
    try {
      await signInWithPopup(auth, proveedor);
    } catch (e) {
      // Algunos navegadores de celular bloquean las ventanas emergentes.
      if (e.code === "auth/popup-blocked" || e.code === "auth/operation-not-supported-in-this-environment") {
        await signInWithRedirect(auth, proveedor);
        return;
      }
      throw e;
    }
  }, []);

  const entrarConCorreo = useCallback(
    (correo, clave) => signInWithEmailAndPassword(auth, correo.trim(), clave),
    [],
  );

  const registrarConCorreo = useCallback(async (nombre, correo, clave) => {
    registrando.current = true;
    try {
      const { user } = await createUserWithEmailAndPassword(auth, correo.trim(), clave);
      await updateProfile(user, { displayName: nombre });
      // PUT /api/yo/nombre registra al usuario en la base de datos (si aún no existe)
      // y guarda su nombre en una sola llamada.
      try {
        setUsuario(await api("/api/yo/nombre", { metodo: "PUT", cuerpo: { nombre } }));
      } catch (e) {
        setErrorUsuario(e);
      }
    } finally {
      registrando.current = false;
    }
  }, []);

  const recuperarClave = useCallback((correo) => sendPasswordResetEmail(auth, correo.trim()), []);

  const cerrarSesion = useCallback(() => signOut(auth), []);

  const valor = {
    cuentaFirebase,
    usuario,
    errorUsuario,
    cargando: cuentaFirebase === undefined || (cuentaFirebase !== null && !usuario && !errorUsuario),
    reintentar: cargarUsuario,
    actualizarUsuario: setUsuario,
    entrarConGoogle,
    entrarConCorreo,
    registrarConCorreo,
    recuperarClave,
    cerrarSesion,
  };

  return <AuthContext.Provider value={valor}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth debe usarse dentro de <AuthProvider>.");
  return ctx;
}
