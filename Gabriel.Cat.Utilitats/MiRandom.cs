/*
 * Creado por SharpDevelop.
 * Usuario: pc
 * Fecha: 24/04/2015
 * Hora: 17:08
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of MiRandom.
	/// </summary>
	public static class MiRandom
	{
		static Random llavor=new Random();
		public static int Next(int minValueInclude,int maxValueExclude)
		{
			return llavor.Next(minValueInclude,maxValueExclude);
		}
		public static int Next()
		{
			return llavor.Next();
		}
		public static int Next(int maxValue)
		{
			return llavor.Next(maxValue);
		}
	}
}
