# Arena Elemental

Proyecto final de 2º semestre (Josman): simulador de combate estratégico por turnos y relevos en Windows Forms.
Idioma de trabajo con el usuario: **español**.

## Restricciones clave
- Debe compilar y funcionar en **SharpDevelop 4.4** (.NET Framework 4.0, **C# 4.0**): sin `$""`, `?.`, `nameof`, `async`, auto-propiedades con inicializador ni miembros con `=>`.
- Si Visual Studio no tiene el targeting pack de v4.0, se puede subir `TargetFrameworkVersion` a v4.8 en la laptop, sin usar sintaxis posterior a C# 4.
- Verificar compatibilidad con: `C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /langversion:4`.
- La plataforma del proyecto es x86.

## Arquitectura (namespace `ArenaElemental.*`)
- `Modelos\` — `Personaje` (abstracta) + `CartaFuego/Agua/Tierra/Aire` (solo cambian el triángulo elemental), `Habilidad` (+ enum `EfectoHabilidad`), `Escenario`.
  Stats base negativos lanzan `ArgumentOutOfRangeException`; HP/ENG/recargas se limitan (clamp) a [0, máx].
  Cada personaje tiene **2 habilidades**: `HabilidadAtaque` (número 0) y `HabilidadUtilidad` (número 1). Los efectos temporales (potenciador de ATK, DEF extra, reflejo) viven en `Personaje` y se limpian al salir del campo.
- `Motor\` — `Combate` (turnos/relevos, sin UI), `EquipoCombate`, `AccionCombate`, `IaRival` (CPU).
  Estructuras de datos de la guía: **`Queue<Personaje>`** = banca de relevos (en `EquipoCombate`) y **`Stack<string>`** = historial de jugadas (en `Combate`).
- `Datos\` — SIN base de datos: `CatalogoBase` (listas estáticas `List<Personaje>` y `List<Escenario>` con los 12 personajes y 4 escenarios), `RegistroPartidas` (estadísticas en memoria, se pierden al cerrar), `EstadisticaPartida`.
- `Formularios\` — WinForms creados por código (sin .Designer.cs): `FrmMenu`, `FrmSeleccionEquipo`, `FrmCombate`, `FrmEstadisticas`, `VistaCarta`, `MiniaturaPersonaje`, `FormularioModerno`, `Ilustraciones`, `Estilo`.
  `FormularioModerno` es el formulario base de todas las pantallas: pinta (una sola vez, en caché) el fondo moderno de `Estilo.PintarFondo` (degradado azul noche, resplandor, hexágonos, viñeta); si el formulario tiene `BackgroundImage` (el tablero del combate) usa esa.
  `FrmMenu` (1120x640): título, nombre del jugador, botones y los 4 héroes de la portada (Pyra, Marina, Gaia, Céfiro: `Ilustraciones.ObtenerPersonajeEncajado`, se cargan después de mostrar la ventana) sobre plataformas luminosas.
  `FrmSeleccionEquipo` (1260x660): galería de 12 `MiniaturaPersonaje` (una columna por elemento; clic = ver, doble clic = agregar), `VistaCarta` grande como vista previa, 3 casillas del equipo con ▲▼× (el orden define el activo inicial y la cola), nombre del equipo y 4 botones de escenario. Conserva la API `EquipoElegido/EscenarioElegido/NombreEquipo`.
  `FrmEstadisticas`: 6 tarjetas de resumen + tabla oscura (el color de "Resultado" se pone en `CellFormatting`, porque al asignar `DataSource` las filas aún no existen).
  `VistaCarta` (UserControl, estética de carta TCG: Panel/PictureBox/Label/ProgressBar, esquinas redondeadas) tiene tamaño grande y pequeño (la pequeña también lleva imagen); se conecta con `ActualizarDatos(Personaje)`. `EsSiguiente = true` le pone borde dorado (carta que entra en el próximo relevo).
  `Ilustraciones` prepara el arte: carga `Imagenes\<nombre>` (.png/.jpg/.jpeg, minúsculas, `_` por espacios, con o sin tildes), le **quita el fondo blanco** (relleno desde los bordes con `LockBits`, más las zonas blancas grandes encerradas, p. ej. entre las piernas; los blancos pequeños como los ojos se conservan), recorta al personaje y lo compone sobre un fondo/resplandor del color del elemento (`ObtenerArte`) o lo entrega suelto y transparente (`ObtenerPersonajeEncajado`). Todo con caché; si no hay imagen o está dañada la carta queda con un color sólido del elemento (nunca lanza excepción).
  `Estilo.BuscarImagen/CargarImagen` buscan en `Imagenes\` junto al .exe, en la carpeta actual o en la del proyecto (bin\Debug -> ..\..). `Estilo.EstilizarBoton/Redondear/CaminoRedondeado` dan el aspecto plano y redondeado a botones y paneles.
  `FrmCombate` (1264x500) es una mesa de duelo: fondo `Imagenes\tablero.jpg` (`ImageLayout.Stretch`), cartas grandes enfrentadas, 5 botones en el centro (Atacar, habilidad de ataque, habilidad de utilidad, Relevo, Pasar; con tooltip de la descripción), bancas en columnas `FlowLayoutPanel` a los lados, y el historial (pila) en un `ListBox` casi negro a la derecha. La ventana tiembla cuando ocurre el Terremoto.
- `Imagenes\` — arte del juego: las 12 ilustraciones del usuario (`pyra.jpg`... personajes de cuerpo completo sobre fondo blanco, copiadas de `bin\Debug\Imagenes` porque `bin\` está en .gitignore) y `tablero.jpg` (generado por código). Ver `Imagenes\LEEME.txt`.
- `Herramientas\` — `GeneradorTablero.cs`: dibuja `tablero.jpg` con GDI+. NO se compila con el proyecto.
- `BaseDatos\` — scripts `ArenaElemental_SQLServer.sql` y `ArenaElemental_MySQL.sql` (mismo esquema). NO se usan ni compilan por ahora; quedan para cuando se retome la BD.

## Estilo de código (pedido del usuario)
Código de estudiante de 2º semestre, ordenado y fácil de explicar en una defensa: POO básica (clases, herencia, encapsulamiento, polimorfismo simple), sin inyección de dependencias, interfaces de repositorio, eventos ni patrones avanzados.

## Reglas del juego (ya implementadas)
- Triángulo: **Agua > Fuego > Aire > Tierra > Agua**; ventaja x1.5, desventaja x0.5.
- Daño: `max(1, ATK * factor * multiplicador * escenario - DEF * (1 - defensa_ignorada))`.
- Equipos de 3; cada turno UNA acción: Atacar, Habilidad de ataque, Habilidad de utilidad, Relevo o Pasar (+3 ENG). Energía y recarga son por habilidad (cada una tiene su costo y su cooldown).
- Relevo por **cola (FIFO)**: entra el primero de la banca y el que sale se forma al final (no se elige quién entra). El orden elegido al armar el equipo define el activo inicial y el orden de la cola.
- No se puede relevar dos turnos seguidos; si cae el activo, entra automáticamente el siguiente de la cola (gratis). El caído no vuelve a la cola.
- **Terremoto** (evento automático, en `Combate`): cada 4 rondas (4, 8, 12...) los dos personajes en el campo pierden 10% de su HP máx. (mínimo 1) y nunca bajan de 1 HP, así que no mata. Cada efecto queda en la pila del historial. Constantes: `RondasEntreTerremotos`, `PorcentajeDanoTerremoto`.
- Escenario: +10% ATK al elemento afín. Ronda 30: muerte súbita (+15% ATK por ronda) para que todo combate termine.
- Habilidades (ataque / utilidad, las define `CatalogoBase` con `Habilidad.Golpe/GolpeDoble/GolpeVampirico/GolpeConRetroceso/Curacion/CuracionEquipo/PotenciarAtaque/PotenciarDefensa/Reflejo/RecargaEnergia`):
  Fuego (daño puro, utilidad = potenciar ATK): Pyra Llamarada / Aura Ígnea · Ignar Tajo Carmesí / Grito de Guerra · Cenizo Explosión de Brasas (retroceso) / Combustión Interna.
  Agua (apoyo, utilidad = curar): Marina Chorro a Presión / Marea Curativa · Tidal Oleada Voraz (roba vida) / Oleaje Reparador · Cascada Torrente (doble golpe) / Lluvia Sanadora (cura a todo el equipo).
  Tierra (tanques, utilidad = defensa o reflejo): Gaia Raíces Punzantes / Muro de Roca · Gólem Puño Sísmico / Coraza Espinosa (devuelve 50% del daño) · Sílex Esquirla Cortante / Filo Reflectante (devuelve 60%).
  Aire (veloces): Céfiro Ráfaga Veloz / Viento a Favor (+6 ENG) · Huracán Tornado Cortante / Ojo de la Tormenta (potencia ATK) · Brisa Cuchilla de Viento / Soplo Sanador (cura).
  El reflejo y el retroceso nunca dejan a nadie por debajo de 1 HP (no pueden matar), igual que el Terremoto.
- 12 personajes (3 por facción), 4 escenarios.

## Invariantes a mantener
- Si se retoma la BD: `Datos\CatalogoBase.cs` debe coincidir con los INSERT de ambos scripts SQL.
- Las tablas extra `Escenarios` y `Equipo_Personajes` existen por normalización (el enunciado pedía 4 tablas).
- El catálogo estático no se modifica en combate: `EquipoCombate` crea copias (`CrearCopia`).

## Verificación hecha
- Compila con `csc.exe /langversion:4` sin errores ni advertencias (x86 y AnyCPU).
- Simulación de 4000 combates IA vs IA con las 2 habilidades, cola de relevos y Terremoto: 100% terminan, máx. ~52 rondas, 0 errores de invariantes (cola sin muertos, HP/ENG en rango). Victorias por personaje entre ~45% y ~58% tras ajustar las curaciones de Agua (Marina/Tidal) y subir un poco a Céfiro, Sílex y Huracán.
- Se renderizaron a imagen `FrmMenu`, `FrmSeleccionEquipo`, `FrmCombate` y `FrmEstadisticas` para revisar el diseño (la prueba se hace con una compilación AnyCPU porque el .exe x86 no se puede cargar desde PowerShell de 64 bits). No se ha jugado a mano ni abierto en SharpDevelop 4.4 real.

## Base de datos (aparcada)
- Decisión del usuario: el juego va 100% en memoria por ahora; la BD se retoma al final, cuando el proyecto esté listo.
- El código ADO.NET anterior (`RepositorioSql`, `IRepositorio`, `ConfiguracionBD`, `RepositorioMemoria`, `FabricaPersonajes`) se eliminó del proyecto; sigue en el historial de git (commit `13ec04d`).
- Estado del entorno: MySQL 8.0 local (servicio `MySQL80`, puerto 3306) con base `arenaelemental` ya creada (12 personajes, 4 escenarios). Falta MySQL Connector/NET (`root` usa `caching_sha2_password`: el conector 6.9.x de .NET 4.0 no lo soporta; el 8.x exige .NET 4.5.2+). La clave real nunca va al repo.

## Pendiente / ideas
- Jugar el núcleo a mano (ahora con 2 habilidades) y afinar el balance si hace falta.
- Probar el proyecto en SharpDevelop 4.4 / Visual Studio.
- Posibles mejoras: vista previa de la carta en la pantalla de selección de equipo, sonidos/animaciones, modo 2 jugadores, ranking global, guardar partidas.
