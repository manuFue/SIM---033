using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIM_Ejercicio33
{
    public class DistribucionUniforme
    {
        private double limiteInferior = 0;
        private double limiteSuperior = 0;
        
        public DistribucionUniforme(double inferior, double superior)
        {
            this.limiteInferior = inferior;
            this.limiteSuperior = superior;
        }

        public double nuevoUniforme(double random)
        {
            double x = 0;
            x = limiteInferior + ((limiteSuperior - limiteInferior) * random);
            return x;
        }
    }
}
