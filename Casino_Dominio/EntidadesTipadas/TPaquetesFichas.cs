using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.EntidadesTipadas
{
    public class TPaquetesFichas
    {
        public int IdPaquete { get; set; }
        public string Nombre { get; set; } = null!;
        public int CantidadFichas { get; set; }
        public decimal Precio { get; set; }
    }
}
