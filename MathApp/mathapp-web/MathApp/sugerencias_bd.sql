-- =====================================================================
-- Cambios sugeridos al esquema (opcionales). El código C# funciona igual
-- con o sin estos cambios. Revisar antes de ejecutar en producción.
-- =====================================================================

-- 1) Un solo registro de progreso por usuario y lección, y un solo
--    registro por usuario y plan. Evita duplicados si llegan dos
--    peticiones al mismo tiempo (doble clic, reintentos de red).
ALTER TABLE Progreso_Lecciones
    ADD CONSTRAINT UQ_Progreso_Usuario_Leccion UNIQUE (Usuario_Id, Leccion_Id);

ALTER TABLE Usuario_Planes
    ADD CONSTRAINT UQ_UsuarioPlanes_Usuario_Plan UNIQUE (Usuario_Id, Plan_Id);

-- 2) UP_Activo no debería poder ser NULL. Los NULL existentes se toman
--    como inactivos (FALSE); los planes nuevos siguen activándose por defecto.
UPDATE Usuario_Planes SET UP_Activo = FALSE WHERE UP_Activo IS NULL;
ALTER TABLE Usuario_Planes MODIFY UP_Activo BOOLEAN NOT NULL DEFAULT TRUE;

-- 3) 25 caracteres se quedan cortos para nombres como
--    "Ecuaciones de primer grado" (26) o "Fracciones equivalentes y simplificación".
ALTER TABLE Temas     MODIFY Tema_Nombre    VARCHAR(80)  NOT NULL;
ALTER TABLE Lecciones MODIFY Leccion_Nombre VARCHAR(100) NOT NULL;

-- 4) Índice para la búsqueda de lecciones por nombre (requisito 8).
CREATE INDEX IX_Lecciones_Nombre ON Lecciones (Leccion_Nombre);

-- 5) Mismo juego de caracteres en todas las tablas (solo Progreso_Lecciones
--    lo declara). utf8mb4 soporta acentos, ñ y símbolos matemáticos (≤, √, π).
ALTER TABLE Planes_Estudio        CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE Usuarios              CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE Temas                 CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE Lecciones             CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE Ejercicios            CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE Usuario_Planes        CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE Resultados_Ejercicios CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
