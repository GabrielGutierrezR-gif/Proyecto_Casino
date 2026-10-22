using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.EntidadesTipadas
{
    public class TJugadoresBlackjack
    {
        public int IdJugadorSala { get; set; }
        public int IdSala { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaUnion { get; set; }
    }
}
