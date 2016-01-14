/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 11/03/2015
 * Hora: 18:02
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Reflection;
using System.Resources;
namespace Gabriel.Cat.Extension
{
	/// <summary>
	/// Description of Recursos.
	/// </summary>
	public static class Recursos
	{
		public static byte[] Obtener(string nameSpaceArchivo,string nombre)
		{
			ResourceManager resources = new ResourceManager(nameSpaceArchivo, Assembly.GetExecutingAssembly());
			byte[] fileData = (byte[]) resources.GetObject(nombre);
			return fileData;

		}
	}
}
