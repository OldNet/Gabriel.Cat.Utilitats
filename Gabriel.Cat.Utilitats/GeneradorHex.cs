/*
 * Creado por SharpDevelop.
 * Usuario: Gabriel
 * Fecha: 13/07/2015
 * Hora: 14:03
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of GeneradorHex.
	/// </summary>
	public class GeneradorHex:GeneradorID<Hex> 
	{
		public GeneradorHex()
			: this("0", (Hex)long.MaxValue)
		{
		}
		public GeneradorHex(string inicioHex)
			: this(inicioHex, null)
		{
		}
		public GeneradorHex(string inicioHex, string finHex)
		{
			if (inicioHex != default(Hex) && !Hex.ValidaString(inicioHex) || finHex != default(Hex) && !Hex.ValidaString(finHex))
				throw new Exception("string hex incorrecta");
			this.inicio = inicioHex;
			this.fin = finHex;
			this.Numero = inicioHex;
			MetodoSiguiente=Siguiente;
			MetodoAnterior=Anterior;
		}

		#region implemented abstract members of GeneradorID

		void Siguiente()
		{

			if (fin != default(Hex) && Numero.Equals(fin)) {
				if (inicio != default(Hex))
					Numero = inicio;
				else {
					Numero = "0";
				}
			} else {
				//le sumo uno
				Numero++;
			}
		
		}


		void Anterior()
		{
			if (inicio != default(Hex) && Numero.Equals(inicio)) {
				if (fin != default(Hex))
					Numero = fin;
				else {
					Numero = long.MaxValue.ToString("X4");
				}
			} else {
				//le resto uno
				Numero--;
			}

		}

		#endregion


	}
}
