using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsGiroResultado
    {
        public int IdPartida { get; set; }
        public int NumeroGanador { get; set; }
        public string ColorGanador { get; set; } = "";
        public decimal TotalApostado { get; set; }
        public decimal TotalGanado { get; set; }      // lo que se le devolvió al jugador
        public decimal NuevoSaldo { get; set; }
        public List<clsResultadoApuesta> Detalle { get; set; } = new();
    }
}
