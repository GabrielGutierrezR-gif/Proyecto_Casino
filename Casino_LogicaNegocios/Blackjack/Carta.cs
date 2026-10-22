using System;
using System.Collections.Generic;
using System.Text;

namespace Casino_LogicaNegocios.Blackjack
{
    // Una carta de la baraja: valor (A, 2-10, J, Q, K) y palo
    public class Carta
    {
        public string Valor { get; }
        public string Palo { get; }

        public Carta(string valor, string palo)
        {
            Valor = valor;
            Palo = palo;
        }

        // Puntos en Blackjack: As = 11 (puede bajar a 1), figuras = 10, el resto su número
        public int Puntos => Valor switch
        {
            "A" => 11,
            "J" or "Q" or "K" => 10,
            _ => int.Parse(Valor)
        };

        public bool EsAs => Valor == "A";

        public override string ToString() => $"{Valor} de {Palo}";
    }
}