using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIM_Ejercicio33
{
    public class FinReparto : Evento
    {
        private string nombreEvento = "Fin_Reparto";
        private double tiempoReparto;
        private double horaFinReparto;

        public string NombreEvento { get { return nombreEvento; } set { nombreEvento = value; } }
        public double TiempoReparto { get { return tiempoReparto; } set { this.tiempoReparto = value; } }
        public double HoraFinReparto { get { return horaFinReparto; } set { this.horaFinReparto = value; } }

        public FinReparto(double tiempoPorArticulo, int capacidad)
        {
            this.tiempoReparto = (tiempoPorArticulo * capacidad);
        }

        public override double getProximaOcurrencia()
        {
            return HoraFinReparto;
        }

        // Calcula la Hora de Reloj final en la que se producirá el Evento FinReparto.
        public override void simular(double reloj)
        {
            this.HoraFinReparto = tiempoReparto + reloj;
        }
    }
}
