/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 04/03/2015
 * Hora: 19:46
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Xml;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of AtributoXsd.
	/// </summary>
	public class AtributoXsd:IComparable<AtributoXsd>,IDeclaracionesXsd
	{
		public enum UsoAtributo
		{
			required,
			prohibited,
			optional
		}
		string nombre;
		TiposBaseXsd tipo;
		string valorPorDefecto;
		string valorFijo;
		UsoAtributo uso;
		public AtributoXsd(string nombre)
			: this(nombre, TiposBaseXsd.String)
		{
			
		}
		public AtributoXsd(string nombre, TiposBaseXsd tipoBase)
			: this(nombre, tipoBase, UsoAtributo.optional)
		{

			
		}
		public AtributoXsd(string nombre, TiposBaseXsd tipoBase, UsoAtributo uso)
		{
			Nombre = nombre;
			TipoBase = tipoBase;
			Uso = uso;
		}
		public AtributoXsd(XmlNode source)
		{
			try {
				nombre = source.Attributes["name"].Value;
				tipo = TiposBaseXsd.Parse(source.Attributes["type"].Value);
			} catch {
				if(source["ref"]==null)
					throw new XsdException("El nodo no es de un atributoXsd valido");
				else
					nombre=source["ref"].Value;
			}
			valorFijo = source.Attributes["fixed"] != null ? source.Attributes["fixed"].Value : null;
			valorPorDefecto = source.Attributes["default"] != null ? source.Attributes["default"].Value : null;
		}

		public string Nombre {
			get {
				return nombre;
			}
			set {
				nombre = value;
			}
		}

		public TiposBaseXsd TipoBase {
			get {
				return tipo;
			}
			set {
				tipo = value;
			}
		}

		public string ValorPorDefecto {
			get {
				return valorPorDefecto;
			}
			set {
				valorPorDefecto = value;
				if (valorPorDefecto != null)
					valorFijo = null;
			}
		}

		public string ValorFijo {
			get {
				return valorFijo;
			}
			set {
				valorFijo = value;
				if (valorFijo != null)
					valorPorDefecto = null;
			}
		}

		public UsoAtributo Uso {
			get {
				return uso;
			}
			set {
				uso = value;
			}
		}
		public string Declaracion()
		{
			string atributo = "<xs:attribute ";
			atributo += "name=\"" + Nombre + "\" ";
			atributo += " type=\"xs:" + tipo.ToString() + "\" ";
			if (ValorFijo != null)
				atributo += " fixed=\"" + ValorFijo + "\"";
			else if (ValorPorDefecto != null)
				atributo += " default=\"" + ValorPorDefecto + "\"";
			atributo += "/>";
			return atributo;
		}
		public string UsoConReferencia()
		{
			return "<xs:attribute ref=\"" + Nombre + "\" use=\"" + Uso + "\"/>";
		}
		public string UsoCompleto()
		{
			return ToString();
		}
		public override string ToString()
		{
			string atributo = "<xs:attribute ";
			atributo += "name=\"" + Nombre + "\" ";
			atributo += "type=\"xs:" + tipo.ToString() + "\" ";
			atributo += "use=\"" + Uso + "\"";
			if (ValorFijo != null)
				atributo += " fixed=\"" + ValorFijo + "\"";
			else if (ValorPorDefecto != null)
				atributo += " default=\"" + ValorPorDefecto + "\"";
			atributo += "/>";
			return atributo;
		}
		#region Equals and GetHashCode implementation
		public override bool Equals(object obj)
		{
			AtributoXsd other = obj as AtributoXsd;
			if (other == null)
				return false;
			return this.nombre == other.nombre && object.Equals(this.tipo, other.tipo) && this.valorPorDefecto == other.valorPorDefecto && this.valorFijo == other.valorFijo && this.uso == other.uso;
		}

		public override int GetHashCode()
		{
			int hashCode = 0;
			unchecked {
				if (nombre != null)
					hashCode += 1000000007 * nombre.GetHashCode();
				if (tipo != null)
					hashCode += 1000000009 * tipo.GetHashCode();
				if (valorPorDefecto != null)
					hashCode += 1000000021 * valorPorDefecto.GetHashCode();
				if (valorFijo != null)
					hashCode += 1000000033 * valorFijo.GetHashCode();
				hashCode += 1000000087 * uso.GetHashCode();
			}
			return hashCode;
		}

		public static bool operator ==(AtributoXsd lhs, AtributoXsd rhs)
		{
			if (ReferenceEquals(lhs, rhs))
				return true;
			if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
				return false;
			return lhs.Equals(rhs);
		}

		public static bool operator !=(AtributoXsd lhs, AtributoXsd rhs)
		{
			return !(lhs == rhs);
		}

		#endregion
		
		#region IComparable implementation
		public int CompareTo(AtributoXsd other)
		{
			return ToString().CompareTo(other.ToString());
		}
		#endregion
	}
}
