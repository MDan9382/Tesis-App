export default function EncabezadoPagina({ titulo, descripcion }) {
  return (
    <div className="page-header">
      <h1>{titulo}</h1>
      {descripcion && <p>{descripcion}</p>}
    </div>
  );
}
