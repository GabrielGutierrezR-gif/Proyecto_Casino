using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.EntidadesTipadas
{
    public class TPartidasRuleta
    {
        public int IdPartida { get; set; }
        public int NumeroGanador { get; set; }
        public string ColorGanador { get; set; } = null!;
        public DateTime FechaPartida { get; set; }
    }
}
