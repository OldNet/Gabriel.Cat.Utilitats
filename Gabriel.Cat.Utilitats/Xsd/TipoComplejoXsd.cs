/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 01/03/2015
 * Hora: 19:40
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.Xml;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of Tipo.
	/// </summary>
	public class TipoComplejoXsd:IClonable,IComparable<TipoComplejoXsd>
	{
		public enum Orden
		{
			All,
			Choice,
			Sequence
		}
		struct ElementoXsdOrdenado:IComparable<ElementoXsdOrdenado>
		{
			public ElementoXsd elemento;
			public int orden;
			public ElementoXsdOrdenado(ElementoXsd elemento, int orden)
			{
				this.elemento = elemento;
				this.orden = orden;
			}
			#region IComparable implementation

			public int CompareTo(ElementoXsdOrdenado other)
			{
				return orden.CompareTo(other.orden);
			}

			#endregion
		}
		//faltan los atributos, las resticciones y mas...
		SortedList<ElementoXsdOrdenado,string> elementos;
		SortedList<AtributoXsd,string> atributos;
		private string nombre;
		string tipoXsdBase;
		Orden orden;
		bool mixed;
		const string HIJOSILIMITADOS = "unbounded";
		int numerosOrden;
		bool isEmpty;
		RestriccionXsd simpleContent;
		bool isExtensibleAttribute;
		bool isExtensibleElements;
		public TipoComplejoXsd():this(""){}
		public TipoComplejoXsd(string nombre)
		{
			elementos = new SortedList<ElementoXsdOrdenado,string>();
			atributos=new SortedList<AtributoXsd, string>();
			this.nombre = nombre!=""?nombre:null;
			this.orden = Orden.Sequence;
			mixed = false;
			numerosOrden = 0;
			isEmpty = false;
			isExtensibleAttribute = false;
			IsExtensibleElement = false;
		}
		public TipoComplejoXsd(XmlNode source):this()
		{
			string orden="sequence";
			try{
				nombre=source.Attributes["name"].Value;
				isEmpty=source.HasChildNodes;
				if(!isEmpty){
					if(source["simpleContent"]!=null)
					{
						//simple
						simpleContent=new RestriccionXsd(source["simpleContent"]["restriccion"]);
					}
					else
					{
						//complejo
						mixed=bool.Parse(source.Attributes["mixed"].Value);
						if(source["extension"]!=null)
						{
							isExtensibleElements=source["extension"]["any"]!=null;
							tipoXsdBase=source["extension"].Attributes["base"].Value;
							if(source["extension"]["all"]!=null)
								orden="all";
							else if (source["extension"]["choice"]!=null)
								orden="choice";
							else if (source["extension"]["sequence"]==null)
								throw new Exception();
							
							foreach(XmlNode elemento in source["extension"][orden].ChildNodes)
								Añadir(new ElementoXsd(elemento),elemento.Attributes["ref"]!=null,int.Parse(elemento.Attributes["minOccurs"].Value),int.Parse(elemento.Attributes["maxOccurs"].Value));
						}
						else
						{
							isExtensibleElements=source["any"]!=null;
							if(source["all"]!=null)
								orden="all";
							else if (source["choice"]!=null)
								orden="choice";
							else if(source["sequence"]==null)
								throw  new Exception();
							
							foreach(XmlNode elemento in source[orden].ChildNodes)
								Añadir(new ElementoXsd(elemento),elemento.Attributes["ref"]!=null,int.Parse(elemento.Attributes["minOccurs"].Value),int.Parse(elemento.Attributes["maxOccurs"].Value));

						}
					}
					//atributos
					foreach(XmlNode atributo in source["attribute"])
						Añadir(new AtributoXsd(atributo),atributo["ref"]!=null);
					isExtensibleAttribute=source["anyAttribute"]!=null;
						
				}
				
				
			}catch{
				throw new XsdException("el nodo no es un TipoComplejoXsd valido");
			}
		}
		public TipoComplejoXsd(string nombre, TipoComplejoXsd tipoXsdBase)
			: this(nombre)
		{
			this.tipoXsdBase = tipoXsdBase.Nombre;
		}

		public string Nombre {
			get {
				return nombre;
			}
			set {
				nombre = value;
			}
		}

		public Orden OrdenElementos {
			get {
				return orden;
			}
			set {
				orden = value;
			}
		}

		public string TipoXsdBase {
			get {
				return tipoXsdBase;
			}
			set {
				tipoXsdBase = value;
			}
		}
		/// <summary>
		/// Si es empty no tiene etiquetas ni contenido dentro solo atributos
		/// </summary>
		public bool IsEmpty {
			get {
				return isEmpty;
			}
			set {
				isEmpty = value;
			}
		}
		/// <summary>
		/// Si solo hay texto no pueden haber etiquetas pero si atributos
		/// </summary>
		public bool IsTextOnly {
			get {
				return SimpleContent != null;
			}

		}

		public bool IsExtensibleAttribute {
			get {
				return isExtensibleAttribute;
			}
			set {
				isExtensibleAttribute = value;
			}
		}

		public bool IsExtensibleElement {
			get{ return isExtensibleElements; }
			set{ isExtensibleElements = value; }
		}

		public RestriccionXsd SimpleContent {
			get {
				return simpleContent;
			}
			set {
				simpleContent = value;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="elementoXsd"></param>
		/// <param name="estaDeclarado"></param>
		/// <param name="minOccurs"></param>
		/// <param name="maxOccurs">si maxOccurs es mas pequeño que minOccurs se pondrá unbounded</param>
		public bool Añadir(ElementoXsd elementoXsd, bool estaDeclarado, int minOccurs = 1, int maxOccurs = 1)
		{
			bool añadido = false;
			if (!Equals(elementoXsd, default(ElementoXsd))) {
				
				string elemento;
				if (!estaDeclarado)
					elemento = elementoXsd.UsoCompleto();
				else
					elemento = elementoXsd.UsoConReferencia();
				elemento = elemento.Substring(0, elemento.Length - 2);
				if (minOccurs >= 0) {
					elemento += " minOccurs=\"" + minOccurs + "\"";
				}
				if (maxOccurs < minOccurs)
					elemento += " maxOccurs=\"" + HIJOSILIMITADOS + "\"/>";
				else
					elemento += " maxOccurs=\"" + maxOccurs + "\"/>";
				elementos.Add(new ElementoXsdOrdenado(elementoXsd, numerosOrden++), elemento);
				añadido = true;
			}
			return añadido;
		}
		public void Quitar(ElementoXsd elementoXsd)
		{
			if (!elementoXsd.Equals(default(ElementoXsd))) {
				ElementoXsdOrdenado elementoAQuitar = default(ElementoXsdOrdenado);
				var elementosEnum = elementos.GetEnumerator();
				while (elementoAQuitar.Equals(default(ElementoXsdOrdenado)) && elementosEnum.MoveNext()) {
					if (elementosEnum.Current.Key.elemento.Equals(elementoXsd))
						elementoAQuitar = elementosEnum.Current.Key;
				}
				if (!elementoAQuitar.Equals(default(ElementoXsdOrdenado)))
					if (elementos.ContainsKey(elementoAQuitar))
						elementos.Remove(elementoAQuitar);
			}
			
		}
		public bool Añadir(AtributoXsd atributo, bool estaDeclarado)
		{
			bool añadido = false;
			string atributoString = "";
			if (estaDeclarado)
				atributoString = atributo.UsoConReferencia();
			else
				atributoString = atributo.UsoCompleto();
			if (!atributos.ContainsKey(atributo)) {
				atributos.Add(atributo, atributoString);
				añadido = true;
			}
			return añadido;
			
		}
		public bool Quitar(AtributoXsd atributo)
		{
			bool sePuedeQuitar = atributos.ContainsKey(atributo);
			if (sePuedeQuitar)
				atributos.Remove(atributo);
			return sePuedeQuitar;
		}
		#region IClonable implementation

		public dynamic Clon()
		{
			TipoComplejoXsd clon = new TipoComplejoXsd(this.nombre);
			foreach (var elemento in elementos)
				clon.elementos.Add(elemento.Key, elemento.Value);
			//poner toda la config
			clon.IsEmpty = IsEmpty;
			clon.IsExtensibleAttribute = IsExtensibleAttribute;
			clon.IsExtensibleElement = IsExtensibleElement;
			clon.OrdenElementos = OrdenElementos;
			clon.SimpleContent = SimpleContent != null ? SimpleContent.Clon() : null;
			clon.TipoXsdBase = TipoXsdBase;
			return clon;
		}

		#endregion
		public override string ToString()
		{

			text elementoComplejo = "";
			text simpleContent="";
			elementoComplejo = "<xs:complexType";
			if (Nombre != null)
				elementoComplejo += " name=\"" + nombre + "\"";
			elementoComplejo += ">";
			if (IsTextOnly) {
				elementoComplejo+="xs:simpleContent>";
				simpleContent = SimpleContent.ToString();
				simpleContent.Remove("</xs:restriccion>");
				elementoComplejo += simpleContent;//con sus restricciones
				
			} else if (!IsEmpty) {
				
				elementoComplejo = "<xs:complexType";
				if (Nombre != null)
					elementoComplejo += " name=\"" + nombre + "\"";
				elementoComplejo += " mixed=\"" + mixed.ToString().ToLower() + "\"><xs:complexContent>";
				if (tipoXsdBase != null)
					elementoComplejo += "<xs:extension base=\"" + tipoXsdBase + "\">";
				elementoComplejo += "<" + orden.ToString().ToLower() + ">";
				foreach (var elemento in elementos)
					elementoComplejo += elemento.Value;
				elementoComplejo += "</" + orden.ToString().ToLower() + ">";
				if (IsExtensibleElement)
					elementoComplejo += "<xs:any minOccurs=\"0\"/>";
			}
			foreach (var atributo in atributos)
				elementoComplejo += atributo.Value;
			if (tipoXsdBase != null && !IsEmpty && !IsTextOnly)
				elementoComplejo += "</xs:extension>";
			if (IsExtensibleAttribute)
				elementoComplejo += "<xs:anyAttribute/>";
			if (!IsEmpty && !IsTextOnly)
				elementoComplejo += "</xs:complexContent>";
			if (IsTextOnly){
				elementoComplejo += "</xs:restriccion>";
				elementoComplejo+="</xs:simpleContent>";
			}
			elementoComplejo += "</xs:complexType>";
			
			return elementoComplejo;
			
			
		}
		#region Equals and GetHashCode implementation
		public override bool Equals(object obj)
		{
			TipoComplejoXsd other = obj as TipoComplejoXsd;
			if (other == null)
				return false;
			return object.Equals(this.elementos, other.elementos) && object.Equals(this.atributos, other.atributos) && this.nombre == other.nombre && object.Equals(this.tipoXsdBase, other.tipoXsdBase) && this.orden == other.orden && this.mixed == other.mixed && this.numerosOrden == other.numerosOrden && this.isEmpty == other.isEmpty && object.Equals(this.simpleContent, other.simpleContent) && this.isExtensibleAttribute == other.isExtensibleAttribute && this.isExtensibleElements == other.isExtensibleElements;
		}

		public override int GetHashCode()
		{
			int hashCode = 0;
			unchecked {
				if (elementos != null)
					hashCode += 1000000007 * elementos.GetHashCode();
				if (atributos != null)
					hashCode += 1000000009 * atributos.GetHashCode();
				if (nombre != null)
					hashCode += 1000000021 * nombre.GetHashCode();
				if (tipoXsdBase != null)
					hashCode += 1000000033 * tipoXsdBase.GetHashCode();
				hashCode += 1000000087 * orden.GetHashCode();
				hashCode += 1000000093 * mixed.GetHashCode();
				hashCode += 1000000097 * numerosOrden.GetHashCode();
				hashCode += 1000000103 * isEmpty.GetHashCode();
				if (simpleContent != null)
					hashCode += 1000000123 * simpleContent.GetHashCode();
				hashCode += 1000000181 * isExtensibleAttribute.GetHashCode();
				hashCode += 1000000207 * isExtensibleElements.GetHashCode();
			}
			return hashCode;
		}

		public static bool operator ==(TipoComplejoXsd lhs, TipoComplejoXsd rhs) {
			if (ReferenceEquals(lhs, rhs))
				return true;
			if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
				return false;
			return lhs.Equals(rhs);
		}

		public static bool operator !=(TipoComplejoXsd lhs, TipoComplejoXsd rhs) {
			return !(lhs == rhs);
		}

		#endregion
		public int CompareTo(TipoComplejoXsd other)
		{
			return ToString().CompareTo(other.ToString());
		}
	}

}
