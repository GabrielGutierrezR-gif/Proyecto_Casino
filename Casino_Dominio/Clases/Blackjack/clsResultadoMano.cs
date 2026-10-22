using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsResultadoMano
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = "";
        public List<clsCarta> Cartas { get; set; } = new();
        public int Puntuacion { get; set; }
        public decimal Apuesta { get; set; }
        public string Resultado { get; set; } = "";   // Gana, Pierde, Empate, Blackjack
        public decimal Ganancia { get; set; }
    }
}
