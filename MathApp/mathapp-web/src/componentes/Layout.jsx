import { useEffect, useRef, useState } from "react";
import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

const MODULOS = [
  { ruta: "/lecciones", texto: "Lecciones" },
  { ruta: "/practicas", texto: "Prácticas" },
  { ruta: "/plan", texto: "Plan de estudio" },
  { ruta: "/progreso", texto: "Progreso" },
];

/** Barra superior y contenedor de todas las páginas con sesión iniciada. */
export default function Layout() {
  const { usuario, cuentaFirebase, cerrarSesion } = useAuth();
  const [menuAbierto, setMenuAbierto] = useState(false);
  const menuRef = useRef(null);

  const nombre = usuario?.nombre || cuentaFirebase?.displayName || cuentaFirebase?.email || "";
  const inicial = (nombre.trim()[0] ?? "?").toUpperCase();

  // Cerrar el menú al hacer clic fuera o con Escape.
  useEffect(() => {
    if (!menuAbierto) return;
    const alClic = (e) => menuRef.current && !menuRef.current.contains(e.target) && setMenuAbierto(false);
    const alTecla = (e) => e.key === "Escape" && setMenuAbierto(false);
    document.addEventListener("mousedown", alClic);
    document.addEventListener("keydown", alTecla);
    return () => {
      document.removeEventListener("mousedown", alClic);
      document.removeEventListener("keydown", alTecla);
    };
  }, [menuAbierto]);

  return (
    <>
      <nav className="nav">
        <NavLink to="/" className="nav-logo">MatemáticaApp</NavLink>
        <div className="nav-modulos">
          {MODULOS.map((m) => (
            <NavLink key={m.ruta} to={m.ruta} className={({ isActive }) => `nav-btn ${isActive ? "activo" : ""}`}>
              {m.texto}
            </NavLink>
          ))}
        </div>
        <div className="nav-cuenta" ref={menuRef}>
          <button
            className="nav-avatar"
            onClick={() => setMenuAbierto((a) => !a)}
            aria-haspopup="menu"
            aria-expanded={menuAbierto}
            aria-label="Menú de la cuenta"
          >
            {inicial}
          </button>
          {menuAbierto && (
            <div className="menu-cuenta" role="menu">
              <div className="menu-cuenta-info">
                <p className="menu-cuenta-nombre">{usuario?.nombre || "Estudiante"}</p>
                <p className="menu-cuenta-correo">{cuentaFirebase?.email}</p>
              </div>
              <button role="menuitem" className="menu-cuenta-opcion" onClick={cerrarSesion}>
                Cerrar sesión
              </button>
            </div>
          )}
        </div>
      </nav>
      <main className="contenido">
        <Outlet />
      </main>
    </>
  );
}
