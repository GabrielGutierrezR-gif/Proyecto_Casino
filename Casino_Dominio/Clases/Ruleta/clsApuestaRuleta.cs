using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsApuestaRuleta
    {
        public string TipoApuesta { get; set; } = "";   // Numero, Color, ParImpar, Docena
        public string ValorApuesta { get; set; } = "";  // "17", "Rojo", "Par", "2"...
        public decimal Monto { get; set; }
    }
}
