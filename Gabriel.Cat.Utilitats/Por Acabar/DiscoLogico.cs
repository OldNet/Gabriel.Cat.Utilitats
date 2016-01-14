using Gabriel.Cat.Extension;
using Gabriel.Cat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Xml;

namespace Gabriel.Cat
{
    public delegate void UnidadEventHandler(DiscoLogico disco, DiscoLogicoEventArgs args);
    public delegate void SinArgumentosEventHandler();
    public delegate FileInfo[] FiltroArchivosMethod(DirectoryInfo dir);
    public delegate void ObjetoIOEventHandler(DiscoLogico disco, IOArgs archivo);
    public delegate void EstadoEventHandler(DiscoLogico disco,DiscoLogico.EstadoEscaneo estado);
    public class IOArgs
    {
        string path;
        //si quiere poner un progreso :)
        //si es un archivo el progreso sera del total de la carpeta...
        int poscion;
        int total;
        bool esUnDirectorio;
        public IOArgs(string path, bool esUnDirectorio, int poscion, int total)
        {
            this.Path = path;
            this.Poscion = poscion;
            this.Total = total;
            this.EsUnDirectorio = esUnDirectorio;
        }

        public string Path
        {
            get
            {
                return path;
            }

            private set
            {
                path = value;
            }
        }

        public int Poscion
        {
            get
            {
                return poscion;
            }

            private set
            {
                poscion = value;
            }
        }

        public int Total
        {
            get
            {
                return total;
            }

            private set
            {
                total = value;
            }
        }

        public bool EsUnDirectorio
        {
            get
            {
                return esUnDirectorio;
            }

            private set
            {
                esUnDirectorio = value;
            }
        }
    }

    public class DiscoLogico
    {
        //modo escaneo
        public enum ModoEscaneo
        {
            /// <summary>
            /// comprueba si esta todo correcto hasta encontrar una variacion entonces  escanea y asi todo el tiempo.
            /// </summary>
            Continuo,
            /// <summary>
            /// comprueba cuando toca, si hay variaciones escanea y luego espera el tiempo.
            /// </summary>
            CadaXTiempo,
            /// <summary>
            /// comprueba cuando toca si no hay variaciones en X veces incrementa el tiempo. Haciendo reset cuando haya actividad
            /// </summary>
            CadaXTiempoIncrementandolo,
            /// <summary>
            /// Comprueba cuando toca si hay variaciones durante X Ciclos y luego lo pone manual
            /// </summary>
            CadaXTiempoUnosXCiclosLuegoManual,
            /// <summary>
            /// comprueba y  si hay variaciones escanea hecho de forma manual.
            /// </summary>
            Manual,
        }
        [Flags]
        public enum VelocidadEscaneo
        {
            MuyLenta = 100,
            Lenta = 5,
            Normal = 30,
            Rapida = 10,
            MuyRapida = 0
        }
        public enum EstadoEscaneo
        {
            ComprovandoIntegridad,
            Escaneando,
            /// <summary>
            /// espera al temporizador para empezar
            /// </summary>
            Esperando,
            /// <summary>
            /// espera a la persona para empezar
            /// </summary>
            Acabado
        }
        /// <summary>
        /// Si se esta debugeando y es true los mensajes se veran :)
        /// </summary>
        public static bool VerMensajesDebugger;
        //comprovar unidades activas
        static ModoEscaneo modoEscaneoEnBuscaDeUnidadesActivas;
        static int intervalBusquedaUnidades;
        static int numCiclosMaxBusquedaUnidadesSinCambios;
        static int numCiclosSinCambiosEnBusquedaUnidades;
        static int incrementoBusquedaUnidades;
        static Temporizador temporizadoEscaneoBusquedaUnidades;
        static EstadoEscaneo estadoBusquedaUnidadesActivas;
        static LlistaOrdenada<string, DiscoLogico> discosEncontrados;
        static bool conservarDiscosPerdidos;
        static bool usarObjetosIOPorDefecto;
        static Thread hiloEscaneoUnidadesActivas;
        private static VelocidadEscaneo velocidadPorDefecto;
        public static event SinArgumentosEventHandler FinComprovacionUnidadesNuevas;
        public static event UnidadEventHandler UnidadEncontrada;
        public static event UnidadEventHandler UnidadPerdida;
        public static event UnidadEventHandler UnidadActualizada;
        /// <summary>
        /// Siempre que la carpeta se vaya a usar se lanzara
        /// </summary>
        public static event ObjetoIOEventHandler CarpetaVigilada;
        /// <summary>
        /// Si se deja de vigilar se lanzara
        /// </summary>
        public static event ObjetoIOEventHandler CarpetaNoVigilada;
        /// <summary>
        /// Siempre que se encuentre con una carpeta bloqueada se lanzara
        /// </summary>
        public static event ObjetoIOEventHandler CarpetaBloqueada;
        public static FiltroArchivosMethod MetodoParaFiltrarPorDefecto;
        delegate void TratarCarpeta(DirectoryInfo dir, int posicion, int total);
        /// <summary>
        /// Cuando se descubre uno de nuevo se lanza
        /// </summary>
        public static event ObjetoIOEventHandler DirectorioNuevo;
        /// <summary>
        /// Al final de escanear el disco se lanza
        /// </summary>
        public static event ObjetoIOEventHandler DirectorioPerdido;
        /// <summary>
        /// Cuando se descubre uno de nuevo se lanza
        /// </summary>
        public static event ObjetoIOEventHandler ArchivoNuevo;
        /// <summary>
        /// Al final de escanear el disco se lanza
        /// </summary>
        public static event ObjetoIOEventHandler ArchivoPerdido;
        //unidad
        string root;
        //comprovar BD
        ModoEscaneo modoEscaneoBD;
        EstadoEscaneo estadoBD;
        int intervalComprobarBDUnidad;
        int numCiclosMaxComprobarBDUnidadSinCambios;
        int numCiclosSinCambiosEnComprobarBDUnidad;
        int incrementoComprbarBDUnidad;
        Temporizador temporizadoEscaneoComprovarBD;
        //Base de Datos
        bool usarObjetosIO;
        bool desactivada;


        //directorios
        LlistaOrdenada<string, DirectoryInfo> directoriosIOEncontrados;
        LlistaOrdenada<string, string> directoriosEncontrados;
        //archivos
        LlistaOrdenada<string, FileInfo> archivosIOEncontrados;
        LlistaOrdenada<string, string> archivosEncontrados;
        LlistaOrdenada<string, LlistaOrdenada<string, FileInfo>> archivosIOEncontradosPorDirectorio;
        LlistaOrdenada<string, LlistaOrdenada<string, string>> archivosEncontradosPorDirectorio;
        System.Threading.Thread hiloComprovarBaseDeDatos;
        System.Threading.Thread hiloEscanear;
        MiPool<DirectoryInfo[]> mpComprovarIntegridad;
        MiPool<DirectoryInfo[]> mpEscanear;
        public FiltroArchivosMethod MetodoParaFiltrar;
        VelocidadEscaneo velocidad;
        public event EstadoEventHandler EstadoCambiado;
       
        static LlistaOrdenada<string, string> blackList;

        static DiscoLogico()
        {
            blackList = new LlistaOrdenada<string, string>();
            velocidadPorDefecto = VelocidadEscaneo.Normal;
            modoEscaneoEnBuscaDeUnidadesActivas = ModoEscaneo.CadaXTiempoIncrementandolo;
            intervalBusquedaUnidades = 5 * 1000;
            DiscoLogico.NumCiclosMaxBusquedaUnidadesSinCambios = 5;
            DiscoLogico.NumCiclosSinCambiosEnBusquedaUnidades = 0;
            incrementoBusquedaUnidades = 15 * 1000;

            temporizadoEscaneoBusquedaUnidades = new Temporizador(intervalBusquedaUnidades);
            temporizadoEscaneoBusquedaUnidades.Elapsed = BuscaUnidadesNuevas;
            discosEncontrados = new LlistaOrdenada<string, DiscoLogico>();
            ConservarDiscosPerdidos = true;
            UsarObjetosIOPorDefecto = true;
            VerMensajesDebugger = false;
        }

        public DiscoLogico(string root)
        {
            desactivada = false;
            blackList = new LlistaOrdenada<string, string>();
            usarObjetosIO = usarObjetosIOPorDefecto;
            directoriosIOEncontrados = new LlistaOrdenada<string, DirectoryInfo>();
            directoriosEncontrados = new LlistaOrdenada<string, string>();
            archivosEncontrados = new LlistaOrdenada<string, string>();
            archivosIOEncontrados = new LlistaOrdenada<string, FileInfo>();
            archivosEncontradosPorDirectorio = new LlistaOrdenada<string, LlistaOrdenada<string, string>>();
            archivosIOEncontradosPorDirectorio = new LlistaOrdenada<string, LlistaOrdenada<string, FileInfo>>();
            velocidad = VelocidadPorDefecto;
            modoEscaneoBD = ModoEscaneo.CadaXTiempoIncrementandolo;
            estadoBD = EstadoEscaneo.Esperando;
            intervalComprobarBDUnidad = 5 * 1000;
            this.NumCiclosMaxComprobarBDUnidadSinCambios = 2;
            this.NumCiclosSinCambiosEnComprobarBDUnidad = 0;
            incrementoComprbarBDUnidad = 100 * 1000;
            temporizadoEscaneoComprovarBD = new Temporizador(intervalComprobarBDUnidad);
            temporizadoEscaneoComprovarBD.Elapsed = ComprovarBaseDeDatos;
            mpComprovarIntegridad = new MiPool<DirectoryInfo[]>();
            mpEscanear = new MiPool<DirectoryInfo[]>();
            mpComprovarIntegridad.MaxThreads = 3;
            mpEscanear.MaxThreads = 3;
            this.Root = root;
        }
        public DiscoLogico(XmlNode nodoDisco)
            : this(nodoDisco.FirstChild.InnerText.DescaparCaracteresXML())
        {
            modoEscaneoBD = (ModoEscaneo)Enum.Parse(typeof(ModoEscaneo), nodoDisco.ChildNodes[1].InnerText);
            velocidad = (VelocidadEscaneo)Convert.ToInt32(nodoDisco.ChildNodes[2].InnerText);
            intervalComprobarBDUnidad = Convert.ToInt32(nodoDisco.ChildNodes[3].InnerText);
            incrementoComprbarBDUnidad = Convert.ToInt32(nodoDisco.ChildNodes[4].InnerText);
            numCiclosMaxComprobarBDUnidadSinCambios = Convert.ToInt32(nodoDisco.ChildNodes[5].InnerText);
        }


        public void StopAndClear()
        {
            AcabaActividad();
            directoriosIOEncontrados.Buida();
            directoriosEncontrados.Buida();
            archivosEncontrados.Buida();
            archivosIOEncontrados.Buida();
            archivosEncontradosPorDirectorio.Buida();
            archivosIOEncontradosPorDirectorio.Buida();
        }
        public void PonApunto()
        {
            ModoEscaneoBD = modoEscaneoBD;
        }
        #region Inicio Automatico y Acabar DiscoLogico
        public void AcabaActividad()
        {
            AcabaActividad(true);

        }
        public void AcabaActividad(bool ponerManual)
        {
            if (estadoBD == EstadoEscaneo.ComprovandoIntegridad)
            {
                //abortar miPoolComprbarBD
                this.hiloComprovarBaseDeDatos.Abort();
            }
            if (estadoBD == EstadoEscaneo.Escaneando)
            {
                //abortar miPoolEscanear
                this.hiloEscanear.Abort();
            }
            if (mpComprovarIntegridad != null)
                mpComprovarIntegridad.AcabaActividad();
            if (mpEscanear != null)
                mpEscanear.AcabaActividad();
            this.temporizadoEscaneoComprovarBD.StopAndAbort();

            EstadoBD = EstadoEscaneo.Acabado;
            if(ponerManual)
             ModoEscaneoBD = ModoEscaneo.Manual;
        }
        public void IniciaActividad()
        {
            if (estadoBD == EstadoEscaneo.Acabado)
            {
                EstadoBD = EstadoEscaneo.Esperando;
                if (ModoEscaneoBD == ModoEscaneo.Manual)
                   ModoEscaneoBD = ModoEscaneo.CadaXTiempoIncrementandolo;
                temporizadoEscaneoComprovarBD.Start();
            }
        }
        #endregion
        #region Comprobar BD
        private void ComprovarBaseDeDatos(Temporizador temp)
        {
            IComprobarBaseDeDatos();
        }
        public void ComprobarBaseDeDatosAsync()
        {
            new Thread(() => ComprobarBaseDeDatos()).Start();
        }
        public void ComprobarBaseDeDatos()
        {
            ModoEscaneo modoAnt = ModoEscaneoBD;//asi si es automatico no se fastidia
            modoEscaneoBD = ModoEscaneo.Manual;
            IComprobarBaseDeDatos();
            modoEscaneoBD = modoAnt;
        }
        public void IComprobarBaseDeDatos()
        {
 
            if (estadoBD != EstadoEscaneo.ComprovandoIntegridad && estadoBD != EstadoEscaneo.Escaneando)
            {
                EstadoBD = EstadoEscaneo.ComprovandoIntegridad;
                hiloComprovarBaseDeDatos = new Thread(() =>
                {
                    MensajeDebugger("Inicio comprobar BD " + Root);
                    const int NUMREPARTIRDIRS = 13;

                    DirectoryInfo dir = new DirectoryInfo(Root);
                    DirectoryInfo[] subdirectorios;
                    bool abortado = false;
                    Semaphore semafor = null;
                    Exception ex = null;
                    TratarCarpeta metodo = (directorio, pos, total) =>
                    {
                        if (!blackList.Existeix(directorio.FullName))
                        {
                            FileInfo[] filesDir;
                            Thread.Sleep((int)velocidad);
                            if (MetodoParaFiltrar != null)
                            {
                                filesDir = MetodoParaFiltrar(directorio);
                            }
                            else if (MetodoParaFiltrarPorDefecto != null)
                            {
                                filesDir = MetodoParaFiltrarPorDefecto(directorio);
                            }
                            else
                            {
                                //no filtro
                                filesDir = directorio.GetFiles();
                            }
                            //ahora miro si estan...por un lado o por el otro es que ha cambiado...
                            //a mas o menos...
                            if (filesDir != null && filesDir.Length > 0)
                            {
                                Thread.Sleep((int)velocidad);
                                if (usarObjetosIO)
                                {
                                    if (archivosIOEncontradosPorDirectorio[directorio.FullName] == null || archivosIOEncontradosPorDirectorio[directorio.FullName].Count != filesDir.Length)
                                        throw new Exception("");
                                    for (int j = 0; j < filesDir.Length; j++)
                                    {
                                        Thread.Sleep((int)velocidad);
                                        if (!archivosIOEncontradosPorDirectorio[directorio.FullName].Existeix(filesDir[j].FullName))
                                            throw new Exception("");
                                    }
                                }
                                else
                                {
                                    if (archivosIOEncontradosPorDirectorio[directorio.FullName] == null || archivosEncontradosPorDirectorio[directorio.FullName].Count != filesDir.Length)
                                        throw new Exception("");
                                    for (int j = 0; j < filesDir.Length; j++)
                                    {
                                        Thread.Sleep((int)velocidad);
                                        if (!archivosEncontradosPorDirectorio[directorio.FullName].Existeix(filesDir[j].FullName))
                                            throw new Exception("");
                                    }
                                }
                            }
                        }
                    };
                 //   EstadoBD = EstadoEscaneo.ComprovandoIntegridad;
                    //comprueba la integridad...
                    try
                    {
                        try
                        {
                            metodo(dir, -1, -1);//si peta no hace falta que escanee :D
                        }
                        catch (Exception exception)
                        {
                            ex = exception;
                            abortado = true;
                        }
                        if (!abortado)
                        {
                            subdirectorios = dir.SubDirectoris();//si peta es porque no existe la root
                            //usar mpCompruebaBD
                            mpComprovarIntegridad.Exception += (excepcion) =>
                            {
                                try
                                {
                                    ex = excepcion;
                                    abortado = true;
                                    semafor.Release();
                                }
                                catch { }//si peta 


                            };//si peta hago que salga
                            semafor = mpComprovarIntegridad.HazTrabajoAsinc((dirs) =>
                               {
                                   if (dirs != null)
                                       for (int i = 0; i < dirs.Length; i++)
                                       {
                                           if (dirs[i] != null)
                                           {
                                               if (!Directory.Exists(dirs[i].FullName))
                                                   throw new Exception();
                                               metodo(dirs[i], -1, -1);
                                           }
                                       }
                               }, subdirectorios.ToMatriu(NUMREPARTIRDIRS, DimensionMatriz.Columna).AgrupaList(DimensionMatriz.Columna));
                            semafor.WaitOne();//espero a que acabe o pete :)
                        }
                        //todo sigue igual
                        if (!abortado)
                        {
                            if (ModoEscaneoBD != ModoEscaneo.Manual)
                            {
                                numCiclosSinCambiosEnComprobarBDUnidad++;
                                if (this.NumCiclosSinCambiosEnComprobarBDUnidad >= this.NumCiclosMaxComprobarBDUnidadSinCambios)
                                {
                                    switch (ModoEscaneoBD)
                                    {
                                        case ModoEscaneo.CadaXTiempoIncrementandolo:
                                            temporizadoEscaneoComprovarBD.Interval += IncrementoComprbarBDUnidad;
                                            MensajeDebugger("Se ha incrementado el tiempo de " + Root + " en " + this.temporizadoEscaneoComprovarBD.Interval);
                                            break;
                                        case ModoEscaneo.CadaXTiempoUnosXCiclosLuegoManual:
                                            ModoEscaneoBD = ModoEscaneo.Manual;
                                            break;
                                    }
                                    this.NumCiclosSinCambiosEnComprobarBDUnidad = 0;

                                }
                            }
                            MensajeDebugger("Se ha acabado de comprobar " + Root + " sin cambios");
                        }
                        else
                        {
                            mpComprovarIntegridad.AcabaActividad();
                            throw ex;
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        //pasa cuando acaban la actividad del discologico :D
                    }
                    catch (Exception exception)
                    {
                        if (!String.IsNullOrEmpty(exception.Message))
                            MensajeDebugger("Excepcion no controlada comprobando " + Root + "\n" + exception.Message);
                        if (Directory.Exists(Root))
                        {
                            //como hay cambios lo reseteo :D
                            this.NumCiclosSinCambiosEnComprobarBDUnidad = 0;
                        }
                        else
                        {
                            Desactivada = true;
                            abortado = false;//que no escanee
                        }
                    }
                    if (abortado)
                    {
                        //si no esta bien escaneo
                        IEscanear(false);
                    }
                    if (modoEscaneoBD == ModoEscaneo.Manual)
                        EstadoBD = EstadoEscaneo.Acabado;
                    else if (Directory.Exists(Root))
                    {
                        EstadoBD = EstadoEscaneo.Esperando;

                    }
                    MensajeDebugger("Fin comprobar BD " + Root);
                });
                hiloComprovarBaseDeDatos.Start();
                hiloComprovarBaseDeDatos.Join();
            }
            else if (hiloComprovarBaseDeDatos != null && hiloComprovarBaseDeDatos.IsAlive)
                hiloComprovarBaseDeDatos.Join();//si esta activo espero a que acabe :D
        }
        #endregion
        #region Escanear Disco
        public void Escanear()
        {
            if (estadoBD != EstadoEscaneo.Escaneando)
            {
                ModoEscaneo modoAnt = modoEscaneoBD;
                modoEscaneoBD = ModoEscaneo.Manual;//como se hace  manual...pues es manual :D
                IEscanear(true);
                if (Directory.Exists(Root))//si no existe estara manual,Acabado asi que no se tiene que cambiar
                {
                    modoEscaneoBD = modoAnt;
                    if (modoEscaneoBD == ModoEscaneo.Manual)
                        EstadoBD = EstadoEscaneo.Acabado;
                    else
                        EstadoBD = EstadoEscaneo.Esperando;
                }
            }
            else if (hiloEscanear != null && hiloEscanear.IsAlive)
                hiloEscanear.Join();//si esta activo espero a que acabe :D
        }
        public void EscanearAsync()
        {
            new Thread(() => Escanear()).Start();
        }
        private void IEscanear(bool abortarComprovacion)
        {
            if (abortarComprovacion && hiloComprovarBaseDeDatos.IsAlive)
                hiloComprovarBaseDeDatos.Abort();//aborto porque no vale la pena esperar
            if (estadoBD != EstadoEscaneo.Escaneando)
            {
                hiloEscanear = new Thread(() =>
                {

                    MensajeDebugger("Inicio Escaneo " + Root);
                    //HAY UN NULL POR ALLI QUE NO SE DE DONE VIENE....MiPool,Llista,MetodoLambda....no se porque hay...
                    const int NUMREPARTIRDIRS = 13;
                    EstadoBD = EstadoEscaneo.Escaneando;
                    Semaphore semafor = null;
                    bool abortado = false;
                    Exception exception = null;  
                    DiscoLogicoEventArgs args;
                    //usar mpEscanear :D
                    //directorios
                    LlistaOrdenada<string, DirectoryInfo> directoriosIOEncontradosAux = new LlistaOrdenada<string, DirectoryInfo>();
                    LlistaOrdenada<string, string> directoriosEncontradosAux = new LlistaOrdenada<string, string>();
                    LlistaOrdenada<string, string> directoriosNuevos = new LlistaOrdenada<string, string>();
                    //archivos
                    LlistaOrdenada<string, string> archivosNuevos = new LlistaOrdenada<string, string>();
                    LlistaOrdenada<string, FileInfo> archivosIOEncontradosAux = new LlistaOrdenada<string, FileInfo>();
                    LlistaOrdenada<string, string> archivosEncontradosAux = new LlistaOrdenada<string, string>();
                    LlistaOrdenada<string, LlistaOrdenada<string, FileInfo>> archivosIOEncontradosPorDirectorioAux = new LlistaOrdenada<string, LlistaOrdenada<string, FileInfo>>();
                    LlistaOrdenada<string, LlistaOrdenada<string, string>> archivosEncontradosPorDirectorioAux = new LlistaOrdenada<string, LlistaOrdenada<string, string>>();
                    FileInfo[] filesDir;
                    DirectoryInfo dir;
                    DirectoryInfo[] directoriosAMirar;
                    //lleno la base de datos
                    try
                    {//si peta por que no hay acceso a la Root pues lo trato
                    dir = new DirectoryInfo(Root);
                    directoriosAMirar = dir.SubDirectoris();//si peta es porque no existe la root

                    MensajeDebugger("Hay " + (directoriosAMirar.Length + 1) + " directorios en " + Root);

                    //Hago un metodoConParametros para poder escanear el directorio Raiz evitando añadirlo a la coleccion...perdiendo tiempo...con la  conversion
                    TratarCarpeta metodo = (directorio, posicionDir, totalDirs) =>
                    {
                        if (!blackList.Existeix(directorio.FullName))
                        {
                            filesDir = null;
                            Thread.Sleep((int)velocidad);
                            if (MetodoParaFiltrar != null)
                            {
                                filesDir = MetodoParaFiltrar(directorio);
                            }
                            else if (MetodoParaFiltrarPorDefecto != null)
                            {
                                filesDir = MetodoParaFiltrarPorDefecto(directorio);
                            }
                            else
                            {
                                //no filtro
                                filesDir = directorio.GetFiles();
                            }

                            if (filesDir != null && filesDir.Length != 0)
                            {//solo guardo las carpetas que tienen archivos validos :)
                                if(CarpetaVigilada!=null)
                                    CarpetaVigilada(this,new IOArgs(directorio.FullName,true,0,0));
                                Thread.Sleep((int)velocidad);
                                if (usarObjetosIO)
                                {

                                    //pongo si no esta
                                    if (!directoriosIOEncontradosAux.Existeix(directorio.FullName))
                                    {
                                        directoriosIOEncontradosAux.Afegir(directorio.FullName, directorio);

                                    }
                                    if (!archivosIOEncontradosPorDirectorioAux.Existeix(directorio.FullName))
                                    {
                                        archivosIOEncontradosPorDirectorioAux.Afegir(directorio.FullName, new LlistaOrdenada<string, FileInfo>());
                                    }
                                    //si esta lo quito para que me queden los que ya no estan
                                    if (directoriosIOEncontrados.Existeix(directorio.FullName))
                                        directoriosIOEncontrados.Elimina(directorio.FullName);
                                    //me  quedo con los nuevos
                                    else
                                    {
                                        directoriosNuevos.Afegir(directorio.FullName, directorio.FullName);
                                        if (DirectorioNuevo != null)
                                            DirectorioNuevo(this, new IOArgs(directorio.FullName, true, posicionDir, totalDirs));
                                    }
                                    for (int j = 0; j < filesDir.Length; j++)
                                    {
                                        Thread.Sleep((int)velocidad);
                                        //si no esta lo pongo
                                        if (!archivosIOEncontradosPorDirectorioAux.Existeix(filesDir[j].FullName))
                                            archivosIOEncontradosPorDirectorioAux[directorio.FullName].Afegir(filesDir[j].FullName, filesDir[j]);
                                        if (!archivosIOEncontradosAux.Existeix(filesDir[j].FullName))
                                            archivosIOEncontradosAux.Afegir(filesDir[j].FullName, filesDir[j]);
                                        //si esta lo quito para quedarme con los perdidos
                                        if (archivosIOEncontrados.Existeix(filesDir[j].FullName))
                                        {
                                            archivosIOEncontrados.Elimina(filesDir[j].FullName);
                                        }
                                        //si no lo guardo con los nuevos
                                        else
                                        {
                                            archivosNuevos.Afegir(filesDir[j].FullName, filesDir[j].FullName);
                                            if (ArchivoNuevo != null)
                                                ArchivoNuevo(this, new IOArgs(filesDir[j].FullName, false, j, filesDir.Length));
                                        }
                                    }
                                }
                                else
                                {
                                    if (!directoriosEncontradosAux.Existeix(directorio.FullName))
                                    {
                                        directoriosEncontradosAux.Afegir(directorio.FullName, directorio.FullName);

                                    }
                                    if (!archivosEncontradosPorDirectorioAux.Existeix(directorio.FullName))
                                    {
                                        archivosEncontradosPorDirectorioAux.Afegir(directorio.FullName, new LlistaOrdenada<string, string>());
                                    }

                                    if (directoriosEncontrados.Existeix(directorio.FullName))
                                        directoriosEncontrados.Elimina(directorio.FullName);//asi solo me quedan los perdidos :D
                                    else
                                    {
                                        directoriosNuevos.Afegir(directorio.FullName, directorio.FullName);
                                        if (DirectorioNuevo != null)
                                            DirectorioNuevo(this, new IOArgs(directorio.FullName, true, posicionDir, totalDirs));
                                    }
                                    for (int j = 0; j < filesDir.Length; j++)
                                    {
                                        Thread.Sleep((int)velocidad);
                                        if (!archivosEncontradosPorDirectorioAux.Existeix(filesDir[j].FullName))
                                            archivosEncontradosPorDirectorioAux[directorio.FullName].Afegir(filesDir[j].FullName, filesDir[j].FullName);
                                        if (!archivosEncontradosAux.Existeix(filesDir[j].FullName))
                                            archivosEncontradosAux.Afegir(filesDir[j].FullName, filesDir[j].FullName);
                                        if (archivosEncontrados.Existeix(filesDir[j].FullName))
                                            archivosEncontrados.Elimina(filesDir[j].FullName);//asi me quedo con los archivos perdidos :D
                                        else
                                        {
                                            archivosNuevos.Afegir(filesDir[j].FullName, filesDir[j].FullName);
                                            if (ArchivoNuevo != null)
                                                ArchivoNuevo(this, new IOArgs(filesDir[j].FullName, false, j, filesDir.Length));
                                        }
                                    }
                                }
                            }
                        }
                        else if (CarpetaBloqueada != null)
                            CarpetaBloqueada(this, new IOArgs(directorio.FullName, true, 0, 0));
                    };
                        //usar mpCompruebaBD
                        try
                        {
                            metodo(dir, 0, directoriosAMirar.Length + 1);
                        }
                        catch (Exception ex)
                        {
                            abortado = true;
                            exception = ex;
                        }
                        if (!abortado)
                        {
                            mpEscanear.Exception += (ex) =>
                            {
                                try
                                {
                                    abortado = true;
                                    exception = ex;
                                    semafor.Release();
                                }
                                catch { }

                            };//si peta hago que salga asi aborto el escaneo por la excepcion producida en el metodoConParametros de Filtra
                            semafor = mpEscanear.HazTrabajoAsinc((dirs) =>
                              {
                                  if (dirs != null)
                                      for (int i = 0; i < dirs.Length; i++)
                                      {
                                          if (dirs[i] != null)
                                          {
                                              if (!Directory.Exists(dirs[i].FullName))
                                                  throw new Exception();
                                              metodo(dirs[i], i + 1, directoriosAMirar.Length + 1);
                                          }

                                      }
                              }, directoriosAMirar.ToMatriu(NUMREPARTIRDIRS, DimensionMatriz.Columna).AgrupaList(DimensionMatriz.Columna));
                            semafor.WaitOne();
                            if (abortado)
                            {
                                mpEscanear.AcabaActividad();
                                throw exception;
                            }

                        }

                    }
                    catch (ThreadAbortException)
                    {
                        //pasa cuando acaban la actividad del discologico :D
                    }
                    catch (Exception ex)
                    {
                        if (Directory.Exists(Root))
                        {//si peta porque no existe lanzo el evento
                      //    throw ex;
                        }
                        
                            //si peta por el metodoConParametros de filtrar lo vuelvo a lanzar
                    }
                    if (!Directory.Exists(Root))
                    {
                        Desactivada = true;
                        abortado = true;
                    }
                    if (!abortado)
                    {
                        if (usarObjetosIO)
                        {
                            args = new DiscoLogicoEventArgs(directoriosNuevos.KeysToArray(), directoriosIOEncontrados.KeysToArray(), archivosNuevos.KeysToArray(), archivosIOEncontrados.KeysToArray(), this);
                            //pongo la ultima version de la bd
                            directoriosIOEncontrados = directoriosIOEncontradosAux;
                            archivosIOEncontradosPorDirectorio = archivosIOEncontradosPorDirectorioAux;
                            archivosIOEncontrados = archivosIOEncontradosAux;
                        }
                        else
                        {
                            args = new DiscoLogicoEventArgs(directoriosNuevos.KeysToArray(), directoriosEncontrados.KeysToArray(), archivosNuevos.KeysToArray(), archivosEncontrados.KeysToArray(), this);
                            //pongo la ultima version de la bd
                            directoriosEncontrados = directoriosEncontradosAux;
                            archivosEncontradosPorDirectorio = archivosEncontradosPorDirectorioAux;
                            archivosEncontrados = archivosEncontradosAux;
                        }
                        if (DirectorioPerdido != null)
                        {
                            //si se a suscrito al evento lo aviso :)
                            for (int i = 0; i < args.DirectoriosQuitados.Length; i++)
                                DirectorioPerdido(this, new IOArgs(args.DirectoriosQuitados[i], true, i, args.DirectoriosQuitados.Length));
                        }
                        if (ArchivoPerdido != null)
                        {
                            //si se a suscrito al evento lo aviso :)
                            for (int i = 0; i < args.ArchivosQuitados.Length; i++)
                                ArchivoPerdido(this, new IOArgs(args.ArchivosQuitados[i], false, i, args.ArchivosQuitados.Length));
                        }
                        //mando la actualizacion 
                        if (UnidadActualizada != null)
                            UnidadActualizada(this, args);


                        if (modoEscaneoBD == ModoEscaneo.Manual)
                            EstadoBD = EstadoEscaneo.Acabado;
                        else
                            EstadoBD = EstadoEscaneo.Esperando;

                    }
                    else
                    {
                        //lo pongo manual?? desconfigurar porque se ha ido??
                      //  ModoEscaneoBD = ModoEscaneo.Manual;
                        EstadoBD = EstadoEscaneo.Acabado;
                    }
                    MensajeDebugger("Fin Escaneo " + Root);

                });
                hiloEscanear.Start();
                hiloEscanear.Join();
            }
        }
        #endregion
        #region Datos BD
        public string[] Archivos()
        {
            string[] archivosRutas;
            if (usarObjetosIO)
                archivosRutas = archivosIOEncontrados.KeysToArray();
            else
                archivosRutas = archivosEncontrados.KeysToArray();
            return archivosRutas;

        }
        /// <summary>
        /// Devuelve las rutas de los archivos de la carpeta indicada
        /// </summary>
        /// <param name="carpeta"></param>
        /// <returns>devuvelve nul en caso de no existir dicha carpeta</returns>
        public string[] Archivos(string carpeta)
        {
            string[] archivosRutas = null;
            if (usarObjetosIO)
            {
                if (archivosIOEncontradosPorDirectorio.Existeix(carpeta))
                    archivosRutas = archivosIOEncontradosPorDirectorio[carpeta].KeysToArray();
            }
            else
            {
                if (archivosEncontradosPorDirectorio.Existeix(carpeta))
                    archivosRutas = archivosEncontradosPorDirectorio[carpeta].KeysToArray();
            }
            if (archivosRutas != null)
                archivosRutas = archivosRutas.Filtra((file) => { return !blackList.Existeix(Path.GetDirectoryName(file)); }).ToArray();
            return archivosRutas;

        }
        public FileInfo[] ArchivosIO()
        {
            FileInfo[] archivosRutas;
            string[] archivosRutasString = null;
            if (usarObjetosIO)
                archivosRutas = archivosIOEncontrados.ValuesToArray();
            else
            {
                archivosRutasString = archivosEncontrados.KeysToArray();
                archivosRutas = new FileInfo[archivosRutasString.Length];
                for (int i = 0; i < archivosRutasString.Length; i++)
                    archivosRutas[i] = new FileInfo(archivosRutasString[i]);
            }
            if (archivosRutas != null)
                archivosRutas = archivosRutas.Filtra((file) => { return !blackList.Existeix(file.Directory.FullName); }).ToArray();
            return archivosRutas;

        }
        /// <summary>
        /// Devuelve los archivos de la carpeta indicada
        /// </summary>
        /// <param name="carpeta"></param>
        /// <returns>devuvelve nul en caso de no existir dicha carpeta</returns>
        public FileInfo[] ArchivosIO(string carpeta)
        {
            FileInfo[] archivosRutas = null;
            string[] archivosRutasString = null;
            if (!blackList.Existeix(carpeta))
            {
                if (usarObjetosIO)
                {
                    if (archivosIOEncontradosPorDirectorio.Existeix(carpeta))
                        archivosRutas = archivosIOEncontradosPorDirectorio[carpeta].ValuesToArray();
                }
                else
                {
                    if (archivosEncontradosPorDirectorio.Existeix(carpeta))
                    {
                        archivosRutasString = archivosEncontradosPorDirectorio[carpeta].KeysToArray();
                        archivosRutas = new FileInfo[archivosRutasString.Length];
                        for (int i = 0; i < archivosRutasString.Length; i++)
                            archivosRutas[i] = new FileInfo(archivosRutasString[i]);
                    }
                }
            }
            return archivosRutas;

        }
        public string[] Directorios()
        {
            string[] directorios;
            if (usarObjetosIO)
                directorios = directoriosIOEncontrados.KeysToArray();
            else
                directorios = directoriosEncontrados.KeysToArray();
            directorios = directorios.Filtra((dir) => { return !blackList.Existeix(dir); }).ToArray();
            return directorios;
        }
        public DirectoryInfo[] DirectoriosIO()
        {
            DirectoryInfo[] directorios = null;
            string[] rutasDirs;
            if (usarObjetosIO)
                directorios = directoriosIOEncontrados.ValuesToArray();
            else
            {
                rutasDirs = directoriosEncontrados.KeysToArray();
                directorios = new DirectoryInfo[rutasDirs.Length];
                for (int i = 0; i < rutasDirs.Length; i++)
                    directorios[i] = new DirectoryInfo(rutasDirs[i]);

            }
            directorios = directorios.Filtra((dir) => { return !blackList.Existeix(dir.FullName); }).ToArray();
            return directorios;
        }
        #endregion
        public override string ToString()
        {
            return Root;
        }
        #region Buscar Unidades Nuevas y/o activar las desactivadas encontradas
        private static void BuscaUnidadesNuevas(Temporizador temp)
        {
            BuscaUnidadesNuevas();
        }
        public static void BuscaUnidadesNuevasAsync()
        {
            new Thread(() => BuscaUnidadesNuevas()).Start();
        }
        public static void BuscaUnidadesNuevas()
        {
            if (estadoBusquedaUnidadesActivas != EstadoEscaneo.Escaneando)
            {
                if (hiloEscaneoUnidadesActivas == null || !hiloEscaneoUnidadesActivas.IsAlive)
                {
                    hiloEscaneoUnidadesActivas = new Thread(() =>
                    {
                        EstadoBusquedaUnidadesActivas = EstadoEscaneo.Escaneando;
                        //busco unidades nuevas
                        string[] unidadesDetectadas = Directory.GetLogicalDrives();
                        DiscoLogico discoEncontrado = null;
                        bool todoIgual = true;
                        for (int i = 0; i < unidadesDetectadas.Length; i++)
                        {
                            try
                            {
                                if (!discosEncontrados.Existeix(unidadesDetectadas[i]) && !blackList.Existeix(unidadesDetectadas[i]))
                                {
                                    Directory.GetDirectories(unidadesDetectadas[i]);//si no puedo es que no se puede leer como el dvd :D
                                    discoEncontrado = new DiscoLogico(unidadesDetectadas[i]);
                                    if (UnidadEncontrada != null)
                                        UnidadEncontrada(discoEncontrado, new DiscoLogicoEventArgs(discoEncontrado));
                                    todoIgual = false;
                                }
                                else if (discosEncontrados.Existeix(unidadesDetectadas[i]) && discosEncontrados[unidadesDetectadas[i]].Desactivada)
                                {
                                    discosEncontrados[unidadesDetectadas[i]].Desactivada = false;
                                    todoIgual = false;
                                }
                            }
                            catch
                            {
                            }
                        }
                        if (todoIgual)
                        {
                            numCiclosSinCambiosEnBusquedaUnidades++;
                            if (numCiclosSinCambiosEnBusquedaUnidades >= numCiclosMaxBusquedaUnidadesSinCambios)
                            {
                                switch (modoEscaneoEnBuscaDeUnidadesActivas)
                                {
                                    case ModoEscaneo.CadaXTiempoIncrementandolo:
                                        temporizadoEscaneoBusquedaUnidades.Interval += IncrementoBusquedaUnidades;
                                        MensajeDebugger("Se ha incrementado la comprobacion de unidades ahora es cada :" + temporizadoEscaneoBusquedaUnidades.Interval);
                                        break;
                                    case ModoEscaneo.CadaXTiempoUnosXCiclosLuegoManual:
                                        modoEscaneoEnBuscaDeUnidadesActivas = ModoEscaneo.Manual;
                                        break;
                                }
                            }


                        }
                        else
                        {
                            numCiclosSinCambiosEnBusquedaUnidades = 0;

                            switch (modoEscaneoEnBuscaDeUnidadesActivas)
                            {
                                case ModoEscaneo.CadaXTiempoIncrementandolo:
                                    if (temporizadoEscaneoBusquedaUnidades.Interval + IntervalBusquedaUnidades > 0)
                                        temporizadoEscaneoBusquedaUnidades.Interval = IntervalBusquedaUnidades;
                                    else
                                        temporizadoEscaneoBusquedaUnidades.Interval = 0;//por si el intervalo es negativo que no tire una excepcion...

                                    break;
                            }

                        }
                        if (ModoEscaneoEnBuscaDeUnidadesActivas == ModoEscaneo.Manual)
                            EstadoBusquedaUnidadesActivas = EstadoEscaneo.Acabado;
                        else
                            EstadoBusquedaUnidadesActivas = EstadoEscaneo.Esperando;
                        if (ModoEscaneoEnBuscaDeUnidadesActivas == ModoEscaneo.Manual && FinComprovacionUnidadesNuevas != null)
                            FinComprovacionUnidadesNuevas();
                    });
                    hiloEscaneoUnidadesActivas.Start();

                }
            }
            if (hiloEscaneoUnidadesActivas != null && hiloEscaneoUnidadesActivas.IsAlive)
                hiloEscaneoUnidadesActivas.Join();//si ya se ha ejecutado me espero a que acabe :D
        }
        #endregion
        #region Control Discos
        public static bool EstaLaRuta(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new Exception("Se necesita una ruta para buscar");
            return discosEncontrados.Existeix(path);
        }
        public static void AñadirDisco(DiscoLogico disco)
        {
            if (!discosEncontrados.Existeix(disco.Root))
            {
                discosEncontrados.Afegir(disco.Root, disco);
                disco.PonApunto();
            }
        }
        public static void QuitarDisco(DiscoLogico disco)
        {
            if (discosEncontrados.Existeix(disco.Root))
            {
                discosEncontrados.Elimina(disco.Root);
            }
            //si es una unidad logica pues poner en la blackList...i poder quitar de esa blacklist
        }
        public static DiscoLogico[] Discos()
        {
            return discosEncontrados.ValuesToArray();
        }
        #endregion
        #region BlackList
        public static void BlackList(string path, bool add)
        {
            string rootPath = Path.GetPathRoot(path);
            if (add)
            {
                blackList.AfegirORemplaçar(path, path);
            }
            else
            {
                blackList.Elimina(path);
            }
        }
        public static string[] GetBlackList()
        {
            return blackList.ValuesToArray();
        }
        public static void BlackList(IEnumerable<string> paths, bool add)
        {
            foreach (string path in paths)
                BlackList(path, add);
        }
        #endregion
        private static void MensajeDebugger(string mensaje)
        {
            if (VerMensajesDebugger && System.Diagnostics.Debugger.IsAttached)
                Console.WriteLine(mensaje);
        }

        #region Control actividad Clase
        public static void AcabaTodaActividad()
        {
            if (EstadoBusquedaUnidadesActivas != EstadoEscaneo.Escaneando)
                temporizadoEscaneoBusquedaUnidades.StopAndAbort();
            else
            {
                //aborta escaneo
                if (hiloEscaneoUnidadesActivas != null && hiloEscaneoUnidadesActivas.IsAlive)
                    hiloEscaneoUnidadesActivas.Abort();
                //lo pongo manual
                ModoEscaneoEnBuscaDeUnidadesActivas = ModoEscaneo.Manual;
                EstadoBusquedaUnidadesActivas = EstadoEscaneo.Acabado;

            }
            foreach (var disco in discosEncontrados)
                disco.Value.StopAndClear();//asi libera memoria :D
            DiscoLogico.temporizadoEscaneoBusquedaUnidades.StopAndAbort();
        }
        public static void IniciaTodaActividad()
        {
            if (EstadoBusquedaUnidadesActivas != EstadoEscaneo.Escaneando)
            {
                temporizadoEscaneoBusquedaUnidades.Start();
                if (ModoEscaneoEnBuscaDeUnidadesActivas == ModoEscaneo.Manual)
                    ModoEscaneoEnBuscaDeUnidadesActivas = ModoEscaneo.CadaXTiempoIncrementandolo;
                EstadoBusquedaUnidadesActivas = EstadoEscaneo.Esperando;
            }
            foreach (var disco in discosEncontrados)
                disco.Value.IniciaActividad();
        }
        #endregion
        #region XmlDatos
        public static DiscoLogico[] LoadXml(XmlNode nodoXml)
        {
            LlistaOrdenada<string,DiscoLogico> discos = new LlistaOrdenada<string, DiscoLogico>();
            DiscoLogico discoLogico = null;
            string blackPath;
            //cargo la configuracion
            //mirar si el modo se pone bien porque he cargado manual y no tenia el color de parado...
            VerMensajesDebugger = nodoXml.FirstChild.ChildNodes[0].InnerText == true.ToString();
            modoEscaneoEnBuscaDeUnidadesActivas = (ModoEscaneo)Enum.Parse(typeof(ModoEscaneo), nodoXml.FirstChild.ChildNodes[1].InnerText);
            intervalBusquedaUnidades = Convert.ToInt32(nodoXml.FirstChild.ChildNodes[2].InnerText);
            numCiclosMaxBusquedaUnidadesSinCambios = Convert.ToInt32(nodoXml.FirstChild.ChildNodes[3].InnerText);
            incrementoBusquedaUnidades = Convert.ToInt32(nodoXml.FirstChild.ChildNodes[4].InnerText);
            conservarDiscosPerdidos = nodoXml.FirstChild.ChildNodes[5].InnerText == true.ToString();
            usarObjetosIOPorDefecto = nodoXml.FirstChild.ChildNodes[6].InnerText == true.ToString();
            //cargo los dicos
            for (int i = 0; i < nodoXml.ChildNodes[1].ChildNodes.Count; i++) {
                try
                {
                    discoLogico = new DiscoLogico(nodoXml.ChildNodes[1].ChildNodes[i]);
                    if (!discos.Existeix(discoLogico.Root))
                        discos.Afegir(discoLogico.Root, discoLogico);//falla algo
                }
                catch { }//hay un error en el xml
            }

            //cargo la blackList
            for (int i = 0; i < nodoXml.LastChild.ChildNodes.Count; i++)
            {
                blackPath = nodoXml.LastChild.ChildNodes[i].InnerText.EscaparCaracteresXML();
                blackList.AfegirORemplaçar(blackPath, blackPath);
            }
            return discos.ValuesToArray();

        }
        public static XmlNode ToXml(IEnumerable<DiscoLogico> discosNoAñadidos)
        {
            text stringXml = "";
            XmlDocument xml = new XmlDocument();
            LlistaOrdenada<string, DiscoLogico> discos = new LlistaOrdenada<string, DiscoLogico>();
            stringXml += "<DiscosLogicos>";
            //configuracion
            //comprovar unidades activas
            stringXml += "<Configuracion>";
            stringXml += "<VerMensajesDebugger>" + VerMensajesDebugger + "</VerMensajesDebugger>";
            stringXml += "<ModoEscaneoEnBuscaDeUnidadesActivas>" + modoEscaneoEnBuscaDeUnidadesActivas.ToString() + "</ModoEscaneoEnBuscaDeUnidadesActivas>";
            stringXml += "<IntervalBusquedaUnidades>" + intervalBusquedaUnidades + "</IntervalBusquedaUnidades>";
            stringXml += "<NumCiclosMaxBusquedaUnidadesSinCambios>" + NumCiclosMaxBusquedaUnidadesSinCambios + "</NumCiclosMaxBusquedaUnidadesSinCambios>";
            stringXml += "<IncrementoBusquedaUnidades>" + incrementoBusquedaUnidades + "</IncrementoBusquedaUnidades>";
            stringXml += "<ConservarDiscosPerdidos>" + conservarDiscosPerdidos + "</ConservarDiscosPerdidos>";
            stringXml += "<UsarObjetosIOPorDefecto>" + usarObjetosIOPorDefecto + "</UsarObjetosIOPorDefecto>";
            stringXml += "</Configuracion>";
            //guardo los discos
            if (discosNoAñadidos != null)
            {
                foreach (DiscoLogico disco in discosNoAñadidos)
                    if (!discos.Existeix(disco.Root))
                        discos.Afegir(disco.Root, disco);
            }
            discos.AfegirMolts(discosEncontrados);
            stringXml += "<Discos>";
            foreach (var discoAGuardar in discos)
                stringXml += discoAGuardar.Value.ToXml().OuterXml;
            stringXml += "</Discos>";
            //guardo la blackList
            stringXml += "<BlackList>";
            foreach (var blackDir in blackList)
                stringXml += "<RutaBloqueada>" + blackDir.Value.EscaparCaracteresXML() + "</RutaBloqueada>";
            stringXml += "</BlackList>";
            stringXml += "</DiscosLogicos>";
            xml.LoadXml(stringXml);
            xml.Normalize();
            return xml.FirstChild;
        }
        public static void LoadXml(XmlNode xmlNode, bool cargarDiscos)
        {
            DiscoLogico[] discos = LoadXml(xmlNode);
            if (cargarDiscos)
                for (int i = 0; i < discos.Length; i++)
                    AñadirDisco(discos[i]);
        }
        public XmlNode ToXml()
        {
            text stringXml = "";
            XmlDocument xml = new XmlDocument();
            stringXml += "<DiscoLogico>";
            stringXml += "<Root>" + Root.EscaparCaracteresXML() + "</Root>";
            stringXml += "<ModoEscaneo>" + modoEscaneoBD.ToString() + "</ModoEscaneo>";
            stringXml += "<Velocidad>" + ((int)velocidad) + "</Velocidad>";//por si son flags
            stringXml += "<IntervalComprobarBDUnidad>" + intervalComprobarBDUnidad.ToString() + "</IntervalComprobarBDUnidad>";
            stringXml += "<IncrementoComprbarBDUnidad>" + incrementoComprbarBDUnidad.ToString() + "</IncrementoComprbarBDUnidad>";
            stringXml += "<NumCiclosMaxComprobarBDUnidadSinCambios>" + numCiclosMaxComprobarBDUnidadSinCambios.ToString() + "</NumCiclosMaxComprobarBDUnidadSinCambios>";
            stringXml += "</DiscoLogico>";
            xml.LoadXml(stringXml);
            xml.Normalize();
            return xml.FirstChild;
        }
        #endregion
        #region Propiedades Clase
        public static ModoEscaneo ModoEscaneoEnBuscaDeUnidadesActivas
        {
            get
            {
                return modoEscaneoEnBuscaDeUnidadesActivas;
            }

            set
            {
                modoEscaneoEnBuscaDeUnidadesActivas = value;
                if(modoEscaneoEnBuscaDeUnidadesActivas==ModoEscaneo.Continuo)
                temporizadoEscaneoBusquedaUnidades.Interval = 0;
                if (modoEscaneoEnBuscaDeUnidadesActivas != ModoEscaneo.Manual)
                    temporizadoEscaneoBusquedaUnidades.Start();
                else
                    temporizadoEscaneoBusquedaUnidades.Stop();
                       
               
            }
        }
        public static EstadoEscaneo EstadoBusquedaUnidadesActivas
        {
            get
            {
                return estadoBusquedaUnidadesActivas;
            }

            private set
            {
                estadoBusquedaUnidadesActivas = value;
            }
        }
        public static int IntervalBusquedaUnidades
        {
            get
            {
                return intervalBusquedaUnidades;
            }

            set
            {
                if (value < 0)
                    throw new Exception("Tiene que ser mas grande o igual a 0 ");
                intervalBusquedaUnidades = value;
            }
        }

        public static int NumCiclosMaxBusquedaUnidadesSinCambios
        {
            get
            {
                return numCiclosMaxBusquedaUnidadesSinCambios;
            }

            set
            {
                if (value < 0)
                    throw new Exception("tiene que ser mas grande o igual a 0");
                numCiclosMaxBusquedaUnidadesSinCambios = value;
                NumCiclosSinCambiosEnBusquedaUnidades = 0;
            }
        }

        public static int NumCiclosSinCambiosEnBusquedaUnidades
        {
            get
            {
                return numCiclosSinCambiosEnBusquedaUnidades;
            }

            private set
            {
                numCiclosSinCambiosEnBusquedaUnidades = value;
            }
        }

        public static int IncrementoBusquedaUnidades
        {
            get
            {
                return incrementoBusquedaUnidades;
            }

            set
            {
                incrementoBusquedaUnidades = value;
            }
        }

        #endregion
        #region Propiedades Objeto
        public string Root
        {
            get { return root; }
            set
            {

                if (root != value)
                {
                    if (!Directory.Exists(value))
                        throw new Exception("La ruta no es valida para ser escaneada porque no existe");
                    if (discosEncontrados.Existeix(value))
                        throw new Exception("La ruta ya se esta escaneando actualmente");
                    ModoEscaneo modo = modoEscaneoBD;
                    string[] dirsDejadosDeVigilar;
                    discosEncontrados.Elimina(root);
                    root = value;
                    discosEncontrados.Afegir(root, this);
                    //como cambio de Root dejo de vigilar esas carpetas...lo malo es que si hay otro disco con una root que inculya alguna de estas carpetas...se daran de bajaa...
                    dirsDejadosDeVigilar = Directorios();
                    for (int i = 0; i < dirsDejadosDeVigilar.Length; i++)
                        CarpetaNoVigilada(this, new IOArgs(dirsDejadosDeVigilar[i], true, i, dirsDejadosDeVigilar.Length));//como paso el disco...se puede saber quien da de baja la carpeta
                    StopAndClear();
                    modoEscaneoBD = modo;
                    if (modoEscaneoBD != ModoEscaneo.Manual)
                    {
                        temporizadoEscaneoComprovarBD.Start();
                        EstadoBD = EstadoEscaneo.Esperando;
                    }


                }
            }
        }
        public VelocidadEscaneo Velocidad
        {
            get { return velocidad; }
            set { velocidad = value; }
        }
        public ModoEscaneo ModoEscaneoBD
        {
            get
            {
                return modoEscaneoBD;
            }

            set
            {
                modoEscaneoBD = value;
                if (modoEscaneoBD != ModoEscaneo.Manual)
                {
                    EstadoBD = EstadoEscaneo.Esperando;
                    temporizadoEscaneoComprovarBD.Start();
                }
                else
                {
                    EstadoBD = EstadoEscaneo.Acabado;
                    temporizadoEscaneoComprovarBD.StopAndAbort();
                }
                
            }
        }



        public EstadoEscaneo EstadoBD
        {
            get
            {
                return estadoBD;
            }

            private set
            {
                estadoBD = value;
                if (EstadoCambiado != null)
                    EstadoCambiado(this, estadoBD);
            }
        }

        public int IntervalComprobarBDUnidad
        {
            get
            {
                return intervalComprobarBDUnidad;
            }

            set
            {
                if (value < 0)
                    throw new Exception("Tiene que ser mas grande o igual a 0");
                intervalComprobarBDUnidad = value;
            }
        }

        public int NumCiclosMaxComprobarBDUnidadSinCambios
        {
            get
            {
                return numCiclosMaxComprobarBDUnidadSinCambios;
            }

            set
            {
                if (value < 0)
                    throw new Exception("El valor tiene que ser mas grande o igual a 0");
                numCiclosMaxComprobarBDUnidadSinCambios = value;
                NumCiclosSinCambiosEnBusquedaUnidades = 0;
            }
        }

        public int NumCiclosSinCambiosEnComprobarBDUnidad
        {
            get
            {
                return numCiclosSinCambiosEnComprobarBDUnidad;
            }

            private set
            {
                numCiclosSinCambiosEnComprobarBDUnidad = value;
            }
        }

        public int IncrementoComprbarBDUnidad
        {
            get
            {
                return incrementoComprbarBDUnidad;
            }

            set
            {
                incrementoComprbarBDUnidad = value;
            }
        }

        public static bool ConservarDiscosPerdidos
        {
            get
            {
                return conservarDiscosPerdidos;
            }

            set
            {
                conservarDiscosPerdidos = value;
            }
        }
        public bool Desactivada
        {
            get { return desactivada; }
            private set { desactivada = value;
            if (desactivada)
            {
                if (UnidadPerdida != null)
                    UnidadPerdida(this, new DiscoLogicoEventArgs(this));
                if (!ConservarDiscosPerdidos)
                    QuitarDisco(this);
                string[] lostDirs;
                MensajeDebugger("Ya no existe " + Root);
                temporizadoEscaneoComprovarBD.StopAndAbort();
                if (DirectorioPerdido != null)
                {
                    lostDirs = Directorios();
                    for (int i = 0; i < lostDirs.Length; i++)
                        DirectorioPerdido(this, new IOArgs(lostDirs[i], true, i, lostDirs.Length));
                  
                }
            }
            else
            {
                if (UnidadEncontrada != null)
                {
                    UnidadEncontrada(this, new DiscoLogicoEventArgs(this));
                }
                if (ModoEscaneoBD != ModoEscaneo.Manual)
                    temporizadoEscaneoComprovarBD.Start();
            }
            
            }
        }
        public static bool UsarObjetosIOPorDefecto
        {
            get
            {
                return usarObjetosIOPorDefecto;
            }

            set
            {
                usarObjetosIOPorDefecto = value;
            }
        }
        public static VelocidadEscaneo VelocidadPorDefecto
        {
            get { return velocidadPorDefecto; }
            set { velocidadPorDefecto = value; }
        }
        #endregion

        #region ModoAutoInicio


        public  void ModoAuto()
        {
            if (ModoEscaneoBD == ModoEscaneo.Manual)
                ModoAuto(ModoEscaneo.CadaXTiempoIncrementandolo);
            else
                ModoAuto(ModoEscaneoBD);
        }
        public void ModoAuto(ModoEscaneo modoEscaneoBD)
        {
            modoEscaneoEnBuscaDeUnidadesActivas = modoEscaneoBD;
            incrementoBusquedaUnidades = 1000;
            numCiclosMaxBusquedaUnidadesSinCambios = 5;
        }
        public static void ModoAutoBuscandoUnidades()
        {
            if (ModoEscaneoEnBuscaDeUnidadesActivas == ModoEscaneo.Manual)
                ModoAutoBuscandoUnidades(ModoEscaneo.CadaXTiempoIncrementandolo);
            else ModoAutoBuscandoUnidades(ModoEscaneoEnBuscaDeUnidadesActivas);
        }
        public static void ModoAutoBuscandoUnidades(ModoEscaneo modoEscaneoBD)
        {
            if (modoEscaneoBD != ModoEscaneo.Manual)
            {
                modoEscaneoEnBuscaDeUnidadesActivas = modoEscaneoBD;
                incrementoBusquedaUnidades = 1000;
                numCiclosMaxBusquedaUnidadesSinCambios = 5;
                temporizadoEscaneoBusquedaUnidades.Start();
            }
        }
        #endregion
        #region Modo Auto Stop
        /// <summary>
        /// para la actividad automatica pero no cambia la configuracion
        /// </summary>
        public static void StopModoAutoBuscarUnidades()
        {
            temporizadoEscaneoBusquedaUnidades.Stop();
        }
        /// <summary>
        /// para la actividad automatica pero no cambia la configuracion
        /// </summary>
        public void StopModoAutoComprobarBD()
        {
            temporizadoEscaneoComprovarBD.Stop();
        }
        #endregion
    }
    public class DiscoLogicoEventArgs : EventArgs
    {
        string[] directoriosNuevos;
        string[] directoriosQuitados;

        string[] archivosNuevos;
        string[] archivosQuitados;

        DiscoLogico disco;

        public DiscoLogicoEventArgs(DiscoLogico disco)
        {
            this.Disco = disco;
            directoriosNuevos = new string[] { };
            directoriosQuitados = new string[] { };
            archivosNuevos = new string[] { };
            archivosQuitados = new string[] { };

        }
        public DiscoLogicoEventArgs(IEnumerable<string> directoriosNuevos, IEnumerable<string> directoriosQuitados, IEnumerable<string> archivosNuevos, IEnumerable<string> archivosQuitados, DiscoLogico disco)
            : this(disco)
        {
            if (directoriosNuevos != null)
                this.DirectoriosNuevos = directoriosNuevos.ToTaula();
            if (DirectoriosQuitados != null)
                this.DirectoriosQuitados = directoriosQuitados.ToTaula();
            if (archivosNuevos != null)
                this.ArchivosNuevos = archivosNuevos.ToTaula();
            if (archivosQuitados != null)
                this.ArchivosQuitados = archivosQuitados.ToTaula();

        }

        public bool HayCambios
        {
            get { return directoriosNuevos.Length != 0 || directoriosQuitados.Length != 0 || archivosNuevos.Length != 0 || archivosQuitados.Length != 0; }
        }
        public string[] DirectoriosNuevos
        {
            get
            {
                return directoriosNuevos;
            }

            private set
            {
                directoriosNuevos = value;
            }
        }

        public string[] DirectoriosQuitados
        {
            get
            {
                return directoriosQuitados;
            }

            private set
            {
                directoriosQuitados = value;
            }
        }

        public string[] ArchivosNuevos
        {
            get
            {
                return archivosNuevos;
            }

            private set
            {
                archivosNuevos = value;
            }
        }

        public string[] ArchivosQuitados
        {
            get
            {
                return archivosQuitados;
            }

            private set
            {
                archivosQuitados = value;
            }
        }

        public DiscoLogico Disco
        {
            get
            {
                return disco;
            }

            private set
            {
                disco = value;
            }
        }
        public override string ToString()
        {
            return Disco.Root + "\nCarpetas Nuevas " + DirectoriosNuevos.Length + "\nCarpetas Perdidas " + DirectoriosQuitados.Length + "\nArchivos Nuevos " + ArchivosNuevos.Length + "\nArchivos perdidos " + ArchivosQuitados.Length;

        }
    }
}
