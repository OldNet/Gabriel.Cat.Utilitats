using Gabriel.Cat.Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Gabriel.Cat
{
	/// <summary>
	/// Sirve para convertir a byte[] los objetos compatibles y de byte[] al objeto en cuestion
	/// </summary>
	public class Serializar
	{
		#region AssemblyNames
		//AssemblyName primero poner la string como constante
		public const string STRINGASSEMBLYNAME = "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		public const string INTASSEMBLYNAME = "System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		public const string SHORTASSEMBLYNAME = "System.Int16, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		public const string LONGASSEMBLYNAME = "System.Int64, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		public const string USHORTASSEMBLYNAME = "System.UInt16, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		public const string ULONGASSEMBLYNAME = "System.UInt64, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		public const string DOUBLEASSEMBLYNAME = "System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		public const string CHARASSEMBLYNAME = "System.Char, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		public const string FLOATASSEMBLYNAME = "System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		public const string DATETIMEASSEMBLYNAME = "System.DateTime, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		public const string BYTEARRAYASSEMBLYNAME = "System.Byte[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		public const string BYTEASSEMBLYNAME = "System.Byte, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		public const string BOOLEANASSEMBLYNAME = "System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		public const string BITMAPASSEMBLYNAME = "System.Drawing.Bitmap, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
		public const string UINTASSEMBLYNAME = "System.UInt32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		public const string POINTASSEMBLYNAME = "System.Drawing.Point, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
		public const string COLORASSEMBLYNAME = "System.Drawing.Color, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
//por mirar
		//mis clases
		public const string POINTZASSEMBLYNAME = "Gabriel.Cat.PointZ, Gabriel.Cat.Utilitats, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
//mientras no le cambie esta info sera valida...
		//se añade la constante en la lista :D
		public static readonly string[] AsseblyQualifiedNameTiposMicrosoft = new String[] {
			STRINGASSEMBLYNAME,
			INTASSEMBLYNAME,
			SHORTASSEMBLYNAME,
			LONGASSEMBLYNAME,
			USHORTASSEMBLYNAME,
			ULONGASSEMBLYNAME,
			DOUBLEASSEMBLYNAME,
			CHARASSEMBLYNAME,
			FLOATASSEMBLYNAME,
			DATETIMEASSEMBLYNAME,
			BYTEARRAYASSEMBLYNAME,
			BOOLEANASSEMBLYNAME,
			BITMAPASSEMBLYNAME,
			UINTASSEMBLYNAME,
			BYTEASSEMBLYNAME,
			POINTASSEMBLYNAME,
			POINTZASSEMBLYNAME,
			COLORASSEMBLYNAME
		};
		#endregion
		//Si se añaden mas se tiene que dar de alta aqui :D y en el metodo ToTipoAceptado y GetBytes(TipoAceptado,Object)
		public enum TiposAceptados
		{
			Null,
			Byte,
			Bool,
			Short,
			UShort,
			Int,
			UInt,
			Long,
			ULong,
			Double,
			Float,
			Char,
			DateTime,
			String,
			Bitmap,
			Point,
			PointZ,
			Color
		}




		//la clase es para convertir a byte[] los objetos
		#region GetBytes

		public static byte[] GetBytes(IEnumerable<Object> objsTipoAceptado)
		{
			List<byte> bytesList = new List<byte>();
			foreach (Object obj in objsTipoAceptado) {
				bytesList.AddRange(GetBytes(obj));
			}
			return bytesList.ToArray();
		}
		public static byte[] GetBytes(Object objTipoAceptado)
		{
			TiposAceptados tipo = GetType(objTipoAceptado);
			byte[] bytes = new byte[] { };
			try {
				switch (tipo) {
					case TiposAceptados.Point:
						bytes = GetBytes((Point)objTipoAceptado);
						break;
					case TiposAceptados.PointZ:
						bytes = GetBytes((PointZ)objTipoAceptado);
						break;
					case TiposAceptados.Bool:
						bytes = GetBytes((bool)objTipoAceptado);
						break;
					case TiposAceptados.Short:
						bytes = GetBytes((short)objTipoAceptado);
						break;
					case TiposAceptados.UShort:
						bytes = GetBytes((ushort)objTipoAceptado);
						break;
					case TiposAceptados.Int:
						bytes = GetBytes((int)objTipoAceptado);
						break;
					case TiposAceptados.UInt:
						bytes = GetBytes((uint)objTipoAceptado);
						break;
					case TiposAceptados.Long:
						bytes = GetBytes((long)objTipoAceptado);
						break;
					case TiposAceptados.ULong:
						bytes = GetBytes((ulong)objTipoAceptado);
						break;
					case TiposAceptados.Double:
						bytes = GetBytes((double)objTipoAceptado);
						break;
					case TiposAceptados.Float:
						bytes = GetBytes((float)objTipoAceptado);
						break;
					case TiposAceptados.Char:
						bytes = GetBytes((char)objTipoAceptado);
						break;
					case TiposAceptados.String:
						bytes = GetBytes((string)objTipoAceptado);
						break;
					case TiposAceptados.Bitmap:
						bytes = GetBytes((Bitmap)objTipoAceptado);
						break;
					case TiposAceptados.DateTime:
						bytes = GetBytes((DateTime)objTipoAceptado);
						break;
					case TiposAceptados.Byte:
						bytes = GetBytes((byte)objTipoAceptado);
						break;
					case TiposAceptados.Null:
						bytes = new byte[] { 0x00 };
						break;
					case TiposAceptados.Color:
						bytes = GetBytes((Color)objTipoAceptado);
						break;
				}
			} catch {
				throw new Exception("El objeto no es del tipo indicado como parametro");
			}
			return bytes;
		}
		/// <summary>
		/// Devuelve el tipo del objeto...si no es un tipoAceptado devuelve TipoAceptado.Null
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static TiposAceptados GetType(object obj)
		{
			return obj != null ? AssemblyToEnumTipoAceptado(obj.GetType().AssemblyQualifiedName) : TiposAceptados.Null;
		}
		public static TiposAceptados AssemblyToEnumTipoAceptado(string assemblyName)
		{
			TiposAceptados tipo = TiposAceptados.Null;
			switch (assemblyName) {
			//todos los tiposAceptados :)
				case Serializar.BITMAPASSEMBLYNAME:
					tipo = TiposAceptados.Bitmap;
					break;
				case Serializar.BOOLEANASSEMBLYNAME:
					tipo = TiposAceptados.Bool;
					break;
				case Serializar.BYTEASSEMBLYNAME:
					tipo = TiposAceptados.Byte;
					break;
				case Serializar.CHARASSEMBLYNAME:
					tipo = TiposAceptados.Char;
					break;
				case Serializar.DATETIMEASSEMBLYNAME:
					tipo = TiposAceptados.DateTime;
					break;
				case Serializar.DOUBLEASSEMBLYNAME:
					tipo = TiposAceptados.Double;
					break;
				case Serializar.FLOATASSEMBLYNAME:
					tipo = TiposAceptados.Float;
					break;
				case Serializar.INTASSEMBLYNAME:
					tipo = TiposAceptados.Int;
					break;
				case Serializar.LONGASSEMBLYNAME:
					tipo = TiposAceptados.Long;
					break;
				case Serializar.SHORTASSEMBLYNAME:
					tipo = TiposAceptados.Short;
					break;
				case Serializar.STRINGASSEMBLYNAME:
					tipo = TiposAceptados.String;
					break;
				case Serializar.UINTASSEMBLYNAME:
					tipo = TiposAceptados.UInt;
					break;
				case Serializar.ULONGASSEMBLYNAME:
					tipo = TiposAceptados.ULong;
					break;
				case Serializar.USHORTASSEMBLYNAME:
					tipo = TiposAceptados.UShort;
					break;
				case Serializar.POINTASSEMBLYNAME:
					tipo = TiposAceptados.Point;
					break;
				case Serializar.POINTZASSEMBLYNAME:
					tipo = TiposAceptados.PointZ;
					break;
				case Serializar.COLORASSEMBLYNAME:
					tipo = TiposAceptados.Color;
					break;
			}
			return tipo;
		}
		public static byte[] GetBytes(Color color)
		{
			return new byte[] { color.A, color.R, color.G, color.B };
		}
		public static byte[] GetBytes(Point point)
		{
			return GetBytes(new PointZ(point.X, point.Y, 0)).SubArray(0, 8);
		}
		public static byte[] GetBytes(PointZ point)
		{
			List<byte> bytesPoint = new List<byte>();
			bytesPoint.AddRange(GetBytes(point.X));
			bytesPoint.AddRange(GetBytes(point.Y));
			bytesPoint.AddRange(GetBytes(point.Z));
			return bytesPoint.ToArray();

		}
		//mirar de poder serializar null
		public static byte[] GetBytes(Bitmap img)
		{
			return GetBytes(img, img.RawFormat);
		}
		public static byte[] GetBytes(Bitmap img, System.Drawing.Imaging.ImageFormat formato)
		{
			return img.ToStream(formato).GetAllBytes();
		}

		public	static byte[] GetBytes(string str)
		{//sacado de http://stackoverflow.com/questions/16072709/converting-string-to-byte-array-in-c-sharp
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}


		public static byte[] GetBytes(char caracter)
		{
			return GetBytes(caracter + "");
		}
		public static byte[] GetBytes(DateTime data)
		{
			return GetBytes(data.Ticks);
		}
		public static byte[] GetBytes(int numero)
		{
			return BitConverter.GetBytes(numero);
		}
		public static byte[] GetBytes(ulong numero)
		{
			return BitConverter.GetBytes(numero);
		}
		public static byte[] GetBytes(long numero)
		{
			return BitConverter.GetBytes(numero);
		}
		public static byte[] GetBytes(uint numero)
		{
			return BitConverter.GetBytes(numero);
		}
		public static byte[] GetBytes(short numero)
		{
			return BitConverter.GetBytes(numero);
		}
		public static byte[] GetBytes(double numero)
		{
			return BitConverter.GetBytes(numero);
		}
		public static byte[] GetBytes(float numero)
		{
			return BitConverter.GetBytes(numero);
		}
		public static byte[] GetBytes(ushort numero)
		{
			return BitConverter.GetBytes(numero);
		}
		public static byte[] GetBytes(bool bolean)
		{
			return BitConverter.GetBytes(bolean);
		}
		public static byte[] GetBytes(byte byteToArray)
		{
			return new byte[] { byteToArray };
		}


		#endregion
		#region To
		public static object ToObjetoAceptado(TiposAceptados objHaLeer, Stream ms)
		{
			object obj = null;
			switch (objHaLeer) {
				case TiposAceptados.Null:
					break;
				case TiposAceptados.Byte:
					obj = ms.ReadByte();
					break;
				case TiposAceptados.Bool:
					obj = ToBoolean(ms.Read(sizeof(bool)));
					break;
				case TiposAceptados.Short:
					obj = ToShort(ms.Read(sizeof(short)));
					break;
				case TiposAceptados.UShort:
					obj = ToUShort(ms.Read(sizeof(ushort)));
					break;
				case TiposAceptados.Int:
					obj = ToInt(ms.Read(sizeof(int)));
					break;
				case TiposAceptados.UInt:
					obj = ToUInt(ms.Read(sizeof(uint)));
					break;
				case TiposAceptados.Long:
					obj = ToLong(ms.Read(sizeof(long)));
					break;
				case TiposAceptados.ULong:
					obj = ToULong(ms.Read(sizeof(ulong)));
					break;
				case TiposAceptados.Double:
					obj = ToDouble(ms.Read(sizeof(double)));
					break;
				case TiposAceptados.Float:
					obj = ToFloat(ms.Read(sizeof(float)));
					break;
				case TiposAceptados.Char:
					obj = ToChar(ms.Read(sizeof(char)));
					break;
				case TiposAceptados.DateTime:
					obj = ToDateTime(ms.Read(sizeof(long)));
					break;
				case TiposAceptados.Point:
					obj = ToPoint(ms.Read(sizeof(int)*2));
					break;
				case TiposAceptados.PointZ:
					obj = ToPointZ(ms.Read(sizeof(int) * 3));
					break;
				case TiposAceptados.String:
					obj = ToString(ms.Read(ToLong(ms.Read(sizeof(long)))));
					break;
				case TiposAceptados.Bitmap:
					obj = ToBitmap(ms.Read(ToLong(ms.Read(sizeof(long)))));
					break;
				case TiposAceptados.Color:
					obj = ToColor(ms.Read(sizeof(int)));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			return obj;
		}

		public static T ToObjetoAceptadoTipado<T>(TiposAceptados objHaLeer, Stream ms)
		{
			return (T)ToObjetoAceptado(objHaLeer, ms);
		}
		public static Object ToTipoAceptado(string assemblyQualifiedName, byte[] bytesTipo)
		{
			return ToTipoAceptado(Serializar.AssemblyToEnumTipoAceptado(assemblyQualifiedName), bytesTipo);
		}
            
		/// <summary>
		/// Convierte a obj los bytes al tipo especificado
		/// </summary>
		/// <param name="tipo"></param>
		/// <param name="bytesTipo"></param>
		/// <returns>si no se puede devuelve null</returns>
		public static Object ToTipoAceptado(TiposAceptados tipo, byte[] bytesTipo)
		{
			Object obj = null;
			Stream str = new MemoryStream(bytesTipo);
			obj = ToObjetoAceptado(tipo, str);
			str.Close();
			return obj;
		}
		public static T ToTipoAceptadoTipado<T>(TiposAceptados tipo, byte[] bytesTipo)
		{
			return (T)ToTipoAceptado(tipo, bytesTipo);
		}
		#region Desserializar Medida Fija
		public static Color ToColor(byte[] bytesObj)
		{
			if (bytesObj.Length != 4)
				throw new ArgumentException("Un color consta de 4 bytes ARGB");
			return Color.FromArgb(bytesObj[0], bytesObj[1], bytesObj[2], bytesObj[3]);
		}
		public static PointZ ToPointZ(byte[] bytesObj)
		{
			return new PointZ(ToPoint(bytesObj), ToInt(bytesObj.SubArray(8)));
		}

		public static Point ToPoint(byte[] bytesObj)
		{
			return new Point(ToInt(bytesObj), ToInt(bytesObj.SubArray(4)));
		}
		public static bool ToBoolean(byte[] boolean)
		{
			return BitConverter.ToBoolean(boolean, 0);
		}
		public static int ToInt(byte[] numero)
		{
			return BitConverter.ToInt32(numero, 0);
		}
		public static ulong ToULong(byte[] numero)
		{
			return BitConverter.ToUInt64(numero, 0);
		}
		public static long ToLong(byte[] numero)
		{
			return BitConverter.ToInt64(numero, 0);
		}
		public static uint ToUInt(byte[] numero)
		{
			return BitConverter.ToUInt32(numero, 0);
		}
		public static short ToShort(byte[] numero)
		{
			return BitConverter.ToInt16(numero, 0);
		}
		public static double ToDouble(byte[] numero)
		{
			return BitConverter.ToDouble(numero, 0);
		}
		public static float ToFloat(byte[] numero)
		{
			return BitConverter.ToSingle(numero, 0);
		}
		public static ushort ToUShort(byte[] numero)
		{
			return BitConverter.ToUInt16(numero, 0);
		}
		public static DateTime ToDateTime(byte[] data)
		{
			return new DateTime(ToLong(data));
		}
		public static char ToChar(byte[] caracter)
		{
			return ToString(caracter)[0];
		}
		#endregion
		#region Deserializar medias dynamicas
		/// <summary>
		/// int,int,int,byte[](los bytes que queden)[datos img]
		/// </summary>
		/// <param name="bytesImgSerializada"></param>
		/// <returns></returns>
		public static Bitmap ToBitmap(byte[] bytesImgSerializada)
		{
			Bitmap imgDeserializada = null;
			try {
				imgDeserializada = new Bitmap(new MemoryStream(bytesImgSerializada));
			} catch {
			}
			return imgDeserializada;
			
		}


		public static string ToString(byte[] bytes)
		{//sacado de http://stackoverflow.com/questions/16072709/converting-string-to-byte-array-in-c-sharp
			const int sizeChar=sizeof(char);
			char[] chars = new char[bytes.Length / sizeChar];
			System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			return new string(chars);
		}
		#endregion

		#endregion


	}
}
