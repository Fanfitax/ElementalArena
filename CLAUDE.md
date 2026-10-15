# Arena Elemental

Proyecto final de 2º semestre (Josman): simulador de combate estratégico por turnos y relevos en Windows Forms.
Idioma de trabajo con el usuario: **español**.

## Restricciones clave
- Debe compilar y funcionar en **SharpDevelop 4.4** (.NET Framework 4.0, **C# 4.0**): sin `$""`, `?.`, `nameof`, `async`, auto-propiedades con inicializador ni miembros con `=>`.
- Si Visual Studio no tiene el targeting pack de v4.0, se puede subir `TargetFrameworkVersion` a v4.8 en la laptop, sin usar sintaxis posterior a C# 4.
- Verificar compatibilidad con: `C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /langversion:4`.
- La plataforma del proyecto es x86.

## Arquitectura (namespace `ArenaElemental.*`)
- `Modelos\` — `Personaje` (abstracta) + `CartaFuego/Agua/Tierra/Aire`, `Escenario`, `FabricaPersonajes`.
  Stats base negativos lanzan `ArgumentOutOfRangeException`; HP/ENG/cooldown se limitan (clamp) a [0, máx].
- `Motor\` — `Combate` (turnos/relevos, sin UI), `EquipoCombate`, `AccionCombate`, `IaRival` (CPU).
  Estructuras de datos de la guía: **`Queue<Personaje>`** = banca de relevos (en `EquipoCombate`) y **`Stack<string>`** = historial de jugadas (en `Combate`).
- `Datos\` — SIN base de datos: `CatalogoBase` (listas estáticas `List<Personaje>` y `List<Escenario>` con los 12 personajes y 4 escenarios), `RegistroPartidas` (estadísticas en memoria, se pierden al cerrar), `EstadisticaPartida`.
- `Formularios\` — WinForms creados por código (sin .Designer.cs): `FrmMenu`, `FrmSeleccionEquipo`, `FrmCombate`, `FrmEstadisticas`, `TarjetaPersonaje`, `Estilo`.
  `FrmCombate` muestra la cola de cada banca y la pila del historial en `ListBox`.
- `BaseDatos\` — scripts `ArenaElemental_SQLServer.sql` y `ArenaElemental_MySQL.sql` (mismo esquema). NO se usan ni compilan por ahora; quedan para cuando se retome la BD.

## Estilo de código (pedido del usuario)
Código de estudiante de 2º semestre, ordenado y fácil de explicar en una defensa: POO básica (clases, herencia, encapsulamiento, polimorfismo simple), sin inyección de dependencias, interfaces de repositorio, eventos ni patrones avanzados.

## Reglas del juego (ya implementadas)
- Triángulo: **Agua > Fuego > Aire > Tierra > Agua**; ventaja x1.5, desventaja x0.5.
- Daño: `max(1, ATK * factor * multiplicador * escenario - DEF * (1 - defensa_ignorada))`.
- Equipos de 3; cada turno UNA acción: Atacar, Habilidad, Relevo o Descansar (+3 ENG).
- Relevo por **cola (FIFO)**: entra el primero de la banca y el que sale se forma al final (no se elige quién entra). El orden elegido al armar el equipo define el activo inicial y el orden de la cola.
- No se puede relevar dos turnos seguidos; si cae el activo, entra automáticamente el siguiente de la cola (gratis). El caído no vuelve a la cola.
- Escenario: +10% ATK al elemento afín. Ronda 30: muerte súbita (+15% ATK por ronda) para que todo combate termine.
- Habilidades: Llamarada (x2, ignora 50% DEF), Marea Curativa (25% HP máx + 2*Empatía, elige aliado), Muro de Roca (+DEF 2 turnos), Ráfaga Veloz (2 golpes x0.75, +2 ENG).
- 12 personajes (3 por facción), 4 escenarios.

## Invariantes a mantener
- Si se retoma la BD: `Datos\CatalogoBase.cs` debe coincidir con los INSERT de ambos scripts SQL.
- Las tablas extra `Escenarios` y `Equipo_Personajes` existen por normalización (el enunciado pedía 4 tablas).
- El catálogo estático no se modifica en combate: `EquipoCombate` crea copias (`CrearCopia`).

## Verificación hecha
- Compila con `csc.exe /langversion:4` sin errores ni advertencias (probado tras quitar la BD y agregar Queue/Stack).
- Simulación de 3000 combates IA vs IA con la cola de relevos: 100% terminan, máx. 60 rondas, reparto ~54/46, cola sin personajes muertos (invariante revisado en cada acción).
- Los formularios se construyen sin errores (prueba sin ventana). No se ha jugado a mano ni abierto en SharpDevelop 4.4 real.

## Base de datos (aparcada)
- Decisión del usuario: el juego va 100% en memoria por ahora; la BD se retoma al final, cuando el proyecto esté listo.
- El código ADO.NET anterior (`RepositorioSql`, `IRepositorio`, `ConfiguracionBD`, `RepositorioMemoria`, `FabricaPersonajes`) se eliminó del proyecto; sigue en el historial de git (commit `13ec04d`).
- Estado del entorno: MySQL 8.0 local (servicio `MySQL80`, puerto 3306) con base `arenaelemental` ya creada (12 personajes, 4 escenarios). Falta MySQL Connector/NET (`root` usa `caching_sha2_password`: el conector 6.9.x de .NET 4.0 no lo soporta; el 8.x exige .NET 4.5.2+). La clave real nunca va al repo.

## Pendiente / ideas
- Jugar el núcleo a mano y revisar cómo se ve `FrmCombate` (cola y pila).
- Probar el proyecto en SharpDevelop 4.4 / Visual Studio.
- Posibles mejoras: sonidos/animaciones, modo 2 jugadores, ranking global, balance de personajes, guardar partidas.
