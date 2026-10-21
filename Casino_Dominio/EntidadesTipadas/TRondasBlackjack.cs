using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.EntidadesTipadas
{
    public class TRondasBlackjack
    {
        public int IdRonda { get; set; }
        public int IdSala { get; set; }
        public int NumeroRonda { get; set; }
        public string Estado { get; set; } = null!;
        public int? ResultadoDealer { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
