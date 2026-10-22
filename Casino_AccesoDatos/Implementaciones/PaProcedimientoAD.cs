using Casino_AccesoDatos;
using Casino_AccesoDatos.Contexto;
using Casino_Dominio.Clases;
using Casino_Dominio.Clases.Historial;
using Casino_Dominio.InterfacesAD;
using Casino_Utilitarios;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;


    // Ejecuta procedimientos almacenados y convierte el resultado con DataReaderExtensions
    public class PaProcedimientoAD : IPaProcedimientoAD
    {
        // Nombres de los SP (igual que la práctica)
        private const string spHistorialCompras = "spHistorialCompras";
        private const string spHistorialJuegos = "spHistorialJuegos";

        private readonly CasinoContext _contexto;

        public PaProcedimientoAD(CasinoContext contexto)
        {
            _contexto = contexto;
        }

        public Task<Respuesta<IEnumerable<clsHistorialCompra>>> HistorialComprasAsync(int idUsuario, int cantidad)
            => EjecutarAsync<clsHistorialCompra>(spHistorialCompras,
                   new SqlParameter("@IdUsuario", idUsuario),
                   new SqlParameter("@Cantidad", cantidad));

        public Task<Respuesta<IEnumerable<clsHistorialJuego>>> HistorialJuegosAsync(int idUsuario, int cantidad)
            => EjecutarAsync<clsHistorialJuego>(spHistorialJuegos,
                   new SqlParameter("@IdUsuario", idUsuario),
                   new SqlParameter("@Cantidad", cantidad));

        // Método genérico: sirve para cualquier SP que devuelva una lista
        private async Task<Respuesta<IEnumerable<T>>> EjecutarAsync<T>(string nombreSp, params SqlParameter[] parametros)
        {
            try
            {
                // 1) Abrir la conexión a SQL Server
                await _contexto.Database.OpenConnectionAsync();

                // 2) Preparar el comando: "ejecuta este SP con estos parámetros"
                await using var cmd = _contexto.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = nombreSp;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(parametros);

                // 3) Ejecutar y leer las filas
                await using var reader = await cmd.ExecuteReaderAsync();

                // 4) Convertir cada fila en un objeto (DataReaderExtensions)
                return Respuesta<IEnumerable<T>>.Ok(reader.MapToList<T>());
            }
            catch (Exception ex)
            {
                return Respuesta<IEnumerable<T>>.Fail(ex.GetBaseException().Message);
            }
            finally
            {
                // 5) Cerrar la conexión siempre, haya error o no
                await _contexto.Database.CloseConnectionAsync();
            }
        }
    }
