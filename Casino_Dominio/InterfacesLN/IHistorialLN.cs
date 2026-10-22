using Casino_Dominio.Clases;
using Casino_Dominio.Clases.Historial;
using Casino_Utilitarios;
using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_Dominio.InterfacesLN
{
    public interface IHistorialLN
    {
        Task<Respuesta<IEnumerable<clsHistorialCompra>>> ComprasAsync(int idUsuario, int cantidad);

        Task<Respuesta<IEnumerable<clsHistorialJuego>>> JuegosAsync(int idUsuario, int cantidad);
    }
}
