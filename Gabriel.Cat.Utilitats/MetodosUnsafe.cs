/*
 * Creado por SharpDevelop.
 * Usuario: Pikachu240
 * Fecha: 18/10/2017
 * Hora: 18:55
 * Licencia GNU GPL V3
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of MetodosUnsafe.
	/// </summary>
	public static class MetodosUnsafe
	{
		#region Pointers
		public unsafe static byte[] ReadBytes( byte* ptrBytes,int lenght)
		{
			byte[] array=new byte[lenght];
			byte* ptrArray;
			fixed(byte* ptArray=array)
			{
				ptrArray=ptArray;
				for(int i=0;i<lenght;i++)
				{
					*ptrArray=*ptrBytes;
					ptrArray++;
					ptrBytes++;
				}
			}
			return array;
		}
		public unsafe static void WriteBytes( byte* ptrBytesLeft,byte* ptrBytesRight,int lenght)
		{
			
			for(int i=0;i<lenght;i++)
			{
				*ptrBytesLeft=*ptrBytesRight;
				ptrBytesLeft++;
				ptrBytesRight++;
			}

		}
		#endregion
	}
}
