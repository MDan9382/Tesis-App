import { Navigate, Route, Routes } from "react-router-dom";
import RutaProtegida from "./auth/RutaProtegida";
import Layout from "./componentes/Layout";
import Entrar from "./paginas/Entrar";
import Proximamente from "./paginas/Proximamente";

export default function App() {
  return (
    <Routes>
      <Route path="/entrar" element={<Entrar />} />

      <Route element={<RutaProtegida />}>
        <Route element={<Layout />}>
          <Route index element={<Navigate to="/lecciones" replace />} />
          <Route
            path="/lecciones"
            element={<Proximamente titulo="Lecciones" descripcion="Selecciona un tema para ver sus lecciones y ejercicios disponibles." />}
          />
          <Route
            path="/practicas"
            element={<Proximamente titulo="Prácticas" descripcion="Practica con ejercicios y pruebas de cada tema." />}
          />
          <Route
            path="/plan"
            element={<Proximamente titulo="Plan de estudio" descripcion="Activa los planes que deseas trabajar y explora sus temas y lecciones." />}
          />
          <Route
            path="/progreso"
            element={<Proximamente titulo="Progreso y estadísticas" descripcion="Revisa tu avance en el plan de estudio y los ejercicios completados." />}
          />
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
