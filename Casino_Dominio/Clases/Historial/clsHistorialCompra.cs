using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsHistorialCompra
    {
        public int IdCompra { get; set; }
        public DateTime FechaCompra { get; set; }
        public string Paquete { get; set; } = "";
        public int CantidadFichas { get; set; }
        public decimal Precio { get; set; }
        public string TipoTarjeta { get; set; } = "";
        public string UltimosDigitos { get; set; } = "";
    }
}
