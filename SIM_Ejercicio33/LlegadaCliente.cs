using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIM_Ejercicio33
{
    public class LlegadaCliente : Evento
    {
        private string nombreEvento = "Llegada_Cliente";
        private double randomTiempo;
        private double tiempoEntreLlegadas;
        private double proximaLlegada;
        private DistribucionUniforme distribucion;

        private Random random = new Random();

        public string NombreEvento { get { return nombreEvento; } set { this.nombreEvento = value; } }
        public double RandomTiempo { get { return randomTiempo; } set { this.randomTiempo = value; } }
        public double TiempoEntreLlegadas { get { return tiempoEntreLlegadas; } set { this.tiempoEntreLlegadas = value; } }

        public LlegadaCliente(double inicio, double fin)
        {
            distribucion = new DistribucionUniforme(inicio, fin);
        }

        public override double getProximaOcurrencia()
        {
            return proximaLlegada;
        }

        // Calcula la Hora de Reloj final en la que se producirá el Evento LlegadaCliente.
        public override void simular(double reloj)
        {
            RandomTiempo = random.NextDouble();
            TiempoEntreLlegadas = distribucion.nuevoUniforme(RandomTiempo);
            proximaLlegada = TiempoEntreLlegadas + reloj;
        }
    }
}
