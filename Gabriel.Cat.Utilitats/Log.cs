/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 15/04/2016
 * Hora: 16:53
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;

namespace Gabriel.Cat.Utilitats
{
	public delegate void LogEventHandler(string message,int idMessage);
	/// <summary>
	/// Description of Log.
	/// </summary>
	public static  class Log
	{
		public static event LogEventHandler Listen;
		
		public static void Send(string message,int idMessage)
		{
			if(Listen!=null)
				Listen(message,idMessage);
		}
		
	}
}
