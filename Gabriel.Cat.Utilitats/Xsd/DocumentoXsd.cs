/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 05/03/2015
 * Hora: 16:42
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
namespace Gabriel.Cat
{
	/// <summary>
	/// Description of DocumentoXsd.
	/// </summary>
	public class DocumentoXsd
	{
		readonly	text CABEZERAXSD = "<?xml version=\"1.0\" encoding=\"utf-8\"?><xs:schema targetNamespace=\"http://tempuri.org/XMLSchema.xsd\"  elementFormDefault=\"qualified\"  xmlns=\"http://tempuri.org/XMLSchema.xsd\" xmlns:mstns=\"http://tempuri.org/XMLSchema.xsd\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">";

		//controlar que los elementos que se declaran tengan nombre...
		SortedList< TipoComplejoXsd, string> declaracionTiposComplejos;
		SortedList< TipoSimpleXsd, string> declaracionTiposSimples;
		SortedList< AtributoXsd, string> declaracionAtributos;
		SortedList< ElementoXsd, string> declaracionElementos;
		public DocumentoXsd()
		{
			declaracionAtributos = new SortedList<AtributoXsd, string>();
			declaracionElementos = new SortedList<ElementoXsd, string>();
			declaracionTiposComplejos = new SortedList<TipoComplejoXsd, string>();
			declaracionTiposSimples = new SortedList<TipoSimpleXsd, string>();
		}
		public bool[] Añadir(IEnumerable<TipoComplejoXsd> lista)
		{
			List<bool> resultadoAñadir=new List<bool>();
			foreach(var elementoLista in lista)
				resultadoAñadir.Add(Añadir(elementoLista));
			return resultadoAñadir.ToArray();
		}
		public bool Añadir(TipoComplejoXsd tipo)
		{
			if (tipo.Nombre == null)
				throw new XsdException("El tipo complejo tiene que tener nombre o estar dentro de la declaracion de un elemento");
			bool añadir = true;
			if (!declaracionTiposComplejos.ContainsKey(tipo))
				declaracionTiposComplejos.Add(tipo, null);
			else
				añadir = false;
			return añadir;
			
		}
		public bool[] Añadir(IEnumerable<TipoSimpleXsd> lista)
		{
			List<bool> resultadoAñadir=new List<bool>();
			foreach(var elementoLista in lista)
				resultadoAñadir.Add(Añadir(elementoLista));
			return resultadoAñadir.ToArray();
		}
		public bool Añadir(TipoSimpleXsd tipo)
		{
			if (tipo.Nombre == null)
				throw new  XsdException("El tipo simple tiene que tener nombre o estar dentro de la declaracion de otro elemento");
			bool añadir = true;
			if (!declaracionTiposSimples.ContainsKey(tipo))
				declaracionTiposSimples.Add(tipo, null);
			else
				añadir = false;
			return añadir;
			
		}
		public bool[] Añadir(IEnumerable<AtributoXsd> lista)
		{
			List<bool> resultadoAñadir=new List<bool>();
			foreach(var elementoLista in lista)
				resultadoAñadir.Add(Añadir(elementoLista));
			return resultadoAñadir.ToArray();
		}
		public bool Añadir(AtributoXsd tipo)
		{
			bool añadir = true;
			if (!declaracionAtributos.ContainsKey(tipo))
				declaracionAtributos.Add(tipo, null);
			else
				añadir = false;
			return añadir;
			
		}
		public bool[] Añadir(IEnumerable<ElementoXsd> lista)
		{
			List<bool> resultadoAñadir=new List<bool>();
			foreach(var elementoLista in lista)
				resultadoAñadir.Add(Añadir(elementoLista));
			return resultadoAñadir.ToArray();
		}
		public bool Añadir(ElementoXsd tipo)
		{
			if (tipo.Nombre == null)
				throw new XsdException("El elemento tiene que tener un nombre");
			bool añadir = true;
			if (!declaracionElementos.ContainsKey(tipo))
				declaracionElementos.Add(tipo, null);
			else
				añadir = false;
			return añadir;
			
		}
		public bool Quitar(TipoComplejoXsd tipo)
		{
			bool quitado = true;
			if (declaracionTiposComplejos.ContainsKey(tipo))
				declaracionTiposComplejos.Remove(tipo);
			else
				quitado = false;
			return quitado;
			
		}
		public bool Quitar(TipoSimpleXsd tipo)
		{
			bool quitado = true;
			if (declaracionTiposSimples.ContainsKey(tipo))
				declaracionTiposSimples.Remove(tipo);
			else
				quitado = false;
			return quitado;
			
		}
		public bool Quitar(AtributoXsd tipo)
		{
			bool quitado = true;
			if (declaracionAtributos.ContainsKey(tipo))
				declaracionAtributos.Remove(tipo);
			else
				quitado = false;
			return quitado;
			
		}
		public bool Quitar(ElementoXsd tipo)
		{
			bool quitado = true;
			if (declaracionElementos.ContainsKey(tipo))
				declaracionElementos.Remove(tipo);
			else
				quitado = false;
			return quitado;
			
		}
		public void Reemplazar(TipoComplejoXsd tipo)
		{
			Quitar(tipo);
			Añadir(tipo);
		}
		public void Reemplazar(TipoSimpleXsd tipo)
		{
			Quitar(tipo);
			Añadir(tipo);
		}
		public void Reemplazar(AtributoXsd tipo)
		{
			Quitar(tipo);
			Añadir(tipo);
		}
		public void Reemplazar(ElementoXsd tipo)
		{
			Quitar(tipo);
			Añadir(tipo);
		}
		public void Guardar()
		{
			Guardar(Directory.GetCurrentDirectory());
		}
		public void Guardar(string ruta)
		{
			int numero = 0;
			while (File.Exists(ruta + "//validador(" + numero + ").xsd"))
				numero++;
			Guardar(ruta, "validador(" + numero + ")");
		}
		public void Guardar(string ruta, string nombreArchivo)
		{
			if (File.Exists(ruta + "//" + nombreArchivo + ".xsd"))
				File.Delete(ruta + "//" + nombreArchivo + ".xsd");
			StreamWriter archivo = File.CreateText(ruta + "//" + nombreArchivo + ".xsd");
			archivo.Write(ToString());
			archivo.Close();
		}
		public void Cargar(string path)
		{
			if(File.Exists(path))
			{
				text documento=File.ReadAllText(path);
				XmlDocument xml=new XmlDocument();
				documento.Remove(CABEZERAXSD);
				documento.Remove("xs:");
				xml.LoadXml(documento);
				foreach(XmlNode elemento in xml.GetElementsByTagName("element"))
				{
					Añadir(new ElementoXsd(elemento));
				}
				foreach(XmlNode tipoSimple in xml.GetElementsByTagName("simpleType"))
				{
					Añadir(new TipoSimpleXsd(tipoSimple));
				}
				foreach(XmlNode atributo in xml.GetElementsByTagName("attribute"))
				{
					Añadir(new AtributoXsd(atributo));
				}
				foreach(XmlNode tipoComplejo in xml.GetElementsByTagName("complexType"))
				{
					Añadir(new TipoComplejoXsd(tipoComplejo));
				}
			}
		}
		public override string ToString()
		{
			text documentoXsd = CABEZERAXSD;
			foreach (var elemento in declaracionElementos)
				documentoXsd += elemento.Key.Declaracion();
			foreach (var tipoSimple in declaracionTiposSimples)
				documentoXsd += tipoSimple.Key.ToString();
			foreach (var atributo in declaracionAtributos)
				documentoXsd += atributo.Key.Declaracion();
			foreach (var tipoComplejo in declaracionTiposComplejos)
				documentoXsd += tipoComplejo.Key.ToString();
			documentoXsd += "</xs:schema>";
			return documentoXsd;
		}
		public static bool ValidarXmlConXsd(string xmlUri, string xsdUri)
		{
			bool valido = false;
			try {
				XmlReaderSettings xmlSettings = new XmlReaderSettings();
				xmlSettings.Schemas = new System.Xml.Schema.XmlSchemaSet();
				xmlSettings.Schemas.Add(null, xsdUri);
				xmlSettings.ValidationType = ValidationType.Schema;
				using (XmlReader reader = XmlReader.Create(xmlUri, xmlSettings)) {
					while (reader.Read())		// Parse the file.
					{
					}
				}

				

				valido = true;
			} catch {
			}
			return valido;
		}

		
	}
	
}
