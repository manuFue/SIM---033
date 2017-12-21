using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SIM_Ejercicio33
{
    public partial class Pantalla : Form
    {
        private bool comoSimular; // TRUE = Por Minutos | FALSE = Por Eventos
        private double[] tiemposSimulacion = new double[3]; // [ CUANTO simular | DESDE cuando mostrar | HASTA cuando mostrar ]
        private int[] eventosSimulacion = new int[3]; // [ CUANTO simular | DESDE cuando mostrar | HASTA cuando mostrar ]
        private string[] vectorAnterior = new string[23];
        private string[] vectorPresente = new string[23];

        private double[] probabilidades = new double[2]; // [ PROB EFECTIVO | PROB CREDITO ]
        private double[] distribucion_CompraEfectivo = new double[2]; // [ LIMITES DISTRIBUCIÓN UNIFORME ]
        private double[] distribucion_CompraCredito = new double[2]; // [ LIMITES DISTRIBUCIÓN UNIFORME ]
        private double[] distribucion_Cliente = new double[2]; // [ LIMITES DISTRIBUCIÓN UNIFORME ]
        private int tiempoEntrega;
        private int capacidadDeCarga;

        private LlegadaCliente llegada_Cliente;
        private FinAtencion fin_Atencion_V1;
        private FinAtencion fin_Atencion_V2;
        private FinReparto fin_Reparto;
        private List<Cliente> clientesActuales = new List<Cliente>();

        private double relojCentral = 0.00;
        private int eventosSimulados = 0;
        private int cantidadClientes = 0;
        private bool mostrarClientes = false;

        private Random random = new Random();

        public Pantalla()
        {
            InitializeComponent();
        }

        // BOTONES

        private void btn_Simular_Click(object sender, EventArgs e)
        {
            activarCursores(true);
            reiniciarTabla();

            try
            { simularVenta(); }
            catch
            {
                dgv_VentaArticulos.Rows.Clear();
                reiniciarTabla();

                MessageBox.Show("Con los parámetros ingresados, es imposible mostrar el estado de todos los clientes que interactúan con el sistema." + Environment.NewLine + Environment.NewLine
                    + "Por favor, desmarque la casilla 'Mostrar Clientes' para ejecutar la simulación requerida, o cambie los valores ingresados.", "Simulación Cancelada", MessageBoxButtons.OK);
            }

            mostrarResultado();
            dgv_VentaArticulos.ClearSelection();
            activarCursores(false);
        }

        private void btn_Simular_Disponible(bool disponible)
        {
            btn_Simular.Enabled = disponible;
            if (disponible)
                btn_Simular.BackColor = System.Drawing.Color.MediumTurquoise;
            else
                btn_Simular.BackColor = System.Drawing.Color.Silver;
        }

        private void btn_Reiniciar_Click(object sender, EventArgs e)
        {
            reiniciarPantalla();
            reiniciarTabla();
            estado_panelResultado(false);
        }

        // SIMULAR //

        private void simularVenta()
        {
            relojCentral = 0.00;
            eventosSimulados = 0;
            cantidadClientes = 0;
            clientesActuales = new List<Cliente>();
            vectorAnterior = new string[24];
            vectorPresente = new string[24];
            bool bandera_ClientesActivosAgregados = false;

            cargarParametros();
            inicializarEventos();
            setearVectorAnterior();

            dgv_VentaArticulos.Rows.Clear();

            if ((comoSimular && tiemposSimulacion[0] == 0) || ((!comoSimular) && eventosSimulacion[0] == 0))
                return;

            llegada_Cliente.simular(relojCentral);
            mostrarPrimeraFila(llegada_Cliente.RandomTiempo, llegada_Cliente.TiempoEntreLlegadas, llegada_Cliente.getProximaOcurrencia());

            while (relojCentral <= tiemposSimulacion[0] || eventosSimulados <= eventosSimulacion[0])
            {
                eventosSimulados++;

                Evento siguiente = llegada_Cliente.getSiguienteEvento(fin_Atencion_V1.getSiguienteEvento(fin_Atencion_V2.getSiguienteEvento(fin_Reparto)));
                relojCentral = siguiente.getProximaOcurrencia();

                vectorPresente[0] = Math.Round(relojCentral, 2).ToString();

                if (siguiente is LlegadaCliente)
                {
                    vectorPresente[1] = ((LlegadaCliente)siguiente).NombreEvento;
                    llegada_Cliente.simular(relojCentral);

                    cantidadClientes++;
                    Cliente clienteNuevo = new Cliente("Cliente_" + cantidadClientes);

                    vectorPresente[2] = Math.Round(llegada_Cliente.RandomTiempo, 4).ToString();
                    vectorPresente[3] = Math.Round(llegada_Cliente.TiempoEntreLlegadas, 2).ToString();
                    vectorPresente[4] = Math.Round(llegada_Cliente.getProximaOcurrencia(), 2).ToString();

                    if (vectorAnterior[19] == "0" && (vectorAnterior[17] == "Libre" || vectorAnterior[18] == "Libre"))
                    {
                        double randomTipoVenta = random.NextDouble();
                        vectorPresente[5] = Math.Round(randomTipoVenta, 4).ToString();
                        vectorPresente[6] = obtenerTipoVenta(randomTipoVenta);
                        if (vectorAnterior[17] == "Libre")
                        {
                            cambiarDistribucion(fin_Atencion_V1, vectorPresente[6]);
                            fin_Atencion_V1.simular(relojCentral);

                            vectorPresente[7] = Math.Round(fin_Atencion_V1.RandomTiempo, 4).ToString();
                            vectorPresente[8] = Math.Round(fin_Atencion_V1.TiempoAtencion, 2).ToString();
                            vectorPresente[9] = Math.Round(fin_Atencion_V1.getProximaOcurrencia(), 2).ToString();
                            vectorPresente[10] = "-";
                            vectorPresente[11] = "-";
                            vectorPresente[12] = vectorAnterior[12];

                            vectorPresente[17] = "Ocupado";
                            vectorPresente[18] = vectorAnterior[18];

                            clienteNuevo.Estado = _EstadosClientes.Siendo_Atendido_V1;
                        }
                        else
                        {
                            cambiarDistribucion(fin_Atencion_V2, vectorPresente[6]);
                            fin_Atencion_V2.simular(relojCentral);

                            vectorPresente[7] = "-";
                            vectorPresente[8] = "-";
                            vectorPresente[9] = vectorAnterior[9];
                            vectorPresente[10] = Math.Round(fin_Atencion_V2.RandomTiempo, 4).ToString();
                            vectorPresente[11] = Math.Round(fin_Atencion_V2.TiempoAtencion, 2).ToString();
                            vectorPresente[12] = Math.Round(fin_Atencion_V2.getProximaOcurrencia(), 2).ToString();

                            vectorPresente[17] = vectorAnterior[17];
                            vectorPresente[18] = "Ocupado";

                            clienteNuevo.Estado = _EstadosClientes.Siendo_Atendido_V2;
                        }
                        vectorPresente[19] = "0";
                    }
                    else
                    {
                        for (int i = 5; i <= 12; i++)
                            vectorPresente[i] = "-";
                        vectorPresente[9] = vectorAnterior[9];
                        vectorPresente[12] = vectorAnterior[12];

                        vectorPresente[17] = vectorAnterior[17];
                        vectorPresente[18] = vectorAnterior[18];
                        vectorPresente[19] = (Convert.ToInt32(vectorAnterior[19]) + 1).ToString();

                        clienteNuevo.Estado = _EstadosClientes.En_Cola;
                    }

                    for (int i = 13; i <= 15; i++)
                        vectorPresente[i] = "-";
                    vectorPresente[16] = vectorAnterior[16];

                    for (int i = 20; i <= 23; i++)
                        vectorPresente[i] = vectorAnterior[i];

                    if (mostrarClientes)
                    {
                        if (bandera_ClientesActivosAgregados)
                        {
                            if ((relojCentral >= tiemposSimulacion[1] && relojCentral <= tiemposSimulacion[2] && relojCentral != 0.00) || (eventosSimulados >= eventosSimulacion[1] && eventosSimulados <= eventosSimulacion[2]))
                                agregarCliente_Tabla(clienteNuevo);
                        }
                    }

                    clientesActuales.Add(clienteNuevo);
                }

                else if (siguiente is FinAtencion)
                {
                    vectorPresente[1] = ((FinAtencion)siguiente).NombreEvento;

                    int vendedor = Convert.ToInt16(((FinAtencion)siguiente).NombreEvento.Split('.')[1]);

                    vectorPresente[2] = "-";
                    vectorPresente[3] = "-";
                    vectorPresente[4] = vectorAnterior[4];

                    int indexLista = determinarClienteAtendido(vendedor, clientesActuales);

                    for (int i = 5; i <= 15; i++)
                        vectorPresente[i] = "-";

                    if (vectorAnterior[19] == "0")
                    {
                        if (vendedor == 1)
                        {
                            vectorPresente[12] = vectorAnterior[12];
                            vectorPresente[17] = "Libre";
                            vectorPresente[18] = vectorAnterior[18];
                            fin_Atencion_V1.HoraFinAtencion = 0.00;
                        }
                        else
                        {
                            vectorPresente[9] = vectorAnterior[9];
                            vectorPresente[17] = vectorAnterior[17];
                            vectorPresente[18] = "Libre";
                            fin_Atencion_V2.HoraFinAtencion = 0.00;
                        }
                        vectorPresente[19] = "0";
                    }
                    else
                    {
                        int clienteIndex = clientesActuales.FindIndex(x => x.Estado == _EstadosClientes.En_Cola);

                        double randomTipoVenta = random.NextDouble();
                        vectorPresente[5] = Math.Round(randomTipoVenta, 4).ToString();
                        vectorPresente[6] = obtenerTipoVenta(randomTipoVenta);

                        if (vendedor == 1)
                        {
                            clientesActuales[clienteIndex].Estado = _EstadosClientes.Siendo_Atendido_V1;
                            cambiarDistribucion(fin_Atencion_V1, vectorPresente[6]);
                            fin_Atencion_V1.simular(relojCentral);

                            vectorPresente[7] = Math.Round(fin_Atencion_V1.RandomTiempo, 4).ToString();
                            vectorPresente[8] = Math.Round(fin_Atencion_V1.TiempoAtencion, 2).ToString();
                            vectorPresente[9] = Math.Round(fin_Atencion_V1.getProximaOcurrencia(), 2).ToString();
                            vectorPresente[10] = "-";
                            vectorPresente[11] = "-";
                            vectorPresente[12] = vectorAnterior[12];
                        }
                        else
                        {
                            clientesActuales[clienteIndex].Estado = _EstadosClientes.Siendo_Atendido_V2;
                            cambiarDistribucion(fin_Atencion_V2, vectorPresente[6]);
                            fin_Atencion_V2.simular(relojCentral);

                            vectorPresente[7] = "-";
                            vectorPresente[8] = "-";
                            vectorPresente[9] = vectorAnterior[9];
                            vectorPresente[10] = Math.Round(fin_Atencion_V2.RandomTiempo, 4).ToString();
                            vectorPresente[11] = Math.Round(fin_Atencion_V2.TiempoAtencion, 2).ToString();
                            vectorPresente[12] = Math.Round(fin_Atencion_V2.getProximaOcurrencia(), 2).ToString();
                        }

                        vectorPresente[17] = vectorAnterior[17];
                        vectorPresente[18] = vectorAnterior[18];
                        vectorPresente[19] = ((Convert.ToInt64(vectorAnterior[19])) - 1).ToString();
                    }

                    double randomCantidadArticulos = random.NextDouble();
                    vectorPresente[13] = Math.Round(randomCantidadArticulos, 4).ToString();
                    vectorPresente[14] = obtenerCantidadArtículos(randomCantidadArticulos).ToString();

                    clientesActuales[indexLista].HoraFinVenta = relojCentral;
                    if (vectorPresente[14] == "1")
                        clientesActuales[indexLista].Estado = _EstadosClientes.Esperando_1_ART;
                    else
                        clientesActuales[indexLista].Estado = _EstadosClientes.Esperando_2_ART;

                    int cantidadParaRepartir = Convert.ToInt16(vectorPresente[14].ToString()) + Convert.ToInt32(vectorAnterior[21].ToString());
                    vectorPresente[15] = "-";
                    vectorPresente[16] = vectorAnterior[16];
                    vectorPresente[20] = vectorAnterior[20];
                    vectorPresente[21] = cantidadParaRepartir.ToString();

                    if (vectorAnterior[20] == "Libre")
                    {
                        if (cantidadParaRepartir >= capacidadDeCarga)
                        {
                            fin_Reparto.simular(relojCentral);
                            vectorPresente[15] = Math.Round(fin_Reparto.TiempoReparto, 2).ToString();
                            vectorPresente[16] = Math.Round(fin_Reparto.getProximaOcurrencia(), 2).ToString();
                            vectorPresente[20] = "Repartiendo";
                            vectorPresente[21] = (cantidadParaRepartir - capacidadDeCarga).ToString();
                        }
                    }

                    vectorPresente[22] = vectorAnterior[22];
                    vectorPresente[23] = vectorAnterior[23];
                }

                else if (siguiente is FinReparto)
                {
                    vectorPresente[1] = ((FinReparto)siguiente).NombreEvento;

                    for (int i = 2; i <= 12; i++)
                        vectorPresente[i] = "-";
                    vectorPresente[4] = vectorAnterior[4];
                    vectorPresente[9] = vectorAnterior[9];
                    vectorPresente[12] = vectorAnterior[12];
                    vectorPresente[17] = vectorAnterior[17];
                    vectorPresente[18] = vectorAnterior[18];
                    vectorPresente[19] = vectorAnterior[19];

                    if ((Convert.ToInt32(vectorAnterior[21]) >= 4))
                    {
                        fin_Reparto.simular(relojCentral);
                        vectorPresente[15] = Math.Round(fin_Reparto.TiempoReparto, 2).ToString();
                        vectorPresente[16] = Math.Round(fin_Reparto.getProximaOcurrencia(), 2).ToString();
                        vectorPresente[20] = vectorAnterior[20];
                        vectorPresente[21] = (Convert.ToInt32(vectorAnterior[21]) - capacidadDeCarga).ToString();
                    }
                    else
                    {
                        fin_Reparto.HoraFinReparto = 0.00;
                        vectorPresente[15] = "-";
                        vectorPresente[16] = "-";
                        vectorPresente[20] = "Libre";
                        vectorPresente[21] = vectorAnterior[21];
                    }

                    vectorPresente[22] = (Convert.ToInt64(vectorAnterior[22]) + capacidadDeCarga).ToString();

                    repartirArticulos();
                }

                if (mostrarClientes)
                {
                    if (!bandera_ClientesActivosAgregados)
                    {
                        if ((relojCentral >= tiemposSimulacion[1] && relojCentral <= tiemposSimulacion[2] && relojCentral != 0.00) || (eventosSimulados >= eventosSimulacion[1] && eventosSimulados <= eventosSimulacion[2]))
                        {
                            agregarClientesActivos_Tabla(clientesActuales);
                            bandera_ClientesActivosAgregados = true;
                        }
                    }
                }

                if ((relojCentral >= tiemposSimulacion[1] && relojCentral <= tiemposSimulacion[2]) || (eventosSimulados >= eventosSimulacion[1] && eventosSimulados <= eventosSimulacion[2]))
                {
                    mostrarNuevaFila(vectorPresente, clientesActuales);
                    if (dgv_VentaArticulos.RowCount != 2)
                        pintar_EventoAnterior(siguiente, false);
                }
                if ((relojCentral <= tiemposSimulacion[0]) || (eventosSimulados < eventosSimulacion[2]))
                    intercambiarVectores();
            }

            Evento eventoFuturo = llegada_Cliente.getSiguienteEvento(fin_Atencion_V1.getSiguienteEvento(fin_Atencion_V2.getSiguienteEvento(fin_Reparto)));
            pintar_EventoAnterior(eventoFuturo, true);
        }

        // VECTORES

        private void intercambiarVectores()
        {
            for (int i = 0; i < vectorAnterior.Length; i++)
                vectorAnterior[i] = vectorPresente[i];
            vectorPresente = new string[24];
        }

        private void setearVectorAnterior()
        {
            vectorAnterior[0] = "0.00";
            vectorAnterior[1] = "-";
            vectorAnterior[2] = "-";
            vectorAnterior[3] = "-";
            vectorAnterior[4] = "-";
            vectorAnterior[5] = "-";
            vectorAnterior[6] = "-";
            vectorAnterior[7] = "-";
            vectorAnterior[8] = "-";
            vectorAnterior[9] = "-";
            vectorAnterior[10] = "-";
            vectorAnterior[11] = "-";
            vectorAnterior[12] = "-";
            vectorAnterior[13] = "-";
            vectorAnterior[14] = "-";
            vectorAnterior[15] = "-";
            vectorAnterior[16] = "-";
            vectorAnterior[17] = "Libre";
            vectorAnterior[18] = "Libre";
            vectorAnterior[19] = "0";
            vectorAnterior[20] = "Libre";
            vectorAnterior[21] = "0";
            vectorAnterior[22] = "0";
            vectorAnterior[23] = "0.00";
        }

        // FUNCIONALIDAD SIMULACIÓN

        private void cambiarDistribucion(FinAtencion evento, string tipoVenta)
        {
            if (tipoVenta == "EFECTIVO")
                evento.modificarDistribucion(distribucion_CompraEfectivo[0], distribucion_CompraEfectivo[1]);
            else
                evento.modificarDistribucion(distribucion_CompraCredito[0], distribucion_CompraCredito[1]);
        }

        private void cargarParametros()
        {
            comoSimular = radiob_porTiempo.Checked;
            if (comoSimular)
            {
                tiemposSimulacion[0] = Convert.ToDouble(txt_minutosSimulacion.Text);
                tiemposSimulacion[1] = Convert.ToDouble(txt_minutosDesde.Text);
                tiemposSimulacion[2] = Convert.ToDouble(txt_minutosHasta.Text);
                eventosSimulacion[0] = 0;
                eventosSimulacion[1] = 0;
                eventosSimulacion[2] = 0;
            }
            else
            {
                eventosSimulacion[0] = Convert.ToInt32(txt_eventosSimulacion.Text);
                eventosSimulacion[1] = Convert.ToInt32(txt_eventosDesde.Text);
                eventosSimulacion[2] = Convert.ToInt32(txt_eventosHasta.Text);
                tiemposSimulacion[0] = 0.00;
                tiemposSimulacion[1] = 0.00;
                tiemposSimulacion[2] = 0.00;
            }

            probabilidades[0] = Convert.ToDouble(txt_prob_Credito.Text);
            probabilidades[1] = Convert.ToDouble(txt_prob_Efectivo.Text);
            distribucion_CompraCredito[0] = Convert.ToDouble(txt_uniformeCredito_Desde.Text);
            distribucion_CompraCredito[1] = Convert.ToDouble(txt_uniformeCredito_Hasta.Text);
            distribucion_CompraEfectivo[0] = Convert.ToDouble(txt_uniformeEfectivo_Desde.Text);
            distribucion_CompraEfectivo[1] = Convert.ToDouble(txt_uniformeEfectivo_Hasta.Text);
            distribucion_Cliente[0] = Convert.ToDouble(txt_cliente_Desde.Text);
            distribucion_Cliente[1] = Convert.ToDouble(txt_cliente_Hasta.Text);
            tiempoEntrega = Convert.ToInt32(txt_tiempoEntrega.Text);
            capacidadDeCarga = Convert.ToInt32(txt_capacidadCarga.Text);
            mostrarClientes = cbox_mostrarClientes.Checked;
        }

        private int determinarClienteAtendido(int numeroVendedor, List<Cliente> clientes)
        {
            int index = 0;
            if (numeroVendedor == 1)
                index = clientes.FindIndex(x => x.Estado == _EstadosClientes.Siendo_Atendido_V1);
            else
                index = clientes.FindIndex(x => x.Estado == _EstadosClientes.Siendo_Atendido_V2);
            return index;
        }

        private void inicializarEventos()
        {
            llegada_Cliente = new LlegadaCliente(distribucion_Cliente[0], distribucion_Cliente[1]);
            fin_Atencion_V1 = new FinAtencion("1");
            fin_Atencion_V2 = new FinAtencion("2");
            fin_Reparto = new FinReparto(tiempoEntrega, capacidadDeCarga);
        }

        private int obtenerCantidadArtículos(double numeroRandom)
        {
            if (numeroRandom <= 0.50)
                return 1;
            else
                return 2;
        }

        private string obtenerTipoVenta(double numeroRandom)
        {
            if (numeroRandom <= probabilidades[0])
                return "CRÉDITO";
            else
                return "EFECTIVO";
        }

        private void repartirArticulos()
        {
            int cantidad = capacidadDeCarga;

            vectorPresente[23] = vectorAnterior[23];
            while (cantidad > 0)
            {
                int indexLista;
                List<Cliente> listaAuxiliar = new List<Cliente>();
                listaAuxiliar = clientesActuales.OrderBy(x => x.HoraFinVenta).ToList();
                indexLista = listaAuxiliar.FindIndex(x => x.HoraFinVenta != 0);

                if (listaAuxiliar[indexLista].Estado == _EstadosClientes.Esperando_1_ART)
                {
                    cantidad--;
                    vectorPresente[23] = ((Convert.ToDouble(vectorPresente[23])) + (relojCentral - listaAuxiliar[indexLista].HoraFinVenta)).ToString();
                    clientesActuales.Remove(listaAuxiliar[indexLista]);
                }
                else
                {
                    if (cantidad >= 2)
                    {
                        vectorPresente[23] = ((Convert.ToDouble(vectorPresente[23])) + ((relojCentral - listaAuxiliar[indexLista].HoraFinVenta) * 2)).ToString();
                        cantidad = cantidad - 2;
                        clientesActuales.Remove(listaAuxiliar[indexLista]);
                    }
                    else
                    {
                        vectorPresente[23] = ((Convert.ToDouble(vectorPresente[23])) + (relojCentral - listaAuxiliar[indexLista].HoraFinVenta)).ToString();
                        cantidad--;
                        string id = listaAuxiliar[indexLista].Id;
                        clientesActuales.Find(x => x.Id == id).Estado = _EstadosClientes.Esperando_1_ART;
                    }
                }
            }

            vectorPresente[23] = Math.Round(Convert.ToDouble(vectorPresente[23]), 2).ToString();
        }

        // FUNCIONALIDAD TABLA

        private void agregarCliente_Tabla(Cliente esteCliente)
        {
            dgv_VentaArticulos.Columns.Add(esteCliente.Id.ToString(), "Estado Cliente" + esteCliente.Id.Split('_')[1]);
            dgv_VentaArticulos.Columns[esteCliente.Id.ToString()].Width = 110;
            dgv_VentaArticulos.Columns.Add("horaFinVenta" + esteCliente.Id.Split('_')[1], "Fin Venta" + esteCliente.Id.Split('_')[1]);
            dgv_VentaArticulos.Columns["horaFinVenta" + esteCliente.Id.Split('_')[1]].Width = 50;
        }

        private void agregarClientesActivos_Tabla(List<Cliente> listaClientes)
        {
            List<Cliente> estaLista = listaClientes;
            estaLista.Sort((p, q) => Convert.ToInt64(p.Id.Split('_')[1]).CompareTo(Convert.ToInt64(q.Id.Split('_')[1])));
            foreach (Cliente clienteActivo in estaLista)
            {
                dgv_VentaArticulos.Columns.Add(clienteActivo.Id.ToString(), "Estado Cliente" + clienteActivo.Id.Split('_')[1]);
                dgv_VentaArticulos.Columns[clienteActivo.Id.ToString()].Width = 110;
                dgv_VentaArticulos.Columns.Add("horaFinVenta" + clienteActivo.Id.Split('_')[1], "Fin Venta" + clienteActivo.Id.Split('_')[1]);
                dgv_VentaArticulos.Columns["horaFinVenta" + clienteActivo.Id.Split('_')[1]].Width = 50;
            }
        }

        private void mostrarPrimeraFila(double rnd_Llegada, double tiempo_Llegada, double hora_Llegada)
        {
            dgv_VentaArticulos.Rows.Add("0.00", "Inicio Simulación", Math.Round(rnd_Llegada, 4).ToString(), Math.Round(tiempo_Llegada, 2).ToString(), Math.Round(hora_Llegada, 2).ToString(), "-", "-", "-", "-", "-", "-", "-", "-", "-", "-", "-", "-", "Libre", "Libre", "0", "Libre", "0", "0", "0.00");
            dgv_VentaArticulos.Rows[0].DefaultCellStyle.BackColor = System.Drawing.Color.MediumTurquoise;
            dgv_VentaArticulos.Rows[0].Cells[4].Style.BackColor = System.Drawing.Color.GreenYellow;
        }

        private void mostrarNuevaFila(string[] vectorAMostrar, List<Cliente> listaClientes)
        {
            int i;
            i = dgv_VentaArticulos.Rows.Add(vectorAMostrar[0], vectorAMostrar[1], vectorAMostrar[2], vectorAMostrar[3], vectorAMostrar[4],
                vectorAMostrar[5], vectorAMostrar[6], vectorAMostrar[7], vectorAMostrar[8], vectorAMostrar[9], vectorAMostrar[10],
                vectorAMostrar[11], vectorAMostrar[12], vectorAMostrar[13], vectorAMostrar[14], vectorAMostrar[15], vectorAMostrar[16],
                vectorAMostrar[17], vectorAMostrar[18], vectorAMostrar[19], vectorAMostrar[20], vectorAMostrar[21], vectorAMostrar[22],
                vectorAMostrar[23]);

            if (mostrarClientes)
            {
                if ((relojCentral >= tiemposSimulacion[1] && relojCentral <= tiemposSimulacion[2] && relojCentral != 0.00) || (eventosSimulados >= eventosSimulacion[1] && eventosSimulados <= eventosSimulacion[2]))
                    mostrarClientesActivos(listaClientes, i);
            }
        }

        private void mostrarClientesActivos(List<Cliente> lista, int numeroFila)
        {
            foreach (Cliente cliente in lista)
            {
                string horaFin;
                dgv_VentaArticulos.Rows[numeroFila].Cells[cliente.Id].Value = cliente.Estado;
                if (cliente.HoraFinVenta != 0)
                    horaFin = Math.Round(cliente.HoraFinVenta, 2).ToString();
                else
                    horaFin = "-";
                dgv_VentaArticulos.Rows[numeroFila].Cells["horaFinVenta" + cliente.Id.Split('_')[1]].Value = horaFin;
            }
        }

        private void pintar_EventoAnterior(Evento actual, bool finalizo)
        {
            int i;
            if (actual is LlegadaCliente)
                i = 4;
            else if (actual is FinAtencion)
            {
                if (((FinAtencion)actual).NombreEvento.Split('.')[1] == "1")
                    i = 9;
                else
                    i = 12;
            }
            else
                i = 16;

            if (finalizo)
                dgv_VentaArticulos.Rows[dgv_VentaArticulos.RowCount - 1].Cells[i].Style.BackColor = System.Drawing.Color.GreenYellow;
            else
                dgv_VentaArticulos.Rows[dgv_VentaArticulos.RowCount - 2].Cells[i].Style.BackColor = System.Drawing.Color.GreenYellow;
        }

        private void reiniciarTabla()
        {
            dgv_VentaArticulos.Rows.Clear();
            if (dgv_VentaArticulos.ColumnCount > 24)
            {
                int columnasExtra = dgv_VentaArticulos.ColumnCount - 24;
                int primerCliente = Convert.ToInt32(dgv_VentaArticulos.Columns[24].Name.Split('_')[1]);
                if (columnasExtra > 0)
                {
                    for (int i = primerCliente; i <= (primerCliente + (columnasExtra / 2)); i++)
                    {
                        try
                        {
                            dgv_VentaArticulos.Columns.Remove("Cliente_" + i);
                            dgv_VentaArticulos.Columns.Remove("horaFinVenta" + i);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        // CONTROLES DE PANTALLA //

        private void activarCursores(bool estado)
        {
            if (estado)
            {
                this.Cursor = Cursors.WaitCursor;
                dgv_VentaArticulos.Cursor = Cursors.WaitCursor;
            }
            else
            {
                this.Cursor = Cursors.Arrow;
                dgv_VentaArticulos.Cursor = Cursors.Hand;
            }
        }

        private void estado_txtSimulacion(bool estado)
        {
            radiob_porTiempo.Checked = estado;
            txt_minutosSimulacion.Enabled = estado;
            txt_minutosDesde.Enabled = estado;
            txt_minutosHasta.Enabled = estado;

            radiob_porEventos.Checked = !estado;
            txt_eventosSimulacion.Enabled = !estado;
            txt_eventosDesde.Enabled = !estado;
            txt_eventosHasta.Enabled = !estado;

            if (estado)
            {
                txt_minutosSimulacion.Text = "500";
                txt_minutosDesde.Text = "400";
                txt_minutosHasta.Text = "500";
                txt_eventosSimulacion.Text = "";
                txt_eventosDesde.Text = "";
                txt_eventosHasta.Text = "";
            }
            else
            {
                txt_minutosSimulacion.Text = "";
                txt_minutosDesde.Text = "";
                txt_minutosHasta.Text = "";
                txt_eventosSimulacion.Text = "100";
                txt_eventosDesde.Text = "80";
                txt_eventosHasta.Text = "100";
            }
        }

        private void estado_panelResultado(bool estado)
        {
            panel_resultado.Visible = estado;
        }

        private void txt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((TextBox)sender) == txt_prob_Credito || ((TextBox)sender) == txt_prob_Efectivo)
            {
                if (Char.IsNumber(e.KeyChar) || e.KeyChar == (Char)Keys.Back || e.KeyChar == '.')
                    e.Handled = false;
                else
                    e.Handled = true;
            }
            else
            {
                if (Char.IsNumber(e.KeyChar) || e.KeyChar == (Char)Keys.Back)
                    e.Handled = false;
                else
                    e.Handled = true;
            }
        }

        private void mostrarResultado()
        {
            if (dgv_VentaArticulos.RowCount != 0)
            {
                if (vectorPresente[23] == "0.00")
                    txt_Resultado.Text = "0.00";
                else
                    txt_Resultado.Text = Math.Round((Convert.ToDouble(vectorPresente[23]) / (Convert.ToDouble(vectorPresente[22]))), 2).ToString() + " min.";
                estado_panelResultado(true);
            }
            else
                estado_panelResultado(false);
        }

        private void radiob_porTiempo_CheckedChanged(object sender, EventArgs e)
        {
            estado_txtSimulacion(radiob_porTiempo.Checked);
        }

        private void reiniciarPantalla()
        {
            estado_txtSimulacion(true);

            txt_prob_Credito.Text = "0.75";
            txt_prob_Efectivo.Text = "0.25";
            txt_uniformeCredito_Desde.Text = "8";
            txt_uniformeCredito_Hasta.Text = "12";
            txt_uniformeEfectivo_Desde.Text = "16";
            txt_uniformeEfectivo_Hasta.Text = "24";
            txt_cliente_Desde.Text = "11";
            txt_cliente_Hasta.Text = "15";
            txt_tiempoEntrega.Text = "10";
            txt_capacidadCarga.Text = "4";
        }

        // EVENTOS - LOST_FOCUS - VALIDANTES //

        private void txt_cliente_Desde_LostFocus(object sender, EventArgs e)
        {
            if (txt_cliente_Desde.Text == "")
                txt_cliente_Desde.Text = "11";
            if (Convert.ToInt64(txt_cliente_Desde.Text) > Convert.ToInt64(txt_cliente_Hasta.Text))
                txt_cliente_Desde.Text = txt_cliente_Hasta.Text;
        }

        private void txt_cliente_Hasta_LostFocus(object sender, EventArgs e)
        {
            if (txt_cliente_Hasta.Text == "")
                txt_cliente_Hasta.Text = "15";
            if (Convert.ToInt64(txt_cliente_Hasta.Text) < Convert.ToInt64(txt_cliente_Desde.Text))
                txt_cliente_Hasta.Text = txt_cliente_Desde.Text;
        }

        private void txt_uniformeCredito_Desde_LostFocus(object sender, EventArgs e)
        {
            if (txt_uniformeCredito_Desde.Text == "")
                txt_uniformeCredito_Desde.Text = "8";
            if (Convert.ToInt64(txt_uniformeCredito_Desde.Text) > Convert.ToInt64(txt_uniformeCredito_Hasta.Text))
                txt_uniformeCredito_Desde.Text = txt_uniformeCredito_Hasta.Text;
        }

        private void txt_uniformeCredito_Hasta_LostFocus(object sender, EventArgs e)
        {
            if (txt_uniformeCredito_Hasta.Text == "")
                txt_uniformeCredito_Hasta.Text = "12";
            if (Convert.ToInt64(txt_uniformeCredito_Hasta.Text) < Convert.ToInt64(txt_uniformeCredito_Desde.Text))
                txt_uniformeCredito_Hasta.Text = txt_uniformeCredito_Desde.Text;
        }

        private void txt_uniformeEfectivo_Desde_LostFocus(object sender, EventArgs e)
        {
            if (txt_uniformeEfectivo_Desde.Text == "")
                txt_uniformeEfectivo_Desde.Text = "16";
            if (Convert.ToInt64(txt_uniformeEfectivo_Desde.Text) > Convert.ToInt64(txt_uniformeEfectivo_Hasta.Text))
                txt_uniformeEfectivo_Desde.Text = txt_uniformeEfectivo_Hasta.Text;
        }

        private void txt_uniformeEfectivo_Hasta_LostFocus(object sender, EventArgs e)
        {
            if (txt_uniformeEfectivo_Hasta.Text == "")
                txt_uniformeEfectivo_Hasta.Text = "24";
            if (Convert.ToInt64(txt_uniformeEfectivo_Hasta.Text) < Convert.ToInt64(txt_uniformeEfectivo_Desde.Text))
                txt_uniformeEfectivo_Hasta.Text = txt_uniformeEfectivo_Desde.Text;
        }

        private void txt_minutosSimulacion_LostFocus(object sender, EventArgs e)
        {
            if (txt_minutosSimulacion.Text == "")
                txt_minutosSimulacion.Text = "500";
            txt_minutosDesde.Text = Math.Round((Convert.ToDouble(txt_minutosSimulacion.Text) * 0.80)).ToString();
            txt_minutosHasta.Text = txt_minutosSimulacion.Text;
        }

        private void txt_minutosDesdse_LostFocus(object sender, EventArgs e)
        {
            if (txt_minutosDesde.Text == "")
                txt_minutosDesde.Text = Math.Round((Convert.ToDouble(txt_minutosSimulacion.Text) * 0.80)).ToString();
            if (Convert.ToInt64(txt_minutosDesde.Text) > Convert.ToInt64(txt_minutosHasta.Text))
                txt_minutosDesde.Text = txt_minutosHasta.Text;
        }

        private void txt_minutosHasta_LostFocus(object sender, EventArgs e)
        {
            if (txt_minutosHasta.Text == "")
                txt_minutosHasta.Text = txt_minutosSimulacion.Text;
            if (Int64.Parse(txt_minutosHasta.Text) > Int64.Parse(txt_minutosSimulacion.Text))
            {
                txt_minutosHasta.Text = txt_minutosSimulacion.Text;
                return;
            }

            if (Int64.Parse(txt_minutosHasta.Text) < Int64.Parse(txt_minutosDesde.Text))
                txt_minutosHasta.Text = txt_minutosDesde.Text;
        }

        private void txt_eventosSimulacion_LostFocus(object sender, EventArgs e)
        {
            if (txt_eventosSimulacion.Text == "")
                txt_eventosSimulacion.Text = "100";
            txt_eventosDesde.Text = Math.Round((Convert.ToDouble(txt_eventosSimulacion.Text) * 0.80)).ToString();
            txt_eventosHasta.Text = txt_eventosSimulacion.Text;
        }

        private void txt_eventosDesde_LostFocus(object sender, EventArgs e)
        {
            if (txt_eventosDesde.Text == "")
                txt_eventosDesde.Text = Math.Round((Convert.ToDouble(txt_eventosSimulacion.Text) * 0.80)).ToString();
            if (Convert.ToInt64(txt_eventosDesde.Text) > Convert.ToInt64(txt_eventosHasta.Text))
                txt_eventosDesde.Text = txt_eventosHasta.Text;
        }

        private void txt_eventosHasta_LostFocus(object sender, EventArgs e)
        {
            if (txt_eventosHasta.Text == "")
                txt_eventosHasta.Text = txt_eventosSimulacion.Text;
            if (Int64.Parse(txt_eventosHasta.Text) > Int64.Parse(txt_eventosSimulacion.Text))
            {
                txt_eventosHasta.Text = txt_eventosSimulacion.Text;
                return;
            }

            if (Int64.Parse(txt_eventosHasta.Text) < Int64.Parse(txt_eventosDesde.Text))
                txt_eventosHasta.Text = txt_eventosDesde.Text;
        }

        private void txt_parametros_LostFocus(object sender, EventArgs e)
        {
            if (txt_tiempoEntrega.Text == "")
                txt_tiempoEntrega.Text = "10";
            if (txt_capacidadCarga.Text == "")
                txt_capacidadCarga.Text = "4";

            if (Convert.ToDouble(txt_capacidadCarga.Text) == 0)
                txt_capacidadCarga.Text = "1";
        }

        private void txt_probabilidad_LostFocus(object sender, EventArgs e)
        {
            if (!(double.TryParse(txt_prob_Credito.Text, out double result)))
                txt_prob_Credito.Text = "0.00";
            if (!(double.TryParse(txt_prob_Efectivo.Text, out double result2)))
                txt_prob_Efectivo.Text = "0.00";

            if ((Convert.ToDouble(txt_prob_Credito.Text) + Convert.ToDouble(txt_prob_Efectivo.Text)) != 1)
            {
                lbl_errorProbabilidades.Visible = true;
                btn_Simular_Disponible(false);
            }
            else
            {
                lbl_errorProbabilidades.Visible = false;
                btn_Simular_Disponible(true);
            }
        }
    }
}
