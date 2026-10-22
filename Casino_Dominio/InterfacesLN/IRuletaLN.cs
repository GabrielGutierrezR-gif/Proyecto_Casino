using Casino_Dominio.Clases;
using Casino_Utilitarios;
using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.InterfacesLN
{
    public interface IRuletaLN
    {
        Task<Respuesta<clsGiroResultado>> GirarAsync(int idUsuario, clsGiroRequest datos);
    }
}
