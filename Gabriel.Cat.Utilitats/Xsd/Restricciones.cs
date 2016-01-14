/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 06/03/2015
 * Hora: 17:29
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;

namespace Gabriel.Cat
{
	/// <summary>
	/// Enumerador de Restricciones.
	/// </summary>
	public sealed class Restricciones:Enumerador
	{
		public static readonly Restricciones Enumeration = new Restricciones("Enumeration", 1 | 2 | 4 | 8);
		//(a Boolean data type cannot use this constraint)
		public static readonly Restricciones MaxExclusive = new Restricciones("MaxExclusive", 4 | 2);
		public static readonly Restricciones MinExclusive = new Restricciones("MinExclusive", 4 | 2);
		public static readonly Restricciones MaxInclusive = new Restricciones("MaxInclusive", 4 | 2);
		public static readonly Restricciones MinInclusive = new Restricciones("MinInclusive", 4 | 2);
		public static readonly Restricciones Pattern = new Restricciones("Pattern", 1 | 2 | 4 | 8);
		// (NMTOKENS, IDREFS, and ENTITIES cannot use this constraint)
		public static readonly Restricciones WhiteSpace = new Restricciones("WhiteSpace", 1 | 2 | 4 | 8);
		public static readonly Restricciones Length = new Restricciones("Length", 1 | 8);
		//(a Boolean data type cannot use this constraint)
		public static readonly Restricciones MaxLength = new Restricciones("MaxLength", 1 | 8);
		//(a Boolean data type cannot use this constraint)
		public static readonly Restricciones MinLength = new Restricciones("MinLength", 1 | 8);
		//(a Boolean data type cannot use this constraint)
		public static readonly Restricciones FractionDigits = new Restricciones("FractionDigits", 2);
		public static readonly Restricciones TotalDigits = new Restricciones("TotalDigits", 2);
		
		
		static SortedList<string,Restricciones> listaRestricciones;
		static Restricciones()
		{
			listaRestricciones=new SortedList<string,Restricciones>();
			foreach(Restricciones restriccion in Lista())
				listaRestricciones.Add(restriccion.DisplayName,restriccion);
		}
				public static IEnumerable<Restricciones> Lista()
		{
			return Enumerador.GetAll<Restricciones>();
		}
		public static Restricciones Parse(string nombre)
		{
			Restricciones restriccion=null;
			if(listaRestricciones.ContainsKey(nombre))
				restriccion=listaRestricciones[nombre];
			return restriccion;
				
		}
		private Restricciones(string key, int value)
			: base(key, value)
		{
		}
		public Restricciones()
		{
		}

	}
}
