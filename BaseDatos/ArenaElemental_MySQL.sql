/* =====================================================================
   ARENA ELEMENTAL - Esquema de base de datos (MySQL 8 / MariaDB)
   Ejecutar completo en MySQL Workbench o phpMyAdmin.
   (Los CHECK solo se aplican en MySQL >= 8.0.16 / MariaDB >= 10.2.1)
   ===================================================================== */

CREATE DATABASE IF NOT EXISTS ArenaElemental
    CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE ArenaElemental;

/* ---------- Limpieza (orden inverso a las llaves foráneas) ---------- */
SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS Estadisticas_Partidas;
DROP TABLE IF EXISTS Equipo_Personajes;
DROP TABLE IF EXISTS Equipos;
DROP TABLE IF EXISTS Personajes;
DROP TABLE IF EXISTS Escenarios;
DROP TABLE IF EXISTS Jugadores;
SET FOREIGN_KEY_CHECKS = 1;

/* ---------- Jugadores ---------- */
CREATE TABLE Jugadores (
    id_jugador      INT          NOT NULL AUTO_INCREMENT,
    nombre_usuario  VARCHAR(30)  NOT NULL,
    fecha_registro  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id_jugador),
    UNIQUE KEY UQ_Jugadores_usuario (nombre_usuario)
) ENGINE=InnoDB;

/* ---------- Escenarios (4) ---------- */
CREATE TABLE Escenarios (
    id_escenario     INT          NOT NULL AUTO_INCREMENT,
    nombre           VARCHAR(40)  NOT NULL,
    elemento_afin    VARCHAR(10)  NOT NULL,
    bonus_ataque_pct INT          NOT NULL DEFAULT 10,
    descripcion      VARCHAR(200) NULL,
    PRIMARY KEY (id_escenario),
    UNIQUE KEY UQ_Escenarios_nombre (nombre),
    CONSTRAINT CK_Escenarios_elemento CHECK (elemento_afin IN ('Fuego', 'Agua', 'Tierra', 'Aire')),
    CONSTRAINT CK_Escenarios_bonus CHECK (bonus_ataque_pct >= 0)
) ENGINE=InnoDB;

/* ---------- Personajes (12 base: 3 por facción) ---------- */
CREATE TABLE Personajes (
    id_personaje       INT          NOT NULL AUTO_INCREMENT,
    nombre             VARCHAR(30)  NOT NULL,
    elemento           VARCHAR(10)  NOT NULL,
    hp_max             INT          NOT NULL,
    atk                INT          NOT NULL,
    defensa            INT          NOT NULL,
    energia_max        INT          NOT NULL,
    empatia            INT          NOT NULL,
    nombre_habilidad   VARCHAR(30)  NOT NULL,
    costo_habilidad    INT          NOT NULL,
    cooldown_habilidad INT          NOT NULL,
    PRIMARY KEY (id_personaje),
    UNIQUE KEY UQ_Personajes_nombre (nombre),
    CONSTRAINT CK_Personajes_elemento CHECK (elemento IN ('Fuego', 'Agua', 'Tierra', 'Aire')),
    CONSTRAINT CK_Personajes_hp CHECK (hp_max > 0),
    CONSTRAINT CK_Personajes_stats CHECK (atk >= 0 AND defensa >= 0 AND energia_max >= 0
                                          AND empatia >= 0 AND costo_habilidad >= 0
                                          AND cooldown_habilidad >= 0)
) ENGINE=InnoDB;

/* ---------- Equipos (cabecera) ---------- */
CREATE TABLE Equipos (
    id_equipo       INT          NOT NULL AUTO_INCREMENT,
    id_jugador      INT          NOT NULL,
    nombre          VARCHAR(40)  NOT NULL,
    fecha_creacion  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id_equipo),
    UNIQUE KEY UQ_Equipos_jugador_nombre (id_jugador, nombre),
    CONSTRAINT FK_Equipos_Jugadores FOREIGN KEY (id_jugador) REFERENCES Jugadores (id_jugador)
) ENGINE=InnoDB;

/* ---------- Equipo_Personajes (detalle: 3 cartas por equipo, con orden de relevo) ---------- */
CREATE TABLE Equipo_Personajes (
    id_equipo     INT              NOT NULL,
    id_personaje  INT              NOT NULL,
    orden         TINYINT UNSIGNED NOT NULL,
    PRIMARY KEY (id_equipo, id_personaje),
    UNIQUE KEY UQ_EqPers_orden (id_equipo, orden),
    CONSTRAINT FK_EqPers_Equipos    FOREIGN KEY (id_equipo)    REFERENCES Equipos (id_equipo) ON DELETE CASCADE,
    CONSTRAINT FK_EqPers_Personajes FOREIGN KEY (id_personaje) REFERENCES Personajes (id_personaje),
    CONSTRAINT CK_EqPers_orden CHECK (orden BETWEEN 1 AND 3)
) ENGINE=InnoDB;

/* ---------- Estadisticas_Partidas ---------- */
CREATE TABLE Estadisticas_Partidas (
    id_partida     INT          NOT NULL AUTO_INCREMENT,
    id_jugador     INT          NOT NULL,
    id_equipo      INT          NOT NULL,
    id_escenario   INT          NOT NULL,
    resultado      VARCHAR(10)  NOT NULL,
    turnos         INT          NOT NULL,
    dano_total     INT          NOT NULL,
    fecha_partida  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id_partida),
    CONSTRAINT FK_Partidas_Jugadores  FOREIGN KEY (id_jugador)   REFERENCES Jugadores (id_jugador),
    CONSTRAINT FK_Partidas_Equipos    FOREIGN KEY (id_equipo)    REFERENCES Equipos (id_equipo),
    CONSTRAINT FK_Partidas_Escenarios FOREIGN KEY (id_escenario) REFERENCES Escenarios (id_escenario),
    CONSTRAINT CK_Partidas_resultado CHECK (resultado IN ('Victoria', 'Derrota', 'Abandono')),
    CONSTRAINT CK_Partidas_valores CHECK (turnos >= 0 AND dano_total >= 0)
) ENGINE=InnoDB;

/* ---------- Datos base: 4 escenarios ---------- */
INSERT INTO Escenarios (nombre, elemento_afin, bonus_ataque_pct, descripcion) VALUES
('Volcán Ardiente',  'Fuego',  10, 'Ríos de lava potencian a las cartas de Fuego.'),
('Lago Cristalino',  'Agua',   10, 'Aguas serenas potencian a las cartas de Agua.'),
('Cañón de Roca',    'Tierra', 10, 'Paredes de piedra potencian a las cartas de Tierra.'),
('Cumbre Ventosa',   'Aire',   10, 'Vientos de altura potencian a las cartas de Aire.');

/* ---------- Datos base: 12 personajes ---------- */
INSERT INTO Personajes
    (nombre, elemento, hp_max, atk, defensa, energia_max, empatia, nombre_habilidad, costo_habilidad, cooldown_habilidad) VALUES
('Pyra',     'Fuego',   90, 18,  4, 10, 3, 'Llamarada',       4, 3),
('Ignar',    'Fuego',  100, 16,  6, 10, 4, 'Llamarada',       4, 3),
('Cenizo',   'Fuego',   80, 20,  3, 10, 2, 'Llamarada',       4, 3),
('Marina',   'Agua',   100, 13,  5, 10, 8, 'Marea Curativa',  4, 3),
('Tidal',    'Agua',   110, 12,  6, 10, 7, 'Marea Curativa',  4, 3),
('Cascada',  'Agua',    95, 14,  5, 10, 9, 'Marea Curativa',  4, 3),
('Gaia',     'Tierra', 125, 12,  9,  8, 6, 'Muro de Roca',    3, 4),
('Gólem',    'Tierra', 135, 11, 11,  8, 5, 'Muro de Roca',    3, 4),
('Sílex',    'Tierra', 120, 13,  8,  8, 4, 'Muro de Roca',    3, 4),
('Céfiro',   'Aire',    85, 15,  4, 12, 4, 'Ráfaga Veloz',    3, 3),
('Huracán',  'Aire',    80, 17,  3, 12, 3, 'Ráfaga Veloz',    3, 3),
('Brisa',    'Aire',    88, 14,  5, 12, 6, 'Ráfaga Veloz',    3, 3);

/* ---------- Verificación rápida ---------- */
SELECT elemento, COUNT(*) AS cartas FROM Personajes GROUP BY elemento;
SELECT COUNT(*) AS escenarios FROM Escenarios;
