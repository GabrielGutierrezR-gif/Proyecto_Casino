using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Casino_LogicaNegocios.Blackjack
{
    // Zapato de cartas: varias barajas mezcladas, como en los casinos reales
    public class Mazo
    {
        private static readonly string[] Palos = { "Corazones", "Diamantes", "Treboles", "Picas" };
        private static readonly string[] Valores = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

        private readonly int _cantidadBarajas;
        private readonly List<Carta> _cartas = new();

        public Mazo(int cantidadBarajas = 4)
        {
            _cantidadBarajas = cantidadBarajas;
            Barajar();
        }

        public int CartasRestantes => _cartas.Count;

        // Si queda menos de un 25% de la baraja, se vuelve a barajar antes de la siguiente ronda
        public bool NecesitaBarajar => _cartas.Count < (_cantidadBarajas * 52) / 4;

        // Arma las barajas completas y las mezcla
        public void Barajar()
        {
            _cartas.Clear();

            for (int b = 0; b < _cantidadBarajas; b++)
                foreach (var palo in Palos)
                    foreach (var valor in Valores)
                        _cartas.Add(new Carta(valor, palo));

            // Mezcla Fisher-Yates con números aleatorios seguros (nadie puede predecir el orden)
            for (int i = _cartas.Count - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (_cartas[i], _cartas[j]) = (_cartas[j], _cartas[i]);
            }
        }

        // Saca la carta de arriba
        public Carta Sacar()
        {
            if (_cartas.Count == 0) Barajar();

            var carta = _cartas[^1];
            _cartas.RemoveAt(_cartas.Count - 1);
            return carta;
        }
    }
}
