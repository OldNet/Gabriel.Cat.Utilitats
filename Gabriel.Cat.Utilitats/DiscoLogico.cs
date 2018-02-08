/*
 * Creado por SharpDevelop.
 * Usuario: pc
 * Fecha: 04/02/2018
 * Hora: 12:34
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.IO;
using Gabriel.Cat.Extension;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of DiscoLogico.
	/// </summary>
	public static class DiscoLogico// de momento lo pongo static más adelante ya hare algo :)
	{
		public static List<FileInfo> GetFiles(params string[] discos)
		{
			if(discos.Length==0)
				discos=Directory.GetLogicalDrives();
			
			List<FileInfo> files=new List<FileInfo>();
			
			for(int i=0;i<discos.Length;i++)
			{
				if(discos[i]!=null&&Directory.Exists(discos[i]))
					files.AddRange(new DirectoryInfo(discos[i]).GetAllFiles());
			}
			return files;
		}
	}
}
