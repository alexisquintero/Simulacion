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
        private double nroPromedioClientesSistema;
        private double nroPromedioClientesCola;
        private double tiempoSoloUnClienteSistema;

        //Excel
        private string file;
        private Workbook workbook;
        private Worksheet worksheet;
        private int fila;
        private int columnaReloj;
        private int columnaEstadoServidor;
        private int columnaCantidadClientesSistema;
        private int columnaCantidadClientesCola;
        private int columnaTiempoPromSistema;
        private int columnaTiempoPromCola;
        private int columnaClientesAtendidos;
        private int columnaNroPromClientesSistema;
        private int columnaNroPromClientesCola;
        private int columnaProbabilidadUnClienteSistema;

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
            this.nroPromedioClientesSistema = 0;
            this.nroPromedioClientesCola = 0;
            this.tiempoSoloUnClienteSistema = 0;

            //Generar Arribo
            this.listaProxEvento[(int)eventos.Arribo] = new Arribo(reloj + generador.generarArribo());
            //poner al tiempo de partida en infinito
            this.listaProxEvento[(int)eventos.Partida] = new Partida(finSimulacion * 2);

            //Archivo Excel
            columnaReloj = 0;
            columnaEstadoServidor = 1;
            columnaCantidadClientesSistema = 2;
            columnaCantidadClientesCola = 3;
            columnaTiempoPromSistema = 4;
            columnaTiempoPromCola = 5;
            columnaClientesAtendidos = 6;
            columnaNroPromClientesSistema = 7;
            columnaNroPromClientesCola = 8;
            columnaProbabilidadUnClienteSistema = 9;

            fila = 0;
            file = "Simulacion.xls"; //Nombre del archivo
            workbook = new Workbook();
            worksheet = new Worksheet("Primer pagina");
            //Formato del archivo
            worksheet.Cells[fila, 0] = new Cell("Simulacion de sistema MM1");
            worksheet.Cells[fila, 1] = new Cell("Lambda = " + this.lambda);
            worksheet.Cells[fila, 2] = new Cell("Mu = " + this.mu);
            worksheet.Cells[fila, 3] = new Cell("Tiempo final de la simulacion = " + this.finSimulacion);
            //En las primeras 5 rows van las medidas de rendimiento 
            fila = 4;
            worksheet.Cells[fila, columnaReloj] = new Cell("Reloj");     
            worksheet.Cells[fila, columnaEstadoServidor] = new Cell("Estado del servidor");
            worksheet.Cells[fila, columnaCantidadClientesSistema] = new Cell("Cantidad de clientes en el sistema");
            worksheet.Cells[fila, columnaCantidadClientesCola] = new Cell("Cantidad de clientes en cola");
            worksheet.Cells[fila, columnaTiempoPromSistema] = new Cell("Tiempo promedio en el sistema");
            worksheet.Cells[fila, columnaTiempoPromCola] = new Cell("Tiempo promedio en cola");
            worksheet.Cells[fila, columnaClientesAtendidos] = new Cell("Clientes atendidos");
            worksheet.Cells[fila, columnaNroPromClientesSistema] = new Cell("Número promedio de clientes en el sistema");
            worksheet.Cells[fila, columnaNroPromClientesCola] = new Cell("Número promedio de clientes en cola");
            worksheet.Cells[fila, columnaProbabilidadUnClienteSistema] = new Cell("Probabilidad de que haya 1 cliente en el sistema");
            worksheet.Cells.ColumnWidth[0] = 12000;
            worksheet.Cells.ColumnWidth[1] = 12000;
            worksheet.Cells.ColumnWidth[2] = 12000;
            worksheet.Cells.ColumnWidth[3] = 12000;
            worksheet.Cells.ColumnWidth[4] = 12000;
            worksheet.Cells.ColumnWidth[5] = 12000;
            worksheet.Cells.ColumnWidth[6] = 12000;
            worksheet.Cells.ColumnWidth[7] = 12000;
            worksheet.Cells.ColumnWidth[8] = 12000;
            worksheet.Cells.ColumnWidth[9] = 12000;

            worksheet.Cells[fila, columnaNroPromClientesSistema + 10] = new Cell("E[X^2] de número de clientes en sistema");
            worksheet.Cells.ColumnWidth[17] = 12000;
            fila = 5;
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
            this.guardarDatos();           
        }
        private void reporte()
        {
            Console.WriteLine("reloj: {0}", this.reloj);
            worksheet.Cells[1, 0] = new Cell("Reloj: " + this.reloj);
            double utilizacionServidor = this.utilizacionDelServidor / this.finSimulacion;
            Console.WriteLine("Utilización del servidor: {0}", utilizacionServidor );
            worksheet.Cells[1, 1] = new Cell("Utilización del servidor: " + utilizacionServidor.ToString());
            double tiempoPromCola = this.deltaClientesEnCola / this.nroClientesAtendidos;
            Console.WriteLine("Tiempo promedio en cola : {0}", tiempoPromCola);
            worksheet.Cells[1, 2] = new Cell("Tiempo promedio en cola : " + tiempoPromCola.ToString());
            double tiempoPromSistema = this.deltaClientesEnSistema / this.nroClientesAtendidos;
            Console.WriteLine("Tiempo promedio en el sistema: {0}", tiempoPromSistema);
            worksheet.Cells[1, 3] = new Cell("Tiempo promedio en el sistema: " + tiempoPromSistema.ToString());
            Console.WriteLine("Clientes atendidos: {0}", this.nroClientesAtendidos);
            worksheet.Cells[1, 4] = new Cell("Clientes atendidos: " + this.nroClientesAtendidos.ToString());
            double nroPromClientesSistema = this.nroPromedioClientesSistema / this.reloj;
            Console.WriteLine("Número promedio de clientes en el sistema: {0}", nroPromClientesSistema);
            worksheet.Cells[1, 5] = new Cell("Número promedio de clientes en el sistema: " + nroPromClientesSistema.ToString());
            double nroPromClientesCola = this.nroPromedioClientesCola / this.reloj;
            Console.WriteLine("Número promedio de clientes en cola: {0}", nroPromClientesCola);
            worksheet.Cells[1, 6] = new Cell("Número promedio de clientes en cola: " + nroPromClientesCola.ToString());
            double probUnClienteSistema = this.tiempoSoloUnClienteSistema / this.reloj;
            Console.WriteLine("Probabilidad de que haya 1 cliente en el sistema: {0}", probUnClienteSistema);
            worksheet.Cells[1, 7] = new Cell("Probabilidad de que haya 1 cliente en el sistema: " + probUnClienteSistema.ToString());

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
                this.nroPromedioClientesSistema += (this.reloj - this.tiempoUltimoEvento) * this.cantidadClientesSistema;   //Calcula número promedio de clientes en sistema                                              
                this.cantidadClientesSistema += 1;  //Aumento el número de clientes en el sistema

                this.tiempoUltimoEvento = this.reloj;   //Actualizo tiempo del último evento
            }
            else
            {
                this.deltaClientesEnCola += (this.reloj - this.tiempoUltimoEvento) * this.cantidadClientesCola; //Actualizo área bajo Q(T)
                this.deltaClientesEnSistema += (this.reloj - this.tiempoUltimoEvento) * this.cantidadClientesSistema;   //Actualizo área clientes en sistema
                this.nroPromedioClientesCola += (this.reloj - this.tiempoUltimoEvento) * this.cantidadClientesCola; //Calcula número promedio de clientes en cola
                this.nroPromedioClientesSistema += (this.reloj - this.tiempoUltimoEvento) * this.cantidadClientesSistema;   //Calcula número promedio de clientes en sistema 
                this.tiempoSoloUnClienteSistema += this.cantidadClientesSistema == 1 ? (this.reloj - this.tiempoUltimoEvento) : 0;    //Calcula la prob. de 1 cliente en el sistema
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
                this.nroPromedioClientesSistema += (this.reloj - this.tiempoUltimoEvento) * this.cantidadClientesSistema;   //Calcula número promedio de clientes en sistema 
                this.nroPromedioClientesCola += (this.reloj - this.tiempoUltimoEvento) * this.cantidadClientesCola; //Calcula número promedio de clientes en cola
                this.tiempoSoloUnClienteSistema += this.cantidadClientesSistema == 1 ? (this.reloj - this.tiempoUltimoEvento) : 0;    //Calcula la prob. de 1 cliente en el sistema

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
                this.nroPromedioClientesSistema += (this.reloj - this.tiempoUltimoEvento) * this.cantidadClientesSistema;   //Calcula número promedio de clientes en sistema 
                this.tiempoSoloUnClienteSistema += this.cantidadClientesSistema == 1 ? (this.reloj - this.tiempoUltimoEvento) : 0;    //Calcula la prob. de 1 cliente en el sistema

                this.estadoServidor = estadoDelServidor.Desocupado; //Cambio el estado del servidor
                this.cantidadClientesSistema -= 1;  //Disminuyo la cantidad de clientes en el sistema
                this.tiempoUltimoEvento = reloj;    //Actualizo tiempo del último evento
            }
        }
        private void guardarDatos()
        {
            worksheet.Cells[fila, columnaReloj] = new Cell(this.reloj);
            worksheet.Cells[fila, columnaEstadoServidor] = new Cell(this.estadoServidor.ToString());
            worksheet.Cells[fila, columnaCantidadClientesSistema] = new Cell(this.cantidadClientesSistema);
            worksheet.Cells[fila, columnaCantidadClientesCola] = new Cell(this.cantidadClientesCola);
            worksheet.Cells[fila, columnaTiempoPromSistema] = new Cell(this.deltaClientesEnSistema / this.nroClientesAtendidos);
            worksheet.Cells[fila, columnaTiempoPromCola] = new Cell(this.deltaClientesEnCola / this.nroClientesAtendidos);
            worksheet.Cells[fila, columnaClientesAtendidos] = new Cell(this.nroClientesAtendidos);
            worksheet.Cells[fila, columnaNroPromClientesSistema] = new Cell(this.nroPromedioClientesSistema / this.reloj);
            worksheet.Cells[fila, columnaNroPromClientesCola] = new Cell(this.nroPromedioClientesCola / this.reloj);

            //E[X^2]    restarle E[X]^2, que sería: R6 - H6^2
            worksheet.Cells[fila, columnaNroPromClientesSistema + 10] = new Cell((this.nroPromedioClientesSistema * this.nroPromedioClientesSistema) / this.reloj);

            //Probabilidad de que haya 1 cliente en el sistema
            //Tiempo en que hay 1 cliente en el sistema dividido el reloj de la simulación
            worksheet.Cells[fila, columnaProbabilidadUnClienteSistema] = new Cell(this.tiempoSoloUnClienteSistema / this.reloj);
            fila++;
        }

    }
}
