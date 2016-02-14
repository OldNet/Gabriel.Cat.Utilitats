/*
 * Creado por SharpDevelop.
 * Usuario: pc
 * Fecha: 16/04/2015
 * Hora: 22:36
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gabriel.Cat
{

	public delegate void TrabajaEventHandler<TipoTrabajo>(TipoTrabajo pilaTrabajo);
	public delegate void TrabajaEventHandler();
	public delegate void ExceptionOnTiket<TipoTrabajo>(Tiket<TipoTrabajo> tiket, Exception ex);
	public delegate void ExceptionAtWork(Exception ex);
	public delegate void TrabajoAcabadoEventHandler(dynamic objectoMiPoolAcabado);
	/// <summary>
	/// Description of MiPool.
	/// </summary>
	public class MiPool<TipoTrabajo>
	{
		//clase
		static int maxThreadsStatic = 5;
		static Llista<Tiket<TipoTrabajo>> faenasEstaticas = new Llista<Tiket<TipoTrabajo>>();
		static LlistaOrdenada<int, KeyValuePair<Thread, Tiket<TipoTrabajo>>> feanasHaciendose = new LlistaOrdenada<int, KeyValuePair<Thread, Tiket<TipoTrabajo>>>();
		static bool estaOrdenado = true;
		static Thread hiloPrincipalFaena;
		static LlistaOrdenada<string, Thread> hilosFaenas = new LlistaOrdenada<string, Thread>();
		static GeneradorHex generadorId = new GeneradorHex();
		static GeneradorHex generadorIdPool = new GeneradorHex();
		//objetoClase
		int maxThreads;
		string idUnicoMiPool;
		Thread[] hilosActivos;
		Pila<TipoTrabajo> pilaTrabajo;
		TrabajaEventHandler<TipoTrabajo> metodoParaHacerLaFaena;
		Thread hiloPrincipal;
		public event TrabajoAcabadoEventHandler TrabajoAcabado;
		/// <summary>
		/// Excepcion producida durante la ejecucion del metodoConParametros
		/// </summary>
		public event ExceptionAtWork Exception;
		private static ThreadPriority prioridadThreads;
		private System.Threading.ThreadPriority prioridadPorDefecto;
		public MiPool(int maxThreads)
		{
			if (maxThreads < 1)
				maxThreads = 1;
			MaxThreads = maxThreads;
			pilaTrabajo = new Pila<TipoTrabajo>();
			pilaTrabajo.ThrowExceptionIfEmpty = true;
			idUnicoMiPool = (generadorIdPool.Siguiente()).ToString() + "G";
			estaOrdenado = false;
			prioridadPorDefecto = ThreadPriority.Normal;
		}
		public MiPool()
			: this(5)
		{
		}
		~MiPool()
		{
			AcabaFaena();
		}
		public int MaxThreads {
			get {
				return maxThreads;
			}
			set {
				maxThreads = value;
			}
		}
		public void HazTrabajo(TrabajaEventHandler<TipoTrabajo> metodoParaHacerLaFaena, IEnumerable<TipoTrabajo> pilaTrabajo)
		{

			this.pilaTrabajo.Clear();
			this.pilaTrabajo.Push(pilaTrabajo);
			this.metodoParaHacerLaFaena = metodoParaHacerLaFaena;
			IHazTrabajo(null);
		}

		/// <summary>
		/// Realiza el trabajo en un hilo aparte
		/// </summary>
		/// <param name="metodoParaHacerLaFaena">se tiene que tener en cuenta un valor por defecto en vez de una pila de trabajo</param>
		/// <param name="pilaTrabajo"></param>
		/// <returns>devuelve un semaforo que si se le hace WhaitOne espera hasta que acabe la faena :)</returns>
		public Semaphore HazTrabajoAsinc(TrabajaEventHandler<TipoTrabajo> metodoParaHacerLaFaena, IEnumerable<TipoTrabajo> pilaTrabajo)
		{
			this.pilaTrabajo.Clear();
			this.pilaTrabajo.Push(pilaTrabajo);
			this.metodoParaHacerLaFaena = metodoParaHacerLaFaena;
			Semaphore acabaFaena = new Semaphore(0, 1);
			hiloPrincipal = new Thread(() => IHazTrabajo(acabaFaena));
			hiloPrincipal.Priority = PrioridadThreads;
			hiloPrincipal.Start();
			return acabaFaena;
		}

		void IHazTrabajo(Semaphore acabaFaena)
		{
			hilosActivos = new Thread[MaxThreads];
			for (int i = 0; i < MaxThreads; i++) {
				hilosActivos[i] = new Thread(() => IHazFaena());
				hilosActivos[i].Priority = PrioridadThreads;
				hilosActivos[i].Start();

			}
			//espero a que acaben la faena

			for (int i = 0; i < MaxThreads; i++) {
				if (hilosActivos[i].IsAlive)
					hilosActivos[i].Join();
			}
			//acabado :)
			if (TrabajoAcabado != null)
				TrabajoAcabado(this);
            if (acabaFaena != null)
                try
                {
                    acabaFaena.Release();
                }
                catch { /*en caso de no usar el semaforo peta ...*/}
		}

		private void IHazFaena()
		{
			TipoTrabajo trabajo;
			while (!pilaTrabajo.Empty) {
				try {
					trabajo = pilaTrabajo.Pop();
					//si aborta manda una excepcion...
					try {
						metodoParaHacerLaFaena(trabajo);
					} catch (ThreadAbortException) {
						// Console.WriteLine(ex.Message); "Subproceso anulado"
					} catch (Exception ex) {
						if (Exception != null)
							Exception(ex);
					}
				} catch {
				}//si peta por el pop
			}
		}

		public void AcabaActividad()
		{
			if (hiloPrincipal != null && hiloPrincipal.IsAlive)
				hiloPrincipal.Abort();

			if (hilosActivos != null)
				for (int i = 0; i < hilosActivos.Length; i++)
					if (hilosActivos[i] != null && hilosActivos[i].IsAlive)
						hilosActivos[i].Abort();
			pilaTrabajo.Clear();
		}
		#region clase

		public static int MaxThreadsClase {
			get {
				return maxThreadsStatic;
			}
			set {
				maxThreadsStatic = value;
			}
		}

		public string IdUnicoMiPool {
			get {
				return idUnicoMiPool;
			}
		}

		public static int HilosActivos
		{ get { return hilosFaenas.Count; } }
		public static bool EstaOrdenado {
			get {
				return estaOrdenado;
			}
			set {
				estaOrdenado = value;
				if (estaOrdenado)
					faenasEstaticas.Ordena();
				else
					faenasEstaticas.Desordena();
			}
		}
		public static void AñadirFaena(Tiket<TipoTrabajo> tiket)
		{
			tiket.DentroPool = true;
			faenasEstaticas.Afegir(tiket);
			if (estaOrdenado)
				faenasEstaticas.Ordena();

		}
		public static Tiket<TipoTrabajo> AñadirFaena(TrabajaEventHandler<TipoTrabajo> metodo, TipoTrabajo trabajo)
		{
			return AñadirFaena(metodo, trabajo, Tiket<TipoTrabajo>.PrioridadEnum.Ninguna);
		}
		public static Tiket<TipoTrabajo> AñadirFaena(TrabajaEventHandler<TipoTrabajo> metodo, TipoTrabajo trabajo, Tiket<TipoTrabajo>.PrioridadEnum prioridad)
		{
			Tiket<TipoTrabajo> faena = new Tiket<TipoTrabajo>(metodo, trabajo, prioridad);
			AñadirFaena(faena);
			return faena;

		}
		public static void QuitarFaena(Tiket<TipoTrabajo> tiketFaena)
		{
			faenasEstaticas.Elimina(tiketFaena);
			tiketFaena.DentroPool = false;
		}

		public static void OrdenarFaena()
		{
			faenasEstaticas.Ordena();

		}

		public static void IniciaFaena()
		{
			if (hiloPrincipalFaena == null || !hiloPrincipalFaena.IsAlive) {
				hiloPrincipalFaena = new Thread(() => IIniciaFaena());
				hiloPrincipalFaena.Start();
			}

		}
		public static void ParaFaena()
		{
			if (hiloPrincipalFaena != null && hiloPrincipalFaena.IsAlive)
				try {
				hiloPrincipalFaena.Suspend();
			} catch {
			}

		}
		public static void AcabaFaena()
		{
			if (hiloPrincipalFaena != null && hiloPrincipalFaena.IsAlive)
				try {
				hiloPrincipalFaena.Abort();
				hiloPrincipalFaena = null;
				for(int i=0;i< faenasEstaticas.Count;i++)
					QuitaOAbortaFaena(faenasEstaticas[i]);

			} catch {
			}
		}
		public static void VaciaListaFaenas()
		{
			faenasEstaticas.Buida();
		}

		static void IIniciaFaena()
		{
            Thread hiloFaena;

            for (int i = hilosFaenas.Count; i < MaxThreadsClase; i++) {//pongo los threads que voy a necesitar
				hiloFaena = new Thread(() => HazFaena());
				hiloFaena.Priority = ThreadPriority;
				hiloFaena.Name = generadorId.Siguiente();
				hilosFaenas.Afegir(hiloFaena.Name, hiloFaena);
				hiloFaena.Start();
			}

		}

		static void HazFaena()
		{
			Tiket<TipoTrabajo> tiket = null;
			while (faenasEstaticas.Count > 0) {
				tiket = faenasEstaticas.Pop();
				if (tiket != null) {
					feanasHaciendose.Afegir(tiket.IdUnico, new KeyValuePair<Thread, Tiket<TipoTrabajo>>(Thread.CurrentThread, tiket));
					tiket.EstadoFaena = Tiket<TipoTrabajo>.EstadoFaenaEnum.Haciendose;
					try {
						tiket.HazFaena();
					} catch (Exception ex) {
						tiket.Excepcion(ex);
					}
					QuitarFaena(tiket);
					feanasHaciendose.Elimina(tiket.IdUnico);
					tiket.EstadoFaena = Tiket<TipoTrabajo>.EstadoFaenaEnum.Acabado;
					tiket.FaenaHechaEvent();
				}
			}
			hilosFaenas.Elimina(Thread.CurrentThread.Name);

		}
		public static void QuitaOAbortaFaena(Tiket<TipoTrabajo> faenaHaAbortar)
		{
			if (feanasHaciendose.Existeix(faenaHaAbortar.IdUnico)) {
				hilosFaenas.Elimina(feanasHaciendose[faenaHaAbortar.IdUnico].Key.Name);
				try {
					feanasHaciendose[faenaHaAbortar.IdUnico].Key.Abort();
				} catch {
				}
				feanasHaciendose.Elimina(faenaHaAbortar.IdUnico);
				faenaHaAbortar.DentroPool = false;
			} else if (faenasEstaticas.Existeix(faenaHaAbortar)) {
				faenasEstaticas.Elimina(faenaHaAbortar);
				faenaHaAbortar.DentroPool = false;
			}
		}

		public void EsperaFin()
		{
			while (this.hiloPrincipal.IsAlive)
				Thread.Sleep(100);
		}

		public static void EsperarHaAcabar()
		{
			while (faenasEstaticas.Count > 0)
				Thread.Sleep(100);
		}
		#endregion


		public static ThreadPriority ThreadPriority {
			get { return prioridadThreads; }
			set {
				prioridadThreads = value;
				foreach (KeyValuePair<string,Thread> faenaHaciendose in hilosFaenas)
					faenaHaciendose.Value.Priority = ThreadPriority;
			}
		}
		public ThreadPriority PrioridadThreads {
			get { return prioridadPorDefecto; }
			set {
				prioridadPorDefecto = value;
				if (hiloPrincipal != null && hiloPrincipal.IsAlive) {
					this.hiloPrincipal.Priority = prioridadPorDefecto;
					for(int i=0;i< this.hilosActivos.Length;i++)
						if (this.hilosActivos[i] != null && this.hilosActivos[i].IsAlive)
                            this.hilosActivos[i].Priority = prioridadPorDefecto;
				}
			}
		}

	}
    public delegate void TiketEventHandler<Trabajo>(Tiket<Trabajo> tiket,EventArgs args);
	public class Tiket<TipoTrabajo> : IComparable<Tiket<TipoTrabajo>>, IClauUnicaPerObjecte
	{
		public enum PrioridadEnum
		{
			Primero,
			DeLosPrimeros,
			Ninguna,
			DeLosUltimos,
			Ultimo
		}
		public enum EstadoFaenaEnum
		{
			Pendiente,
			Haciendose,
			Acabado,
			Abortada,
			AbortadaPorExcepcionNoControlada
		}
		static GeneradorHex generadorId = new GeneradorHex();
		int idUnico;
		int posicionId;
		PrioridadEnum prioridad;
		TrabajaEventHandler<TipoTrabajo> metodoConParametros;
		TrabajaEventHandler metodoSinParametros;

		TipoTrabajo trabajo;
		bool dentroPool;
		EstadoFaenaEnum estadoFaena;
		Semaphore esperaTreballFet;
		System.Timers.Timer dejeDeEsperar;
        private Task taskHaciendoSeEnClase;

        public event TiketEventHandler<TipoTrabajo> FaenaHecha;
		public event ExceptionOnTiket<TipoTrabajo> ExcepcionDuranteLaEjecucion;
		public Tiket(TrabajaEventHandler metodo)
			: this(null, default(TipoTrabajo))
		{
			metodoSinParametros = metodo;
		}
		public Tiket(TrabajaEventHandler<TipoTrabajo> metodo, TipoTrabajo trabajo)
			: this(metodo, trabajo, PrioridadEnum.Ninguna)
		{
		}
		public Tiket(TrabajaEventHandler<TipoTrabajo> metodo, TipoTrabajo trabajo, PrioridadEnum prioridad)
		{
			this.metodoConParametros = metodo;
			this.trabajo = trabajo;
			posicionId = -1;
			Prioridad = prioridad;
			dentroPool = false;
			idUnico = generadorId.Siguiente();
			EstadoFaena = EstadoFaenaEnum.Pendiente;
			esperaTreballFet = new Semaphore(0, 1);
			dejeDeEsperar = new System.Timers.Timer();
			dejeDeEsperar.Elapsed += AcabaEsperaEvento;
		}

		public int IdUnico {
			get {
				return idUnico;
			}
		}

		public EstadoFaenaEnum EstadoFaena {
			get {
				return estadoFaena;
			}
			internal set {
				estadoFaena = value;
			}
		}

		public bool DentroPool {
			get {
				return dentroPool;
			}
			internal set {
				dentroPool = value;
			}
		}

		public int PosicionId {
			get {
				return posicionId;
			}
			set {
				if (dentroPool) {
					posicionId = value;

					MiPool<TipoTrabajo>.OrdenarFaena();
				}
			}
		}

		public PrioridadEnum Prioridad {
			get {
				return prioridad;
			}
			set {
				prioridad = value;
				if (posicionId != -1)
					MiPool<TipoTrabajo>.OrdenarFaena();
			}
		}
		public TrabajaEventHandler<TipoTrabajo> MetodoConParametros {
			get {
				return metodoConParametros;
			}
		}
		public TrabajaEventHandler MetodoSinParametros {
			get { return metodoSinParametros; }
		}
		public TipoTrabajo Trabajo {
			get {
				return trabajo;
			}
		}
		public void AñadirPool()
		{
			if (!DentroPool)
				MiPool<TipoTrabajo>.AñadirFaena(this);
			MiPool<TipoTrabajo>.IniciaFaena();

		}
		public void QuitarPool()
		{
			if (DentroPool)
				MiPool<TipoTrabajo>.QuitarFaena(this);
		}
		internal void FaenaHechaEvent()
		{
			if (FaenaHecha != null)
				FaenaHecha(this, new EventArgs());

		}
		public async Task HazFaenaAsync()
		{
            EstadoFaena = EstadoFaenaEnum.Haciendose;
            try {
                if (MetodoConParametros != null)
                    MetodoConParametros(trabajo);
                else if (MetodoSinParametros != null)
                    MetodoSinParametros();
                EstadoFaena = EstadoFaenaEnum.Acabado;
                if (FaenaHecha != null)
                    FaenaHecha(this, new EventArgs());
               
            }
            catch { EstadoFaena = EstadoFaenaEnum.AbortadaPorExcepcionNoControlada; }
            finally { DejarDeEsperar(); }
           
		}
        public void HazFaena()
        {
            taskHaciendoSeEnClase = HazFaenaAsync();
            taskHaciendoSeEnClase.Wait();
            taskHaciendoSeEnClase = null;
        }
        #region IClauUnicaPerObjecte implementation
        public IComparable Clau()
		{
			return idUnico;
		}
		#endregion
		/// <summary>
		/// Espera como maximo el tiempo especificado
		/// </summary>
		/// <param name="milisegundosEspera"></param>
		public void Esperar(double milisegundosEspera)
		{
			dejeDeEsperar.Interval = milisegundosEspera;
			try {
				dejeDeEsperar.Start();
			} catch {
				dejeDeEsperar.Stop();
				dejeDeEsperar.Start();
			}
			Esperar();
		}
		/// <summary>
		/// Espera a que acabe la faena
		/// </summary>
		public void Esperar()
		{
			if (!DentroPool && !EstadoFaena.Equals(EstadoFaenaEnum.Acabado)||taskHaciendoSeEnClase!=null)
				throw new Exception("El proceso no esta asociado MiPool");
			if (!EstadoFaena.Equals(EstadoFaenaEnum.Acabado)) {
				try {
					FaenaHecha += AcabaDeEsperar;
					esperaTreballFet.WaitOne();

				} catch {
				}
                finally { FaenaHecha -= AcabaDeEsperar; }
			}
		}

		private void AcabaDeEsperar(object sender, EventArgs e)
		{
			DejarDeEsperar();
		}

		void AcabaEsperaEvento(object sender, System.Timers.ElapsedEventArgs e)
		{

			DejarDeEsperar();
			dejeDeEsperar.Stop();
		}

		/// <summary>
		/// Se necesita llamarlo desde otro hilo porque el otro esta ocupado esperando...
		/// </summary>
		public void DejarDeEsperar()
		{
			try {
				esperaTreballFet.Release();
			} catch {
			}
		}


		#region IComparable implementation

		public int CompareTo(Tiket<TipoTrabajo> other)
		{
			int compareTo = prioridad.CompareTo(other.prioridad);
			if (compareTo == 0)
				return PosicionId.CompareTo(other.PosicionId);
			else
				return compareTo;
		}

		internal void Excepcion(Exception ex)
		{
			if (ExcepcionDuranteLaEjecucion != null)
				ExcepcionDuranteLaEjecucion(this, ex);
			AbortaTrabajo();
			estadoFaena = EstadoFaenaEnum.AbortadaPorExcepcionNoControlada;
		}

		#endregion

		public void AbortaTrabajo()
		{
			MiPool<TipoTrabajo>.QuitaOAbortaFaena(this);
			estadoFaena = EstadoFaenaEnum.Abortada;
			DejarDeEsperar();
		}

	}




}
