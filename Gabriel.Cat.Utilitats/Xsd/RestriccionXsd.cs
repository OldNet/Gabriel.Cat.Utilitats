/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 04/03/2015
 * Hora: 20:41
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.Xml;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of RestriccionXsd.
	/// </summary>
	public class RestriccionXsd:IClonable,IEnumerable<KeyValuePair<Restricciones,string>>
	{

		public enum WhiteSpaceValues
		{
			collapse,
			perserver,
			replace
		}

		
		private TiposBaseXsd tipoBaseRestriccion;
		private SortedList<Restricciones,string> restricciones;
		private List<string> elementosEnumerados;
		public RestriccionXsd()
			: this(TiposBaseXsd.String)
		{
			
		}
		public RestriccionXsd(TiposBaseXsd tipo)
		{
			tipoBaseRestriccion = tipo;
			restricciones = new SortedList<Restricciones, string>();
			elementosEnumerados = new List<string>();

		}

		public RestriccionXsd(XmlElement xmlElement):this()
		{
			try {
				string tipoBase = TiposBaseXsd.Parse(xmlElement.Attributes["base"].Value);
				

				//sacar restricciones
				foreach (XmlNode restriccion in xmlElement.ChildNodes)
					Añadir(Restricciones.Parse(restriccion.Name), restriccion.Attributes["value"].Value);
			} catch {
				throw new XsdException("El nodo no es de un restriccionXsd valido");
				
			}
			
		}
		public TiposBaseXsd TipoBaseRestriccion {
			get {
				return tipoBaseRestriccion;
			}
			set {
				tipoBaseRestriccion = value;
			}
		}

		public bool Añadir(Restricciones restriccion, string valor, bool excepcionPorIncompativilidad = false)
		{
			//usar | para saber si es compatible la restriccion con el tipo sino puede añadir=false
			bool añadir = ((int)restriccion & (int)TipoBaseRestriccion) != (int)TipoBaseRestriccion;
			if (!añadir && excepcionPorIncompativilidad)
				throw new XsdException("El tipo base " + TipoBaseRestriccion.ToString().Substring(1) + " no puede con la restriccion \"" + restriccion + "\"");

			//Controlo compatibilidad
			if (TipoBaseRestriccion.Equals(TiposBaseXsd.Boolean)) {
				if (restriccion.Equals(Restricciones.Enumeration) || restriccion.Equals(Restricciones.Length) || restriccion.Equals(Restricciones.MinLength) || restriccion.Equals(Restricciones.MaxLength))
				if (excepcionPorIncompativilidad)
					throw new XsdException("El tipo base Boolean no puede con la restriccion \"" + restriccion + "\"");
				añadir = false;
			} else if (TipoBaseRestriccion.Equals(TiposBaseXsd.ENTITIES) || TipoBaseRestriccion.Equals(TiposBaseXsd.IDREFS) || TipoBaseRestriccion.Equals(TiposBaseXsd.NMTOKENS)) {
				if (restriccion.Equals(Restricciones.Pattern))
				if (excepcionPorIncompativilidad)
					throw new XsdException("El tipo base " + TipoBaseRestriccion.ToString().Substring(1) + " no puede con la restriccion \"" + restriccion + "\"");
				añadir = false;
			}
			if (añadir) {
				if (!restricciones.ContainsKey(restriccion))
					restricciones.Add(restriccion, null);
				restricciones[restriccion] = valor;
			}
			return añadir;

			//visual studio lo omite...      //quitar restricines que sean incompatibles entre ellas...y si es true lanzar excepcion
			//usar switch con el toString de la restriccion
		}
		public void Quitar(Restricciones restriccion)
		{
			if (restricciones.ContainsKey(restriccion))
				restricciones.Remove(restriccion);
			if (restriccion.Equals(Restricciones.Enumeration))
				elementosEnumerados.Clear();
		}

		#region IClonable implementation

		public dynamic Clon()
		{
			RestriccionXsd clon = new RestriccionXsd(tipoBaseRestriccion);
			foreach (var restriccion in restricciones)
				clon.restricciones.Add(restriccion.Key, restriccion.Value);
			clon.elementosEnumerados.AddRange(elementosEnumerados);
			return clon;
			
		}
		#endregion
		public override string ToString()
		{
			
			string restriccionString = "<xs:restriccion base=\"xs:" + TipoBaseRestriccion + "\">";
			foreach (var restriccion in this)
				restriccionString += "<xs:" + restriccion.Key + " value=\"" + restriccion.Value + "\"/>";
			restriccionString += "</xs:restriccion>";
			return restriccionString;
		}
		
		

		
		#region IEnumerable implementation
		public IEnumerator<KeyValuePair<Restricciones, string>> GetEnumerator()
		{
			foreach (var restriccion in restricciones)
				if (!restriccion.Key.Equals(Restricciones.Enumeration))
					yield return restriccion;
				else {
					for (int i = 0; i < elementosEnumerados.Count; i++)
						yield return new KeyValuePair<Restricciones, string>(restriccion.Key, elementosEnumerados[i]);
				
				}
		}
		#endregion
		#region IEnumerable implementation
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion
		#region Equals and GetHashCode implementation
		public override bool Equals(object obj)
		{
			RestriccionXsd other = obj as RestriccionXsd;
			if (other == null)
				return false;
			return object.Equals(this.tipoBaseRestriccion, other.tipoBaseRestriccion) && object.Equals(this.restricciones, other.restricciones) && object.Equals(this.elementosEnumerados, other.elementosEnumerados);
		}

		public override int GetHashCode()
		{
			int hashCode = 0;
			unchecked {
				if (tipoBaseRestriccion != null)
					hashCode += 1000000007 * tipoBaseRestriccion.GetHashCode();
				if (restricciones != null)
					hashCode += 1000000009 * restricciones.GetHashCode();
				if (elementosEnumerados != null)
					hashCode += 1000000021 * elementosEnumerados.GetHashCode();
			}
			return hashCode;
		}

		public static bool operator ==(RestriccionXsd lhs, RestriccionXsd rhs)
		{
			if (ReferenceEquals(lhs, rhs))
				return true;
			if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
				return false;
			return lhs.Equals(rhs);
		}

		public static bool operator !=(RestriccionXsd lhs, RestriccionXsd rhs)
		{
			return !(lhs == rhs);
		}

		#endregion
	}
	public class XsdException:Exception
	{
		public XsdException()
			: base()
		{
		}
		public XsdException(string mensaje)
			: base(mensaje)
		{
		}
	}
}
