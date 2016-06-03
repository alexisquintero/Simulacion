using System;
using System.Collections.Generic;
using ExcelLibrary.SpreadSheet;
using ExcelLibrary.CompoundDocumentFormat;

namespace MM1
{
    class MM1
    {
        private enum estadoDelServidor { Ocupado , Desocupado};
        private enum eventos { Arribo = 0 , Partida = 1 };

        private double reloj;
        private double finSimulacion;
        private Evento[] listaProxEvento = new Evento[2];   //Uso la enumeración eventos como ínidice
        private eventos proxEvento; 
        private double lambda;
        private double mu;
        private estadoDelServidor estadoServidor;
        private int cantidadClientesSistema;
        private int cantidadClientesCola;
        private Queue<Arribo> cola = new Queue<Arribo>();
        private GeneradorDeTiempos generador;
        private double tiempoUltimoEvento;

        //Medidas de rendimiento
        private double utilizacionDelServidor;
        private double deltaClientesEnCola;     //delta: número promedio o área bajo X(t); X siendo genérico
        private double deltaClientesEnSistema;
        private int nroClientesAtendidos;

        //Excel
        private string file;
        private Workbook workbook;
        private Worksheet worksheet;
        private int fila;

        public void programa()  //Programa principal
        {
            this.inicializacion();
            while (this.reloj <= this.finSimulacion)
            {
                this.tiempos();
                switch (proxEvento)
                {
                    case eventos.Arribo:this.arribo(); break;
                    case eventos.Partida:this.partida(); ; break;
                    default: Console.WriteLine("Ocurrió un problema"); break;
                }
            }
            this.reporte();
        }

        private void inicializacion()
        {
            this.reloj = 0;
            this.finSimulacion = 1000000;
            this.lambda = 0.02;
            this.mu = 0.08;
            this.estadoServidor = estadoDelServidor.Desocupado;
            this.generador = new GeneradorDeTiempos(lambda, mu);
            this.tiempoUltimoEvento = 0;
            this.cantidadClientesCola = 0;
            this.cantidadClientesSistema = 0;

            this.utilizacionDelServidor = 0;
            this.deltaClientesEnCola = 0;
            this.deltaClientesEnSistema = 0;
            this.nroClientesAtendidos = 0;

            //Generar Arribo
            this.listaProxEvento[(int)eventos.Arribo] = new Arribo(reloj + generador.generarArribo());
            //poner al tiempo de partida en infinito
            this.listaProxEvento[(int)eventos.Partida] = new Partida(finSimulacion * 2);

            //Archivo Excel
            fila = 5;
            file = "Simulacion.xls"; //Nombre del archivo
            workbook = new Workbook();
            worksheet = new Worksheet("Primer pagina");
            //Formato del archivo
            worksheet.Cells[0, 0] = new Cell("Simulacion de sistema MM1");
            worksheet.Cells[0, 1] = new Cell("Lambda = " + this.lambda);
            worksheet.Cells[0, 2] = new Cell("Mu = " + this.mu);
            worksheet.Cells[0, 3] = new Cell("Tiempo final de la simulacion = " + this.finSimulacion);

            worksheet.Cells[4, 0] = new Cell("Reloj");  //En las primeras 5 rows van las medidas de rendimiento    
            worksheet.Cells[4, 1] = new Cell("Estado del servidor");
            worksheet.Cells[4, 2] = new Cell("Cantidad de clientes en el sistema");
            worksheet.Cells[4, 3] = new Cell("Cantidad de clientes en cola");
            worksheet.Cells[4, 4] = new Cell("Tiempo promedio en el sistema");
            worksheet.Cells[4, 5] = new Cell("Tiempo promedio en cola");
            worksheet.Cells[4, 6] = new Cell("Clientes atendidos");

        }
        private void tiempos()
        {
            if (listaProxEvento[(int)eventos.Arribo].tiempo <= listaProxEvento[(int)eventos.Partida].tiempo)    //Compara el tiempo de arribo con el de partida de la lista
            {                                                                                                   //de la lista de próximos eventos
                this.proxEvento = eventos.Arribo;   //Asigna Arribo al próximo evento
            }
            else
            {
                this.proxEvento = eventos.Partida;  //Asigna partida al próximo evento
            }
//            this.tiempoUltimoEvento = this.reloj;   //Comentado porque se cambia en las rutinas de los eventos
            this.reloj = this.listaProxEvento[(int)this.proxEvento].tiempo; //Actualiza el reloj

            //Datos para la Spreadsheet
            worksheet.Cells[fila, 0] = new Cell(this.reloj);
            worksheet.Cells[fila, 1] = new Cell(this.estadoServidor.ToString());
            worksheet.Cells[fila, 2] = new Cell(this.cantidadClientesSistema);
            worksheet.Cells[fila, 3] = new Cell(this.cantidadClientesCola);
            worksheet.Cells[fila, 4] = new Cell(this.deltaClientesEnSistema / this.nroClientesAtendidos);
            worksheet.Cells[fila, 5] = new Cell(this.deltaClientesEnCola / this.nroClientesAtendidos);
            worksheet.Cells[fila, 6] = new Cell(this.nroClientesAtendidos);
            fila++;
        }
        private void reporte()
        {
            Console.WriteLine("reloj: {0}", this.reloj);
            Console.WriteLine("Utilización del servidor: {0}", this.utilizacionDelServidor/this.finSimulacion );
            Console.WriteLine("Tiempo promedio en cola : {0}", this.deltaClientesEnCola/this.nroClientesAtendidos);
            Console.WriteLine("Tiempo promedio en el sistema: {0}", this.deltaClientesEnSistema / this.nroClientesAtendidos);
            Console.WriteLine("Clientes atendidos: {0}", this.nroClientesAtendidos);            

            workbook.Worksheets.Add(worksheet);
            workbook.Save(file);    //Crea el archivo

            Console.Read();
        }
        private void arribo()
        {
            this.listaProxEvento[(int)eventos.Arribo] = new Arribo(this.reloj + generador.generarArribo()); //Genero próximo arribo
            if(this.estadoServidor == estadoDelServidor.Desocupado) //Compruebo estado del servidor
            {
                this.listaProxEvento[(int)eventos.Partida] = new Partida(this.reloj + generador.generarPartida());   //Genero próxima partida
                this.nroClientesAtendidos += 1; //Aumento el número de clientes atendidos/que completaron su demora
                this.estadoServidor = estadoDelServidor.Ocupado;    //Cambio el estado del servidor
                this.cantidadClientesSistema += 1;  //Aumento el número de clientes en el sistema

                this.tiempoUltimoEvento = this.reloj;   //Actualizo tiempo del último evento
            }
            else
            {
                this.deltaClientesEnCola += (this.reloj - this.tiempoUltimoEvento) * this.cantidadClientesCola; //Actualizo área bajo Q(T)
                this.deltaClientesEnSistema += (this.reloj - this.tiempoUltimoEvento) * this.cantidadClientesSistema;   //Actualizo área clientes en sistema
                this.cantidadClientesCola += 1; //Actualizo cantidad de clientes en cola
                this.cantidadClientesSistema += 1; //Aumento el número de clientes en el sistema
                this.cola.Enqueue((Arribo)this.listaProxEvento[(int)eventos.Arribo]);   //Agrego cliente a la cola
                this.utilizacionDelServidor += (this.reloj - this.tiempoUltimoEvento);   //Actualizo tiempo de utilización del servidor

                this.tiempoUltimoEvento = reloj;    //Actualizo tiempo del último evento
            }
        }
        private void partida()
        {
            if (this.cola.Count > 0)
            {
                this.listaProxEvento[(int)eventos.Partida] = new Partida(this.reloj + generador.generarPartida());  //Genero próxima partida
                this.deltaClientesEnCola += (this.reloj - this.tiempoUltimoEvento) * this.cantidadClientesCola; //Actualizo área bajo Q(t)
                this.deltaClientesEnSistema += (this.reloj - this.tiempoUltimoEvento) * this.cantidadClientesSistema;   //Actualizo área clientes en sistema
                this.utilizacionDelServidor += (this.reloj - this.tiempoUltimoEvento);   //Actualizo tiempo de utilización del servidor

                this.cola.Dequeue();    //Saco el cliente de la cola
                this.cantidadClientesCola -= 1; //Disminuyo la cantidad de clientes en la cola
                this.cantidadClientesSistema -= 1;  //Disminuyo la cantidad de clientes en el sistema
                this.nroClientesAtendidos += 1; //Aumento el número de clientes atendidos/que completaron su demora
                this.tiempoUltimoEvento = reloj;    //Actualizo tiempo del último evento
            }
            else
            {
                this.utilizacionDelServidor += (this.reloj - this.tiempoUltimoEvento);   //Actualizo tiempo de utilización del servidor
                this.listaProxEvento[(int)eventos.Partida] = new Partida(this.finSimulacion * 2);  //Genero próxima partida con tiempo infinito
                this.deltaClientesEnSistema += (this.reloj - this.tiempoUltimoEvento) * this.cantidadClientesSistema;   //Actualizo área clientes en sistema

                this.estadoServidor = estadoDelServidor.Desocupado; //Cambio el estado del servidor
                this.cantidadClientesSistema -= 1;  //Disminuyo la cantidad de clientes en el sistema
                this.tiempoUltimoEvento = reloj;    //Actualizo tiempo del último evento
            }
        }

    }
}
