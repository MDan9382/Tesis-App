export default function PantallaError({ mensaje, onReintentar, onCerrarSesion }) {
  return (
    <div className="pantalla-centro">
      <div className="tarjeta tarjeta-estrecha">
        <h1 className="titulo-tarjeta">No pudimos cargar tu cuenta</h1>
        <p className="texto-suave">{mensaje}</p>
        <div className="fila-botones">
          <button className="boton boton-primario" onClick={onReintentar}>Reintentar</button>
          <button className="boton boton-secundario" onClick={onCerrarSesion}>Cerrar sesión</button>
        </div>
      </div>
    </div>
  );
}
