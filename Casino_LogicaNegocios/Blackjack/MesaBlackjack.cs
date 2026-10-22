using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_LogicaNegocios.Blackjack
{
    // Una mesa activa con todo lo que pasa en la ronda actual (solo vive en memoria)
    public class MesaBlackjack
    {
        public int IdSala { get; }
        public string CodigoSala { get; }
        public string NombreSala { get; }

        // Candado: solo una acción a la vez por mesa (si dos jugadores tocan al mismo tiempo, se ordenan)
        public SemaphoreSlim Candado { get; } = new(1, 1);

        public Mazo Mazo { get; } = new(4);
        public List<JugadorMesa> Jugadores { get; } = new();
        public List<Carta> CartasDealer { get; } = new();

        public string Fase { get; set; } = FasesMesa.Apuestas;
        public int? IdRondaActual { get; set; }      // fila en RondasBlackjack
        public int NumeroRonda { get; set; }

        // Turno actual: posición del jugador en la lista y cuál de sus manos (por si dividió)
        public int TurnoJugador { get; set; } = -1;
        public int TurnoMano { get; set; } = -1;
        public DateTime? FinTurnoUtc { get; set; }
        public int SegundosPorTurno { get; set; } = 30;
        public CancellationTokenSource? Temporizador { get; set; }

        public bool Cerrada { get; set; }

        public MesaBlackjack(int idSala, string codigoSala, string nombreSala)
        {
            IdSala = idSala;
            CodigoSala = codigoSala;
            NombreSala = nombreSala;
        }

        public JugadorMesa? Buscar(int idUsuario) => Jugadores.FirstOrDefault(j => j.IdUsuario == idUsuario);

        // ¿Todos los jugadores conectados ya apostaron? (para arrancar la ronda solo)
        public bool TodosApostaron =>
            Jugadores.Any(j => j.Manos.Count > 0) &&
            Jugadores.Where(j => j.Conectado).All(j => j.Manos.Count > 0);
    }

    public static class FasesMesa
    {
        public const string Apuestas = "Apuestas";   // esperando apuestas
        public const string Jugando = "Jugando";     // ronda en curso
    }
}