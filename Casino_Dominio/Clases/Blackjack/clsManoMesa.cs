using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsManoMesa
    {
        public List<clsCarta> Cartas { get; set; } = new();
        public int Puntuacion { get; set; }
        public decimal Apuesta { get; set; }
        public string Estado { get; set; } = "";     // Jugando, Plantado, Pasado, Blackjack
        public bool EsTurno { get; set; }            // ¿le toca a esta mano?
        public bool PuedeDoblar { get; set; }        // para activar/desactivar botones en la app
        public bool PuedeDividir { get; set; }
    }
}
