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
- `Datos\` — `IRepositorio`, `RepositorioSql` (ADO.NET, SQL Server o MySQL), `RepositorioMemoria` (respaldo sin BD), `CatalogoBase` (copia de los datos base), `ConfiguracionBD` (única configuración de conexión).
- `Formularios\` — WinForms creados por código (sin .Designer.cs): `FrmMenu`, `FrmSeleccionEquipo`, `FrmCombate`, `FrmEstadisticas`, `TarjetaPersonaje`, `Estilo`.
- `BaseDatos\` — scripts `ArenaElemental_SQLServer.sql` y `ArenaElemental_MySQL.sql` (mismo esquema).

## Reglas del juego (ya implementadas)
- Triángulo: **Agua > Fuego > Aire > Tierra > Agua**; ventaja x1.5, desventaja x0.5.
- Daño: `max(1, ATK * factor * multiplicador * escenario - DEF * (1 - defensa_ignorada))`.
- Equipos de 3; cada turno UNA acción: Atacar, Habilidad, Relevo o Descansar (+3 ENG).
- No se puede relevar dos turnos seguidos; si cae el activo, relevo forzado gratis.
- Escenario: +10% ATK al elemento afín. Ronda 30: muerte súbita (+15% ATK por ronda) para que todo combate termine.
- Habilidades: Llamarada (x2, ignora 50% DEF), Marea Curativa (25% HP máx + 2*Empatía, elige aliado), Muro de Roca (+DEF 2 turnos), Ráfaga Veloz (2 golpes x0.75, +2 ENG).
- 12 personajes (3 por facción), 4 escenarios.

## Invariantes a mantener
- `Datos\CatalogoBase.cs` debe coincidir con los INSERT de ambos scripts SQL.
- Las tablas extra `Escenarios` y `Equipo_Personajes` existen por normalización (el enunciado pedía 4 tablas).

## Verificación hecha
- Pruebas de modelos (validaciones, triángulo, habilidades) y simulación de 3000 combates IA vs IA: 100% terminan, máx. 59 rondas, reparto ~54/46.
- Los scripts SQL NO se han ejecutado contra un servidor real; el proyecto no se ha abierto en SharpDevelop 4.4 real (MSBuild se colgó en la PC original; csc compiló sin errores).

## Estado de MySQL (local)
- Servidor MySQL 8.0 instalado (servicio `MySQL80`, puerto 3306). Base creada: `arenaelemental` (12 personajes, 4 escenarios).
- `ConfiguracionBD.cs` apunta a `Database=ArenaElemental` con `Pwd=TU_CLAVE`. La clave real NO va al repo: cada máquina la pone localmente.
- Falta instalar MySQL Connector/NET y agregar la referencia `MySql.Data` al `.csproj`. No está en la PC.
- Problema de versiones: `root` usa `caching_sha2_password`. Conector 6.9.x (.NET 4.0) no lo soporta; conector 8.x exige .NET 4.5.2+ (subir a v4.8). Decisión pendiente.
- Decisión del usuario: dejar la conexión a MySQL para el final, cuando el proyecto esté listo.

## Pendiente / ideas
- Probar scripts SQL y `RepositorioSql` contra un servidor real.
- Probar el proyecto en SharpDevelop 4.4 / Visual Studio.
- Posibles mejoras: sonidos/animaciones, modo 2 jugadores, ranking global, balance de personajes, guardar partidas.
