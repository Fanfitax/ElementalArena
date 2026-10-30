using System;
using System.Collections.Generic;
using ArenaElemental.Modelos;

namespace ArenaElemental.Datos
{
    // Los textos de "Antropología Filosófica" de un personaje: título, concepto filosófico que encarna,
    // historia (lore), reflexión antropológica y el dilema que plantea.
    // Cada texto se guarda en español y en inglés; las propiedades devuelven el del idioma elegido.
    public class FichaLore
    {
        private string titulo, concepto, historia, reflexion, dilema;
        private string tituloIngles, conceptoIngles, historiaIngles, reflexionIngles, dilemaIngles;

        public FichaLore(string titulo, string concepto, string historia, string reflexion, string dilema,
                         string tituloIngles, string conceptoIngles, string historiaIngles, string reflexionIngles, string dilemaIngles)
        {
            this.titulo = titulo;
            this.concepto = concepto;
            this.historia = historia;
            this.reflexion = reflexion;
            this.dilema = dilema;
            this.tituloIngles = tituloIngles;
            this.conceptoIngles = conceptoIngles;
            this.historiaIngles = historiaIngles;
            this.reflexionIngles = reflexionIngles;
            this.dilemaIngles = dilemaIngles;
        }

        public string Titulo
        {
            get { return Idioma.Texto(titulo, tituloIngles); }
        }

        public string Concepto
        {
            get { return Idioma.Texto(concepto, conceptoIngles); }
        }

        public string Historia
        {
            get { return Idioma.Texto(historia, historiaIngles); }
        }

        public string Reflexion
        {
            get { return Idioma.Texto(reflexion, reflexionIngles); }
        }

        public string Dilema
        {
            get { return Idioma.Texto(dilema, dilemaIngles); }
        }
    }

    // Catálogo de lore del juego (igual que CatalogoBase, todo en memoria). Cada facción aborda un eje antropológico:
    //  - Fuego: pasión, voluntad de poder y autodestrucción.
    //  - Agua: alteridad, empatía y ética del cuidado.
    //  - Tierra: trascendencia, orden y resistencia ante la finitud.
    //  - Aire: libertad, contingencia y búsqueda de sentido.
    // Los textos son una lectura propia del juego a partir de autores clásicos: sirven de punto de partida, no sustituyen
    // la lectura de las obras citadas.
    public static class LorePersonajes
    {
        // Dos listas paralelas: la ficha número i es del personaje cuyo nombre está en la posición i
        // (el nombre es el mismo que en CatalogoBase).
        private static List<string> nombresFichas = new List<string>();
        private static List<FichaLore> fichas = new List<FichaLore>();

        // Se llama una sola vez al iniciar el programa (desde Main) para llenar las fichas.
        public static void Cargar()
        {
            CargarFuego();
            CargarAgua();
            CargarTierra();
            CargarAire();
        }

        // ---------- Consultas ----------

        // La ficha completa del personaje, o una ficha vacía si todavía no tiene lore escrito.
        public static FichaLore ObtenerFicha(Personaje p)
        {
            if (p != null)
            {
                for (int i = 0; i < nombresFichas.Count; i++)
                {
                    if (nombresFichas[i] == p.Clave) return fichas[i];
                }
            }
            return new FichaLore("Sin registro", "—", "Todavía no hay una historia escrita para este personaje.",
                                 "Todavía no hay una reflexión escrita para este personaje.", "—",
                                 "No record", "—", "There is no story written for this character yet.",
                                 "There is no reflection written for this character yet.", "—");
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

        // El eje antropológico (título corto) de cada facción.
        public static string EjeDe(Elemento elemento)
        {
            if (elemento == Elemento.Fuego)
                return Idioma.Texto("Pasión, voluntad de poder y autodestrucción", "Passion, will to power and self-destruction");
            if (elemento == Elemento.Agua)
                return Idioma.Texto("Alteridad, empatía y ética del cuidado", "Otherness, empathy and ethics of care");
            if (elemento == Elemento.Tierra)
                return Idioma.Texto("Trascendencia, orden y resistencia ante la finitud", "Transcendence, order and resistance in the face of finitude");
            return Idioma.Texto("Libertad, contingencia y búsqueda de sentido", "Freedom, contingency and the search for meaning");
        }

        // Explicación breve del eje de la facción: de qué trata y qué pregunta deja abierta.
        public static string ExplicacionDelEje(Elemento elemento)
        {
            if (elemento == Elemento.Fuego)
            {
                return Idioma.Texto(
                    "El fuego solo existe consumiendo: para arder agota su materia. Es el elemento de la voluntad (la «voluntad de " +
                    "poder» de Nietzsche, el querer ciego de Schopenhauer) y de su finitud: lo que más afirma la vida es lo que más " +
                    "rápido la gasta.\n¿Es la pasión plenitud o una forma acelerada de dejar de estar vivo?",
                    "Fire only exists by consuming: to burn it exhausts its matter. It is the element of the will (Nietzsche's «will to " +
                    "power», Schopenhauer's blind wanting) and of its finitude: what most affirms life is what spends it fastest." +
                    "\nIs passion fulfillment or an accelerated way of ceasing to be alive?");
            }
            if (elemento == Elemento.Agua)
            {
                return Idioma.Texto(
                    "El agua toma la forma de lo que la contiene: solo existe en relación con otro. Es el elemento de la alteridad: " +
                    "el rostro del Otro me hace responsable (Levinas), el «Yo-Tú» me constituye (Buber) y la ética del cuidado atiende " +
                    "a quien tengo delante.\n¿Hasta dónde llega mi responsabilidad sin perderme a mí mismo?",
                    "Water takes the shape of what contains it: it only exists in relation to another. It is the element of otherness: " +
                    "the face of the Other makes me responsible (Levinas), the «I-Thou» constitutes me (Buber) and the ethics of care " +
                    "attends to whoever is in front of me.\nHow far does my responsibility go without losing myself?");
            }
            if (elemento == Elemento.Tierra)
            {
                return Idioma.Texto(
                    "La tierra es lo que permanece: cimientos, montañas, tumbas. Responde a la finitud con orden y obra, frente al " +
                    "ser-para-la-muerte de Heidegger y el hambre de inmortalidad de Unamuno.\n¿Resistir a la muerte es trascenderla " +
                    "o la forma más antigua de negarla?",
                    "Earth is what remains: foundations, mountains, tombs. It answers finitude with order and work, against " +
                    "Heidegger's being-toward-death and Unamuno's hunger for immortality.\nIs resisting death a way of transcending " +
                    "it or the oldest way of denying it?");
            }
            return Idioma.Texto(
                "El aire no tiene forma ni lugar fijo: es movimiento y posibilidad. Es el elemento de la libertad y su vértigo: " +
                "Sartre nos condena a ser libres, Camus ve el absurdo y Frankl responde con la voluntad de sentido.\n" +
                "Cuando nada nos da un sentido hecho, ¿la libertad es plenitud o desamparo?",
                "Air has no fixed form or place: it is movement and possibility. It is the element of freedom and its vertigo: " +
                "Sartre condemns us to be free, Camus sees the absurd and Frankl answers with the will to meaning.\n" +
                "When nothing gives us a ready-made meaning, is freedom fullness or abandonment?");
        }

        // Autores y obras para profundizar en el eje de cada facción.
        public static string[] LecturasDe(Elemento elemento)
        {
            if (Idioma.Actual == Idioma.Ingles) return LecturasEnIngles(elemento);

            if (elemento == Elemento.Fuego)
            {
                return new string[]
                {
                    "Nietzsche — Así habló Zaratustra",
                    "Schopenhauer — El mundo como voluntad y representación",
                    "Heráclito — fragmentos sobre el fuego y el devenir",
                    "Freud — Más allá del principio del placer",
                    "Bataille — La parte maldita",
                    "Bachelard — El psicoanálisis del fuego · Vernant — Mito y pensamiento en la Grecia antigua (Hestia)",
                    "Platón — Protágoras (Prometeo) · Hegel — Fenomenología del espíritu (Faetón)",
                    "Esquilo — Prometeo encadenado · Ovidio — Metamorfosis (Faetón)"
                };
            }
            if (elemento == Elemento.Agua)
            {
                return new string[]
                {
                    "Levinas — Totalidad e infinito",
                    "Buber — Yo y Tú",
                    "Edith Stein — Sobre el problema de la empatía",
                    "Gilligan — In a Different Voice (1982) · Noddings — Caring (1984)",
                    "Ricoeur — Sí mismo como otro",
                    "Homero — Ilíada y Odisea (Tetis, Poseidón) · Simone Weil — La Ilíada o el poema de la fuerza",
                    "Max Scheler — Esencia y formas de la simpatía (Anfitrite)"
                };
            }
            if (elemento == Elemento.Tierra)
            {
                return new string[]
                {
                    "Heidegger — Ser y tiempo",
                    "Spinoza — Ética (el conatus)",
                    "Unamuno — Del sentimiento trágico de la vida",
                    "Platón — Fedón · Arendt — La condición humana",
                    "Pico della Mirandola — Discurso sobre la dignidad del hombre",
                    "Hesíodo — Teogonía (Atlas) · Apolonio de Rodas — Argonáuticas (Talos)",
                    "Richard Sennett — El artesano · Hannah Arendt — La condición humana (Hefesto)"
                };
            }
            return new string[]
            {
                "Sartre — El existencialismo es un humanismo",
                "Kierkegaard — El concepto de la angustia",
                "Camus — El mito de Sísifo",
                "Frankl — El hombre en busca de sentido",
                "Aristóteles — Metafísica, libro I (el asombro)",
                    "Ovidio — Metamorfosis (Atalanta) · Apuleyo — El asno de oro (Psique)",
                    "Camus — El hombre rebelde (Tifón) · Beauvoir — Para una moral de la ambigüedad (Atalanta)"
            };
        }

        // Las mismas lecturas con los títulos de las obras en inglés.
        private static string[] LecturasEnIngles(Elemento elemento)
        {
            if (elemento == Elemento.Fuego)
            {
                return new string[]
                {
                    "Nietzsche — Thus Spoke Zarathustra",
                    "Schopenhauer — The World as Will and Representation",
                    "Heraclitus — fragments on fire and becoming",
                    "Freud — Beyond the Pleasure Principle",
                    "Bataille — The Accursed Share",
                    "Bachelard — The Psychoanalysis of Fire · Vernant — Myth and Thought among the Greeks (Hestia)",
                    "Plato — Protagoras (Prometheus) · Hegel — Phenomenology of Spirit (Phaethon)",
                    "Aeschylus — Prometheus Bound · Ovid — Metamorphoses (Phaethon)"
                };
            }
            if (elemento == Elemento.Agua)
            {
                return new string[]
                {
                    "Levinas — Totality and Infinity",
                    "Buber — I and Thou",
                    "Edith Stein — On the Problem of Empathy",
                    "Gilligan — In a Different Voice (1982) · Noddings — Caring (1984)",
                    "Ricoeur — Oneself as Another",
                    "Homer — Iliad and Odyssey (Thetis, Poseidon) · Simone Weil — The Iliad, or the Poem of Force",
                    "Max Scheler — The Nature of Sympathy (Amphitrite)"
                };
            }
            if (elemento == Elemento.Tierra)
            {
                return new string[]
                {
                    "Heidegger — Being and Time",
                    "Spinoza — Ethics (the conatus)",
                    "Unamuno — The Tragic Sense of Life",
                    "Plato — Phaedo · Arendt — The Human Condition",
                    "Pico della Mirandola — Oration on the Dignity of Man",
                    "Hesiod — Theogony (Atlas) · Apollonius of Rhodes — Argonautica (Talos)",
                    "Richard Sennett — The Craftsman · Hannah Arendt — The Human Condition (Hephaestus)"
                };
            }
            return new string[]
            {
                "Sartre — Existentialism Is a Humanism",
                "Kierkegaard — The Concept of Anxiety",
                "Camus — The Myth of Sisyphus",
                "Frankl — Man's Search for Meaning",
                "Aristotle — Metaphysics, Book I (wonder)",
                    "Ovid — Metamorphoses (Atalanta) · Apuleius — The Golden Ass (Psyche)",
                    "Camus — The Rebel (Typhon) · Beauvoir — The Ethics of Ambiguity (Atalanta)"
            };
        }
        // ---------- Textos de cada personaje ----------

        // Primero van los cinco textos en español y después los mismos cinco en inglés.
        private static void Agregar(string nombre, string titulo, string concepto, string historia, string reflexion, string dilema,
                                    string tituloIngles, string conceptoIngles, string historiaIngles, string reflexionIngles, string dilemaIngles)
        {
            nombresFichas.Add(nombre);
            fichas.Add(new FichaLore(titulo, concepto, historia, reflexion, dilema,
                                     tituloIngles, conceptoIngles, historiaIngles, reflexionIngles, dilemaIngles));
        }

        private static void CargarFuego()
        {
            Agregar("Hestia", "La Espada Encendida", "La voluntad de poder y el fuego que centra",
                "Los antiguos cuentan que Hestia fue la primera hija de Crono en ser devorada y la última en volver a la luz. " +
                "Quizá por eso aprendió a quedarse quieta mientras el mundo ardía a su alrededor: en el centro de cada casa, de " +
                "cada ciudad y de cada sacrificio había un hogar, y el hogar era ella. Cuando los demás dioses se disputaron " +
                "tronos, Hestia cedió el suyo y eligió el fuego que no se mueve. Pero el fuego, aunque calme, también quiere " +
                "crecer. Se dice que la noche en que el mundo se quedó sin hogares forjó una espada con la llama del primer " +
                "altar, y que por eso no la apaga ni el agua ni el miedo: la Espada Encendida no conquista, defiende el centro.",
                "Hestia es la voluntad de poder entendida al revés. Nietzsche llamó «voluntad de poder» al impulso de la vida por " +
                "crecer, superarse y dar forma, y solemos imaginarla con espadas y conquistas; pero Hestia, la más serena de los " +
                "olímpicos, ejerce el poder más antiguo: el de sostener un centro. Para Jean-Pierre Vernant, en el pensamiento " +
                "griego Hestia es lo fijo, el ombligo del espacio que ordena la casa y la ciudad, mientras que Hermes es el " +
                "movimiento y el cruce de caminos. Un ser humano necesita las dos cosas: un lugar al que volver y la fuerza para " +
                "salir.\n\nEl fuego es además el elemento de la ambigüedad. Gaston Bachelard mostró que el fuego es a la vez " +
                "calor íntimo y destrucción, sueño de hogar y sueño de incendio; Heráclito lo hizo principio del mundo, porque " +
                "todo se transforma y nada permanece idéntico. Schopenhauer le advertiría a Hestia que querer sin descanso es la " +
                "raíz del sufrimiento: la llama que se alimenta de sí misma nunca se sacia. Si la voluntad es solo impulso, somos " +
                "su instrumento; si aprende a custodiarse como el fuego del hogar, se convierte en libertad.\n\nPor eso Hestia " +
                "pelea distinto al resto. Su Llamarada golpea con enorme fuerza e ignora parte de la defensa, pero su Aura Ígnea " +
                "no consume: potencia. En ella la pasión no se gasta, se cuida. Y la pregunta antropológica queda abierta: ¿es el " +
                "poder más verdadero el que arrasa o el que se mantiene encendido cuando todos se han ido?",
                "Si quiero algo con todas mis fuerzas, ¿decido yo lo que quiero o solo obedezco a un impulso que no elegí?",
                "The Blazing Sword", "The will to power and the fire that centers",
                "The ancients say Hestia was the first daughter of Cronus to be devoured and the last to return to the light. " +
                "Perhaps that is why she learned to stay still while the world burned around her: at the center of every house, " +
                "every city and every sacrifice there was a hearth, and the hearth was her. When the other gods fought over " +
                "thrones, Hestia gave up hers and chose the fire that does not move. But fire, even when it soothes, also wants " +
                "to grow. It is said that on the night the world was left without hearths she forged a sword from the flame of " +
                "the first altar, and that is why neither water nor fear can put it out: the Blazing Sword does not conquer, it " +
                "defends the center.",
                "Hestia is the will to power understood in reverse. Nietzsche gave the name «will to power» to life's drive to " +
                "grow, overcome itself and give form, and we tend to picture it with swords and conquests; but Hestia, the " +
                "calmest of the Olympians, wields the oldest power: that of sustaining a center. For Jean-Pierre Vernant, in " +
                "Greek thought Hestia is the fixed, the navel of space that orders the house and the city, while Hermes is " +
                "movement and the crossroads. A human being needs both: a place to return to and the strength to leave.\n\nFire " +
                "is also the element of ambiguity. Gaston Bachelard showed that fire is at once intimate warmth and destruction, " +
                "the dream of home and the dream of conflagration; Heraclitus made it the principle of the world, because " +
                "everything transforms and nothing stays identical. Schopenhauer would warn Hestia that wanting without rest is " +
                "the root of suffering: the flame that feeds on itself is never sated. If the will is mere impulse, we are its " +
                "instrument; if it learns to keep itself like the fire of the hearth, it becomes freedom.\n\nThat is why Hestia " +
                "fights differently from the rest. Her Blaze strikes with enormous force and ignores part of the defense, but her " +
                "Fiery Aura does not consume: it empowers. In her, passion is not spent, it is tended. And the anthropological " +
                "question remains open: is the truest power the one that sweeps everything away or the one that stays lit when " +
                "everyone else has gone?",
                "If I want something with all my strength, do I decide what I want, or do I only obey an impulse I did not " +
                "choose?");

            Agregar("Prometeo", "El Aprendiz de la Llama", "La técnica y la ambivalencia de la pasión",
                "Prometeo, «el que piensa antes», es un titán joven que entendió algo que los dioses olvidaron: los humanos " +
                "habían sido repartidos sin garras, sin pelaje y sin colmillos, y el fuego era lo único que podía compensarlo. Lo " +
                "escondió en el hueco de una caña y lo bajó del Olimpo bajo su capa roja. Zeus lo encadenó a una roca del " +
                "Cáucaso, donde un águila le devoraba el hígado cada día y cada noche volvía a crecer. Desde entonces camina por " +
                "la Arena con una pequeña llama en la palma de la mano, que no dejó de cuidar ni siquiera cuando le costó la " +
                "libertad: sabe que quien regala el fuego responde por todo lo que ese fuego queme.",
                "Prometeo es el mito griego de la técnica y del origen de la cultura. En el Protágoras de Platón, Epimeteo («el " +
                "que piensa después») repartió las cualidades entre los animales y dejó al ser humano desnudo; Prometeo roba el " +
                "fuego y las artes para compensar esa carencia. La antropología moderna dice algo parecido: Arnold Gehlen " +
                "describe al ser humano como un «ser carencial», sin instintos especializados, que sobrevive porque fabrica " +
                "cultura. Somos el animal que no está terminado y que, por eso, tiene que inventarse.\n\nPero el don tiene un " +
                "precio. En el Prometeo encadenado de Esquilo, el titán confiesa que hizo que los mortales dejaran de ver de " +
                "antemano su muerte y les dio a cambio «ciegas esperanzas»: la técnica y la esperanza nacen juntas, como consuelo " +
                "ante la finitud. Heráclito recordaba que el fuego vive de lo que consume, y la historia posterior (Pandora, la " +
                "caja, las guerras) muestra que la llama que ilumina también puede incendiar. José Ortega y Gasset diría que la " +
                "técnica no es un capricho sino el modo de hacer posible una vida que la naturaleza, por sí sola, no " +
                "ofrece.\n\nEl Tajo Carmesí de Prometeo golpea ignorando casi toda la defensa, y su Grito de Guerra potencia su " +
                "propio ataque: la pasión como herramienta y como contagio. La tensión que deja abierta es la de toda la " +
                "civilización: ¿debemos moderar el fuego para no destruirnos o aceptar que sin riesgo no hay humanidad?",
                "¿Debo apagar lo que me quema para no dañar a nadie, o aprender a arder sin consumir lo que amo?",
                "The Apprentice of the Flame", "Technology and the ambivalence of passion",
                "Prometheus, «he who thinks ahead», is a young Titan who understood something the gods had forgotten: humans had " +
                "been handed out without claws, without fur and without fangs, and fire was the only thing that could make up for " +
                "it. He hid it in the hollow of a reed and carried it down from Olympus under his red cape. Zeus chained him to a " +
                "rock in the Caucasus, where an eagle devoured his liver each day and each night it grew back. Since then he " +
                "walks the Arena with a small flame in the palm of his hand, which he never stopped tending even when it cost him " +
                "his freedom: he knows that whoever gives away fire answers for everything that fire burns.",
                "Prometheus is the Greek myth of technology and of the origin of culture. In Plato's Protagoras, Epimetheus («he " +
                "who thinks afterwards») distributed qualities among the animals and left the human being naked; Prometheus " +
                "steals fire and the arts to make up for that lack. Modern anthropology says something similar: Arnold Gehlen " +
                "describes the human being as a «deficient being», without specialized instincts, who survives because he makes " +
                "culture. We are the unfinished animal, and for that reason we have to invent ourselves.\n\nBut the gift has a " +
                "price. In Aeschylus's Prometheus Bound, the Titan confesses that he made mortals stop foreseeing their death and " +
                "gave them «blind hopes» in exchange: technology and hope are born together, as a consolation in the face of " +
                "finitude. Heraclitus reminded us that fire lives on what it consumes, and later history (Pandora, the jar, the " +
                "wars) shows that the flame that illuminates can also set things ablaze. José Ortega y Gasset would say that " +
                "technology is not a whim but the way to make possible a life that nature alone does not offer.\n\nPrometheus's " +
                "Crimson Slash strikes while ignoring almost all of the defense, and his War Cry boosts his own attack: passion " +
                "as a tool and as contagion. The tension it leaves open is that of all civilization: should we moderate the fire " +
                "so as not to destroy ourselves, or accept that without risk there is no humanity?",
                "Should I put out what burns me so I harm no one, or learn to burn without consuming what I love?");

            Agregar("Faetón", "El Zorro que Arde", "La autodestrucción y el gasto",
                "Faetón era hijo de Helios, el dios que conduce el carro del Sol, y creció oyendo burlas sobre su origen. Para " +
                "demostrar quién era, pidió a su padre un solo día al mando del carro; Helios, atado por un juramento sobre la " +
                "laguna Estigia, no pudo negárselo. Los caballos notaron que las riendas eran de una mano ligera y se desbocaron: " +
                "el cielo se quemó, las tierras se volvieron desierto y un rayo de Zeus lo derribó al río Erídano. Sus hermanas, " +
                "las Helíades, lloraron tanto que se convirtieron en álamos y sus lágrimas en ámbar. De aquel incendio quedó un " +
                "zorro pequeño de cola encendida que ríe, juega con su fuego y nunca aprendió a soltarlo: la Arena lo quiere " +
                "tanto como teme por él.",
                "Faetón es el mito del deseo de reconocimiento. No quería poder por ambición sino una prueba: que alguien lo " +
                "mirara y le dijera «eres mío». Hegel pensaba que la conciencia solo se vuelve humana cuando otra la reconoce, y " +
                "el joven del carro apuesta su vida a ese gesto. Ovidio cuenta que Helios intenta disuadirlo y que el muchacho, " +
                "sin saber medir, insiste: el que más necesita ser visto es el que menos mide el precio de serlo.\n\nFreud llamó " +
                "pulsión de muerte a esa tendencia a repetir y a descargar hasta que no queda nada; Georges Bataille sostuvo en " +
                "La parte maldita que toda vida acumula energía de sobra y que su destino es gastarla: en fiesta, en sacrificio o " +
                "en catástrofe. Faetón es ese excedente sin cauce. Su caída es una tragedia, pero también una belleza: del " +
                "incendio nace el ámbar, lo que el dolor deja como joya. El mito sugiere que el gasto es inevitable; lo que " +
                "cambia es si lo convertimos en destrucción o en obra.\n\nSu Explosión de Brasas golpea con más fuerza que " +
                "cualquier otra técnica, pero le devuelve parte del daño: el retroceso es su carro desbocado. Y su Combustión " +
                "Interna potencia su ataque a costa de dejarlo sin respiro. La pregunta que deja es incómoda: cuando sabemos que " +
                "arder nos consume, ¿seguir ardiendo es temeridad o es la única manera de sentirse vivo?",
                "Si sé que arder me consume, ¿arder igual es valentía, ingenuidad o la única manera de estar de verdad vivo?",
                "The Fox That Burns", "Self-destruction and expenditure",
                "Phaethon was the son of Helios, the god who drives the chariot of the Sun, and he grew up hearing mockery about " +
                "his origins. To prove who he was, he asked his father for a single day at the reins; Helios, bound by an oath " +
                "sworn on the river Styx, could not refuse him. The horses sensed that the reins were held by a light hand and " +
                "bolted: the sky burned, the lands turned to desert and a thunderbolt from Zeus struck him down into the river " +
                "Eridanus. His sisters, the Heliades, wept so much that they turned into poplars and their tears into amber. From " +
                "that blaze a small fox with a burning tail remained, who laughs, plays with his fire and never learned to let it " +
                "go: the Arena loves him as much as it fears for him.",
                "Phaethon is the myth of the desire for recognition. He did not want power out of ambition but proof: that " +
                "someone would look at him and say «you are mine». Hegel thought that consciousness only becomes human when " +
                "another consciousness recognizes it, and the boy of the chariot stakes his life on that gesture. Ovid tells that " +
                "Helios tries to dissuade him and that the boy, unable to measure, insists: whoever most needs to be seen is the " +
                "one who least weighs the price of it.\n\nFreud called death drive that tendency to repeat and to discharge until " +
                "nothing is left; Georges Bataille argued in The Accursed Share that every life accumulates surplus energy and " +
                "that its destiny is to spend it: in feast, in sacrifice or in catastrophe. Phaethon is that surplus with no " +
                "channel. His fall is a tragedy, but also a beauty: amber is born from the blaze, what pain leaves behind as a " +
                "jewel. The myth suggests that expenditure is inevitable; what changes is whether we turn it into destruction or " +
                "into a work.\n\nHis Ember Blast hits harder than any other technique, but returns part of the damage to him: the " +
                "recoil is his runaway chariot. And his Inner Combustion boosts his attack at the cost of leaving him no " +
                "breathing room. The question it leaves is uncomfortable: when we know that burning consumes us, is going on " +
                "burning recklessness or the only way to feel alive?",
                "If I know that burning consumes me, is burning anyway courage, naivety, or the only way to be truly alive?");
        }

        private static void CargarAgua()
        {
            Agregar("Tetis", "La Portadora del Cristal", "La responsabilidad ante el rostro del Otro",
                "Tetis es una nereida, hija del mar antiguo, tan hermosa que Zeus y Poseidón la desearon hasta que una profecía " +
                "reveló que su hijo sería más poderoso que su padre. Entonces la entregaron a un mortal, Peleo, que tuvo que " +
                "sujetarla mientras ella cambiaba de forma: fuego, agua, león, serpiente. Fue madre de Aquiles; lo sumergió en la " +
                "laguna Estigia sosteniéndolo del talón y, aunque supo desde el principio que viviría poco, nunca dejó de " +
                "acompañarlo: salió del mar a consolarlo, pidió a Hefesto unas armas para él y lloró por su muerte antes de que " +
                "ocurriera. Hoy recorre la Arena con un báculo de cristal que solo brilla cuando alguien cerca sufre.",
                "Tetis es la figura del cuidado que sabe de antemano que va a perder. En la Ilíada, cada vez que Aquiles llora, " +
                "su madre sale del fondo del mar a sentarse a su lado: no puede evitar su destino, pero no lo deja solo. Emmanuel " +
                "Levinas decía que la ética comienza en el rostro del otro, cuya fragilidad me interpela antes de que yo decida " +
                "nada; Tetis responde a ese llamado aun sabiendo que no tiene medida.\n\nLa ética del cuidado, desarrollada por " +
                "Carol Gilligan y Nel Noddings, defiende algo parecido: que la moral no consiste solo en aplicar reglas, sino en " +
                "atender a quien tengo delante y a la red de relaciones que nos sostiene. Simone Weil, en su lectura de la " +
                "Ilíada, ve en el poema la fuerza que convierte a los seres en cosas, y en gestos como el de una madre que " +
                "acompaña lo único que resiste esa reducción. Y Paul Ricoeur recordaría que Tetis cambia de forma sin dejar de " +
                "ser ella: la identidad es capaz de transformarse y de mantener su palabra («sí mismo como otro»).\n\nSu Chorro a " +
                "Presión y su Marea Curativa expresan las dos caras de esa figura: el agua que golpea cuando hay que defender y " +
                "el agua que sana cuando hay que acompañar; y su empatía, una de las más altas del juego, hace que cada curación " +
                "valga más. Queda una pregunta que Levinas no cierra: si el otro me obliga antes de que yo elija, ¿cuánto de mí " +
                "me queda para cuidarme?",
                "Si el rostro del otro me obliga antes de que yo decida, ¿cuánta libertad me queda para cuidarme a mí misma?",
                "The Crystal Bearer", "Responsibility before the face of the Other",
                "Thetis is a Nereid, daughter of the ancient sea, so beautiful that Zeus and Poseidon desired her until a " +
                "prophecy revealed that her son would be mightier than his father. So they gave her to a mortal, Peleus, who had " +
                "to hold her down while she changed shape: fire, water, lion, serpent. She became the mother of Achilles; she " +
                "dipped him in the river Styx holding him by the heel and, although she knew from the start that he would live a " +
                "short life, she never stopped accompanying him: she rose from the sea to console him, asked Hephaestus for armor " +
                "for him and mourned his death before it happened. Today she roams the Arena with a crystal staff that only " +
                "shines when someone nearby is suffering.",
                "Thetis is the figure of care that knows in advance that it will lose. In the Iliad, every time Achilles weeps, " +
                "his mother rises from the depths of the sea to sit beside him: she cannot prevent his fate, but she does not " +
                "leave him alone. Emmanuel Levinas said that ethics begins in the face of the other, whose fragility calls me " +
                "before I decide anything; Thetis answers that call even knowing that it has no measure.\n\nThe ethics of care, " +
                "developed by Carol Gilligan and Nel Noddings, defends something similar: that morality does not consist only in " +
                "applying rules, but in attending to whoever is in front of me and to the web of relationships that sustains us. " +
                "Simone Weil, in her reading of the Iliad, sees in the poem the force that turns beings into things, and in " +
                "gestures like that of a mother who keeps company the only thing that resists that reduction. And Paul Ricoeur " +
                "would remind us that Thetis changes shape without ceasing to be herself: identity is able to transform and to " +
                "keep its word («oneself as another»).\n\nHer Pressure Jet and her Healing Tide express the two faces of that " +
                "figure: the water that strikes when one must defend and the water that heals when one must accompany; and her " +
                "empathy, one of the highest in the game, makes every heal worth more. A question remains that Levinas does not " +
                "close: if the other obliges me before I choose, how much of me is left to take care of myself?",
                "If the face of the other obliges me before I decide, how much freedom do I have left to take care of myself?");

            Agregar("Poseidón", "El Caballero del Tridente", "El cuidado frente al deber",
                "Poseidón es el que sacude la tierra: dios de los mares, de los caballos y de los terremotos, hermano de Zeus, " +
                "dueño de un tridente que levanta tormentas o las aquieta. Disputó con Atenea la ciudad de Atenas: él clavó el " +
                "tridente y brotó agua salada; ella ofreció un olivo y ganó. Su fama es la de un guardián terco: protege a los " +
                "marineros que le rezan y castiga sin piedad a quien ofende a los suyos. Cuando Odiseo dejó ciego a Polifemo, su " +
                "hijo, Poseidón lo persiguió durante diez años por el mar. En la Arena cuida el último puerto con la disciplina " +
                "de un caballero y la memoria de un dios herido.",
                "Poseidón encarna el conflicto entre el deber y el cuidado. Su lealtad hacia los suyos es su virtud y su condena: " +
                "en la Odisea, su cólera contra Odiseo no nace de la maldad sino del amor a un hijo ofendido. Ahí aparece una " +
                "verdad incómoda: cuidar de los míos puede convertirse en la forma más eficaz de ser cruel con los demás. " +
                "Immanuel Kant lo formularía así: el ser humano debe ser tratado siempre como un fin y nunca solo como un medio; " +
                "el cuidado que usa al otro como instrumento deja de ser moral.\n\nMartin Buber distinguía entre la relación " +
                "Yo-Tú, donde me encuentro con el otro como persona, y la relación Yo-Ello, donde lo trato como cosa. En la " +
                "guerra, el adversario se vuelve un Ello. Levinas añade la figura del «tercero»: siempre hay alguien más mirando, " +
                "y esa presencia obliga a la justicia, es decir, a comparar y a no favorecer solo a los cercanos. La cólera de " +
                "Poseidón es lo que ocurre cuando el amor olvida al tercero.\n\nSu Oleada Voraz le roba vida al rival para " +
                "sostenerse y su Oleaje Reparador restaura a los suyos: cuidar y quitar son un mismo movimiento de la marea. La " +
                "tensión queda sin resolver: ¿puede un guardián proteger a los suyos sin convertir al adversario en un simple " +
                "medio?",
                "¿Puedo cuidar a los míos sin convertir al adversario en un simple medio para lograrlo?",
                "The Knight of the Trident", "Care versus duty",
                "Poseidon is the Earth-Shaker: god of the seas, of horses and of earthquakes, brother of Zeus, owner of a trident " +
                "that raises storms or calms them. He disputed the city of Athens with Athena: he struck his trident and salt " +
                "water gushed forth; she offered an olive tree and won. His fame is that of a stubborn guardian: he protects the " +
                "sailors who pray to him and punishes without pity whoever offends his own. When Odysseus blinded Polyphemus, his " +
                "son, Poseidon pursued him across the sea for ten years. In the Arena he guards the last harbor with the " +
                "discipline of a knight and the memory of a wounded god.",
                "Poseidon embodies the conflict between duty and care. His loyalty to his own is his virtue and his curse: in the " +
                "Odyssey, his wrath against Odysseus is born not of malice but of love for an offended son. Here an uncomfortable " +
                "truth appears: caring for my own can become the most efficient way of being cruel to everyone else. Immanuel " +
                "Kant would put it this way: the human being must always be treated as an end and never merely as a means; care " +
                "that uses the other as an instrument ceases to be moral.\n\nMartin Buber distinguished between the I-Thou " +
                "relation, where I meet the other as a person, and the I-It relation, where I treat them as a thing. In war, the " +
                "adversary becomes an It. Levinas adds the figure of the «third»: there is always someone else watching, and that " +
                "presence demands justice, that is, comparing and not favoring only those close to us. Poseidon's wrath is what " +
                "happens when love forgets the third.\n\nHis Ravenous Surge steals life from the rival to sustain himself and his " +
                "Restoring Swell restores his own: caring and taking are one and the same movement of the tide. The tension " +
                "remains unresolved: can a guardian protect his own without turning the adversary into a mere means?",
                "Can I take care of my own without turning the adversary into a mere means to do so?");

            Agregar("Anfitrite", "La Sirena de la Burbuja", "La empatía y la distancia",
                "Anfitrite es una nereida y la reina de las profundidades. Cuando Poseidón la pretendió, huyó hasta los confines " +
                "del océano, donde el titán Atlas sostiene el cielo, y no volvió hasta que un delfín la convenció de que su " +
                "consentimiento importaba; Poseidón, agradecido, puso al delfín entre las estrellas. Gobierna a las criaturas del " +
                "mar con una calma que no viene del poder sino de saber escuchar. En la Arena vive dentro de una esfera de agua " +
                "que ella misma sostiene: desde allí observa a todos, se aprende sus voces y se hace querer, pero nadie sabe " +
                "dónde está su hogar. Cuando el equipo flaquea, la burbuja se vuelve lluvia.",
                "En el mito, el delfín es un mediador: no obliga ni convence por la fuerza, traduce. Anfitrite es la figura de la " +
                "empatía que respeta la distancia. Edith Stein, discípula de Husserl, definió la empatía (Einfühlung) como la " +
                "experiencia de la vivencia ajena en cuanto ajena: sé lo que sientes sin sentirlo como lo sientes tú. Si la " +
                "distancia desaparece, el otro se convierte en un reflejo mío; si es demasiado grande, no lo alcanzo.\n\nMax " +
                "Scheler añadió que la verdadera simpatía no es contagio emocional: cuando lloro porque lloras no te ayudo, " +
                "simplemente me uno al dolor. Entender sin confundirse es la condición del cuidado. Y es también una enseñanza " +
                "sobre la alteridad: Anfitrite le dice que no a Poseidón hasta que su voluntad es respetada, porque la relación " +
                "con el otro empieza por reconocer que el otro puede decir que no. La burbuja es entonces un límite sano y, a la " +
                "vez, el riesgo de quedarse siempre mirando desde lejos.\n\nSu Torrente golpea dos veces y le devuelve energía; " +
                "su Lluvia Sanadora cura a todo el equipo con su empatía, la más alta de la Arena. Pero la pregunta sigue en pie: " +
                "¿puedo comprender de verdad lo que vive alguien a quien veo solo a través de mi propia burbuja?",
                "¿Puedo comprender de verdad lo que vive alguien a quien solo veo a través de mi propia burbuja?",
                "The Mermaid of the Bubble", "Empathy and distance",
                "Amphitrite is a Nereid and queen of the depths. When Poseidon courted her, she fled to the edge of the ocean, " +
                "where the Titan Atlas holds up the sky, and did not return until a dolphin convinced her that her consent " +
                "mattered; Poseidon, grateful, placed the dolphin among the stars. She rules the creatures of the sea with a calm " +
                "that comes not from power but from knowing how to listen. In the Arena she lives inside a sphere of water that " +
                "she herself sustains: from there she watches everyone, learns their voices and makes herself loved, but nobody " +
                "knows where her home is. When the team falters, the bubble turns into rain.",
                "In the myth, the dolphin is a mediator: he neither obliges nor persuades by force, he translates. Amphitrite is " +
                "the figure of empathy that respects distance. Edith Stein, a student of Husserl, defined empathy (Einfühlung) as " +
                "the experience of another's experience as another's: I know what you feel without feeling it the way you feel " +
                "it. If the distance disappears, the other becomes a reflection of me; if it is too great, I do not reach " +
                "them.\n\nMax Scheler added that true sympathy is not emotional contagion: when I cry because you cry I do not " +
                "help you, I simply join in the pain. Understanding without becoming confused is the condition of care. And it is " +
                "also a lesson about otherness: Amphitrite says no to Poseidon until her will is respected, because the " +
                "relationship with the other begins by recognizing that the other can say no. The bubble is then a healthy " +
                "boundary and, at the same time, the risk of always watching from afar.\n\nHer Torrent strikes twice and returns " +
                "energy to her; her Healing Rain heals the whole team through her empathy, the highest in the Arena. But the " +
                "question remains: can I truly understand what someone lives whom I see only through my own bubble?",
                "Can I truly understand what someone lives whom I see only through my own bubble?");
        }

        private static void CargarTierra()
        {
            Agregar("Atlas", "La Raíz Antigua", "El peso del mundo y el ser-para-la-muerte",
                "Atlas es un titán de la primera estirpe: luchó contra los dioses y perdió, y Zeus lo condenó a sostener la " +
                "bóveda del cielo sobre sus hombros en el extremo occidental del mundo. Una vez Heracles le pidió que le trajera " +
                "las manzanas de oro de las Hespérides mientras él cargaba el cielo; Atlas volvió convencido de que por fin " +
                "podría soltarlo y tuvo que volver a su carga, engañado. Cuando Perseo le mostró la cabeza de Medusa, se volvió " +
                "piedra y montaña. En la Arena es un anciano de manto remendado con hojas que apoya su bastón como quien apoya un " +
                "pilar del cielo: habla poco, casi siempre en preguntas, y nunca se apura.",
                "Atlas es el rostro de la finitud como carga. No puede soltar el cielo, y el mito muestra a un ser que encarna la " +
                "resistencia: Albert Camus escribió sobre Sísifo, el hombre que empuja una roca sin fin, y concluyó que hay que " +
                "imaginarlo feliz; Atlas es su hermano de piedra. Su condena no es una derrota moral sino un destino: sostener " +
                "algo que no eligió y que no puede abandonar.\n\nMartin Heidegger, en Ser y tiempo, define al ser humano como " +
                "ser-para-la-muerte: solo entendemos nuestra vida cuando aceptamos que termina. Para Platón, en el Fedón, " +
                "filosofar es «prepararse para morir»: ejercitar el alma para soltar lo que no es esencial. Atlas encarna una " +
                "tercera vía: no huye de la finitud ni la celebra, simplemente la sostiene, y al final se vuelve montaña: lo que " +
                "carga acaba siendo lo que es. Los siglos que lleva en pie le han enseñado que la tierra no llega tarde a ninguna " +
                "parte.\n\nSu Muro de Roca no vence al tiempo: lo ordena; y sus Raíces Punzantes recuerdan que lo que permanece " +
                "también hiere. La pregunta que deja es la de todo el que ha sostenido algo demasiado grande: si todo lo que " +
                "sostengo terminará por caer, ¿qué hace que valga la pena sostenerlo?",
                "Si todo lo que sostengo terminará por caer, ¿qué hace que valga la pena sostenerlo?",
                "The Ancient Root", "The weight of the world and being-toward-death",
                "Atlas is a Titan of the first lineage: he fought against the gods and lost, and Zeus condemned him to hold up " +
                "the vault of the sky on his shoulders at the western edge of the world. Once Heracles asked him to fetch the " +
                "golden apples of the Hesperides while he carried the sky; Atlas came back convinced he could finally set it " +
                "down, and had to return to his burden, tricked. When Perseus showed him the head of Medusa, he turned to stone " +
                "and became a mountain. In the Arena he is an old man in a mantle patched with leaves who plants his staff like " +
                "someone propping up a pillar of the sky: he speaks little, almost always in questions, and is never in a hurry.",
                "Atlas is the face of finitude as a burden. He cannot let go of the sky, and the myth shows a being who embodies " +
                "endurance: Albert Camus wrote about Sisyphus, the man who pushes a rock without end, and concluded that we must " +
                "imagine him happy; Atlas is his brother in stone. His sentence is not a moral defeat but a destiny: to hold up " +
                "something he did not choose and cannot abandon.\n\nMartin Heidegger, in Being and Time, defines the human being " +
                "as being-toward-death: we only understand our life when we accept that it ends. For Plato, in the Phaedo, to " +
                "philosophize is «to prepare for dying»: to train the soul to let go of what is not essential. Atlas embodies a " +
                "third way: he neither flees finitude nor celebrates it, he simply bears it, and in the end he becomes a " +
                "mountain: what he carries ends up being what he is. The centuries he has stood have taught him that the earth is " +
                "never late anywhere.\n\nHis Rock Wall does not defeat time: it orders it; and his Piercing Roots remind us that " +
                "what endures also wounds. The question he leaves is that of everyone who has held up something too big: if " +
                "everything I hold up will eventually fall, what makes it worth holding up?",
                "If everything I hold up will eventually fall, what makes it worth holding up?");

            Agregar("Talos", "El Guardián Silencioso", "El ser hecho para algo",
                "En Creta se contaba que Talos era un gigante de bronce forjado por Hefesto, o regalado por Zeus a Europa y a su " +
                "hijo Minos, para vigilar la isla: la recorría tres veces al día y lanzaba rocas a las naves extrañas; si alguien " +
                "desembarcaba, se calentaba al rojo vivo y lo abrazaba. No tenía sangre sino una sola vena de icor sellada con un " +
                "clavo en el tobillo. Medea lo hechizó para que se hiriera el tobillo y Talos se vació como un odre. En la Arena " +
                "despierta cada mañana a la entrada con una orden grabada que nadie puede leer, «protege»; en su hombro creció " +
                "una flor y no la arranca: quizá es lo único en él que nadie programó.",
                "Talos es el mito antiguo de la criatura hecha para una función. Aristóteles razonaba que cada cosa se entiende " +
                "por su ergon, su tarea propia: el ojo es para ver, el cuchillo para cortar; y buscó el ergon del ser humano. " +
                "Talos tiene un ergon exacto, proteger, y lo cumple sin dudar. Pero su único punto débil es la vena: allí está su " +
                "verdad. Un ser perfectamente ordenado a un fin queda a un clavo de vaciarse.\n\nGiovanni Pico della Mirandola, " +
                "en su Discurso sobre la dignidad del hombre, imaginó que Dios creó al humano sin un lugar ni una forma propia " +
                "para que fuera el escultor de sí mismo. Talos es su contrario: tiene forma, lugar y función. Immanuel Kant " +
                "añadiría que el humano es un fin en sí mismo, mientras que Talos fue creado como medio. Sin embargo, la flor en " +
                "su hombro sugiere lo que llamamos libertad: lo que aparece donde la función no llegó.\n\nSu Puño Sísmico golpea " +
                "con la fuerza de un ser construido para aplastar; su Coraza Espinosa devuelve el daño recibido, protección sin " +
                "cálculo. Pero su historia pregunta algo que va más allá del bronce: si fui hecho para proteger, ¿puedo elegir " +
                "proteger, o solo cumplo una orden que ya no entiendo?",
                "Si fui hecho para proteger, ¿puedo elegir proteger o solo cumplo una orden que ya no entiendo?",
                "The Silent Guardian", "The being made for something",
                "In Crete it was told that Talos was a giant of bronze forged by Hephaestus, or given by Zeus to Europa and to " +
                "her son Minos, to watch over the island: he circled it three times a day and hurled rocks at foreign ships; if " +
                "someone landed, he heated himself red-hot and embraced them. He had no blood but a single vein of ichor sealed " +
                "with a nail at the ankle. Medea bewitched him so that he would wound his ankle, and Talos drained like a " +
                "wineskin. In the Arena he wakes each morning at the entrance with an engraved order that nobody can read, " +
                "«protect»; a flower grew on his shoulder and he does not pull it out: perhaps it is the only thing in him that " +
                "nobody programmed.",
                "Talos is the ancient myth of the creature made for a function. Aristotle reasoned that each thing is understood " +
                "by its ergon, its proper task: the eye is for seeing, the knife for cutting; and he searched for the ergon of " +
                "the human being. Talos has an exact ergon, to protect, and fulfills it without doubt. But his only weak point is " +
                "the vein: there lies his truth. A being perfectly ordered toward an end is one nail away from draining " +
                "out.\n\nGiovanni Pico della Mirandola, in his Oration on the Dignity of Man, imagined that God created the human " +
                "being without a place or a form of his own so that he could be the sculptor of himself. Talos is his opposite: " +
                "he has form, place and function. Immanuel Kant would add that the human being is an end in himself, whereas " +
                "Talos was created as a means. Yet the flower on his shoulder suggests what we call freedom: what appears where " +
                "the function did not reach.\n\nHis Seismic Fist strikes with the force of a being built to crush; his Thorny " +
                "Armor returns the damage received, protection without calculation. But his story asks something that goes beyond " +
                "bronze: if I was made to protect, can I choose to protect, or am I only following an order I no longer " +
                "understand?",
                "If I was made to protect, can I choose to protect, or am I only following an order I no longer understand?");

            Agregar("Hefesto", "El Herrero de Montaña", "La obra como trascendencia",
                "Hefesto es el único dios que trabaja con las manos. Nació cojo o fue arrojado del Olimpo por su propia madre, y " +
                "cayó durante un día entero hasta la isla de Lemnos; se cuenta que Tetis y Eurínome lo recogieron y lo criaron " +
                "nueve años en una gruta bajo el mar, donde hizo sus primeras joyas. De su fragua salieron el rayo de Zeus, el " +
                "escudo de Aquiles, las trampas de oro y hasta los autómatas que le sirven, entre ellos, según algunas " +
                "tradiciones, el gigante Talos. Vino a la Arena con un martillo gastado por la forma de su mano a probar algo que " +
                "no sabe si dura: a sí mismo.",
                "Hefesto es la divinidad de quien crea con un cuerpo imperfecto. En el Olimpo es el único que no es bello ni " +
                "veloz, y por eso mismo el único que fabrica. Hannah Arendt distinguió tres actividades humanas: la labor " +
                "(sostener la vida), el trabajo (fabricar objetos duraderos que forman un mundo) y la acción (iniciar algo nuevo " +
                "entre los demás). El trabajo de Hefesto es el de crear mundo: lo que hace sobrevive a su hacedor.\n\nRichard " +
                "Sennett, en El artesano, defiende que la habilidad manual es una forma de pensar: quien trabaja bien una pieza " +
                "dialoga con la resistencia del material y aprende de sus fallas. Martin Heidegger veía en la técnica antigua una " +
                "forma de poiesis, un hacer que deja aparecer lo oculto. En la Ilíada, el escudo de Aquiles forjado por Hefesto " +
                "contiene el mundo entero: ciudades, cosechas, danzas. La obra no es solo objeto: es cosmos hecho a mano. Y hay " +
                "una ironía: el dios cojo es el único capaz de dar belleza a los demás.\n\nSu Esquirla Cortante golpea dos veces, " +
                "como el martillo sobre el yunque; su Filo Reflectante devuelve el daño con un orden perfecto. Pero sigue abierta " +
                "la gran pregunta del artesano: si mi obra me sobrevive, ¿sigo existiendo en ella o solo existe ella, sin mí?",
                "Si mi obra me sobrevive, ¿sigo existiendo en ella o solo existe ella, sin mí?",
                "The Mountain Smith", "The work as transcendence",
                "Hephaestus is the only god who works with his hands. He was born lame or was cast out of Olympus by his own " +
                "mother, and fell for an entire day until the island of Lemnos; it is told that Thetis and Eurynome picked him up " +
                "and raised him for nine years in a cave beneath the sea, where he made his first jewels. From his forge came the " +
                "thunderbolt of Zeus, the shield of Achilles, the golden traps and even the automatons that serve him, among " +
                "them, according to some traditions, the giant Talos. He came to the Arena with a hammer worn into the shape of " +
                "his hand to test something he does not know will last: himself.",
                "Hephaestus is the deity of those who create with an imperfect body. On Olympus he is the only one who is neither " +
                "beautiful nor swift, and for that very reason the only one who fabricates. Hannah Arendt distinguished three " +
                "human activities: labor (sustaining life), work (fabricating durable objects that form a world) and action " +
                "(beginning something new among others). Hephaestus's work is that of creating a world: what he makes outlives " +
                "its maker.\n\nRichard Sennett, in The Craftsman, argues that manual skill is a form of thinking: whoever works a " +
                "piece well converses with the resistance of the material and learns from its flaws. Martin Heidegger saw in " +
                "ancient technology a form of poiesis, a making that lets what is hidden appear. In the Iliad, the shield of " +
                "Achilles forged by Hephaestus contains the entire world: cities, harvests, dances. The work is not just an " +
                "object: it is a cosmos made by hand. And there is an irony: the lame god is the only one able to give beauty to " +
                "the others.\n\nHis Cutting Shard strikes twice, like the hammer on the anvil; his Reflecting Edge returns the " +
                "damage with perfect order. But the great question of the craftsman remains open: if my work outlives me, do I go " +
                "on existing in it, or does only it exist, without me?",
                "If my work outlives me, do I go on existing in it, or does only it exist, without me?");
        }

        private static void CargarAire()
        {
            Agregar("Atalanta", "La Arquera del Viento Cambiante", "La libertad radical",
                "Atalanta fue abandonada recién nacida en una montaña porque su padre quería un varón, y una osa la amamantó " +
                "hasta que unos cazadores la criaron. Creció corriendo más rápido que cualquier hombre. Fue la primera en herir " +
                "al jabalí de Calidón y rechazó casarse: un oráculo le había advertido que el matrimonio la perdería. Aceptó un " +
                "único pacto: se casaría con quien la venciera en una carrera, y quien perdiera moriría. Hipómenes la venció " +
                "soltando tres manzanas de oro; ella se detuvo a recogerlas. Se dice que, tiempo después, ambos fueron " +
                "convertidos en leones. En la Arena es arquera: jamás dispara dos veces desde el mismo lugar y le pregunta al " +
                "viento hacia dónde sopla, para ir casi siempre en dirección contraria.",
                "Atalanta es la mujer que se niega a ser definida por otros: ni por el abandono de su padre, ni por el oráculo, " +
                "ni por los pretendientes. Jean-Paul Sartre sostenía que la existencia precede a la esencia: no hay una " +
                "naturaleza previa que decida quién soy; me defino por lo que elijo, y esa libertad no se puede delegar. Simone " +
                "de Beauvoir llevó esa idea a la ética de la ambigüedad: somos libres, pero situados en un cuerpo, una época y " +
                "una sociedad.\n\nEl mito de las manzanas funciona como una lección sobre la facticidad: aun la libertad más " +
                "veloz tiene una debilidad concreta, algo que la detiene. Søren Kierkegaard hablaba del vértigo de la libertad: " +
                "la angustia no es miedo a algo, sino el vértigo ante la posibilidad que abre cada elección. Atalanta corre para " +
                "que nadie la alcance, y precisamente por eso su libertad es frágil: la elección de no quedarse en ningún lugar " +
                "es también una forma de huir de una misma.\n\nSu Ráfaga Veloz golpea dos veces y le devuelve energía, y su " +
                "Viento a Favor le da seis puntos de energía de golpe: un viento que sopla de cara o de espalda y que ella " +
                "aprovecha pero no controla. La pregunta queda abierta: si cada elección me define y ninguna esencia me guía, " +
                "¿cómo sé si estoy eligiendo o solo moviéndome?",
                "Si cada elección me define y ninguna esencia me guía, ¿cómo sé si estoy eligiendo o solo moviéndome?",
                "The Archer of the Shifting Wind", "Radical freedom",
                "Atalanta was abandoned as a newborn on a mountain because her father wanted a boy, and a she-bear nursed her " +
                "until some hunters raised her. She grew up running faster than any man. She was the first to wound the " +
                "Calydonian boar and she refused to marry: an oracle had warned her that marriage would be her ruin. She accepted " +
                "a single pact: she would marry whoever beat her in a race, and whoever lost would die. Hippomenes beat her by " +
                "dropping three golden apples; she stopped to pick them up. It is said that, some time later, both were turned " +
                "into lions. In the Arena she is an archer: she never shoots twice from the same place and asks the wind where it " +
                "blows, to go almost always the opposite way.",
                "Atalanta is the woman who refuses to be defined by others: neither by her father's abandonment, nor by the " +
                "oracle, nor by the suitors. Jean-Paul Sartre held that existence precedes essence: there is no prior nature " +
                "deciding who I am; I define myself by what I choose, and that freedom cannot be delegated. Simone de Beauvoir " +
                "carried that idea into the ethics of ambiguity: we are free, but situated in a body, an era and a " +
                "society.\n\nThe myth of the apples works as a lesson about facticity: even the swiftest freedom has a concrete " +
                "weakness, something that stops it. Søren Kierkegaard spoke of the dizziness of freedom: anxiety is not fear of " +
                "something, but the vertigo before the possibility that each choice opens. Atalanta runs so that no one catches " +
                "her, and for that very reason her freedom is fragile: the choice of not staying anywhere is also a way of " +
                "fleeing from oneself.\n\nHer Swift Gust strikes twice and returns energy to her, and her Tailwind gives her six " +
                "points of energy at once: a wind that blows in her face or at her back and that she uses but does not control. " +
                "The question remains open: if every choice defines me and no essence guides me, how do I know whether I am " +
                "choosing or merely moving?",
                "If every choice defines me and no essence guides me, how do I know whether I am choosing or merely moving?");

            Agregar("Tifón", "El Grifo de la Tormenta", "El absurdo y la revuelta",
                "Tifón fue el último hijo de Gea y del Tártaro, un monstruo tan grande que su cabeza rozaba las estrellas, con " +
                "cien cabezas de serpiente que rugían con voces de toro, de león y de dioses. Se levantó contra Zeus y casi lo " +
                "vence: le cortó los tendones y lo dejó inmóvil hasta que Hermes se los devolvió; al final el rayo lo derribó y " +
                "lo sepultaron bajo el monte Etna, desde donde, dicen, aún escupe fuego y manda los vientos devastadores que " +
                "llevan su nombre. En la Arena aparece como un grifo de águila y león nacido en el único punto en calma de una " +
                "tormenta de tres años: no se deja clasificar, habla poco y mira el horizonte como si esperara una respuesta que " +
                "nunca llega.",
                "Tifón es el monstruo que pone a prueba los límites del orden. Mary Douglas, en Pureza y peligro, mostró que las " +
                "culturas llaman impuro o monstruoso a lo que no cabe en sus categorías: lo que mezcla especies, lo que está " +
                "entre dos cosas. Tifón es mezcla (bestia, serpiente, viento) y por eso asusta; pero también revela que todo " +
                "orden descansa sobre lo que excluye. Lo que el mito condena es, en el fondo, una pregunta: ¿qué pasa con lo que " +
                "no encaja?\n\nAlbert Camus llamó absurdo al choque entre el deseo humano de sentido y el silencio del mundo, y " +
                "ofreció tres respuestas: el suicidio, el salto de fe o la revuelta lúcida. En El hombre rebelde añade que la " +
                "revuelta auténtica dice «no» pero también reconoce un límite: «me rebelo, luego somos». Tifón se rebela sin " +
                "medida, y por eso termina bajo una montaña: la protesta que olvida a los demás se convierte en destrucción. Su " +
                "cautiverio es la advertencia de Camus.\n\nSu Tornado Cortante es devastador y su Ojo de la Tormenta potencia su " +
                "ataque con la calma de quien está en el centro del caos. Queda la pregunta: si el mundo no responde a mi " +
                "pregunta por el sentido, ¿debo callar, huir o rebelarme y seguir volando?",
                "Si el mundo no responde a mi pregunta por el sentido, ¿debo callar, huir o rebelarme y seguir volando?",
                "The Griffin of the Storm", "The absurd and the revolt",
                "Typhon was the last child of Gaia and Tartarus, a monster so huge that his head brushed the stars, with a " +
                "hundred serpent heads that roared with the voices of bulls, lions and gods. He rose against Zeus and almost " +
                "defeated him: he cut out his sinews and left him helpless until Hermes returned them; in the end the thunderbolt " +
                "struck him down and he was buried under Mount Etna, from where, they say, he still spits fire and sends the " +
                "devastating winds that bear his name. In the Arena he appears as a griffin of eagle and lion born in the only " +
                "calm spot of a three-year storm: he cannot be classified, speaks little and gazes at the horizon as if awaiting " +
                "an answer that never comes.",
                "Typhon is the monster that tests the limits of order. Mary Douglas, in Purity and Danger, showed that cultures " +
                "call impure or monstrous whatever does not fit their categories: what mixes species, what lies between two " +
                "things. Typhon is a mixture (beast, serpent, wind) and that is why he frightens; but he also reveals that every " +
                "order rests on what it excludes. What the myth condemns is, deep down, a question: what happens to what does not " +
                "fit?\n\nAlbert Camus called absurd the clash between the human desire for meaning and the silence of the world, " +
                "and offered three answers: suicide, the leap of faith or lucid revolt. In The Rebel he adds that authentic " +
                "revolt says «no» but also recognizes a limit: «I rebel, therefore we are». Typhon rebels without measure, and " +
                "that is why he ends up under a mountain: protest that forgets others turns into destruction. His captivity is " +
                "Camus's warning.\n\nHis Cutting Tornado is devastating and his Eye of the Storm boosts his attack with the calm " +
                "of one who stands at the center of chaos. The question remains: if the world does not answer my question about " +
                "meaning, should I be silent, flee, or rebel and keep flying?",
                "If the world does not answer my question about meaning, should I be silent, flee, or rebel and keep flying?");

            Agregar("Psique", "El Hada Curiosa", "La voluntad de sentido",
                "Psique, cuyo nombre significa «alma», era una princesa mortal tan bella que la gente le rezaba a ella en vez de " +
                "a Afrodita. Eros, enviado a castigarla, se enamoró y la visitó cada noche con una condición: que ella nunca lo " +
                "mirara. Psique, empujada por la curiosidad, encendió una lámpara para verlo y una gota de aceite le cayó en el " +
                "hombro y lo despertó. Para recuperarlo soportó pruebas imposibles: clasificar montañas de semillas (la ayudaron " +
                "las hormigas), traer lana de ovejas feroces, llenar un frasco con agua de la Estigia (la ayudó un águila) y " +
                "bajar al Hades. Al final abrió también la caja que no debía abrir y cayó en un sueño profundo; Eros la despertó " +
                "y Zeus le dio la inmortalidad. En la Arena es un hada pequeña de alas de mariposa que se detiene a preguntar " +
                "«¿para qué?».",
                "En griego psyché significa soplo, aliento: el alma es lo que respira. Platón, en el Fedro, imagina el alma como " +
                "un carro alado que ascendió a contemplar las ideas y cayó al perder sus alas; la filosofía es el esfuerzo por " +
                "recuperarlas. Psique es esa alma: curiosa hasta la desobediencia, castigada por mirar, pero capaz de superar " +
                "pruebas imposibles gracias a la ayuda de seres diminutos: las hormigas, el águila, la torre que habla. El " +
                "sentido, parece decir el mito, se alcanza con otros.\n\nPara Aristóteles, la filosofía nace del asombro: el ser " +
                "humano empieza a pensar cuando se admira ante lo que no comprende. Viktor Frankl, que sobrevivió a los campos " +
                "nazis, sostuvo en El hombre en busca de sentido que la motivación más profunda del ser humano no es el placer ni " +
                "el poder, sino la voluntad de sentido; y que ese sentido se encuentra en una obra, en un amor o en la actitud " +
                "con que afrontamos lo inevitable. Apuleyo, que cuenta el mito, lo narra como una iniciación: el alma pasa por la " +
                "prueba para unirse con el amor.\n\nSu Cuchilla de Viento golpea dos veces con ligereza y su Soplo Sanador cura a " +
                "un aliado con la brisa de quien se queda un momento más. Curiosidad y cuidado: dos formas de atender el mundo. Y " +
                "la pregunta que deja es la de todos: si nadie me entrega un sentido ya hecho, ¿lo encuentro en el mundo o lo " +
                "invento yo?",
                "Si nadie me entrega un sentido ya hecho, ¿lo encuentro en el mundo o lo invento yo?",
                "The Curious Fairy", "The will to meaning",
                "Psyche, whose name means «soul», was a mortal princess so beautiful that people prayed to her instead of to " +
                "Aphrodite. Eros, sent to punish her, fell in love and visited her every night on one condition: that she never " +
                "look at him. Psyche, driven by curiosity, lit a lamp to see him and a drop of oil fell on his shoulder and woke " +
                "him. To win him back she endured impossible trials: sorting mountains of seeds (the ants helped her), fetching " +
                "wool from ferocious sheep, filling a flask with water from the Styx (an eagle helped her) and descending to " +
                "Hades. In the end she also opened the box she must not open and fell into a deep sleep; Eros woke her and Zeus " +
                "granted her immortality. In the Arena she is a small fairy with butterfly wings who stops to ask «what for?».",
                "In Greek, psyché means breath: the soul is what breathes. Plato, in the Phaedrus, imagines the soul as a winged " +
                "chariot that rose to contemplate the Ideas and fell when it lost its wings; philosophy is the effort to regain " +
                "them. Psyche is that soul: curious to the point of disobedience, punished for looking, but capable of overcoming " +
                "impossible trials thanks to the help of tiny beings: the ants, the eagle, the talking tower. Meaning, the myth " +
                "seems to say, is reached together with others.\n\nFor Aristotle, philosophy is born of wonder: the human being " +
                "begins to think when he marvels at what he does not understand. Viktor Frankl, who survived the Nazi camps, " +
                "argued in Man's Search for Meaning that the deepest human motivation is neither pleasure nor power but the will " +
                "to meaning; and that this meaning is found in a work, in a love or in the attitude with which we face the " +
                "inevitable. Apuleius, who tells the myth, narrates it as an initiation: the soul goes through the trial in order " +
                "to unite with love.\n\nHer Wind Blade strikes twice with lightness and her Healing Breath heals an ally with the " +
                "breeze of someone who stays a moment longer. Curiosity and care: two ways of attending to the world. And the " +
                "question she leaves is everyone's: if nobody hands me a ready-made meaning, do I find it in the world or do I " +
                "invent it myself?",
                "If nobody hands me a ready-made meaning, do I find it in the world or do I invent it myself?");
        }
    }
}
