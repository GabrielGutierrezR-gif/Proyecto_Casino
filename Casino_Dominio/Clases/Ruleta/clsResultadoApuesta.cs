using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsResultadoApuesta
    {
        public string TipoApuesta { get; set; } = "";
        public string ValorApuesta { get; set; } = "";
        public decimal Monto { get; set; }
        public string Resultado { get; set; } = "";   // Gana / Pierde
        public decimal Ganancia { get; set; }         // neta: +premio o -monto
    }
}
