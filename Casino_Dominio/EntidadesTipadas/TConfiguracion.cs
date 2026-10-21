using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.EntidadesTipadas
{
    public class TConfiguracion
    {
        public string Clave { get; set; } = null!;
        public string Valor { get; set; } = null!;
        public string? Descripcion { get; set; }
    }
}
