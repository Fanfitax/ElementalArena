namespace ArenaElemental.Modelos
{
    // Escenario de combate. Le da un bonus de ataque a los personajes de su elemento afín
    // (por ejemplo, el Volcán Ardiente da +10% de ataque a los de Fuego).
    public class Escenario
    {
        // El nombre y la descripción se guardan en español y en inglés; las propiedades devuelven el idioma elegido.
        private string nombre;
        private string nombreIngles;
        private string descripcion;
        private string descripcionIngles;

        public Escenario(int id, string nombre, string nombreIngles, Elemento elementoAfin, int bonusAtaquePct,
                         string descripcion, string descripcionIngles)
        {
            Id = id;
            this.nombre = nombre;
            this.nombreIngles = nombreIngles;
            ElementoAfin = elementoAfin;
            BonusAtaquePct = bonusAtaquePct;
            this.descripcion = descripcion;
            this.descripcionIngles = descripcionIngles;
        }

        public int Id { get; private set; }
        public Elemento ElementoAfin { get; private set; }
        public int BonusAtaquePct { get; private set; }

        public string Nombre
        {
            get { return Idioma.Texto(nombre, nombreIngles); }
        }

        public string Descripcion
        {
            get { return Idioma.Texto(descripcion, descripcionIngles); }
        }

        // Multiplicador de ataque para un elemento: 1.1 si es el afín del escenario y 1.0 si no.
        public double MultiplicadorPara(Elemento elemento)
        {
            if (elemento == ElementoAfin)
                return 1.0 + BonusAtaquePct / 100.0;
            return 1.0;
        }

        // Le pone al personaje el multiplicador de este escenario.
        public void AplicarA(Personaje personaje)
        {
            personaje.MultiplicadorEscenario = MultiplicadorPara(personaje.Elemento);
        }

        public override string ToString()
        {
            return Nombre + " (" + Idioma.NombreElemento(ElementoAfin) + " +" + BonusAtaquePct + "% ATK)";
        }
    }
}
