/* =====================================================================
   ARENA ELEMENTAL - Esquema de base de datos (SQL Server / T-SQL)
   Ejecutar completo en SQL Server Management Studio.
   ===================================================================== */

IF DB_ID(N'ArenaElemental') IS NULL
    CREATE DATABASE ArenaElemental;
GO

USE ArenaElemental;
GO

/* ---------- Limpieza (orden inverso a las llaves foráneas) ---------- */
IF OBJECT_ID(N'dbo.Estadisticas_Partidas', N'U') IS NOT NULL DROP TABLE dbo.Estadisticas_Partidas;
IF OBJECT_ID(N'dbo.Equipo_Personajes',     N'U') IS NOT NULL DROP TABLE dbo.Equipo_Personajes;
IF OBJECT_ID(N'dbo.Equipos',               N'U') IS NOT NULL DROP TABLE dbo.Equipos;
IF OBJECT_ID(N'dbo.Personajes',            N'U') IS NOT NULL DROP TABLE dbo.Personajes;
IF OBJECT_ID(N'dbo.Escenarios',            N'U') IS NOT NULL DROP TABLE dbo.Escenarios;
IF OBJECT_ID(N'dbo.Jugadores',             N'U') IS NOT NULL DROP TABLE dbo.Jugadores;
GO

/* ---------- Jugadores ---------- */
CREATE TABLE dbo.Jugadores (
    id_jugador      INT IDENTITY(1,1) NOT NULL,
    nombre_usuario  NVARCHAR(30)      NOT NULL,
    fecha_registro  DATETIME          NOT NULL CONSTRAINT DF_Jugadores_fecha DEFAULT (GETDATE()),
    CONSTRAINT PK_Jugadores PRIMARY KEY (id_jugador),
    CONSTRAINT UQ_Jugadores_usuario UNIQUE (nombre_usuario)
);
GO

/* ---------- Escenarios (4) ---------- */
CREATE TABLE dbo.Escenarios (
    id_escenario     INT IDENTITY(1,1) NOT NULL,
    nombre           NVARCHAR(40)      NOT NULL,
    elemento_afin    NVARCHAR(10)      NOT NULL,
    bonus_ataque_pct INT               NOT NULL CONSTRAINT DF_Escenarios_bonus DEFAULT (10),
    descripcion      NVARCHAR(200)     NULL,
    CONSTRAINT PK_Escenarios PRIMARY KEY (id_escenario),
    CONSTRAINT UQ_Escenarios_nombre UNIQUE (nombre),
    CONSTRAINT CK_Escenarios_elemento CHECK (elemento_afin IN (N'Fuego', N'Agua', N'Tierra', N'Aire')),
    CONSTRAINT CK_Escenarios_bonus CHECK (bonus_ataque_pct >= 0)
);
GO

/* ---------- Personajes (12 base: 3 por facción) ---------- */
CREATE TABLE dbo.Personajes (
    id_personaje      INT IDENTITY(1,1) NOT NULL,
    nombre            NVARCHAR(30)      NOT NULL,
    elemento          NVARCHAR(10)      NOT NULL,
    hp_max            INT               NOT NULL,
    atk               INT               NOT NULL,
    defensa           INT               NOT NULL,
    energia_max       INT               NOT NULL,
    empatia           INT               NOT NULL,
    nombre_habilidad  NVARCHAR(30)      NOT NULL,
    costo_habilidad   INT               NOT NULL,
    cooldown_habilidad INT              NOT NULL,
    CONSTRAINT PK_Personajes PRIMARY KEY (id_personaje),
    CONSTRAINT UQ_Personajes_nombre UNIQUE (nombre),
    CONSTRAINT CK_Personajes_elemento CHECK (elemento IN (N'Fuego', N'Agua', N'Tierra', N'Aire')),
    CONSTRAINT CK_Personajes_hp CHECK (hp_max > 0),
    CONSTRAINT CK_Personajes_stats CHECK (atk >= 0 AND defensa >= 0 AND energia_max >= 0
                                          AND empatia >= 0 AND costo_habilidad >= 0
                                          AND cooldown_habilidad >= 0)
);
GO

/* ---------- Equipos (cabecera) ---------- */
CREATE TABLE dbo.Equipos (
    id_equipo      INT IDENTITY(1,1) NOT NULL,
    id_jugador     INT               NOT NULL,
    nombre         NVARCHAR(40)      NOT NULL,
    fecha_creacion DATETIME          NOT NULL CONSTRAINT DF_Equipos_fecha DEFAULT (GETDATE()),
    CONSTRAINT PK_Equipos PRIMARY KEY (id_equipo),
    CONSTRAINT FK_Equipos_Jugadores FOREIGN KEY (id_jugador) REFERENCES dbo.Jugadores (id_jugador),
    CONSTRAINT UQ_Equipos_jugador_nombre UNIQUE (id_jugador, nombre)
);
GO

/* ---------- Equipo_Personajes (detalle: 3 cartas por equipo, con orden de relevo) ---------- */
CREATE TABLE dbo.Equipo_Personajes (
    id_equipo     INT     NOT NULL,
    id_personaje  INT     NOT NULL,
    orden         TINYINT NOT NULL,
    CONSTRAINT PK_Equipo_Personajes PRIMARY KEY (id_equipo, id_personaje),
    CONSTRAINT FK_EqPers_Equipos    FOREIGN KEY (id_equipo)    REFERENCES dbo.Equipos (id_equipo) ON DELETE CASCADE,
    CONSTRAINT FK_EqPers_Personajes FOREIGN KEY (id_personaje) REFERENCES dbo.Personajes (id_personaje),
    CONSTRAINT UQ_EqPers_orden UNIQUE (id_equipo, orden),
    CONSTRAINT CK_EqPers_orden CHECK (orden BETWEEN 1 AND 3)
);
GO

/* ---------- Estadisticas_Partidas ---------- */
CREATE TABLE dbo.Estadisticas_Partidas (
    id_partida     INT IDENTITY(1,1) NOT NULL,
    id_jugador     INT               NOT NULL,
    id_equipo      INT               NOT NULL,
    id_escenario   INT               NOT NULL,
    resultado      NVARCHAR(10)      NOT NULL,
    turnos         INT               NOT NULL,
    dano_total     INT               NOT NULL,
    fecha_partida  DATETIME          NOT NULL CONSTRAINT DF_Partidas_fecha DEFAULT (GETDATE()),
    CONSTRAINT PK_Estadisticas_Partidas PRIMARY KEY (id_partida),
    CONSTRAINT FK_Partidas_Jugadores  FOREIGN KEY (id_jugador)   REFERENCES dbo.Jugadores (id_jugador),
    CONSTRAINT FK_Partidas_Equipos    FOREIGN KEY (id_equipo)    REFERENCES dbo.Equipos (id_equipo),
    CONSTRAINT FK_Partidas_Escenarios FOREIGN KEY (id_escenario) REFERENCES dbo.Escenarios (id_escenario),
    CONSTRAINT CK_Partidas_resultado CHECK (resultado IN (N'Victoria', N'Derrota', N'Abandono')),
    CONSTRAINT CK_Partidas_valores CHECK (turnos >= 0 AND dano_total >= 0)
);
GO

/* ---------- Datos base: 4 escenarios ---------- */
INSERT INTO dbo.Escenarios (nombre, elemento_afin, bonus_ataque_pct, descripcion) VALUES
(N'Volcán Ardiente',  N'Fuego',  10, N'Ríos de lava potencian a las cartas de Fuego.'),
(N'Lago Cristalino',  N'Agua',   10, N'Aguas serenas potencian a las cartas de Agua.'),
(N'Cañón de Roca',    N'Tierra', 10, N'Paredes de piedra potencian a las cartas de Tierra.'),
(N'Cumbre Ventosa',   N'Aire',   10, N'Vientos de altura potencian a las cartas de Aire.');
GO

/* ---------- Datos base: 12 personajes ---------- */
INSERT INTO dbo.Personajes
    (nombre, elemento, hp_max, atk, defensa, energia_max, empatia, nombre_habilidad, costo_habilidad, cooldown_habilidad) VALUES
(N'Pyra',     N'Fuego',   90, 18,  4, 10, 3, N'Llamarada',       4, 3),
(N'Ignar',    N'Fuego',  100, 16,  6, 10, 4, N'Llamarada',       4, 3),
(N'Cenizo',   N'Fuego',   80, 20,  3, 10, 2, N'Llamarada',       4, 3),
(N'Marina',   N'Agua',   100, 13,  5, 10, 8, N'Marea Curativa',  4, 3),
(N'Tidal',    N'Agua',   110, 12,  6, 10, 7, N'Marea Curativa',  4, 3),
(N'Cascada',  N'Agua',    95, 14,  5, 10, 9, N'Marea Curativa',  4, 3),
(N'Gaia',     N'Tierra', 125, 12,  9,  8, 6, N'Muro de Roca',    3, 4),
(N'Gólem',    N'Tierra', 135, 11, 11,  8, 5, N'Muro de Roca',    3, 4),
(N'Sílex',    N'Tierra', 120, 13,  8,  8, 4, N'Muro de Roca',    3, 4),
(N'Céfiro',   N'Aire',    85, 15,  4, 12, 4, N'Ráfaga Veloz',    3, 3),
(N'Huracán',  N'Aire',    80, 17,  3, 12, 3, N'Ráfaga Veloz',    3, 3),
(N'Brisa',    N'Aire',    88, 14,  5, 12, 6, N'Ráfaga Veloz',    3, 3);
GO

/* ---------- Verificación rápida ---------- */
SELECT elemento, COUNT(*) AS cartas FROM dbo.Personajes GROUP BY elemento;
SELECT COUNT(*) AS escenarios FROM dbo.Escenarios;
GO
