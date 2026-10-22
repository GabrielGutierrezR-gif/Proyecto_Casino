using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsCompraResultado
    {
        public int IdCompra { get; set; }
        public string Paquete { get; set; } = "";
        public int FichasAgregadas { get; set; }
        public decimal PrecioPagado { get; set; }
        public decimal NuevoSaldo { get; set; }
        public DateTime FechaCompra { get; set; }
    }

}
