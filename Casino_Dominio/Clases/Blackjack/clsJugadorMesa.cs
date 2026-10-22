using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsJugadorMesa
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = "";
        public bool Conectado { get; set; }
        public bool YaAposto { get; set; }
        public List<clsManoMesa> Manos { get; set; } = new();   // 2 manos si dividió
    }
}
