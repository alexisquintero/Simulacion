using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MM1
{
    class CantCli
    {
        protected int cantCli;
        protected double tiempo;

        public CantCli(int cant, double pTiempo)
        {
            this.cantCli = cant;
            this.tiempo = pTiempo;
        }
    }
}
