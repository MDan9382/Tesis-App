export default function PantallaCarga({ texto = "Cargando…" }) {
  return (
    <div className="pantalla-centro" role="status" aria-live="polite">
      <div className="spinner" aria-hidden="true" />
      <p className="pantalla-texto">{texto}</p>
    </div>
  );
}
