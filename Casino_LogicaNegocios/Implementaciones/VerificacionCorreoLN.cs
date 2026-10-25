using Casino_Dominio.InterfacesLN;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace Casino_LogicaNegocios.Implementaciones
{
    // Códigos de 6 dígitos para verificar el correo antes de crear la cuenta.
    // Es SINGLETON: los códigos viven en memoria (duran pocos minutos, no hace falta guardarlos en la BD).
    public class VerificacionCorreoLN : IVerificacionCorreoLN
    {
        private const int SegundosEntreEnvios = 60;
        private const int IntentosMaximos = 5;

        private readonly ConcurrentDictionary<string, CodigoPendiente> _codigos = new();

        public int MinutosValidez => 10;

        public (string? codigo, string error) Generar(string correo)
        {
            correo = Normalizar(correo);

            if (_codigos.TryGetValue(correo, out var anterior))
            {
                int espera = SegundosEntreEnvios - (int)(DateTime.UtcNow - anterior.Enviado).TotalSeconds;
                if (espera > 0)
                    return (null, $"Espera {espera} segundos para pedir otro código.");
            }

            // Número aleatorio seguro de 6 dígitos (000000 - 999999)
            string codigo = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

            // Solo se guarda el hash, nunca el código en texto plano
            _codigos[correo] = new CodigoPendiente(
                Hash(codigo),
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(MinutosValidez));

            return (codigo, "");
        }

        public (bool valido, string error) Verificar(string correo, string codigo)
        {
            correo = Normalizar(correo);
            codigo = codigo?.Trim() ?? "";

            if (!_codigos.TryGetValue(correo, out var pendiente))
                return (false, "Primero solicita el código de verificación.");

            if (DateTime.UtcNow > pendiente.Expira)
            {
                _codigos.TryRemove(correo, out _);
                return (false, "El código venció. Solicita uno nuevo.");
            }

            if (pendiente.Intentos >= IntentosMaximos)
            {
                _codigos.TryRemove(correo, out _);
                return (false, "Demasiados intentos. Solicita un código nuevo.");
            }

            if (!CryptographicOperations.FixedTimeEquals(pendiente.HashCodigo, Hash(codigo)))
            {
                pendiente.Intentos++;
                int quedan = IntentosMaximos - pendiente.Intentos;
                return (false, quedan > 0
                    ? $"Código incorrecto. Te quedan {quedan} intentos."
                    : "Código incorrecto. Solicita un código nuevo.");
            }

            _codigos.TryRemove(correo, out _);
            return (true, "");
        }

        private static string Normalizar(string correo) => correo?.Trim().ToLower() ?? "";

        private static byte[] Hash(string codigo) => SHA256.HashData(Encoding.UTF8.GetBytes(codigo));

        private class CodigoPendiente
        {
            public byte[] HashCodigo { get; }
            public DateTime Enviado { get; }
            public DateTime Expira { get; }
            public int Intentos { get; set; }

            public CodigoPendiente(byte[] hashCodigo, DateTime enviado, DateTime expira)
            {
                HashCodigo = hashCodigo;
                Enviado = enviado;
                Expira = expira;
            }
        }
    }
}
