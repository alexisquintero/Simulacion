﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MM1
{
    class Program
    {
        static void Main(string[] args)
        {
            MM1 mm1 = new MM1();
            int i = 0;
            int replicas = 50; //Indica el nro de replicas a realizar
            for (i = 0; i < replicas; i++) //Para hacer varias réplicas
            {         
            mm1.programa(i);
            }
            
        }
    }
}
