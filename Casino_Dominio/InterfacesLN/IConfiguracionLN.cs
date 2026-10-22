using Casino_Dominio.EntidadesTipadas;
using Casino_Utilitarios;
using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.InterfacesLN
{
    public interface IConfiguracionLN
    {
        Task<Respuesta<IEnumerable<TConfiguracion>>> ListarAsync();

        Task<Respuesta<TConfiguracion>> ModificarAsync(string clave, string valor);

        // La usan las demás LN para leer un valor (ej. FichasBienvenida)
        Task<int> ObtenerEnteroAsync(string clave, int porDefecto);
    }
}
