using AutoMapper;
using Casino_Dominio.EntidadesTipadas;
using Casino_Dominio.InterfacesAD;
using Casino_Dominio.InterfacesLN;
using Casino_Utilitarios;
using Microsoft.Extensions.Logging;

namespace Casino_LogicaNegocios.Implementaciones
{
    // Parámetros globales del casino : se cambian sin modificar el código
    public class ConfiguracionLN : IConfiguracionLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly ILogger<ConfiguracionLN> _logger;
        private readonly IMapper _mapper;

        public ConfiguracionLN(IUnidadTrabajoEF unidadTrabajo, ILogger<ConfiguracionLN> logger, IMapper mapper)
        {
            _unidadDeTrabajo = unidadTrabajo;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Respuesta<IEnumerable<TConfiguracion>>> ListarAsync()
        {
            var resultado = new Respuesta<IEnumerable<TConfiguracion>>();

            try
            {
                var respuestaRepositorio = await _unidadDeTrabajo.TConfiguracion.ListarAsync();

                if (!respuestaRepositorio.Success)
                {
                    resultado.Success = false;
                    resultado.Error = respuestaRepositorio.Error;
                    return resultado;
                }

                resultado.Data = _mapper.Map<IEnumerable<TConfiguracion>>(respuestaRepositorio.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar la configuración.");
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

        public async Task<Respuesta<TConfiguracion>> ModificarAsync(string clave, string valor)
        {
            var resultado = new Respuesta<TConfiguracion>();

            try
            {
                // 1) ¿Existe la clave?
                var config = await _unidadDeTrabajo.TConfiguracion.ObtenerEntidadAsync(x => x.Clave == clave);

                if (!config.Success || config.Data == null)
                {
                    resultado.Success = false;
                    resultado.Error = $"No existe la configuración '{clave}'.";
                    return resultado;
                }

                // 2) Todos los valores del casino son números enteros positivos
                if (!int.TryParse(valor, out int numero) || numero <= 0)
                {
                    resultado.Success = false;
                    resultado.Error = "El valor debe ser un número entero mayor a 0.";
                    return resultado;
                }

                // 3) La apuesta mínima no puede ser mayor que la máxima (y al revés)
                if (clave == "ApuestaMinimaBlackjack" && numero > await ObtenerEnteroAsync("ApuestaMaximaBlackjack", 1000))
                {
                    resultado.Success = false;
                    resultado.Error = "La apuesta mínima no puede ser mayor que la máxima.";
                    return resultado;
                }

                if (clave == "ApuestaMaximaBlackjack" && numero < await ObtenerEnteroAsync("ApuestaMinimaBlackjack", 10))
                {
                    resultado.Success = false;
                    resultado.Error = "La apuesta máxima no puede ser menor que la mínima.";
                    return resultado;
                }

                // 4) Guardar
                config.Data.Valor = numero.ToString();
                var respuestaRepositorio = await _unidadDeTrabajo.TConfiguracion.ModificarAsync(config.Data);

                if (!respuestaRepositorio.Success)
                {
                    resultado.Success = false;
                    resultado.Error = respuestaRepositorio.Error;
                    return resultado;
                }

                _logger.LogInformation("Configuración {Clave} cambiada a {Valor}", clave, numero);
                resultado.Data = _mapper.Map<TConfiguracion>(config.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al modificar la configuración {Clave}", clave);
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

        public async Task<int> ObtenerEnteroAsync(string clave, int porDefecto)
        {
            var config = await _unidadDeTrabajo.TConfiguracion.ObtenerEntidadAsync(x => x.Clave == clave);

            if (config.Success && config.Data != null && int.TryParse(config.Data.Valor, out int valor))
                return valor;

            return porDefecto;
        }
    }
}