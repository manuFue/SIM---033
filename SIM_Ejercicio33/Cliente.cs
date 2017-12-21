using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIM_Ejercicio33
{
    public class Cliente
    {
        private string id;
        private _EstadosClientes estado;
        private double horaFinVenta;

        public string Id { get { return id; } set { this.id = value; } }
        public _EstadosClientes Estado { get { return estado; } set { this.estado = value; } }
        public double HoraFinVenta { get { return horaFinVenta; } set { this.horaFinVenta = value; } }

        public Cliente(string id)
        {
            this.Id = id;
        }
    }
}
