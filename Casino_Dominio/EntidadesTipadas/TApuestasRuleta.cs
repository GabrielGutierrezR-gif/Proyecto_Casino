using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.EntidadesTipadas
{
    public class TApuestasRuleta
    {
        public int IdApuestaRuleta { get; set; }
        public int IdPartida { get; set; }
        public int IdUsuario { get; set; }
        public string TipoApuesta { get; set; } = null!;
        public string ValorApuesta { get; set; } = null!;
        public decimal Monto { get; set; }
        public string? Resultado { get; set; }
        public decimal Ganancia { get; set; }
    }
}
