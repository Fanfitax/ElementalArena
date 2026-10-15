using System;
using System.Collections.Generic;
using ArenaElemental.Modelos;

namespace ArenaElemental.Datos
{
    /// <summary>
    /// Los textos de "Antropología Filosófica" de un personaje: título, concepto filosófico que encarna,
    /// historia (lore), reflexión antropológica y el dilema que plantea.
    /// </summary>
    public class FichaLore
    {
        public string Titulo { get; private set; }
        public string Concepto { get; private set; }
        public string Historia { get; private set; }
        public string Reflexion { get; private set; }
        public string Dilema { get; private set; }

        public FichaLore(string titulo, string concepto, string historia, string reflexion, string dilema)
        {
            Titulo = titulo;
            Concepto = concepto;
            Historia = historia;
            Reflexion = reflexion;
            Dilema = dilema;
        }
    }

    /// <summary>
    /// Catálogo de lore del juego (igual que CatalogoBase, todo en memoria). Cada facción aborda un eje antropológico:
    ///  - Fuego: pasión, voluntad de poder y autodestrucción.
    ///  - Agua: alteridad, empatía y ética del cuidado.
    ///  - Tierra: trascendencia, orden y resistencia ante la finitud.
    ///  - Aire: libertad, contingencia y búsqueda de sentido.
    /// Los textos son una lectura propia del juego a partir de autores clásicos: sirven de punto de partida, no sustituyen
    /// la lectura de las obras citadas.
    /// </summary>
    public static class LorePersonajes
    {
        // La clave es el nombre del personaje, igual que en CatalogoBase.
        private static Dictionary<string, FichaLore> fichas = new Dictionary<string, FichaLore>();

        static LorePersonajes()
        {
            CargarFuego();
            CargarAgua();
            CargarTierra();
            CargarAire();
        }

        // ---------- Consultas ----------

        /// <summary>La ficha completa del personaje, o una ficha vacía si todavía no tiene lore escrito.</summary>
        public static FichaLore ObtenerFicha(Personaje p)
        {
            FichaLore ficha;
            if (p != null && fichas.TryGetValue(p.Nombre, out ficha)) return ficha;
            return new FichaLore("Sin registro", "—", "Todavía no hay una historia escrita para este personaje.",
                                 "Todavía no hay una reflexión escrita para este personaje.", "—");
        }

        public static string ObtenerHistoria(Personaje p)
        {
            return ObtenerFicha(p).Historia;
        }

        public static string ObtenerDilema(Personaje p)
        {
            return ObtenerFicha(p).Dilema;
        }

        public static string ObtenerReflexion(Personaje p)
        {
            return ObtenerFicha(p).Reflexion;
        }

        /// <summary>El eje antropológico (título corto) de cada facción.</summary>
        public static string EjeDe(Elemento elemento)
        {
            switch (elemento)
            {
                case Elemento.Fuego: return "Pasión, voluntad de poder y autodestrucción";
                case Elemento.Agua: return "Alteridad, empatía y ética del cuidado";
                case Elemento.Tierra: return "Trascendencia, orden y resistencia ante la finitud";
                default: return "Libertad, contingencia y búsqueda de sentido";
            }
        }

        /// <summary>Explicación breve del eje de la facción: de qué trata y qué pregunta deja abierta.</summary>
        public static string ExplicacionDelEje(Elemento elemento)
        {
            switch (elemento)
            {
                case Elemento.Fuego:
                    return "El fuego solo existe consumiendo: para arder agota su materia. Es el elemento de la voluntad (la «voluntad de " +
                           "poder» de Nietzsche, el querer ciego de Schopenhauer) y de su finitud: lo que más afirma la vida es lo que más " +
                           "rápido la gasta.\n¿Es la pasión plenitud o una forma acelerada de dejar de estar vivo?";

                case Elemento.Agua:
                    return "El agua toma la forma de lo que la contiene: solo existe en relación con otro. Es el elemento de la alteridad: " +
                           "el rostro del Otro me hace responsable (Levinas), el «Yo-Tú» me constituye (Buber) y la ética del cuidado atiende " +
                           "a quien tengo delante.\n¿Hasta dónde llega mi responsabilidad sin perderme a mí mismo?";

                case Elemento.Tierra:
                    return "La tierra es lo que permanece: cimientos, montañas, tumbas. Responde a la finitud con orden y obra, frente al " +
                           "ser-para-la-muerte de Heidegger y el hambre de inmortalidad de Unamuno.\n¿Resistir a la muerte es trascenderla " +
                           "o la forma más antigua de negarla?";

                default:
                    return "El aire no tiene forma ni lugar fijo: es movimiento y posibilidad. Es el elemento de la libertad y su vértigo: " +
                           "Sartre nos condena a ser libres, Camus ve el absurdo y Frankl responde con la voluntad de sentido.\n" +
                           "Cuando nada nos da un sentido hecho, ¿la libertad es plenitud o desamparo?";
            }
        }
        /// <summary>Autores y obras para profundizar en el eje de cada facción.</summary>
        public static string[] LecturasDe(Elemento elemento)
        {
            switch (elemento)
            {
                case Elemento.Fuego:
                    return new string[]
                    {
                        "Nietzsche — Así habló Zaratustra",
                        "Schopenhauer — El mundo como voluntad y representación",
                        "Heráclito — fragmentos sobre el fuego y el devenir",
                        "Freud — Más allá del principio del placer",
                        "Bataille — La parte maldita"
                    };
                case Elemento.Agua:
                    return new string[]
                    {
                        "Levinas — Totalidad e infinito",
                        "Buber — Yo y Tú",
                        "Edith Stein — Sobre el problema de la empatía",
                        "Gilligan — In a Different Voice (1982) · Noddings — Caring (1984)",
                        "Ricoeur — Sí mismo como otro"
                    };
                case Elemento.Tierra:
                    return new string[]
                    {
                        "Heidegger — Ser y tiempo",
                        "Spinoza — Ética (el conatus)",
                        "Unamuno — Del sentimiento trágico de la vida",
                        "Platón — Fedón · Arendt — La condición humana",
                        "Pico della Mirandola — Discurso sobre la dignidad del hombre"
                    };
                default:
                    return new string[]
                    {
                        "Sartre — El existencialismo es un humanismo",
                        "Kierkegaard — El concepto de la angustia",
                        "Camus — El mito de Sísifo",
                        "Frankl — El hombre en busca de sentido",
                        "Aristóteles — Metafísica, libro I (el asombro)"
                    };
            }
        }

        // ---------- Textos de cada personaje ----------

        private static void Agregar(string nombre, string titulo, string concepto, string historia, string reflexion, string dilema)
        {
            fichas[nombre] = new FichaLore(titulo, concepto, historia, reflexion, dilema);
        }

        private static void CargarFuego()
        {
            Agregar("Pyra", "La Espada Encendida", "La voluntad de poder",
                "Pyra creció entre forjas, en una aldea al pie de un volcán dormido, donde aprendió que el metal solo obedece a quien no le " +
                "teme al calor. A los dieciséis templó su propia espada y, la noche en que la llama del filo no se apagó con el agua del " +
                "templado, entendió que había nacido para algo más que fabricar herramientas. Partió hacia la Arena sin despedirse de nadie, " +
                "con una convicción que nadie logró discutirle: lo que se quiere con suficiente fuerza, se consigue.",
                "Pyra es la voluntad en estado puro: Nietzsche vería en ella la vida que se afirma y se supera, pero también una fuerza que confunde la intensidad de su querer con la razón de su querer. Schopenhauer le advertiría que desear sin fin es no descansar nunca: ¿la voluntad nos pertenece o somos su instrumento?",
                "Si quiero algo con todas mis fuerzas, ¿decido yo lo que quiero o solo obedezco a un impulso que no elegí?");

            
Agregar("Ignar", "El Aprendiz de la Llama", "La ambivalencia de la pasión",
                "Ignar aprendió a encender una llama en la palma de la mano antes de aprender a leer. Su maestra le enseñó lo primero que un " +
                "aprendiz de fuego debe saber: que la llama no es buena ni mala y que quien la lleva responde por lo que quema. La capa roja " +
                "que viste fue de ella. Ahora Ignar recorre la Arena con una pequeña llama siempre encendida, que cuida como se cuida un " +
                "recuerdo: con el miedo de que se apague y, todavía más, con el miedo de que crezca más de lo que puede dominar.",
                "Ignar encarna la ambivalencia de la pasión: la misma llama que da calor consume el bosque y, para Heráclito, nada se enciende sin que algo se consuma. Su reto no es apagar el fuego sino educarlo: moderar la pasión sin amputarla.",
                "¿Debo apagar lo que me quema para no dañar a nadie, o aprender a arder sin consumir lo que amo?");

            
Agregar("Cenizo", "El Zorro que Arde", "La autodestrucción y el gasto",
                "Nadie sabe si Cenizo nació con la cola en llamas o si la encendió por curiosidad y ya no pudo apagarla. Es pequeño, " +
                "juguetón y absolutamente sincero: juega con su fuego sin medir las consecuencias, como quien todavía no conoce el precio " +
                "de las cosas. Cada vez que la llama crece, el pelo de su cola se chamusca un poco más. Los demás combatientes lo adoran y " +
                "lo miran con una ternura inquieta: es el que menos teme en la Arena y, quizá por eso, el que más debería.",
                "Cenizo muestra la cara autodestructiva del querer: su Explosión de Brasas golpea fuerte pero le devuelve daño, pues lo que da en intensidad lo paga en finitud. Freud habló de una pulsión de muerte y Bataille de un gasto que solo busca sentirse vivo; Cenizo no sabe que su fuego se acaba, el ser humano sí.",
                "Si sé que arder me consume, ¿arder igual es valentía, ingenuidad o la única manera de estar de verdad vivo?");

        }

        private static void CargarAgua()
        {
            Agregar("Marina", "La Portadora del Cristal", "La responsabilidad ante el rostro del Otro",
                "Marina recorre los caminos con un báculo de cristal que, según dicen, solo brilla cuando alguien cerca sufre. Fue sanadora " +
                "de aldea en aldea y aprendió pronto una verdad que ninguna hierba enseña: no se cura una herida, se acompaña a una persona. " +
                "Llegó a la Arena porque allí el sufrimiento no se esconde: por primera vez nadie le pide que cure a escondidas, porque " +
                "todos saben que van a necesitarla.",
                "Marina vive la ética del rostro de Levinas: el otro me interpela antes de que yo decida y su vulnerabilidad me hace responsable. Su cuidado no elimina la finitud ajena, la hace llevadera, aunque esa responsabilidad no tiene medida.",
                "Si el rostro del otro me obliga antes de que yo decida, ¿cuánta libertad me queda para cuidarme a mí misma?");

            
Agregar("Tidal", "El Caballero del Tridente", "El cuidado frente al deber",
                "Tidal fue guardián del puerto de las Mareas Altas y juró no abandonar su puesto mientras hubiera un barco en el mar. " +
                "Cuando el puerto cayó conservó el juramento y la armadura, y cambió el muelle por la Arena. Cumple su deber con una " +
                "disciplina que algunos confunden con frialdad; en realidad es su manera de querer: nunca falla a quienes protege, aunque " +
                "deba tomar de otros lo que necesita para sostenerlos.",
                "Tidal tensiona el deber y el cuidado: su Oleada Voraz toma del rival para sostener a los suyos. Kant advertiría que el adversario también es un fin, y Buber que en el combate el otro deja de ser un «Tú» para volverse un «Ello».",
                "¿Puedo cuidar a los míos sin convertir al adversario en un simple medio para lograrlo?");

            
Agregar("Cascada", "La Sirena de la Burbuja", "La empatía y la distancia",
                "Cascada vive dentro de una esfera de agua que ella misma sostiene, porque fuera de ella el aire le resulta hostil. Mira la " +
                "Arena a través de ese cristal líquido: curiosa, sensible, siempre atenta a lo que sienten los demás. Es quien más habla con " +
                "todos y la única de la que nadie conoce el hogar. Cuando el equipo flaquea, la burbuja se convierte en lluvia y, por un " +
                "momento, la sirena parece no necesitar el agua para estar a salvo.",
                "La burbuja de Cascada simboliza la empatía: Edith Stein la describe como captar la vivencia ajena sin confundirse con ella. Esa distancia es su límite y su condición, pues sin ella habría un yo repetido y no un otro.",
                "¿Puedo comprender de verdad lo que vive alguien a quien solo veo a través de mi propia burbuja?");

        }

        private static void CargarTierra()
        {
            Agregar("Gaia", "La Raíz Antigua", "El ser-para-la-muerte",
                "Gaia lleva tantos años en pie que la gente del valle ya no distingue dónde termina su bastón y dónde empieza el árbol del que " +
                "fue cortado. Dicen que presenció la fundación de la Arena y que decidió quedarse para ver cómo termina. Habla poco y casi " +
                "siempre en preguntas. Su manto verde está remendado con hojas y, según quienes presenciaron sus combates, Gaia nunca se " +
                "apura: la tierra, explica, no llega tarde a ninguna parte.",
                "Gaia es la conciencia serena de la finitud: para Platón filosofar es practicar el morir y para Heidegger el ser humano es ser-para-la-muerte. Su Muro de Roca no vence al tiempo, lo ordena: la sabiduría es dejar de pelear contra él.",
                "Si todo lo que construyo se volverá polvo, ¿qué hace que valga la pena construirlo?");

            
Agregar("Gólem", "El Guardián Silencioso", "El ser hecho para algo",
                "Nadie recuerda quién modeló al Gólem ni con qué propósito; solo se sabe que despertó un día a la entrada de la Arena, con " +
                "una orden grabada que ya nadie puede leer: «protege». Su rostro —dos huecos y una línea— parece triste, aunque quizá solo " +
                "sea piedra. En su hombro creció una flor, la única señal de que algo vive en él. El Gólem no la arranca: la lleva con " +
                "cuidado, como quien guarda un secreto que ni él mismo entiende.",
                "El Gólem es el ser hecho para algo, frente al ser humano que Pico della Mirandola imagina como escultor de sí mismo. Su Coraza ordena el cuerpo según un propósito dado, pero la flor en su hombro sugiere una libertad que nadie programó.",
                "Si fui hecho para proteger, ¿puedo elegir proteger o solo cumplo una orden que ya no entiendo?");

            
Agregar("Sílex", "El Herrero de Montaña", "La obra como trascendencia",
                "Sílex pasó su vida golpeando la piedra de la montaña para encontrar el filo que esconde. Sostiene que una cosa bien hecha " +
                "le sobrevive a quien la hizo, y por eso firma cada pieza con una pequeña marca que nadie más conoce. Su martillo está tan " +
                "gastado que ya tiene la forma de su mano. Vino a la Arena a probar lo único que no sabe si perdura: a sí mismo.",
                "Sílex responde a la finitud con la obra: según Arendt, el trabajo fabrica un mundo duradero que sobrevive a su hacedor. Su Filo Reflectante muestra el orden de lo bien hecho, aunque queda la duda de si sobrevive el herrero o solo su pieza.",
                "Si mi obra me sobrevive, ¿sigo existiendo en ella o solo existe ella, sin mí?");

        }

        private static void CargarAire()
        {
            Agregar("Céfiro", "La Arquera del Viento Cambiante", "La libertad radical",
                "Céfiro jamás dispara dos veces desde el mismo lugar. Se ha mudado de aldea once veces y, cuando le preguntan por qué, " +
                "responde que no huye: elige, cada vez, no quedarse. Sus flechas llevan plumas de aves que nunca ha visto posarse. En la " +
                "Arena la conocen por su velocidad y por una costumbre que desconcierta: antes de cada combate pregunta al viento hacia " +
                "dónde sopla... y casi siempre va en dirección contraria.",
                "Céfiro es la libertad radical de Sartre: la existencia precede a la esencia y hasta quedarse quieto es una elección. Pero su Viento a Favor recuerda la facticidad: nadie elige en el vacío, y la libertad es lo que hacemos con las condiciones.",
                "Si cada elección me define y ninguna esencia me guía, ¿cómo sé si estoy eligiendo o solo moviéndome?");

            
Agregar("Huracán", "El Grifo de la Tormenta", "El absurdo y la revuelta",
                "Huracán nació en el centro de una tormenta que duró tres años: el único lugar del cielo donde no sopla el viento. Creció " +
                "aprendiendo a leer el caos desde dentro y a no confundir la calma con la paz. Su cuerpo de águila y de león lo vuelve " +
                "imposible de clasificar y él lo prefiere así: allá donde lo ubican, se va. Ataca como el trueno, pero habla muy poco y " +
                "mira siempre el horizonte, como esperando una respuesta que no llega.",
                "Huracán habita el absurdo de Camus: el choque entre nuestra necesidad de sentido y el silencio del mundo. Su Ojo de la Tormenta es la lucidez de quien, arrojado al caos sin elegirlo, responde con revuelta y no con huida.",
                "Si el mundo no responde a mi pregunta por el sentido, ¿debo callar, huir o rebelarme y seguir volando?");

            
Agregar("Brisa", "El Hada Curiosa", "La voluntad de sentido",
                "Brisa es la más pequeña de la Arena y la más curiosa. Se detiene a mirar una gota de rocío, una hormiga que carga una " +
                "migaja, una grieta en el muro; los demás la apuran y ella solo pregunta «¿para qué?». Sus alas apenas la sostienen en vuelo, " +
                "pero su pregunta pesa más que cualquier martillo. Ha curado a más combatientes de los que reconoce, casi sin darse cuenta, " +
                "solo por quedarse a su lado un momento más.",
                "Para Aristóteles la filosofía nace del asombro, y Brisa es esa mirada que no da nada por obvio. Frankl sostenía que el ser humano busca sobre todo sentido, y su Soplo Sanador sugiere que el sentido aparece al ocuparse de alguien.",
                "Si nadie me entrega un sentido ya hecho, ¿lo encuentro en el mundo o lo invento yo?");

        }
    }
}
