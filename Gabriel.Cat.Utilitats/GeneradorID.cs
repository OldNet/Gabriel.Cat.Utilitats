/*
 * Creado por SharpDevelop.
 * Usuario: Gabriel
 * Fecha: 13/07/2015
 * Hora: 13:54
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Threading;
namespace Gabriel.Cat
{
	public delegate void MetodoGeneradorID();
	/// <summary>
	/// Description of GeneradorID.
	/// </summary>
	public  class GeneradorID<TValue>
	{
		public MetodoGeneradorID MetodoSiguiente;
		public MetodoGeneradorID MetodoAnterior;
        private TValue numero;
        protected	TValue inicio;
		protected	TValue fin;
		object objSiguiente=new object(),objAnterior=new object();//para poder usar Monitor...
		public TValue Inicio {
			get {
				return inicio;
			}
			set {
				inicio = value;
			}
		}

		public TValue Fin {
			get {
				return fin;
			}
			set {
				fin = value;
			}
		}

        public TValue Numero
        {
            get
            {
                return numero;
            }

            set
            {
                numero = value;
            }
        }

        public TValue Actual()
		{
			return numero;
		}
		public TValue Siguiente()
		{
			
			if (MetodoSiguiente != null){
				Monitor.Enter(objSiguiente);
                try
                {
                    MetodoSiguiente();
                }
                catch { numero = Inicio; }
                Monitor.Exit(objSiguiente);}
			return numero;
		}
		public TValue Siguiente(long numeroDeVeces)
		{
			
			for (long i = 0; i < numeroDeVeces; i++)
				Siguiente();

			return numero;
		}
		public TValue Anterior()
		{
			if (MetodoAnterior != null){
				Monitor.Enter(objAnterior);
                try
                {
                    MetodoAnterior();
                }
                catch { numero = Inicio; }
				Monitor.Exit(objAnterior);
			}
			return numero;
		}
		public TValue Anterior(long numeroDeVeces)
		{
			for (long i = 0; i < numeroDeVeces; i++)
				Anterior();
			return numero;
		}
		public  void Reset()
		{
			numero = inicio;
		}
		public  void Reset(TValue inicio)
		{
			this.numero = inicio;
		}
		public static GeneradorID<TValue> operator --(GeneradorID<TValue> gen)
		{
			gen.Anterior();
			return gen;
		}
		public static GeneradorID<TValue> operator ++(GeneradorID<TValue> gen)
		{
			gen.Siguiente();
			return gen;
		}
	}
}
