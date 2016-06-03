using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MM1
{
    class GeneradorDeTiempos
    {
        private double lambda;
        private double mu;
        private static Random random = new Random();

        public GeneradorDeTiempos(double pLambda, double pMu)
        {
            this.lambda = pLambda;
            this.mu = pMu;
        }

        public double generarArribo()
        { 
            double U = random.NextDouble();           
            return (-1 * Math.Log(U))/lambda;
        }

        public double generarPartida()
        {
            double U = random.NextDouble();
            return (-1 * Math.Log(U)) / mu;
        }
    }
}
