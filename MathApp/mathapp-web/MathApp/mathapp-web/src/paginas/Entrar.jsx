import { useState } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { ERRORES_SILENCIOSOS, mensajeDeError } from "../auth/erroresFirebase";
import { firebaseConfigurado } from "../firebase";
import "../estilos/entrar.css";

/** Modos del formulario: "entrar", "registro" o "recuperar". */
export default function Entrar() {
  const { cuentaFirebase, entrarConGoogle, entrarConCorreo, registrarConCorreo, recuperarClave } = useAuth();
  const ubicacion = useLocation();

  const [modo, setModo] = useState("entrar");
  const [nombre, setNombre] = useState("");
  const [correo, setCorreo] = useState("");
  const [clave, setClave] = useState("");
  const [verClave, setVerClave] = useState(false);
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState("");
  const [aviso, setAviso] = useState("");

  // Si ya hay sesión, ir a donde quería entrar (o al inicio).
  if (cuentaFirebase) return <Navigate to={ubicacion.state?.desde ?? "/"} replace />;

  function cambiarModo(nuevo) {
    setModo(nuevo);
    setError("");
    setAviso("");
  }

  async function ejecutar(accion) {
    setError("");
    setAviso("");
    setEnviando(true);
    try {
      await accion();
    } catch (e) {
      if (!ERRORES_SILENCIOSOS.has(e?.code)) setError(mensajeDeError(e));
    } finally {
      setEnviando(false);
    }
  }

  function enviarFormulario(evento) {
    evento.preventDefault();
    if (modo === "entrar") {
      ejecutar(() => entrarConCorreo(correo, clave));
    } else if (modo === "registro") {
      if (nombre.trim().length < 2) return setError("Escribe tu nombre.");
      ejecutar(() => registrarConCorreo(nombre.trim(), correo, clave));
    } else {
      ejecutar(async () => {
        await recuperarClave(correo);
        setAviso("Si existe una cuenta con ese correo, te enviamos un enlace para crear una contraseña nueva. Revisa también la carpeta de spam.");
      });
    }
  }

  const titulos = {
    entrar: "Inicia sesión",
    registro: "Crea tu cuenta",
    recuperar: "Recupera tu contraseña",
  };

  return (
    <div className="entrar">
      <div className="entrar-columna">
        <header className="entrar-marca">
          <img src="/icono.svg" alt="" width="44" height="44" />
          <div>
            <p className="entrar-logo">MatemáticaApp</p>
            <p className="entrar-lema">Aprende y practica matemáticas a tu ritmo.</p>
          </div>
        </header>

        <main className="tarjeta entrar-tarjeta">
          {modo !== "recuperar" && (
            <div className="segmentos" role="tablist" aria-label="Tipo de acceso">
              <button
                role="tab"
                aria-selected={modo === "entrar"}
                className={`segmento ${modo === "entrar" ? "activo" : ""}`}
                onClick={() => cambiarModo("entrar")}
              >
                Iniciar sesión
              </button>
              <button
                role="tab"
                aria-selected={modo === "registro"}
                className={`segmento ${modo === "registro" ? "activo" : ""}`}
                onClick={() => cambiarModo("registro")}
              >
                Crear cuenta
              </button>
            </div>
          )}

          <h1 className="titulo-tarjeta">{titulos[modo]}</h1>

          {!firebaseConfigurado && (
            <p className="mensaje mensaje-error">
              Falta la configuración de Firebase. Copia <code>.env.example</code> como <code>.env.local</code> y llénalo.
            </p>
          )}

          {modo !== "recuperar" && (
            <>
              <button
                type="button"
                className="boton boton-google"
                disabled={enviando}
                onClick={() => ejecutar(entrarConGoogle)}
              >
                <svg width="18" height="18" viewBox="0 0 24 24" aria-hidden="true" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <circle cx="12" cy="8" r="4" />
                  <path d="M4 21c0-4 3.6-6.5 8-6.5s8 2.5 8 6.5" />
                </svg>
                Continuar con Google
              </button>
              <div className="separador"><span>o con tu correo</span></div>
            </>
          )}

          <form onSubmit={enviarFormulario} noValidate>
            {modo === "registro" && (
              <div className="campo">
                <label htmlFor="nombre">Nombre</label>
                <input
                  id="nombre"
                  autoComplete="name"
                  maxLength={100}
                  value={nombre}
                  onChange={(e) => setNombre(e.target.value)}
                  placeholder="¿Cómo te llamas?"
                />
              </div>
            )}

            <div className="campo">
              <label htmlFor="correo">Correo electrónico</label>
              <input
                id="correo"
                type="email"
                inputMode="email"
                autoComplete="email"
                value={correo}
                onChange={(e) => setCorreo(e.target.value)}
                placeholder="tucorreo@ejemplo.com"
              />
            </div>

            {modo !== "recuperar" && (
              <div className="campo">
                <div className="campo-cabecera">
                  <label htmlFor="clave">Contraseña</label>
                  {modo === "entrar" && (
                    <button type="button" className="enlace" onClick={() => cambiarModo("recuperar")}>
                      ¿La olvidaste?
                    </button>
                  )}
                </div>
                <div className="campo-clave">
                  <input
                    id="clave"
                    type={verClave ? "text" : "password"}
                    autoComplete={modo === "registro" ? "new-password" : "current-password"}
                    value={clave}
                    onChange={(e) => setClave(e.target.value)}
                    placeholder={modo === "registro" ? "Mínimo 6 caracteres" : ""}
                  />
                  <button
                    type="button"
                    className="ver-clave"
                    onClick={() => setVerClave((v) => !v)}
                    aria-label={verClave ? "Ocultar contraseña" : "Mostrar contraseña"}
                  >
                    {verClave ? "Ocultar" : "Ver"}
                  </button>
                </div>
              </div>
            )}

            <div aria-live="polite">
              {error && <p className="mensaje mensaje-error">{error}</p>}
              {aviso && <p className="mensaje mensaje-exito">{aviso}</p>}
            </div>

            <button type="submit" className="boton boton-primario boton-ancho" disabled={enviando}>
              {enviando
                ? "Un momento…"
                : modo === "entrar"
                  ? "Entrar"
                  : modo === "registro"
                    ? "Crear cuenta"
                    : "Enviar enlace"}
            </button>
          </form>

          {modo === "recuperar" && (
            <button type="button" className="enlace enlace-volver" onClick={() => cambiarModo("entrar")}>
              ← Volver a iniciar sesión
            </button>
          )}
        </main>
      </div>
    </div>
  );
}
