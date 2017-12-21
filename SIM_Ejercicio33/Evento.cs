using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIM_Ejercicio33
{
    public abstract class Evento : IComparable<Evento>
    {
        public abstract double getProximaOcurrencia();
        public abstract void simular(double reloj);

        // Permite que cada evento se compare a otro según su Atributo "próximaLlegada".
        public int CompareTo(Evento otroEvento)
        {
            return Math.Sign(this.getProximaOcurrencia() - otroEvento.getProximaOcurrencia());
        }

        // Obtiene el siguiente EVENTO a ocurrir según el Atributo "próximaLlegada" de cada uno de los eventos, comparándolos.
        public Evento getSiguienteEvento(Evento otroEvento)
        {
            if (getProximaOcurrencia() == 0.00)
            {
                return otroEvento;
            }
            else if (otroEvento == null || otroEvento.getProximaOcurrencia() == 0.00)
            {
                return this;
            }
            else if (this.getProximaOcurrencia() < otroEvento.getProximaOcurrencia())
            {
                return this;
            }
            else
            {
                return otroEvento;
            }

        }
    }
}
