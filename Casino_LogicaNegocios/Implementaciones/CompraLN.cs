using AutoMapper;
using Casino_Dominio.Clases;
using Casino_Dominio.Clases.Compras;
using Casino_Dominio.Entidades;
using Casino_Dominio.EntidadesTipadas;
using Casino_Dominio.InterfacesAD;
using Casino_Dominio.InterfacesLN;
using Casino_Utilitarios;
using Microsoft.Extensions.Logging;

namespace Casino_LogicaNegocios.Implementaciones
{
    public class CompraLN : ICompraLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly ILogger<CompraLN> _logger;
        private readonly IMapper _mapper;

        public CompraLN(IUnidadTrabajoEF unidadTrabajo, ILogger<CompraLN> logger, IMapper mapper)
        {
            _unidadDeTrabajo = unidadTrabajo;
            _logger = logger;
            _mapper = mapper;
        }

        // Catálogo de paquetes 
        public async Task<Respuesta<IEnumerable<TPaquetesFichas>>> ListarPaquetesAsync()
        {
            var resultado = new Respuesta<IEnumerable<TPaquetesFichas>>();

            try
            {
                var respuestaRepositorio = await _unidadDeTrabajo.TPaquetesFichas.ListarAsync();

                if (!respuestaRepositorio.Success)
                {
                    resultado.Success = false;
                    resultado.Error = respuestaRepositorio.Error;
                    return resultado;
                }

                resultado.Data = _mapper.Map<IEnumerable<TPaquetesFichas>>(respuestaRepositorio.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar paquetes.");
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

        //  Agregar tarjeta ficticia 
        public async Task<Respuesta<clsMetodoPago>> AgregarMetodoPagoAsync(int idUsuario, clsMetodoPagoRequest datos)
        {
            var resultado = new Respuesta<clsMetodoPago>();

            try
            {
                // Quitar espacios y guiones: "4111 1111-1111 1111" → "4111111111111111"
                var numero = new string((datos.NumeroTarjeta ?? "").Where(char.IsDigit).ToArray());
                var titular = datos.NombreTitular?.Trim() ?? "";

                if (datos.TipoTarjeta != "Credito" && datos.TipoTarjeta != "Debito")
                {
                    resultado.Success = false;
                    resultado.Error = "El tipo de tarjeta debe ser 'Credito' o 'Debito'.";
                    return resultado;
                }

                if (numero.Length < 13 || numero.Length > 19)
                {
                    resultado.Success = false;
                    resultado.Error = "El número de tarjeta debe tener entre 13 y 19 dígitos.";
                    return resultado;
                }

                if (titular.Length == 0 || titular.Length > 100)
                {
                    resultado.Success = false;
                    resultado.Error = "El nombre del titular es obligatorio (máximo 100 caracteres).";
                    return resultado;
                }

                var metodo = new MetodosPago
                {
                    IdUsuario = idUsuario,
                    TipoTarjeta = datos.TipoTarjeta,
                    NumeroTarjeta = numero[^4..],   // muestra SOLO los últimos 4 dígitos 
                    NombreTitular = titular
                };

                var respuestaRepositorio = await _unidadDeTrabajo.TMetodosPago.InsertarAsync(metodo);

                if (!respuestaRepositorio.Success || respuestaRepositorio.Data == null)
                {
                    resultado.Success = false;
                    resultado.Error = respuestaRepositorio.Error;
                    return resultado;
                }

                resultado.Data = _mapper.Map<clsMetodoPago>(respuestaRepositorio.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar método de pago del usuario {IdUsuario}", idUsuario);
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

        // Mis tarjetas
        public async Task<Respuesta<IEnumerable<clsMetodoPago>>> ListarMetodosPagoAsync(int idUsuario)
        {
            var resultado = new Respuesta<IEnumerable<clsMetodoPago>>();

            try
            {
                // Solo las tarjetas de ESTE usuario
                var respuestaRepositorio =
                    await _unidadDeTrabajo.TMetodosPago.BuscarAsync(x => x.IdUsuario == idUsuario);

                if (!respuestaRepositorio.Success)
                {
                    resultado.Success = false;
                    resultado.Error = respuestaRepositorio.Error;
                    return resultado;
                }

                resultado.Data = _mapper.Map<IEnumerable<clsMetodoPago>>(respuestaRepositorio.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar métodos de pago del usuario {IdUsuario}", idUsuario);
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

        // Comprar paquete
        public async Task<Respuesta<clsCompraResultado>> ComprarPaqueteAsync(int idUsuario, clsCompraRequest datos)
        {
            var resultado = new Respuesta<clsCompraResultado>();

            try
            {
                // 1) ¿Existe el paquete?
                var paquete = await _unidadDeTrabajo.TPaquetesFichas
                    .ObtenerEntidadAsync(x => x.IdPaquete == datos.IdPaquete);

                if (!paquete.Success || paquete.Data == null)
                {
                    resultado.Success = false;
                    resultado.Error = "El paquete seleccionado no existe.";
                    return resultado;
                }

                // 2) ¿La tarjeta existe y es de ESTE usuario? (que nadie pague con tarjeta ajena)
                var metodo = await _unidadDeTrabajo.TMetodosPago
                    .ObtenerEntidadAsync(x => x.IdMetodoPago == datos.IdMetodoPago && x.IdUsuario == idUsuario);

                if (!metodo.Success || metodo.Data == null)
                {
                    resultado.Success = false;
                    resultado.Error = "El método de pago no existe o no te pertenece.";
                    return resultado;
                }

                // 3) Traer al usuario
                var usuario = await _unidadDeTrabajo.TUsuario
                    .ObtenerEntidadAsync(x => x.IdUsuario == idUsuario);

                if (!usuario.Success || usuario.Data == null)
                {
                    resultado.Success = false;
                    resultado.Error = "Usuario no encontrado.";
                    return resultado;
                }

                // 4) TRANSACCIÓN: guardar compra + sumar fichas
                _unidadDeTrabajo.EmpezarTransaccion();

                var compra = new Compra
                {
                    IdUsuario = idUsuario,
                    IdPaquete = paquete.Data.IdPaquete,
                    IdMetodoPago = metodo.Data.IdMetodoPago,
                    FechaCompra = DateTime.Now
                };

                var insertCompra = await _unidadDeTrabajo.TCompra.InsertarAsync(compra);

                if (!insertCompra.Success)
                {
                    _unidadDeTrabajo.Rollback();
                    resultado.Success = false;
                    resultado.Error = insertCompra.Error;
                    return resultado;
                }

                usuario.Data.SaldoFichas += paquete.Data.CantidadFichas;

                var updateUsuario = await _unidadDeTrabajo.TUsuario.ModificarAsync(usuario.Data);

                if (!updateUsuario.Success)
                {
                    _unidadDeTrabajo.Rollback();   // ← se deshace también la compra
                    resultado.Success = false;
                    resultado.Error = updateUsuario.Error;
                    return resultado;
                }

                _unidadDeTrabajo.CompletarTran();  // ← se confirman las dos cosas

                resultado.Data = new clsCompraResultado
                {
                    IdCompra = compra.IdCompra,
                    Paquete = paquete.Data.Nombre,
                    FichasAgregadas = paquete.Data.CantidadFichas,
                    PrecioPagado = paquete.Data.Precio,
                    NuevoSaldo = usuario.Data.SaldoFichas,
                    FechaCompra = compra.FechaCompra
                };
            }
            catch (Exception ex)
            {
                _unidadDeTrabajo.Rollback();
                _logger.LogError(ex, "Error al comprar paquete. Usuario {IdUsuario}", idUsuario);
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }
    }
}