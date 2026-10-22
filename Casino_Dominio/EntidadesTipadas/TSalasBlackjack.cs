using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.EntidadesTipadas
{
    public class TSalasBlackjack
    {
        public int IdSala { get; set; }
        public string CodigoSala { get; set; } = null!;
        public string NombreSala { get; set; } = null!;
        public string TipoSala { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
    }

}
