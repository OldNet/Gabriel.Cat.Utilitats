/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 06/03/2015
 * Hora: 17:51
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of TiposBaseXsd.
	/// </summary>
	public sealed class TiposBaseXsd:Enumerador
	{
		
		//string
		public static readonly TiposBaseXsd String = new TiposBaseXsd("string", 1);
		public static readonly TiposBaseXsd Lenguage = new TiposBaseXsd("lenguage", 1);
		public static readonly TiposBaseXsd NormalizedString = new TiposBaseXsd("normalizedString", 1);
		public static readonly TiposBaseXsd Token = new TiposBaseXsd("token", 1);
		public static readonly TiposBaseXsd QName = new TiposBaseXsd("QName", 1);
		public static readonly TiposBaseXsd Name = new TiposBaseXsd("Name", 1);
		public static readonly TiposBaseXsd ID = new TiposBaseXsd("ID", 1);
		public static readonly TiposBaseXsd IDREF = new TiposBaseXsd("IDREF", 1);
		public static readonly TiposBaseXsd IDREFS = new TiposBaseXsd("IDREFS", 1);
		public static readonly TiposBaseXsd ENTITY = new TiposBaseXsd("ENTITY", 1);
		public static readonly TiposBaseXsd ENTITIES = new TiposBaseXsd("ENTITIES", 1);
		public static readonly TiposBaseXsd NMTOKEN = new TiposBaseXsd("NMTOKEN", 1);
		public static readonly TiposBaseXsd NMTOKENS = new TiposBaseXsd("NMTOKENS", 1);
		public static readonly TiposBaseXsd NCName = new TiposBaseXsd("NCName", 1);
		
		
		//numeric
		public static readonly TiposBaseXsd Byte = new TiposBaseXsd("byte", 2);
		public static readonly TiposBaseXsd Decimal = new TiposBaseXsd("decimal", 2);
		public static readonly TiposBaseXsd Int = new TiposBaseXsd("int", 2);
		public static readonly TiposBaseXsd Integer = new TiposBaseXsd("integer", 2);
		public static readonly TiposBaseXsd Long = new TiposBaseXsd("long", 2);
		public static readonly TiposBaseXsd NegativeInteger = new TiposBaseXsd("negativeInteger", 2);
		public static readonly TiposBaseXsd NonNegativeInteger = new TiposBaseXsd("nonNegativeInteger", 2);
		public static readonly TiposBaseXsd NonPositiveInteger = new TiposBaseXsd("nonPositiveInteger", 2);
		public static readonly TiposBaseXsd PositiveInteger = new TiposBaseXsd("positiveInteger", 2);
		public static readonly TiposBaseXsd Short = new TiposBaseXsd("short", 2);
		public static readonly TiposBaseXsd UnsignedLong = new TiposBaseXsd("unsignedLong", 2);
		public static readonly TiposBaseXsd UnsignedInt = new TiposBaseXsd("unsignedInt", 2);
		public static readonly TiposBaseXsd UnsignedShort = new TiposBaseXsd("unsignedShort", 2);
		public static readonly TiposBaseXsd UnsignedByte = new TiposBaseXsd("unsignedByte", 2);
		
		//date
		public static readonly TiposBaseXsd Date = new TiposBaseXsd("date", 4);
		public static readonly TiposBaseXsd DateTime = new TiposBaseXsd("dateTime", 4);
		public static readonly TiposBaseXsd Duration = new TiposBaseXsd("duration", 4);
		public static readonly TiposBaseXsd GDay = new TiposBaseXsd("gDay", 4);
		public static readonly TiposBaseXsd GMonth = new TiposBaseXsd("gMonth", 4);
		public static readonly TiposBaseXsd GMonthDay = new TiposBaseXsd("gMonthDay", 4);
		public static readonly TiposBaseXsd GYear = new TiposBaseXsd("gYear", 4);
		public static readonly TiposBaseXsd GYearMonth = new TiposBaseXsd("gYearMonth", 4);
		public static readonly TiposBaseXsd Time = new TiposBaseXsd("time", 4);
		
		//misc
		public static readonly TiposBaseXsd Boolean = new TiposBaseXsd("boolean", 8);
		public static readonly TiposBaseXsd Base64Binary = new TiposBaseXsd("base64Binary", 8);
		public static readonly TiposBaseXsd HexBinary = new TiposBaseXsd("hexBinary", 8);
		public static readonly TiposBaseXsd AnyUri = new TiposBaseXsd("anyUri", 8);
		
		
		private TiposBaseXsd(string key, int value)
			: base(key, value)
		{
		}
		public TiposBaseXsd()
		{
		}
		static SortedList<string,TiposBaseXsd> listaTiposBase;
		static TiposBaseXsd()
		{
			listaTiposBase=new SortedList<string,TiposBaseXsd>();
			foreach(TiposBaseXsd restriccion in Lista())
				listaTiposBase.Add(restriccion.DisplayName,restriccion);
		}
		public static IEnumerable<TiposBaseXsd> Lista()
		{
			return Enumerador.GetAll<TiposBaseXsd>();
		}
		public static TiposBaseXsd Parse(string nombre)
		{
			TiposBaseXsd restriccion=null;
			if(listaTiposBase.ContainsKey(nombre))
				restriccion=listaTiposBase[nombre];
			return restriccion;
			
		}
	}
}
