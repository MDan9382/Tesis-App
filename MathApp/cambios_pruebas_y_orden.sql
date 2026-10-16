-- =====================================================================
-- Pruebas por tema y orden de la ruta de aprendizaje.
-- Ejecutar DESPUÉS de sugerencias_bd.sql. Requiere MySQL 8.
-- =====================================================================

-- ---------------------------------------------------------------------
-- 1) Pruebas: una por fila, con su contenido en Markdown (igual que los
--    ejercicios). El frontend se encarga de mostrarla y del tiempo límite.
-- ---------------------------------------------------------------------
CREATE TABLE Pruebas (
    Prueba_Id        INT          NOT NULL AUTO_INCREMENT,
    Prueba_Contenido LONGTEXT     NULL,
    Prueba_EsPrueba  BOOLEAN      NOT NULL DEFAULT FALSE,
    Tema_Id          INT          NOT NULL,
    CONSTRAINT PK_Pruebas PRIMARY KEY (Prueba_Id),
    CONSTRAINT FK_Pruebas_Temas
        FOREIGN KEY (Tema_Id) REFERENCES Temas (Tema_Id)          -- corregido: antes Lecciones
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE Resultados_Pruebas (
    RP_Id               INT          NOT NULL AUTO_INCREMENT,
    RP_Puntaje          INT          NULL,
    RP_Fecha_Completado DATETIME     NULL,
    Prueba_Id           INT          NOT NULL,
    Usuario_Id          INT          NOT NULL,
    CONSTRAINT PK_Resultados_Pruebas PRIMARY KEY (RP_Id),
    CONSTRAINT FK_Resultados_Pruebas_Pruebas
        FOREIGN KEY (Prueba_Id) REFERENCES Pruebas (Prueba_Id),   -- corregido: antes Ejercicios
    CONSTRAINT FK_Resultados_Pruebas_Usuarios                      -- corregido: nombre repetido
        FOREIGN KEY (Usuario_Id) REFERENCES Usuarios (Usuario_Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------
-- 2) Orden para la ruta de aprendizaje
-- ---------------------------------------------------------------------
ALTER TABLE Temas     ADD COLUMN Tema_Orden    INT NOT NULL DEFAULT 0 AFTER Tema_Nombre;
ALTER TABLE Lecciones ADD COLUMN Leccion_Orden INT NOT NULL DEFAULT 0 AFTER Leccion_Contenido;

-- Opcional: numera lo que ya existe de 10 en 10 respetando el orden actual
-- (por Id). Sin esto todo queda en 0 y el backend desempata por Id.
UPDATE Temas t
JOIN (SELECT Tema_Id, ROW_NUMBER() OVER (PARTITION BY Plan_Id ORDER BY Tema_Id) * 10 AS n
      FROM Temas) x ON x.Tema_Id = t.Tema_Id
SET t.Tema_Orden = x.n;

UPDATE Lecciones l
JOIN (SELECT Leccion_Id, ROW_NUMBER() OVER (PARTITION BY Tema_Id ORDER BY Leccion_Id) * 10 AS n
      FROM Lecciones) x ON x.Leccion_Id = l.Leccion_Id
SET l.Leccion_Orden = x.n;

CREATE INDEX IX_Temas_Plan_Orden     ON Temas (Plan_Id, Tema_Orden);
CREATE INDEX IX_Lecciones_Tema_Orden ON Lecciones (Tema_Id, Leccion_Orden);
