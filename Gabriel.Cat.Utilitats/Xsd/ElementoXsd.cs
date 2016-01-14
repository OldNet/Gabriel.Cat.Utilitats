/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 02/03/2015
 * Hora: 19:02
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Xml;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of ElementoXsd.
	/// </summary>
	public struct ElementoXsd : IEquatable<ElementoXsd>,IComparable,IDeclaracionesXsd
	{
		string nombre;
		string tipo;
		string valorFijo;
		string valorPorDefecto;

		public ElementoXsd(string nombre, string tipo)
		{
			this.nombre = nombre;
			this.tipo = tipo;
			valorFijo = null;
			valorPorDefecto = null;

		}
		public ElementoXsd(string nombre, TipoSimpleXsd tipoSimple)
			: this(nombre, tipoSimple.Nombre)
		{
		}
		public ElementoXsd(string nombre, TipoComplejoXsd tipoComplejo)
			: this(nombre, tipoComplejo.Nombre)
		{
			
		}
		public ElementoXsd(XmlNode source):this("","")
		{
			try {
				nombre = source.Attributes["name"].Value;
				tipo = source.Attributes["type"].Value;
				valorFijo = source.Attributes["fixed"] != null ? source.Attributes["fixed"].Value : null;
				valorPorDefecto = source.Attributes["default"] != null ? source.Attributes["default"].Value : null;
				
			} catch {
				if (source.Attributes["ref"] == null)
					throw new XsdException("El nodo no es de un elementoXsd valido");
				else
					nombre = source.Attributes["ref"].Value;
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

		public string Tipo {
			get {
				return tipo;
			}
			set {
				tipo = value;
			}
		}

		public string ValorFijo {
			get {
				return valorFijo;
			}
			set {
				valorFijo = value;
				valorPorDefecto = null;
			}
		}

		public string ValorPorDefecto {
			get {
				return valorPorDefecto;
			}
			set {
				
				valorPorDefecto = value;
				valorFijo = null;
			}
		}
		public string Declaracion()
		{
			return ToString();
		}
		public string UsoConReferencia()
		{
			return "<xs:element ref=\"" + Nombre + "\"/>";
		}
		public string UsoCompleto()
		{
			return ToString();
		}
		public override string ToString()
		{
			string elemento = "<xs:element name=\"" + nombre + "\" type=\"" + Tipo + "\"";
			if (ValorPorDefecto != null)
				elemento += " default=\"" + ValorPorDefecto + "\"";
			else if (ValorFijo != null)
				elemento += " fixed=\"" + ValorFijo + "\"";		
			elemento += "/>";
			return elemento;
		}
		#region Equals and GetHashCode implementation
		// The code in this region is useful if you want to use this structure in collections.
		// If you don't need it, you can just remove the region and the ": IEquatable<ElementoXsd>" declaration.
		
		#region Equals and GetHashCode implementation
		public override bool Equals(object obj)
		{
			return (obj is ElementoXsd) && Equals((ElementoXsd)obj);
		}

		public bool Equals(ElementoXsd other)
		{
			return this.nombre == other.nombre && this.tipo == other.tipo;
		}

		public override int GetHashCode()
		{
			int hashCode = 0;
			unchecked {
				if (nombre != null)
					hashCode += 1000000007 * nombre.GetHashCode();
				if (tipo != null)
					hashCode += 1000000009 * tipo.GetHashCode();
			}
			return hashCode;
		}

		#endregion
		
		
		public static bool operator ==(ElementoXsd left, ElementoXsd right)
		{
			return left.Equals(right);
		}
		
		public static bool operator !=(ElementoXsd left, ElementoXsd right)
		{
			return !left.Equals(right);
		}

		#region IComparable implementation


		public int CompareTo(object obj)
		{
			return ToString().CompareTo(obj);
		}


		#endregion

		#endregion
		public static bool ValidadorElementoXsd(string elementoXsd)
		{
			string[] camposXsd;
			bool valido = elementoXsd != null;
			if (valido) {
				valido = elementoXsd.Contains("\"");
				if (valido) {
					camposXsd = elementoXsd.Split('"');
					valido = camposXsd.Length == 5;
					if (valido) {
						valido = camposXsd[0] == "<xs:element name=\"" && camposXsd[2] == "\" type=" && camposXsd[4] == "\"/>";
					}
				}
			}
			return valido;
		}
	}
}
