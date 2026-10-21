using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.EntidadesTipadas
{
    public class TApuestasBlackjack
    {
        public int IdApuesta { get; set; }
        public int IdRonda { get; set; }
        public int IdUsuario { get; set; }
        public decimal Apuesta { get; set; }
        public string? Accion { get; set; }
        public int? Puntuacion { get; set; }
        public string? Resultado { get; set; }
        public decimal Ganancia { get; set; }
    }
}
