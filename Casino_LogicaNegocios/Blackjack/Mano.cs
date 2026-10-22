using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_LogicaNegocios.Blackjack
{
    // La mano de un jugador en una ronda: sus cartas, su apuesta y cómo le fue
    public class Mano
    {
        public List<Carta> Cartas { get; } = new();
        public decimal Apuesta { get; set; }
        public int IdApuestaBD { get; set; }          // fila en ApuestasBlackjack
        public string Estado { get; set; } = EstadosMano.Jugando;
        public bool EsDividida { get; set; }          // viene de un "Dividir"
        public string? UltimaAccion { get; set; }     // Pedir, Plantarse, Doblar, Dividir...
        public string? Resultado { get; set; }        // Gana, Pierde, Empate, Blackjack
        public decimal Ganancia { get; set; }         // neta: +premio o -apuesta

        // Puntos de la mano: el As vale 11, pero si se pasa de 21 baja a 1
        public int Puntuacion => CalcularPuntos(Cartas);

        // Blackjack natural: 21 con las 2 primeras cartas (no cuenta si viene de dividir)
        public bool EsBlackjack => Cartas.Count == 2 && Puntuacion == 21 && !EsDividida;

        public bool Terminada => Estado != EstadosMano.Jugando;

        // Doblar: solo con las 2 primeras cartas
        public bool PuedeDoblar => !Terminada && Cartas.Count == 2;

        // Dividir: solo con 2 cartas del mismo valor (ej. 8 y 8, o K y Q)
        public bool PuedeDividir => !Terminada && Cartas.Count == 2 && Cartas[0].Puntos == Cartas[1].Puntos;

        public static int CalcularPuntos(IEnumerable<Carta> cartas)
        {
            int total = 0;
            int ases = 0;

            foreach (var carta in cartas)
            {
                total += carta.Puntos;
                if (carta.EsAs) ases++;
            }

            // Cada As que haga falta pasa de 11 a 1 para no pasarse
            while (total > 21 && ases > 0)
            {
                total -= 10;
                ases--;
            }

            return total;
        }
    }

    public static class EstadosMano
    {
        public const string Jugando = "Jugando";      // todavía le toca decidir
        public const string Plantado = "Plantado";    // ya no pide más
        public const string Pasado = "Pasado";        // se pasó de 21, pierde
        public const string Blackjack = "Blackjack";  // 21 natural
    }
}