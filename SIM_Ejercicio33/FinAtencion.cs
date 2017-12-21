using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIM_Ejercicio33
{
    public class FinAtencion : Evento
    {
        private string nombreEvento;
        private double randomTiempo;
        private double tiempoAtencion;
        private double horaFinAtencion;
        private DistribucionUniforme distribucion;
        private Random random = new Random();

        public string NombreEvento { get { return nombreEvento; } set { nombreEvento = value; } }
        public double RandomTiempo { get { return randomTiempo; } set { this.randomTiempo = value; } }
        public double TiempoAtencion { get { return tiempoAtencion; } set { this.tiempoAtencion = value; } }
        public double HoraFinAtencion { get { return horaFinAtencion; } set { this.horaFinAtencion = value; } }

        public FinAtencion(string numero_vendedor)
        {
            this.nombreEvento = "FinAtencion V." + numero_vendedor;
        }

        public void modificarDistribucion(double uniformeInicio, double uniformeFin)
        {
            distribucion = new DistribucionUniforme(uniformeInicio, uniformeFin);
        }

        public override double getProximaOcurrencia()
        {
            return horaFinAtencion;
        }

        // Calcula la Hora de Reloj final en la que se producirá el Evento FinCentroA.
        public override void simular(double reloj)
        {
            randomTiempo = random.NextDouble();
            tiempoAtencion = distribucion.nuevoUniforme(randomTiempo);
            HoraFinAtencion = tiempoAtencion + reloj;
        }
    }
}
