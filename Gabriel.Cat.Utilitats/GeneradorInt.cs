/*
 * Creado por SharpDevelop.
 * Usuario: Gabriel
 * Fecha: 13/07/2015
 * Hora: 13:57
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of GeneradorInt.
	/// </summary>
	public class GeneradorInt:GeneradorID<int>
	{

		public GeneradorInt():this(0)
		{
		}
		public GeneradorInt(int inicio):this(inicio,int.MaxValue)
		{
		}
		public GeneradorInt(int inicio,int fin)
		{
			this.inicio=inicio;
			this.fin=fin;
			MetodoSiguiente=Siguiente;
			MetodoAnterior=Anterior;
		}
		#region implemented abstract members of GeneradorID

		void Siguiente()
		{
			Numero++;
			if(Numero>fin)
                Numero = inicio;
		}

		void Anterior()
		{
            Numero--;
			if(Numero < inicio)
                Numero = fin;
		}

		#endregion
	}
}
