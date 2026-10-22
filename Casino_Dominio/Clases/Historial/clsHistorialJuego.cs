using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases.Historial
{
    public class clsHistorialJuego
    {
        public string Juego { get; set; } = "";            // "Ruleta" o "Blackjack"
        public DateTime Fecha { get; set; }
        public string Detalle { get; set; } = "";          // "Color: Rojo" o "Mesa X - Ronda 3"
        public string ResultadoJuego { get; set; } = "";   // "Salió 14 Rojo" o "Tú 20 vs Dealer 18"
        public decimal Apuesta { get; set; }
        public string Resultado { get; set; } = "";        // Gana, Pierde, Empate, Blackjack
        public decimal Ganancia { get; set; }
    }
}
