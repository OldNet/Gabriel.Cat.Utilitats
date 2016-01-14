/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 01/03/2015
 * Hora: 19:40
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Xml;
namespace Gabriel.Cat
{

	public class TipoSimpleXsd:IClonable,IComparable<TipoSimpleXsd>
	{
		private string nombre;
		private RestriccionXsd restriccion;

		public TipoSimpleXsd(string nombre, RestriccionXsd restriccion)
		{
			Restriccion=restriccion;
			if (nombre != null)
				nombre = nombre.Trim(new char[]{ ' ','\r','\t','\n','"' });//poner caracteres prohibidos
			Nombre = nombre;
			Restriccion = restriccion;
		}
		public TipoSimpleXsd(XmlNode source)
		{
			try{
			nombre=source.Attributes["nombre"].Value;
			restriccion=new RestriccionXsd(source["restriccion"]);
			}catch{
			throw new XsdException("El nodo no es de un TipoSimpleXsd valido");
			}
			
		}

		public string Nombre {
			get {
				return nombre;
			}
			set {
				nombre = value;
			}
		}

		public RestriccionXsd Restriccion {
			get {
				return restriccion;
			}
			set {
				if (value == null)
					throw new NullReferenceException("la restriccion no puede ser null!!");
				restriccion = value;
			}
		}

		
		public override string ToString()
		{
			string tipoSimple;
			tipoSimple = "<xs:simpleType";
			if (Nombre != null)
				tipoSimple += " name=\"" + Nombre + "\"";
			tipoSimple += ">";
			tipoSimple += Restriccion;
			tipoSimple += "</xs:simpleType>";
			return tipoSimple;
		}


		#region IClonable implementation
		public dynamic Clon()
		{

			return new TipoSimpleXsd(this.nombre,Restriccion.Clon());
		}
		#endregion
		#region Equals and GetHashCode implementation
		public override bool Equals(object obj)
		{
			TipoSimpleXsd other = obj as TipoSimpleXsd;
				if (other == null)
					return false;
						return this.nombre == other.nombre && object.Equals(this.restriccion, other.restriccion);
		}

		public override int GetHashCode()
		{
			int hashCode = 0;
			unchecked {
				if (nombre != null)
					hashCode += 1000000007 * nombre.GetHashCode();
				if (restriccion != null)
					hashCode += 1000000009 * restriccion.GetHashCode();
			}
			return hashCode;
		}

		public static bool operator ==(TipoSimpleXsd lhs, TipoSimpleXsd rhs) {
			if (ReferenceEquals(lhs, rhs))
				return true;
			if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
				return false;
			return lhs.Equals(rhs);
		}

		public static bool operator !=(TipoSimpleXsd lhs, TipoSimpleXsd rhs) {
			return !(lhs == rhs);
		}

		#endregion

		#region IComparable implementation
		public int CompareTo(TipoSimpleXsd other)
		{
			return ToString().CompareTo(other.ToString());
		}
		#endregion
	}
}


