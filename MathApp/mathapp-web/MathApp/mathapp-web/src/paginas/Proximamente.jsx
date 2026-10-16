import EncabezadoPagina from "../componentes/EncabezadoPagina";

/** Página temporal para los módulos que todavía no se construyen. */
export default function Proximamente({ titulo, descripcion }) {
  return (
    <>
      <EncabezadoPagina titulo={titulo} descripcion={descripcion} />
      <div className="tarjeta">
        <p className="texto-suave">Este módulo se construirá en el siguiente paso.</p>
      </div>
    </>
  );
}
