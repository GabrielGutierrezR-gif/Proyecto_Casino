using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.Clases
{
    public class clsCrearSalaRequest
    {
        public string NombreSala { get; set; } = "";
        public string TipoSala { get; set; } = "Publica";   // "Publica" o "Privada"
    }
}
