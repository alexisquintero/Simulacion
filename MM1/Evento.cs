using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MM1
{
    class Evento
    {
//        protected double tiempo;
        public double tiempo { get; set; }
        public Evento(double pTiempo) 
        {
            this.tiempo = pTiempo;
        }
    }  
}
