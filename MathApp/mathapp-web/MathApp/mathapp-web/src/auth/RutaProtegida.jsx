import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "./AuthContext";
import PantallaCarga from "../componentes/PantallaCarga";
import PantallaError from "../componentes/PantallaError";

/** Solo deja pasar a usuarios con sesión; a los demás los manda a /entrar. */
export default function RutaProtegida() {
  const { cuentaFirebase, usuario, errorUsuario, cargando, reintentar, cerrarSesion } = useAuth();
  const ubicacion = useLocation();

  if (cargando) return <PantallaCarga />;
  if (!cuentaFirebase) return <Navigate to="/entrar" replace state={{ desde: ubicacion.pathname }} />;
  if (errorUsuario || !usuario) {
    return (
      <PantallaError
        mensaje={errorUsuario?.message ?? "No se pudo cargar tu cuenta."}
        onReintentar={reintentar}
        onCerrarSesion={cerrarSesion}
      />
    );
  }
  return <Outlet />;
}
