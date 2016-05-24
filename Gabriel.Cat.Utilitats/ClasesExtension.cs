/*
 * Creado por SharpDevelop.
 * Usuario: pc
 * Fecha: 15/04/2015
 * Hora: 17:34
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Drawing;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Reflection;
using System.Drawing.Imaging;
using System.Collections;
using System.Runtime.ExceptionServices;

namespace Gabriel.Cat.Extension
{
	public delegate bool MetodoWhileEach<Tvalue>(Tvalue valor);
	public delegate void MetodoTratarByteArray(byte[] byteArray);
	public unsafe delegate void MetodoTratarBytePointer(byte* prtByteArray);
	public delegate bool ComprovaEventHanler<Tvalue>(Tvalue valorAComprovar);
	public delegate T TrataObjeto<T>(T objHaTratar);
	public delegate ContinuaTratando<T> ContinuaTratandoObjeto<T>(T objHaTratar);
	public struct ContinuaTratando<T>
	{
		public bool Continua;
		public T Objeto;
	}

	public enum DimensionMatriz
	{
		Columna = 0,
		Fila = 1,
		Fondo = 2,
		X = Columna,
		Y = Fila,
		Z = Fondo

	}
	public enum Orden
	{
		QuickSort,
		Bubble
	}
	public enum CompareTo
	{
		Iguales = 0,
		Inferior = -1,
		Superior = 1
	}
	public enum Ordre
	{
		Consecutiu,
		ConsecutiuIAlInreves
	}

	#region Comparar
	public delegate int ComparadorEventHandler<T>(T x, T y);
	public class Comparador<T> : Comparer<T>
	{
		ComparadorEventHandler<T> comparador;
		public Comparador(ComparadorEventHandler<T> comparador)
		{
			this.comparador = comparador;
		}

		public override int Compare(T x, T y)
		{
			return comparador(x, y);
		}
	}
	#endregion
	#region Serializar
	public struct Propiedad
	{
		private string nombre;
		private object objeto;
		public Propiedad(string nombre, object obj)
		{
			this.nombre = nombre;
			this.objeto = obj;
		}
		public Type Tipo
		{
			get { return objeto.GetType(); }
		}

		public string Nombre
		{
			get
			{
				return nombre;
			}


		}

		public object Objeto
		{
			get
			{
				return objeto;
			}

		}
	}
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class SerialitzeIgnoreAttribute : Attribute, IComparable<SerialitzeIgnoreAttribute>
	{
		public int CompareTo(SerialitzeIgnoreAttribute other)
		{
			return 0;
		}
	}
	#endregion
	public static class Extension
	{
		#region Extesion Vector


		/// <summary>
		/// Devuelve un vector usando el angulo para orientarlo
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="matriz"></param>
		/// <param name="angulo"></param>
		/// <param name="xInicio"></param>
		/// <param name="yInicio"></param>
		/// <param name="longitud">si es mas grande solo llegará hasta el final,si es negativo es hasta llegar al final</param>
		/// <returns></returns>
		public static Vector GetVector<T>(this T[,] matriz, int angulo = 45, int xInicio = 0, int yInicio = 0, int longitud = -1)
		{
			int width = matriz.GetLength(DimensionMatriz.X), height = matriz.GetLength(DimensionMatriz.Y);
			if (xInicio < 0 || yInicio < 0 || xInicio > width || yInicio > height)
				throw new ArgumentOutOfRangeException("Los puntos inicio tienen que ser validos, osea dentro de la matriz");
			Vector vector = new Vector();
			int alfa, ladoContiguo = 1;
			vector.InicioY = yInicio;
			vector.InicioX = xInicio;

			//si esta en una esquina tiene 90º sino 180º
			if (xInicio == 0 && yInicio == 0 || xInicio == width && yInicio == 0 || yInicio == height && xInicio == width || yInicio == height && xInicio == 0)
			{//si esta en una esquina :D
				alfa = angulo % 90;
				if (alfa > 45)
				{
					ladoContiguo = width;
				}
				else
				{
					ladoContiguo = height;
				}
			}
			else if (xInicio == 0 || yInicio == 0 || xInicio == width || yInicio == height)
			{//si  esta en un borde :D
				alfa = angulo % 180;
				if (alfa > 90)
				{
					if (yInicio == 0 || yInicio == height)
					{
						ladoContiguo = width - xInicio;
					}
					else
					{
						ladoContiguo = xInicio;
					}
				}
				else
				{
					if (xInicio == 0 || xInicio == height)
					{
						ladoContiguo = height - yInicio;
					}
					else
					{
						ladoContiguo = yInicio;
					}
				}
			}
			else
			{
				//esta dentro de la matriz
				alfa = angulo % 365;
				if (alfa >= 45 && alfa < 135)
					ladoContiguo = xInicio;
				else if (alfa >= 225 && alfa < 315)
				{
					ladoContiguo = width - xInicio;
				}
				else if (alfa >= 135 && alfa < 225)
				{
					ladoContiguo = height - yInicio;
				}
				else
				{
					ladoContiguo = yInicio;
				}

			}
			if (longitud < 0)
			{
				//uso mates :D
				vector.CalculaFin(alfa, ladoContiguo);
			}
			else
			{
				vector.CalculaFin(alfa, longitud * 1.0);
			}

			if (vector.FinY > 0)
				vector.FinY = vector.FinY > height ? height : vector.FinY;
			else vector.FinY = 0;
			if (vector.FinX > 0)
				vector.FinX = vector.FinX > width ? width : vector.FinX;
			else vector.FinX = 0;

			return vector;

		}
        /// <summary>
        /// Recorre la matriz usando el vector como guia y acaba cuando llega al final o captura una excepcion
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matriz"></param>
        /// <param name="vector"></param>
        /// <param name="metodoTratarObjeto"></param>
		public static void Recorrer<T>(this T[,] matriz, Vector vector, TrataObjeto<T> metodoTratarObjeto)
		{
			ContinuaTratandoObjeto<T> continua = (obj) =>
			{
				bool error = false;
				try
				{
					obj = metodoTratarObjeto(obj);
				}
				catch { error = true; }
				return new ContinuaTratando<T>() { Objeto = obj, Continua = !error };
			};
			matriz.Recorrer(vector, continua);
		}
        /// <summary>
        /// Recorre la matriz guiandose por el vector para cuando acaba o el valor de ContinuaTratando.Continua es false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matriz"></param>
        /// <param name="vector"></param>
        /// <param name="metodoTratarObjeto"></param>
		public static void Recorrer<T>(this T[,] matriz, Vector vector, ContinuaTratandoObjeto<T> metodoTratarObjeto)
		{
			if (vector == null || metodoTratarObjeto == null)
				throw new ArgumentNullException();
			try
			{
				matriz[vector.InicioX, vector.InicioY] = matriz[vector.InicioX, vector.InicioY];
				matriz[vector.FinX, vector.FinY] = matriz[vector.FinX, vector.FinY];
			}
			catch
			{
				throw new ArgumentOutOfRangeException("El vector esta fuera de la matriz");
			}

            Vector vectorAux = vector.ClonProfundoConPropiedades();
			
			ContinuaTratando<T> continuaObj = new ContinuaTratando<T>() { Continua = true };
			//miro las direcciones

			if (vectorAux.Sentido != Sentido.Centro)//si se mueve :D
			{
				do
				{

					//tratoElObj
					continuaObj = metodoTratarObjeto(matriz[vectorAux.InicioX, vectorAux.InicioY]);
                    matriz[vectorAux.InicioX, vectorAux.InicioY] = continuaObj.Objeto;
					//muevo
					//depende del sentido suma o resta a uno o a otro...
					switch (vectorAux.Sentido)
					{
							case Sentido.Arriba: vectorAux.InicioY--; break;
							case Sentido.Abajo: vectorAux.InicioY++; break;
							case Sentido.Derecha: vectorAux.InicioX++; break;
							case Sentido.Izquierda: vectorAux.InicioY--; break;
							//va en diagonal
						case Sentido.DiagonalDerechaAbajo:
							if (vectorAux.DiagonalPura)
							{
								vectorAux.InicioX++; vectorAux.InicioY++;
							}
							else
							{
								//depende del angulo
                                //menos45->X++
                                //45->diagonal
                                //mas45->Y++????????

							} break;
						case Sentido.DiagonalDerechaArriba:
                            if (vectorAux.DiagonalPura)
							{
								vectorAux.InicioX++; vectorAux.InicioY--;
							}
							else
							{
								//depende del angulo
							} break;
						case Sentido.DiagonalIzquierdaAbajo:
                            if (vectorAux.DiagonalPura)
							{
								vectorAux.InicioX--; vectorAux.InicioY++;
							}
							else
							{
								//depende del angulo
							} break;
						case Sentido.DiagonalIzquierdaArriba:
                            if (vectorAux.DiagonalPura)
							{
								vectorAux.InicioX--; vectorAux.InicioY--;
							}
							else
							{
								//depende del angulo
							} break;
					}
					if (continuaObj.Continua)
						continuaObj.Continua = vectorAux.InicioX != vector.FinX && vectorAux.InicioY != vector.FinY;

				} while (continuaObj.Continua);
                matriz[vectorAux.FinX, vectorAux.FinY] = metodoTratarObjeto(matriz[vectorAux.FinX,vectorAux.FinY]).Objeto;
			}

		
	}
        #endregion
        #region IComparable
        /// <summary>
        /// Mira en la lista IEnumerable si contiene exactamente todos los elementos de la otra lista
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="listToContain">lista a tener dentro de la otra</param>
        /// <returns></returns>
        public static bool Contains<T>(this IEnumerable<T> list,IEnumerable<T> listToContain) where T:IComparable
        {
            if (listToContain == null)
                throw new ArgumentNullException("La lista ha contener no puede ser null!!");
            bool contains = true;
            listToContain.WhileEach((elementToContain) =>
                {
                    contains = list.Contains(elementToContain);
                    return contains;
                });
            return contains;
        }
        #endregion
        #region IClauUnicaPerObjecte
        public static ListaUnica<T> ToListaUnica<T>(this IEnumerable<T> enumeracion) where T :IClauUnicaPerObjecte
	{
		ListaUnica<T> lista = new ListaUnica<T>();
		lista.Añadir(enumeracion);
		return lista;
	}
	#endregion
	#region Cosas necesarias pero de clase
	#region NormalizarXml
	//voy ha escapar caracteres no permitidos
	static readonly string[] caracteresXmlSustitutos = {
		"&lt;",
		"&gt;",
		"&amp;",
		"&quot;",
		"&apos;"
	};
	static readonly string[] caracteresXmlReservados = {
		"<",
		">",
		"&",
		"\"",
		"\'"
	};
	#endregion
	static LlistaOrdenada<string, string> caracteresReservadosXml;
	static Extension()
	{
		#region NormalizarXml
		caracteresReservadosXml = new LlistaOrdenada<string, string>();
		for (int i = 0; i < caracteresXmlReservados.Length; i++)
		{
			caracteresReservadosXml.Afegir(caracteresXmlReservados[i], caracteresXmlSustitutos[i]);
			caracteresReservadosXml.Afegir(caracteresXmlSustitutos[i], caracteresXmlReservados[i]);
		}
		#endregion
	}
	#endregion
	#region NormalizarXml

	private static string TratarCaracteresXML(string textoHaEscapar, string[] caracteresASustituir)
	{
		text texto = textoHaEscapar;
		for (int j = 0; j < caracteresASustituir.Length; j++)
			texto.Replace(caracteresASustituir[j], caracteresReservadosXml[caracteresASustituir[j]]);
		return texto;
	}
	public static string EscaparCaracteresXML(this string textoHaEscapar)
	{
		return TratarCaracteresXML(textoHaEscapar, caracteresXmlReservados);
	}
	public static string DescaparCaracteresXML(this string textoHaDesescapar)
	{
		return TratarCaracteresXML(textoHaDesescapar, caracteresXmlSustitutos);
	}
	#endregion
	#region ProcessInfo
	public static void Hide(this System.Diagnostics.ProcessStartInfo startInfo)
	{
		startInfo.RedirectStandardOutput = true;
		startInfo.UseShellExecute = false;
		startInfo.CreateNoWindow = true;
		startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;//hacer metodo que lo encapsule!! y el que lo muestre tambien
	}
	public static void Show(this System.Diagnostics.ProcessStartInfo startInfo)
	{
		startInfo.RedirectStandardOutput = false;
		startInfo.UseShellExecute = true;
		startInfo.CreateNoWindow = false;
		startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;//hacer metodo que lo encapsule!! y el que lo muestre tambien
	}
	public static string ExecuteBat(this System.Diagnostics.Process process, string batToExecute)
	{
		string path = Path.GetRandomFileName() + ".bat";
		StreamWriter swBat = File.CreateText(path);
		text informacion = "";
		StreamReader str;
		swBat.Write(batToExecute);
		swBat.Close();
		System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.FileName = path;
		startInfo.Hide();
		process.StartInfo = startInfo;
		process.Start();
		str = process.StandardOutput;
		while (!str.EndOfStream)
			informacion += (char)str.Read();
		str.Close();
		File.Delete(path);
		return informacion.ToString().Substring(informacion.IndexOf('>') + batToExecute.Length + 4);
	}
	#endregion
	#region Llista
	public static TValue Busca<TValue>(this Llista<TValue> llista, IComparable comparador) where TValue : IComparable
	{
		TValue valorEncontrado = default(TValue);
		bool encontrado = false;
		llista.WhileEach((valorAMirar) =>
		                 {
		                 	if (valorAMirar.CompareTo(comparador) == 0)
		                 	{
		                 		valorEncontrado = valorAMirar;
		                 		encontrado = true;
		                 	}
		                 	return !encontrado;
		                 });
		return valorEncontrado;
	}
	#endregion
	#region LlistaOrdenada
	public static void Afegir<TKey, TValue>(this LlistaOrdenada<TKey, TValue> llista, IClauUnicaPerObjecte value)
		where TValue : IClauUnicaPerObjecte
		where TKey : IComparable
	{
		llista.Afegir((TKey)value.Clau(), (TValue)value);
	}
	public static void AfegirORemplaçar<TKey, TValue>(this LlistaOrdenada<TKey, TValue> llista, IClauUnicaPerObjecte value)
		where TValue : IClauUnicaPerObjecte
		where TKey : IComparable
	{
		llista.AfegirORemplaçar((TKey)value.Clau(), (TValue)value);
	}
	#endregion
	#region List<T>Extension
	public static void RemoveRange<T>(this List<T> list, IEnumerable<T> elementsToRemove)
	{
		if (elementsToRemove != null)
			foreach (T element in elementsToRemove)
				list.Remove(element);
	}
	#endregion
	#region DirectoriInfoExtension
	public static KeyValuePair<DirectoryInfo, FileInfo[]>[] GetFiles(this DirectoryInfo dir, bool recursive)
	{//windows si da error no se puede ominit por lo tanto te quedas sin los archivos que puedes coger...es por eso que hago mi metodo...
		List<KeyValuePair<DirectoryInfo, FileInfo[]>> carpetasConSusArchivos = new List<KeyValuePair<DirectoryInfo, FileInfo[]>>();
		DirectoryInfo[] subDirs;
		bool canRead = dir.CanRead();
		if (canRead)
		{
			carpetasConSusArchivos.Add(new KeyValuePair<DirectoryInfo, FileInfo[]>(dir, dir.GetFiles()));

			if (recursive)
			{
				subDirs = dir.SubDirectoris();
				for (int i = 0; i < subDirs.Length; i++)
					if (subDirs[i].CanRead())
						carpetasConSusArchivos.Add(new KeyValuePair<DirectoryInfo, FileInfo[]>(subDirs[i], subDirs[i].GetFiles()));


			}
		}
		return carpetasConSusArchivos.ToTaula();
	}
	#region BuscaConHash
	public static FileInfo BuscaConHash(this DirectoryInfo dir, string fileHash)
	{
		return dir.BuscaConHash(fileHash, false);
	}
	public static FileInfo BuscaConHash(this DirectoryInfo dir, string fileHash, bool recursivo)
	{

		FileInfo[] files = BuscaConHash(dir, new string[] { fileHash }, recursivo);
		if (files.Length > 0)
			return files[0];
		else
			return null;
	}
	public static FileInfo[] BuscaConHash(this DirectoryInfo dir, IEnumerable<string> filesHash)
	{
		return dir.BuscaConHash(filesHash, false);
	}
	public static FileInfo BuscaConSHA256(this DirectoryInfo dir, string fileHash)
	{
		return dir.BuscaConSHA256(fileHash, false);
	}
	public static FileInfo BuscaConSHA256(this DirectoryInfo dir, string fileHash, bool recursivo)
	{

		FileInfo[] files = BuscaConSHA256(dir, new string[] { fileHash }, recursivo);
		if (files.Length > 0)
			return files[0];
		else
			return null;
	}
	public static FileInfo[] BuscaConSHA256(this DirectoryInfo dir, IEnumerable<string> filesHash)
	{
		return dir.BuscaConSHA256(filesHash, false);
	}
	public static FileInfo[] BuscaConSHA256(this DirectoryInfo dir, IEnumerable<string> filesSHA3, bool recursivo)
	{
		return dir.IBuscoConHashOSHA256(filesSHA3, recursivo, false);
	}
	public static FileInfo[] BuscaConHash(this DirectoryInfo dir, IEnumerable<string> filesHash, bool recursivo)
	{
		return dir.IBuscoConHashOSHA256(filesHash, recursivo, true);
	}
	static FileInfo[] IBuscoConHashOSHA256(this DirectoryInfo dir, IEnumerable<string> hashes, bool recursivo, bool isHash)
	{
		//por probar :)
		Llista<DirectoryInfo> dirs = new Llista<DirectoryInfo>();
		LlistaOrdenada<string, string> llista = new LlistaOrdenada<string, string>();
		Llista<FileInfo> filesEncontrados = new Llista<FileInfo>();
		FileInfo[] files = null;
		string hashArchivo = null;
		foreach (string hash in hashes)
			if (!llista.Existeix(hash))
				llista.Afegir(hash, hash);
		dirs.Afegir(dir);
		if (recursivo)
			dirs.AfegirMolts(dir.SubDirectoris());

		for (int i = 0; i < dirs.Count && llista.Count != 0; i++)
		{
			files = dirs[i].GetFiles();
			for (int j = 0; j < files.Length && llista.Count != 0; j++)
			{

				if (isHash)
				{
					hashArchivo = files[j].Hash();

				}
				else {
					hashArchivo = files[j].Sha256();

				}

				if (llista.Existeix(hashArchivo))
				{
					filesEncontrados.Afegir(files[j]);
					llista.Elimina(hashArchivo);

				}
			}
		}
		return filesEncontrados.ToTaula();
	}
	#endregion
	public static FileInfo[] GetAllFiles(this DirectoryInfo dir)
	{
		List<DirectoryInfo> dirs = new List<DirectoryInfo>();
		List<FileInfo> files = new List<FileInfo>();
		dirs.Add(dir);
		dirs.AddRange(dir.SubDirectoris());
		for (int i = 0; i < dirs.Count; i++)
			if (dirs[i].CanRead())
				files.AddRange(dirs[i].GetFiles());
		return files.ToTaula();
	}
	public static FileInfo[] GetFiles(this DirectoryInfo dir, params string[] formatsAdmessos)
	{
		List<FileInfo> files = new List<FileInfo>();
		string formatoTratado;
		if (dir.CanRead())
			for (int i = 0; i < formatsAdmessos.Length; i++)
		{
			formatoTratado = "*" + DameFormatoCorrectamente(formatsAdmessos[i]);

			files.AddRange(dir.GetFiles(formatoTratado));
		}
		return files.ToArray();
	}

	private static string DameFormatoCorrectamente(string formato)
	{
		string[] camposFormato;
		if (formato.Contains('.'))
		{
			camposFormato = formato.Split('.');
			formato = camposFormato[camposFormato.Length - 1];
		}
		formato = "." + formato;
		return formato;
	}

	public static FileInfo[] GetFiles(this DirectoryInfo dir, bool recursivo, params string[] formatsAdmessos)
	{
		Llista<FileInfo> files = new Llista<FileInfo>();
		DirectoryInfo[] subDirs;
		if (dir.CanRead())
		{
			files.AfegirMolts(dir.GetFiles(formatsAdmessos));
			if (recursivo)
			{
				subDirs = dir.SubDirectoris();
				for (int i = 0; i < subDirs.Length; i++)
					if (subDirs[i].CanRead())
						files.AfegirMolts(subDirs[i].GetFiles(formatsAdmessos));
			}
		}
		return files.ToArray();

	}
	public static KeyValuePair<DirectoryInfo, FileInfo[]>[] GetFilesWithDirectory(this DirectoryInfo dir)
	{
		return dir.GetFilesWithDirectory("*");
	}
	public static KeyValuePair<DirectoryInfo, FileInfo[]>[] GetFilesWithDirectory(this DirectoryInfo dir, params string[] formatsAdmessos)
	{
		return dir.GetFilesWithDirectory(false, "*");
	}
	public static KeyValuePair<DirectoryInfo, FileInfo[]>[] GetFilesWithDirectory(this DirectoryInfo dir, bool recursive, params string[] formatsAdmessos)
	{
		List<KeyValuePair<DirectoryInfo, FileInfo[]>> llistaArxiusPerCarpeta = new List<KeyValuePair<DirectoryInfo, FileInfo[]>>();
		DirectoryInfo[] subDirs;
		if (dir.CanRead())
		{
			llistaArxiusPerCarpeta.Add(new KeyValuePair<DirectoryInfo, FileInfo[]>(dir, dir.GetFiles(formatsAdmessos)));
			if (recursive)
			{
				subDirs = dir.SubDirectoris();
				for (int i = 0; i < subDirs.Length; i++)
					if (subDirs[i].CanRead())
						llistaArxiusPerCarpeta.Add(new KeyValuePair<DirectoryInfo, FileInfo[]>(subDirs[i], subDirs[i].GetFiles(formatsAdmessos)));
			}
		}
		return llistaArxiusPerCarpeta.ToArray();
	}
	public static DirectoryInfo[] SubDirectoris(this DirectoryInfo dirPare)
	{
		return dirPare.ISubDirectoris().ToArray();
	}
	static IEnumerable<DirectoryInfo> ISubDirectoris(this DirectoryInfo dirPare)
	{
		List<DirectoryInfo> subDirectoris = new List<DirectoryInfo>();
		DirectoryInfo[] subDirs;
		if (dirPare.CanRead())
		{
			subDirs = dirPare.GetDirectories();
			subDirectoris.AddRange(subDirs);

			for (int i = 0; i < subDirs.Length; i++)
				subDirectoris.AddRange(ISubDirectoris(subDirs[i]));

		}
		return subDirectoris;

	}
	/*Por mirar, revisar que sea optimo y necesario y este bien escrito ;) */
	/// <summary>
	/// Copia el archivo si no esta en la carpeta (mira el nombre y si esta compara el Hash del archivo)
	/// </summary>
	/// <param name="dir"></param>
	/// <param name="pathArchivo">direccion del archivo en memoria</param>
	/// <returns>devuelve la ruta final del archivo en caso de no existir el archivo devuelve null</returns>
	public static string HazSitio(this DirectoryInfo dir, string pathArchivo)
	{
		string direccionArchivoFinal = null;
		if (File.Exists(pathArchivo))
		{

			FileInfo fitxerAFerLloc = new FileInfo(pathArchivo);
			string hashFitxerAFerLloc = fitxerAFerLloc.Sha256();
			int contadorIguales = 1;
			bool encontradoPath = false;
			string nombreArchivo = Path.GetFileNameWithoutExtension(pathArchivo);
			string extension = Path.GetExtension(pathArchivo);
			direccionArchivoFinal = dir.FullName + Path.DirectorySeparatorChar + nombreArchivo + extension;
			while (File.Exists(direccionArchivoFinal) && !encontradoPath)
			{
				//son iguales en nombre miro el hash
				encontradoPath = new FileInfo(direccionArchivoFinal).SHA3Equals(hashFitxerAFerLloc);
				if (!encontradoPath)
				{
					//no son iguales en el hash
					direccionArchivoFinal = dir.FullName + Path.DirectorySeparatorChar + nombreArchivo + "(" + contadorIguales + ")" + extension;
					contadorIguales++;
				}
			}
			if (!File.Exists(direccionArchivoFinal))
				File.Copy(pathArchivo, direccionArchivoFinal);

		}

		return direccionArchivoFinal;
	}
	/// <summary>
	/// Copia el archivo si no esta en el directorio compara el hash de cada archivo de la carpeta.
	/// </summary>
	/// <param name="dir"></param>
	/// <param name="pathArchivo">direccion completa del archivo</param>
	/// <returns>devuelve la ruta final del archivo, si encuentra el archivo no lo copia i devuelve su ruta,devuelve null si no existe el archivo</returns>
	public static string HazSitioSiNoEsta(this DirectoryInfo dir, string pathArchivo)
	{
		string direccionArchivoFinal = null;
		bool encontrado = false;
		string nombreArchivo;
		string extension;
		int contadorIguales = 1;
		if (File.Exists(pathArchivo))
		{
			string hashArchivo = new FileInfo(pathArchivo).Sha256();
			FileInfo[] archivos = dir.GetFiles();
			for (int i = 0; i < archivos.Length && !encontrado; i++)
			{
				encontrado = archivos[i].SHA3Equals(hashArchivo);
				if (encontrado)
					direccionArchivoFinal = archivos[i].FullName;
			}
			if (!encontrado)
			{
				nombreArchivo = Path.GetFileNameWithoutExtension(pathArchivo);
				extension = Path.GetExtension(pathArchivo);
				direccionArchivoFinal = dir.FullName + Path.DirectorySeparatorChar + nombreArchivo + extension;
				while (File.Exists(direccionArchivoFinal))
				{
					direccionArchivoFinal = dir.FullName + Path.DirectorySeparatorChar + nombreArchivo + "(" + contadorIguales + ")" + extension;
					contadorIguales++;

				}
				File.Copy(pathArchivo, direccionArchivoFinal);
			}


		}

		return direccionArchivoFinal;
	}
	/// <summary>
	/// Copia si es necesario los arxivos existentes en la lista.
	/// </summary>
	/// <param name="dir"></param>
	/// <param name="pathsArxius">lista de path de archivos</param>
	/// <returns>key=path final archivo, value= archivo a copiar(solo devuelve los archivos existentes los demas no estan en la lista)</returns>
	public static KeyValuePair<string, FileInfo>[] HazSitioSiNoEsta(this DirectoryInfo dir, IEnumerable<string> pathsArxius)
	{//por provar con idRapido :) no es un hash que mira el archivo pero puede ser fiable...
		SortedList<string, FileInfo> idArxiusPerCopiar = new SortedList<string, FileInfo>();
		FileInfo fitxer;
		IEnumerator<FileInfo> fitxersCarpeta = dir.GetFiles().ObtieneEnumerador();
		TwoKeysList<string, string, FileInfo> pathsFinals = new TwoKeysList<string, string, FileInfo>();
		Llista<KeyValuePair<string, FileInfo>> llistaFinal = new Llista<KeyValuePair<string, FileInfo>>();

		Llista<FileInfo> archivosDuplicados = new Llista<FileInfo>();

		string direccioFinalArxiu;
		int contador;
		if (pathsArxius != null)
			foreach (string path in pathsArxius)
				if (File.Exists(path))
		{
			fitxer = new FileInfo(path);
			if (!idArxiusPerCopiar.ContainsKey(fitxer.IdUnicoRapido()))
				idArxiusPerCopiar.Add(fitxer.IdUnicoRapido(), fitxer);
			else
				archivosDuplicados.Afegir(fitxer);
		}
		foreach (var archiuACopiar in idArxiusPerCopiar)
		{//miro archivo a copiar uno a uno  para ver si se tiene que copiar o no :)

			while (fitxersCarpeta.MoveNext() && !idArxiusPerCopiar.ContainsKey(fitxersCarpeta.Current.IdUnicoRapido()))
				;//mira archivo por archivo de la carpeta si su hash esta en la lista de archivos a copiar
			if (idArxiusPerCopiar.ContainsKey(fitxersCarpeta.Current.IdUnicoRapido()))
			{
				pathsFinals.Add(fitxersCarpeta.Current.FullName, fitxersCarpeta.Current.IdUnicoRapido(), fitxersCarpeta.Current);//si el arxivo esta en la carpeta pongo la ruta
			}
			else {
				contador = 1;
				direccioFinalArxiu = dir.FullName + Path.DirectorySeparatorChar + Path.GetFileName(archiuACopiar.Value.FullName);
				while (File.Exists(direccioFinalArxiu))//mira que no coincida en nombre con ninguno
					direccioFinalArxiu = dir.FullName + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(archiuACopiar.Value.FullName) + "(" + (contador++) + ")" + Path.GetExtension(archiuACopiar.Value.FullName);
				File.Copy(archiuACopiar.Value.FullName, direccioFinalArxiu);//copia el archivo con su nuevo nombre
				pathsFinals.Add(direccioFinalArxiu, fitxersCarpeta.Current.IdUnicoRapido(), archiuACopiar.Value);
			}
		}
		llistaFinal.AfegirMolts(pathsFinals.Key1ValuePair());
		for (int i = 0; i < archivosDuplicados.Count; i++)
		{
			llistaFinal.Afegir(new KeyValuePair<string, FileInfo>(pathsFinals.ObtainValueWithKey2(archivosDuplicados[i].IdUnicoRapido()).FullName, archivosDuplicados[i]));
		}
		return llistaFinal.ToTaula();

	}
	/*Lo demas ya esta revisado :)  */
	public static void Abrir(this DirectoryInfo dir)
	{
		System.Diagnostics.Process.Start(dir.FullName + Path.DirectorySeparatorChar);//sino pongo la separacion me puede abrir un archivo con el nombre de la carpeta...
	}
	public static bool CanWrite(this DirectoryInfo dir)
	{
		bool canWrite = false;
		StreamWriter sw = null;
		string path = dir.FullName + Path.DirectorySeparatorChar + MiRandom.Next() + "Archivo.exTmp.SeTeniaDeHaberBorrado.SePuedeBorrar";
		try
		{
			sw = new StreamWriter(path, false);
			sw.WriteLine("prueba");
			canWrite = true;

		}
		catch
		{
		}
		finally
		{
			if (sw != null)
				sw.Close();
			if (File.Exists(path))
				File.Delete(path);
		}
		return canWrite;
	}

	public static bool CanRead(this DirectoryInfo dir)
	{
		bool puedeLeer = false;
		try
		{
			dir.GetDirectories();
			puedeLeer = true;
		}
		catch
		{
		}
		return puedeLeer;
	}

	#endregion
	#region Int
	public static bool EsPrimero(this int num)
	{
		bool esPrimero = true;
		for (int i = 2, f = Convert.ToInt32(Math.Sqrt(num)); i < f && esPrimero; i++)
			esPrimero = num % i != 0;
		return esPrimero;

	}
	public static int DamePrimeroCercano(this int num)
	{
		while (!num.EsPrimero())
			num++;
		return num;
	}
        #endregion
        #region Bitmap
        public static BitmapAnimated ToAnimatedBitmap(this IEnumerable<Bitmap> bmpsToAnimate, bool repetirSiempre = true)
        {
            return bmpsToAnimate.ToAnimatedBitmap(repetirSiempre, 500);
        }
        public static BitmapAnimated ToAnimatedBitmap(this IEnumerable<Bitmap> bmpsToAnimate, bool repetirSiempre=true,params int[] delay)
        {
            return new BitmapAnimated(bmpsToAnimate, delay) { AnimarCiclicamente = repetirSiempre };
        }
	public static Bitmap RandomPixels(this Bitmap imgRandom)
	{
		const int MAXPRIMERO = 19;
		return imgRandom.RandomPixels(Convert.ToInt32(Math.Sqrt(imgRandom.Width) % MAXPRIMERO));
	}
	public static Bitmap RandomPixels(this Bitmap imgRandom, int cuadradosPorLinea)
	{
		//hay un bug y no lo veo... no hace cuadrados...
		unsafe
		{
			imgRandom.TrataBytes((MetodoTratarBytePointer)((bytesImg) =>
			                                               {
			                                               	const int PRIMERODEFAULT = 13;//al ser un numero Primo no hay problemas
			                                               	Color[] cuadrados;
			                                               	Color colorActual;
			                                               	int a = 3, r = 0, g = 1, b = 2;
			                                               	int lenght = imgRandom.LengthBytes();
			                                               	int pixel = imgRandom.IsArgb() ? 4 : 3;
			                                               	int pixelsLineasHechas;
			                                               	int sumaX;
			                                               	int numPixeles;
			                                               	int posicionCuadrado = 0;
			                                               	if (cuadradosPorLinea < 1)
			                                               		cuadradosPorLinea = PRIMERODEFAULT;
			                                               	else
			                                               		cuadradosPorLinea = cuadradosPorLinea.DamePrimeroCercano();
			                                               	numPixeles = imgRandom.Width / cuadradosPorLinea;
			                                               	numPixeles = numPixeles.DamePrimeroCercano();
			                                               	cuadrados = DamePixeles(cuadradosPorLinea);
			                                               	colorActual = cuadrados[posicionCuadrado];
			                                               	for (int y = 0, xMax = imgRandom.Width * pixel; y < imgRandom.Height; y++)
			                                               	{
			                                               		pixelsLineasHechas = y * xMax;
			                                               		if (y % numPixeles == 0)
			                                               		{
			                                               			cuadrados = DamePixeles(cuadradosPorLinea);
			                                               		}
			                                               		for (int x = 0; x < xMax; x += pixel)
			                                               		{
			                                               			if (x % numPixeles == 0)
			                                               			{
			                                               				colorActual = cuadrados[++posicionCuadrado % cuadrados.Length];
			                                               			}
			                                               			sumaX = pixelsLineasHechas + x;
			                                               			if (pixel == 4)
			                                               			{
			                                               				bytesImg[sumaX + a] = byte.MaxValue;
			                                               			}
			                                               			bytesImg[sumaX + r] = colorActual.R;
			                                               			bytesImg[sumaX + g] = colorActual.G;
			                                               			bytesImg[sumaX + b] = colorActual.B;
			                                               		}
			                                               	}
			                                               })
			                    );
		}
		return imgRandom;
	}

	private static Color[] DamePixeles(int numPixeles)
	{
		Color[] pixeles = new Color[numPixeles];
		for (int i = 0; i < pixeles.Length; i++)
			pixeles[i] = Color.FromArgb(MiRandom.Next());
		return pixeles;
	}

	public static byte[] GetBytes(this Bitmap bmp)
	{
		BitmapData bmpData = bmp.LockBits();
		// Get the address of the first line.
		IntPtr ptr = bmpData.Scan0;

		// Declare an array to hold the bytes of the bitmap.
		int bytes = Math.Abs(bmpData.Stride) * bmp.Height;

		byte[] rgbValues = new byte[bytes];

		// Copy the RGB values into the array.
		ptr.CopyTo(rgbValues);
		// Unlock the bits.
		bmp.UnlockBits(bmpData);
		return rgbValues;
	}
	public static void SetBytes(this Bitmap bmp, byte[] rgbValues)
	{
		BitmapData bmpData = bmp.LockBits();
		// Get the address of the first line.
		IntPtr ptr = bmpData.Scan0;

		// Declare an array to hold the bytes of the bitmap.
		int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
		if (bytes != rgbValues.Length)
			throw new Exception("La array de bytes no se corresponde a la imagen");

		// Copy the RGB values back to the bitmap
		rgbValues.CopyTo(ptr);

		// Unlock the bits.
		bmp.UnlockBits(bmpData);

	}
	public static BitmapData LockBits(this Bitmap bmp)
	{
		return bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
	}
	public static APNG ToApng(this IEnumerable<Bitmap> fotogramas)
	{
		APNG pngAnimated = new APNG();
		pngAnimated.Add(fotogramas);
		return pngAnimated;
	}
	public static Stream ToStream(this Bitmap bmp)
	{
		return ToStream(bmp, System.Drawing.Imaging.ImageFormat.Png);
	}
	public static Stream ToStream(this Bitmap bmp, ImageFormat format)
	{
		MemoryStream stream=new MemoryStream();
		bmp.Save(stream, format);
		return stream;
	}

	public static void TrataBytes(this Bitmap bmp, MetodoTratarByteArray metodo)
	{
		BitmapData bmpData = bmp.LockBits();
		// Get the address of the first line.
		IntPtr ptr = bmpData.Scan0;

		// Declare an array to hold the bytes of the bitmap.
		int bytes = System.Math.Abs(bmpData.Stride) * bmp.Height;

		byte[] rgbValues = new byte[bytes];

		// Copy the RGB values into the array.
		ptr.CopyTo(rgbValues);
		if (metodo != null)
		{
			metodo(rgbValues);//se modifican los bytes :D
			// Copy the RGB values back to the bitmap
			rgbValues.CopyTo(ptr);
		}
		// Unlock the bits.
		bmp.UnlockBits(bmpData);

	}
	public static unsafe void TrataBytes(this Bitmap bmp, MetodoTratarBytePointer metodo)
	{

		BitmapData bmpData = bmp.LockBits();
		// Get the address of the first line.

		IntPtr ptr = bmpData.Scan0;
		if (metodo != null)
		{
			metodo((byte*)ptr.ToPointer());//se modifican los bytes :D
		}
		// Unlock the bits.
		bmp.UnlockBits(bmpData);

	}
	public static int LengthBytes(this Bitmap bmp)
	{
		int multiplicadorPixel = bmp.IsArgb() ? 4 : 3;
		return bmp.Height * bmp.Width * multiplicadorPixel;
	}
	public static bool IsArgb(this Bitmap bmp)
	{
		bool isArgb = false;
		switch (bmp.PixelFormat)
		{
			case PixelFormat.Format16bppArgb1555:
			case PixelFormat.Format32bppArgb:
			case PixelFormat.Format32bppPArgb:
			case PixelFormat.Format64bppArgb:
			case PixelFormat.Format64bppPArgb:
				isArgb = true;
				break;
		}
		return isArgb;
	}

	#endregion
	#region ColorMatrizExtension
	public static Bitmap ToBitmap(this Color[,] matrizColor)
	{
		Bitmap bmp = new Bitmap(matrizColor.GetLength(DimensionMatriz.X), matrizColor.GetLength(DimensionMatriz.Y));
		byte[] bytesImg = bmp.GetBytes();
		int posicion = 0;
		for (int y = 0, yFinal = matrizColor.GetLength(DimensionMatriz.Y); y < yFinal; y++)
			for (int x = 0, xFinal = matrizColor.GetLength(DimensionMatriz.X); x < xFinal; x++)
		{
			bytesImg[posicion] = matrizColor[x, y].A;
			bytesImg[posicion + 1] = matrizColor[x, y].R;
			bytesImg[posicion + 2] = matrizColor[x, y].G;
			bytesImg[posicion + 3] = matrizColor[x, y].B;
			posicion += 4;
		}
		bmp.SetBytes(bytesImg);
		return bmp;
	}
	public static Color[,] ToColorMatriz(this Bitmap bmp)
	{
		Color[,] colorArray = new Color[bmp.Width, bmp.Height];
		byte[] bytesImg = bmp.GetBytes();
		int posicion = 0;
		for (int y = 0, yFinal = colorArray.GetLength(DimensionMatriz.Y); y < yFinal; y++)
			for (int x = 0, xFinal = colorArray.GetLength(DimensionMatriz.X); x < xFinal; x++)
		{
			colorArray[x, y] = Color.FromArgb(bytesImg[posicion], bytesImg[posicion + 1], bytesImg[posicion + 2], bytesImg[posicion + 3]);
			posicion += 4;
		}
		return colorArray;
	}
	#endregion
	#region FileInfoExtension

	public static FileStream GetStream(this FileInfo file)
	{
		return new FileStream(file.FullName, FileMode.Open);
	}
	public static string IdUnicoLento(this FileInfo file)
	{
		return (file.Miniatura().GetBytes().SHA3()) + ";" + file.IdUnicoRapido();
	}

	public static string IdUnicoRapido(this FileInfo file)
	{
		return file.Extension + ";" + file.CreationTimeUtc.Ticks + ";" + file.LastWriteTimeUtc.Ticks + ";" + file.Length;
	}
	/// <summary>
	/// Icono del programa asociado al archivo
	/// </summary>
	/// <param name="file"></param>
	/// <returns></returns>
	public static Bitmap Icono(this FileInfo file)
	{
		if (!file.Exists)
			return null;//si no existe
		Icon ico =
			Icon.ExtractAssociatedIcon(file.FullName);
		return ico.ToBitmap();
	}
	/// <summary>
	/// Miniatura (Thumbnail Handlers) o icono en caso de no tener el archivo con medidas 250x250
	/// </summary>
	/// <param name="file"></param>
	/// <returns></returns>
	public static Bitmap Miniatura(this FileInfo file)
	{
		return file.Miniatura(250, 250);
	}
	/// <summary>
	/// Miniatura (Thumbnail Handlers) o icono en caso de no tener el archivo
	/// </summary>
	/// <param name="file"></param>
	/// <param name="amplitud"></param>
	/// <param name="altura"></param>
	/// <returns></returns>
	public static Bitmap Miniatura(this FileInfo file, int amplitud, int altura)
	{
		Bitmap bmp = null;
		ShellThumbnail thub = new ShellThumbnail();
		try
		{
			bmp = thub.GetThumbnail(file.FullName, amplitud, altura);
		}
		catch
		{
			bmp = file.Icono();
		}
		return bmp.Clone() as Bitmap;
	}
	public static string RutaRelativa(this FileInfo file, DirectoryInfo dir)
	{
		text ruta = ((text)file.FullName);
		ruta.Remove(dir.FullName);
		return ruta;
	}
	public static byte[] GetBytes(this FileInfo file)
	{
		return File.ReadAllBytes(file.FullName);
	}
	public static void Abrir(this FileInfo file)
	{
		System.Diagnostics.Process.Start(file.FullName);
	}
	public static string Sha256(this FileInfo file)
	{
		if (file.Exists)
		{
			FileStream stream = file.GetStream();
			string sha3 = stream.GetSha256Hash();
			stream.Close();
			return sha3;

		}
		else
			return null;
	}
	public static bool SHA3Equals(this FileInfo file, FileInfo file2)
	{
		return file.SHA3Equals(file2.Sha256());
	}
	public static bool SHA3Equals(this FileInfo file, string sha256)
	{
		return Equals(file.Sha256(), sha256);
	}
	/// <summary>
	/// Calcula el Hash del archivo
	/// </summary>
	/// <param name="file"></param>
	/// <returns>devuelve null si el archivo no existe</returns>
	public static string Hash(this FileInfo file)
	{
		if (file.Exists)
		{
			FileStream stream = file.GetStream();
			string hash = stream.GetMd5Hash();
			stream.Close();
			return hash;

		}
		else
			return null;
	}
	public static bool HashEquals(this FileInfo file, string hash)
	{
		return ComparaHash(file.Hash(), hash);
	}
	public static bool HashEquals(this FileInfo file1, FileInfo file2)
	{
		return ComparaHash(file1.Hash(), file2.Hash());
	}
	private static bool ComparaHash(string tmpHash, string tmpNewHash)
	{
		bool bEqual = false;
		int i;
		if (tmpNewHash.Length == tmpHash.Length)
		{
			i = 0;
			while ((i < tmpNewHash.Length) && (tmpNewHash[i] == tmpHash[i]))
			{
				i += 1;
			}
			if (i == tmpNewHash.Length)
			{
				bEqual = true;
			}
		}

		return bEqual;
	}
	private static string ByteArrayToString(byte[] arrInput)
	{
		int i;
		StringBuilder sOutput = new StringBuilder(arrInput.Length);
		for (i = 0; i < arrInput.Length - 1; i++)
		{
			sOutput.Append(arrInput[i].ToString("X2"));
		}
		return sOutput.ToString();
	}

	#endregion
	#region MatriuExtension
	/// <summary>
	/// Agrupa por la fila
	/// </summary>
	/// <typeparam name="Tvalue"></typeparam>
	/// <param name="matriz"></param>
	/// <returns></returns>
	public static Tvalue[][] AgrupaList<Tvalue>(this Tvalue[,] matriz)
	{
		return matriz.AgrupaList(DimensionMatriz.Fila);
	}
	public static Tvalue[][] AgrupaList<Tvalue>(this Tvalue[,] matriz, DimensionMatriz dimensionHaAgrupar)
	{
		List<Tvalue[]> grupos = new List<Tvalue[]>();
		for (int i = 0, f = matriz.GetLength(dimensionHaAgrupar); i < f; i++)
			if (dimensionHaAgrupar == DimensionMatriz.Columna)
				grupos.Add(matriz.Columna(i));
			else
				grupos.Add(matriz.Fila(i));
		return grupos.ToArray();
	}

	public static Tvalue[] Fila<Tvalue>(this Tvalue[,] matriu, int fila)
	{

		Llista<Tvalue> filas = new Llista<Tvalue>();
		if (fila <= matriu.GetLength(DimensionMatriz.Fila))
		{
			for (int x = 0; x < matriu.GetLength(DimensionMatriz.Columna); x++)
				filas.Afegir(matriu[x, fila]);
		}
		return filas.ToTaula();
	}
	public static Tvalue[] Columna<Tvalue>(this Tvalue[,] matriu, int columna)
	{
		Llista<Tvalue> columnas = new Llista<Tvalue>();
		if (columna <= matriu.GetLength(DimensionMatriz.Columna))
		{
			for (int y = 0; y < matriu.GetLength(DimensionMatriz.Fila); y++)
				columnas.Afegir(matriu[columna, y]);
		}
		return columnas.ToTaula();
	}
	public static int GetLength<Tvalue>(this Tvalue[,] matriu, DimensionMatriz dimension)
	{
		int longitud = matriu.GetLength((int)dimension);
		return longitud;
	}
	public static int GetLength<Tvalue>(this Tvalue[,,] matriu, DimensionMatriz dimension)
	{
		int longitud = matriu.GetLength((int)dimension);
		return longitud;
	}
        public static T[] ToTaula<T>(this T[,] matriu)
        {
            T[] tabla = new T[matriu.GetLength(DimensionMatriz.X) * matriu.GetLength(DimensionMatriz.Y)];
            for (int i = 0, y = 0, yFin = matriu.GetLength(DimensionMatriz.Y), xFin = matriu.GetLength(DimensionMatriz.X); y < yFin; y++)
                for (int x = 0; x < xFin; x++, i++)
                    tabla[i] = matriu[x, y];
            return tabla;
        }
	#endregion
	#region T[]
        public static int IndexOf<T>(this T[] array,T[] arrayAContener) where T:IComparable
        {
            return array.IndexOf(0, arrayAContener);
        }
        public static int IndexOf<T>(this T[] array,int inicio, T[] arrayAContener) where T : IComparable
        {
            int indexOf = -1;
            int indexOfAux = -1;
            if (array.Length > arrayAContener.Length)
            {
                for (int i = inicio, j = 0; i < array.Length && indexOf == -1; i++)
                {
                    if (array[i].CompareTo(arrayAContener[j]) != (int)CompareTo.Iguales)
                    {
                        indexOfAux = -1;
                        j = 0;
                    }
                    else if (indexOfAux == -1)
                    {
                        indexOfAux = i;
                        j = 1;
                    }
                    else if (i - indexOfAux == arrayAContener.Length - 1)
                        indexOf = indexOfAux;
                    else j++;

                }
            }
            return indexOf;
        }
        public static T DameElementoActual<T>(this T[] llista, Ordre escogerKey, int contador)
	{
		T objSelected = default(T);
		int posicio;
		switch (escogerKey)
		{
			case Ordre.Consecutiu:
				objSelected = llista[contador % llista.Length];
				break;
			case Ordre.ConsecutiuIAlInreves://repite el primero y el ultimo
				posicio = contador / llista.Length;
				if (posicio % 2 == 0)
				{
					//si esta bajando
					posicio = contador % llista.Length;
				}
				else {
					//esta subiendo
					posicio = llista.Length - (contador % llista.Length) - 1;
				}
				objSelected = llista[posicio];
				break;
		}
		return objSelected;
	}
	#endregion
	#region IEnumerable
	public static SortedList<T, T> ToSortedList<T>(this IEnumerable<T> list) where T : IComparable<T>
	{
		SortedList<T, T> sortedList = new SortedList<T, T>();
		foreach (T element in list)
			if (!sortedList.ContainsKey(element))
				sortedList.Add(element, element);
		return sortedList;
	}
        public static LlistaOrdenada<T, T> ToLlistaOrdenada<T>(this IEnumerable<T> list) where T : IComparable<T>
        {
            LlistaOrdenada<T, T> llistaOrdenada = new LlistaOrdenada<T, T>();
            foreach (T element in list)
                if (!llistaOrdenada.Existeix(element))
                    llistaOrdenada.Afegir(element, element);
            return llistaOrdenada;
        }
        /// <summary>
        /// Ordena por elemetodo de orden indicado sin modificar la coleccion que se va a ordenar
        /// </summary>
        /// <param name="list"></param>
        /// <param name="orden"></param>
        /// <returns>devuelve una array ordenada</returns>
        public static T[] Sort<T>(this IEnumerable<T> list, Orden orden) where T : IComparable
	{
		return list.ToArray().Sort(orden);
	}
	/// <summary>
	/// Ordena la array actual
	/// </summary>
	/// <param name="list"></param>
	/// <param name="orden"></param>
	/// <returns></returns>
	public static T[] Sort<T>(this T[] list, Orden orden) where T : IComparable
	{
		T[] listSorted = null;
		switch (orden)
		{
			case Orden.QuickSort:
				listSorted = list.SortByQuickSort();
				break;
			case Orden.Bubble:
				listSorted = list.SortByBubble();
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		return listSorted;
	}
	public static T[] SortByQuickSort<T>(this IEnumerable<T> list) where T : IComparable
	{
		return SortByQuickSort(list.ToArray());
	}
	public static T[] SortByQuickSort<T>(this T[] elements) where T : IComparable
	{
		int left = 0, right = elements.Length - 1;
		return ISortByQuickSort(elements, left, right);
	}
	private static T[] ISortByQuickSort<T>(T[] elements, int left, int right) where T : IComparable
	{
		//algoritmo sacado se internet
		//todos los derechos son de http://snipd.net/quicksort-in-c
		int i = left, j = right;
		IComparable pivot = elements[(left + right) / 2];
		IComparable tmp;
		while (i <= j)
		{
			while (elements[i].CompareTo(pivot) < 0)
			{
				i++;
			}

			while (elements[j].CompareTo(pivot) > 0)
			{
				j--;
			}

			if (i <= j)
			{
				// Swap
				tmp = elements[i];
				elements[i] = elements[j];
				elements[j] = (T)tmp;

				i++;
				j--;
			}
		}

		// Recursive calls
		if (left < j)
		{
			ISortByQuickSort(elements, left, j);
		}

		if (i < right)
		{
			ISortByQuickSort(elements, i, right);
		}


		return elements;
	}
	public static T[] SortByBubble<T>(this IEnumerable<T> list) where T : IComparable
	{
		return SortByBubble(list.ToArray());

	}
	public static T[] SortByBubble<T>(this T[] listaParaOrdenar) where T : IComparable
	{
		//codigo de internet adaptado :)
		//Todos los derechos//http://www.c-sharpcorner.com/UploadFile/3d39b4/bubble-sort-in-C-Sharp/
		bool flag = true;
		T temp;
		int numLength = listaParaOrdenar.Length;

		//sorting an array
		for (int i = 1; (i <= (numLength - 1)) && flag; i++)
		{
			flag = false;
			for (int j = 0; j < (numLength - 1); j++)
			{
				if (listaParaOrdenar[j + 1].CompareTo(listaParaOrdenar[j]) == (int)CompareTo.Superior)
				{
					temp = listaParaOrdenar[j];
					listaParaOrdenar[j] = listaParaOrdenar[j + 1];
					listaParaOrdenar[j + 1] = temp;
					flag = true;
				}
			}
		}
		return listaParaOrdenar;

	}
	public static IEnumerable<T> Clon<T>(this IEnumerable<T> listaHaClonar) where T : IClonable<T>
	{
		List<T> clones = new List<T>();
		foreach (T item in listaHaClonar)
			clones.Add(item.Clon());
		return clones;
	}
	public static IEnumerable<T> Clone<T>(this IEnumerable<T> listaHaClonar) where T : ICloneable
	{
		List<T> clones = new List<T>();
		foreach (T item in listaHaClonar)
			clones.Add((T)item.Clone());
		return clones;
	}
	public static int IndexOf<TValue>(this IEnumerable<TValue> valores, TValue valor) where TValue : IComparable
	{
		const int INDICESINENCONTRAR = -1;
		int index = INDICESINENCONTRAR;
		int posicion = 0;
		valores.WhileEach((valorAComparar) =>
		                  {
		                  	if (valor.CompareTo(valorAComparar) == (int)CompareTo.Iguales)
		                  		index = posicion;
		                  	posicion++;
		                  	return index == INDICESINENCONTRAR;
		                  }

		                 );
		return index;
	}
	public static void WhileEach<TValue>(this IEnumerable<TValue> valores, MetodoWhileEach<TValue> metodoAusar)
	{
		if (metodoAusar == null)
			throw new ArgumentNullException("se necesita el metodo para realizar la operacion");
		IEnumerator<TValue> enumeracion = valores.GetEnumerator();
		while (enumeracion.MoveNext() && metodoAusar(enumeracion.Current))
			;
		enumeracion.Dispose();
	}
	public static bool Existe<Tvalue>(this IEnumerable<Tvalue> valors, Tvalue valor) where Tvalue : IComparable<Tvalue>
	{//por provar!!
		bool noExiste = false;
		valors.WhileEach((valorAComparar) =>
		                 {
		                 	noExiste = valor.CompareTo(valorAComparar) != (int)CompareTo.Iguales;
		                 	return noExiste;
		                 });
		return !noExiste;
	}
	public static IEnumerator<Tvalue> ObtieneEnumerador<Tvalue>(this IEnumerable<Tvalue> valors)
	{
		return (IEnumerator<Tvalue>)valors.GetEnumerator();
	}
	public static Tvalue[,] ToMatriu<Tvalue>(this IEnumerable<Tvalue> valores, int filas)
	{
		return ToMatriu(valores, filas, DimensionMatriz.Fila);
	}
	//poder hacer que se pueda poner los valores en el orden contrario, de izquierda a derecha o  al rebes o por culumnas en vez de por filas...(y=0,x=0,y=1,x=0...)
	public static Tvalue[,] ToMatriu<Tvalue>(this IEnumerable<Tvalue> valores, int numeroDimension, DimensionMatriz dimensionTamañoMax)
	{
		if (numeroDimension < 1)
			throw new Exception("Como minimo 1 " + dimensionTamañoMax.ToString());

		Llista<Tvalue> llista = new Llista<Tvalue>(valores);
		int numeroOtraDimension = (llista.Count / (numeroDimension * 1.0)) > (llista.Count / numeroDimension) ? (llista.Count / numeroDimension) + 1 : (llista.Count / numeroDimension);
		int contador = 0;
		Tvalue[,] matriu;

		if (dimensionTamañoMax.Equals(DimensionMatriz.Fila))
			matriu = new Tvalue[numeroOtraDimension, numeroDimension];
		else
			matriu = new Tvalue[numeroDimension, numeroOtraDimension];

		for (int y = 0; y < matriu.GetLength(DimensionMatriz.Y) && contador < llista.Count; y++)
			for (int x = 0; x < matriu.GetLength(DimensionMatriz.X) && contador < llista.Count; x++)
				matriu[x, y] = llista[contador++];

		return matriu;

	}
	//para los tipos genericos :) el tipo generico se define en el NombreMetodo<Tipo> y se usa en todo el metodoConParametros ;)
	public static IEnumerable<Tvalue> Filtra<Tvalue>(this IEnumerable<Tvalue> valors, ComprovaEventHanler<Tvalue> comprovador)
	{
		if (comprovador == null)
			throw new ArgumentNullException("El metodo para realizar la comparacion no puede ser null");

		Llista<Tvalue> valorsOk = new Llista<Tvalue>();
		foreach (Tvalue valor in valors)
			if (comprovador(valor))
				valorsOk.Afegir(valor);
		return valorsOk;

	}
	public static IEnumerable<Tvalue> Desordena<Tvalue>(this IEnumerable<Tvalue> valors)
	{
		Llista<Tvalue> llistaOrdenada = new Llista<Tvalue>(valors);
		int posicionAzar;
		Llista<Tvalue> llistaDesordenada = new Llista<Tvalue>();

		for (int i = 0, total = llistaOrdenada.Count; i < total; i++)
		{
			posicionAzar = MiRandom.Next(0, llistaOrdenada.Count);
			llistaDesordenada.Afegir(llistaOrdenada[posicionAzar]);
			llistaOrdenada.Elimina(posicionAzar);

		}
		return llistaDesordenada;
	}
	public static byte[] CastingToByte(this object[] source)
	{
		byte[] bytes = new byte[source.Length];
		for (int i = 0; i < source.Length; i++)
		{
			try
			{
				bytes[i] = (Convert.ToByte(source[i]));
			}
			catch { }
		}
		return bytes;
	}
	public static IEnumerable<TResult> Casting<TResult>(this System.Collections.IEnumerable source)
	{
		return source.Casting<TResult>(false);
	}
	public static IEnumerable<TResult> Casting<TResult>(this System.Collections.IEnumerable source, bool conservarNoCompatiblesCasting)
	{
		Llista<TResult> llista = new Llista<TResult>();
		foreach (Object obj in source)
		{
			try
			{
				llista.Afegir((TResult)obj);//lo malo es cuando no es un int casting double
			}
			catch
			{
				//mirar de poder convertir el valor en caso de que sea posible ...
				if (conservarNoCompatiblesCasting)
					llista.Afegir(default(TResult));
			}
		}


		return llista;
	}
	//filtra los IEnumerable que tienen este metodoConParametros con el where
	public static IEnumerable<Tvalue> Ordena<Tvalue>(this IEnumerable<Tvalue> valors) where Tvalue : IComparable<Tvalue>
	{
		Llista<Tvalue> llista = new Llista<Tvalue>(valors);
		llista.Ordena();
		return llista;
	}
	public static IEnumerable<Tvalue> Treu<Tvalue>(this IEnumerable<Tvalue> valors, IEnumerable<Tvalue> valorsATreure) where Tvalue : IComparable
	{

		if (valorsATreure != null)
		{

			Llista<Tvalue> llista = new Llista<Tvalue>(valors);
			int compareTo = -1;

			return llista.Filtra(valor =>
			                     {

			                     	valorsATreure.WhileEach((valorAComparar) =>
			                     	                        {
			                     	                        	compareTo = valor.CompareTo(valorAComparar);
			                     	                        	return compareTo != (int)CompareTo.Iguales;
			                     	                        });
			                     	return compareTo != (int)CompareTo.Iguales;
			                     });
		}
		else
			return valors;

	}
	public static IEnumerable<Tvalue> AfegirValor<Tvalue>(this IEnumerable<Tvalue> valors, Tvalue valorNou)
	{
		Llista<Tvalue> valorsFinals = new Llista<Tvalue>(valors);
		valorsFinals.Afegir(valorNou);
		return valorsFinals;
	}
	public static IEnumerable<Tvalue> AfegirValors<Tvalue>(this IEnumerable<Tvalue> valors, IEnumerable<Tvalue> valorsNous, bool noPosarValorsJaExistents) where Tvalue : IComparable
	{
		if (valorsNous != null)
		{
			Llista<Tvalue> llista = new Llista<Tvalue>(valors);
			bool valorEnLista = true;
			if (valorsNous != null)
				foreach (Tvalue valor in valorsNous)
			{
				if (noPosarValorsJaExistents)
					valorEnLista = llista.Contains(valor);
				if (!valorEnLista && noPosarValorsJaExistents)
					llista.Afegir(valor);
				else if (!noPosarValorsJaExistents)
				{
					llista.Afegir(valor);
				}
			}
			return llista;
		}
		else
			return valors;

	}
	public static IEnumerable<Tvalue> AfegirValors<Tvalue>(this IEnumerable<Tvalue> valors, IEnumerable<Tvalue> valorsNous)
	{
		Llista<Tvalue> llista = new Llista<Tvalue>(valors);
		if (valorsNous != null)
			llista.AfegirMolts(valorsNous);
		return llista;

	}
	public static IEnumerable<Tkey> GetKeys<Tkey, Tvalue>(this IEnumerable<KeyValuePair<Tkey, Tvalue>> pairs)
	{
		Llista<Tkey> llista = new Llista<Tkey>();
		foreach (KeyValuePair<Tkey, Tvalue> pair in pairs)
			llista.Afegir(pair.Key);
		return llista;
	}
	public static Tvalue[] ToTaula<Tvalue>(this IEnumerable<Tvalue> valors)
	{

		return valors.ToArray();
	}
	public static Tvalue[] SubArray<Tvalue>(this IEnumerable<Tvalue> arrayB, long inicio)
	{
		return arrayB.SubArray(inicio, arrayB.Count() - inicio);
	}
	public static Tvalue[] SubArray<Tvalue>(this IEnumerable<Tvalue> arrayB, long inicio, long longitud)
	{
		Tvalue[] array;
		Llista<Tvalue> subArray;

		if (inicio < 0 || longitud <= 0)
			throw new IndexOutOfRangeException();
		array = arrayB.ToTaula();
		if (longitud + inicio > array.Length)
			throw new IndexOutOfRangeException();
		subArray = new Llista<Tvalue>();

		for (long i = inicio, fin = inicio + longitud; i < fin; i++)
			subArray.Afegir(array[i]);
		return subArray.ToArray();

	}
	#endregion
	#region IEnumerable<T[]>
	public static bool Contains<T>(this IEnumerable<T> list,IComparable objToFind)
	{
		return !list.Busca(objToFind).Equals(default(T));
	}
	public static T Busca<T>(this IEnumerable<T> list, IComparable objToFind)
	{
		bool contenida = false;
		object objToFindCasting = objToFind;
		T valueTrobat = default(T);
		try
		{
			bool isIComparable = list.ElementAt(0) is IComparable;
			list.WhileEach((objToCompare) =>
			               {
			               	if (isIComparable)
			               		contenida = (objToCompare as IComparable).CompareTo(objToFind) == (int)CompareTo.Iguales;
			               	else
			               		contenida = (object)objToCompare == objToFindCasting;
			               	if (contenida)
			               		valueTrobat = objToCompare;
			               	return !contenida;
			               });
		}
		catch { }
		return valueTrobat;
	}
	public static T[,] ToMatriu<T>(this IEnumerable<T[]> listaTablas)
	{
		T[,] toMatriuResult;
		T[][] tablaLista = listaTablas.ToArray();
		if (tablaLista.Length > 0)
			toMatriuResult = new T[listaTablas.LongitudMasGrande(), tablaLista.Length];
		else
			toMatriuResult = new T[0, 0];
		for (int y = 0; y < toMatriuResult.GetLength(DimensionMatriz.Y); y++)
			if (tablaLista[y] != null)
				for (int x = 0; x < toMatriuResult.GetLength(DimensionMatriz.X); x++)
					toMatriuResult[x, y] = tablaLista[y][x];
		return toMatriuResult;
	}
	public static int LongitudMasGrande<T>(this IEnumerable<T[]> listaTablas)
	{
		int longitudMax = -1;
		foreach (T[] tablaToCompare in listaTablas)
			if (tablaToCompare != null)
				if (longitudMax < tablaToCompare.Length)
					longitudMax = tablaToCompare.Length;
		return longitudMax;
	}
	#endregion
	#region IEnumerable KeyValuePair
	public static IEnumerable<Tvalue> GetValues<Tkey, Tvalue>(this IEnumerable<KeyValuePair<Tkey, Tvalue>> pairs)
	{
		List<Tvalue> llista = new List<Tvalue>();
		foreach (KeyValuePair<Tkey, Tvalue> pair in pairs)
			llista.Add(pair.Value);
		return llista;
	}
	public static Tvalue[] ValuesToArray<Tkey, Tvalue>(this IEnumerable<KeyValuePair<Tkey, Tvalue>> pairs)
	{
		return pairs.GetValues().ToArray();
	}
	public static Tkey[] KeysToArray<Tkey, Tvalue>(this IEnumerable<KeyValuePair<Tkey, Tvalue>> pairs)
	{
		return pairs.GetKeys().ToArray();
	}
	#endregion
	#region IEnumerableBytesExtension

	public static void Save(this IEnumerable<Byte> array, DirectoryInfo dir, string nameWithExtension, Encoding encoding)
	{
		array.Save(dir.FullName + Path.DirectorySeparatorChar + nameWithExtension, encoding);
	}
	public static void Save(this IEnumerable<Byte> array, DirectoryInfo dir, string nameWithExtension)
	{
		array.Save(dir, nameWithExtension, Encoding.UTF8);
	}
	public static void Save(this IEnumerable<Byte> array, string path)
	{
		array.Save(path, Encoding.UTF8);
	}
	public static void Save(this IEnumerable<Byte> array, string path, Encoding encoding)
	{
        if(File.Exists(path))
          File.Delete(path);
		FileStream file = new FileStream(path, FileMode.Create);
		BinaryWriter bin = new BinaryWriter(file, encoding);
		try
		{
			bin.Write(array.ToTaula());
		}
		finally
		{
			bin.Close();
			file.Close();
		}
	}
	#endregion
	#region StringExtension
	/// <summary>
	/// Serveix per facilitar la creació de matrius string[,]
	/// </summary>
	/// <param name="taulaClauCaracterSeparacióValor"></param>
	/// <param name="caracterSplit"></param>
	/// <returns></returns>
	public static string[,] DonamMatriuClauValor(this string[] taulaClauCaracterSeparacióValor, char caracterSplit)
	{
		string[,] taulaClauValor = new string[taulaClauCaracterSeparacióValor.Length, 2];
		string[] campsString;
		for (int x = 0; x < taulaClauValor.GetLength(DimensionMatriz.X); x++)
		{
			campsString = taulaClauCaracterSeparacióValor[x].Split(caracterSplit);
			taulaClauValor[x, 0] = campsString[0];
			taulaClauValor[x, 1] = campsString[1];
		}
		return taulaClauValor;
	}

	public static int IndicePrimeroContenido(this IEnumerable<string> valores, string textoAContener)
	{
		const int POSICIONNOENCONTRADA = -1;
		int pos = POSICIONNOENCONTRADA;
		int contador = 0;
		IEnumerator<string> enumerador;
		string peque;
		string grande;
		if (textoAContener != null)
		{
			enumerador = valores.ObtieneEnumerador();
			while (pos == POSICIONNOENCONTRADA && enumerador.MoveNext())
			{
				if (textoAContener.Length < enumerador.Current.Length)
				{
					peque = textoAContener;
					grande = enumerador.Current;
				}
				else {
					peque = enumerador.Current;
					grande = textoAContener;
				}
				if (grande.Contains(peque))
					pos = contador;
				contador++;
			}
		}
		return pos;
	}
	public static IEnumerable<string> PosaPrefix(this IEnumerable<string> valors, string prefix)
	{
		return valors.PosaPrefixISufix(prefix, null);
	}
	public static IEnumerable<string> PosaSufix(this IEnumerable<string> valors, string sufix)
	{
		return valors.PosaPrefixISufix(null, sufix);
	}
	public static IEnumerable<string> PosaPrefixISufix(this IEnumerable<string> valors, string prefix, string sufix)
	{
		if (sufix == null)
			sufix = "";
		if (prefix == null)
			prefix = "";

		Llista<string> valorsAmbPrefixISufix = new Llista<string>();

		foreach (string valor in valors)
			valorsAmbPrefixISufix.Afegir(prefix + valor + sufix);
		return valorsAmbPrefixISufix;
	}
	public static string[] Contenidos(this string aComparar, IEnumerable<string> acontenerLista)
	{
		if (acontenerLista == null)
			throw new NullReferenceException("La llista a contenir es null!!");
		string aCompararLow = aComparar.ToLower();
		List<string> contenidos = new List<string>();
		foreach (string paraComparar in acontenerLista)
			if (aCompararLow.Contains(paraComparar.ToLower()))
				contenidos.Add(paraComparar);
		return contenidos.ToTaula();
	}
	public static string[] ContenidosExactamente(this string aComparar, IEnumerable<string> acontenerLista)
	{
		if (acontenerLista == null)
			throw new NullReferenceException("La llista a contenir es null!!");
		string aCompararLow = aComparar.ToLower();
		Llista<string> contenidosExcatamente = new Llista<string>();
		string[] ordenados = Contenidos(aComparar, acontenerLista).OrdenaPorLongitud(false).ToTaula();
		text aComprarAux;
		for (int i = 0; i < ordenados.Length; i++)
			if (aCompararLow.Contains(ordenados[i].ToLower()))
		{
			aComprarAux = ((text)aCompararLow);
			aComprarAux.Remove(ordenados[i].ToLower());
			aCompararLow = aComprarAux;
			contenidosExcatamente.Afegir(ordenados[i]);
		}
		return contenidosExcatamente.ToTaula();
	}
	public static IEnumerable<string> OrdenaPorLongitud(this IEnumerable<string> lista)
	{
		List<string> listaOrdenada = new List<string>(lista);
		listaOrdenada.Sort(new Comparador<string>(delegate (string vString1, string vString2)
		                                          {
		                                          	return vString1.Length.CompareTo(vString2.Length);
		                                          }));
		return listaOrdenada;
	}
	public static IEnumerable<string> OrdenaPorLongitud(this IEnumerable<string> lista, bool ordenAscendente)
	{
		List<string> listaOrdenada = new List<string>(lista);
		int compareTo;
		listaOrdenada.Sort(new Comparador<string>(delegate (string vString1, string vString2)
		                                          {
		                                          	if (ordenAscendente)
		                                          		compareTo = vString1.Length.CompareTo(vString2.Length);
		                                          	else
		                                          		compareTo = vString2.Length.CompareTo(vString1.Length);
		                                          	return compareTo;

		                                          }));

		return listaOrdenada;
	}
	public static string[] Ordena(this IEnumerable<string> valores)
	{
		List<string> llista = new List<string>(valores);
		llista.Sort(new Comparador<string>((vString1, vString2) =>
		                                   {
		                                   	return string.Compare(vString1, vString2, StringComparison.CurrentCulture);
		                                   }));
		return llista.ToArray();
	}
	public static string[] Diferentes(this string aComparar, IEnumerable<string> listaAcomparar)
	{
		if (listaAcomparar == null)
			throw new NullReferenceException("La llista a comparar es null!!");
		string aCompararLow = aComparar.ToLower();
		List<string> contenidos = new List<string>();
		foreach (string paraComparar in listaAcomparar)
			if (!aCompararLow.Equals(paraComparar.ToLower()))
				contenidos.Add(paraComparar);
		return contenidos.ToTaula();
	}
	public static string[] NoContenidos(this IEnumerable<string> lista)
	{
		List<string> contenidos = new List<string>();
		string[] listaArray = lista.ToTaula();
		string aCompararLow;
		int contador;
		for (int i = 0; i < listaArray.Length; i++)
		{
			aCompararLow = listaArray[i].ToLower();
			contador = 0;
			for (int j = i; j < listaArray.Length; j++)
			{
				if (listaArray[j].ToLower().Contains(aCompararLow))
					contador++;
			}
			if (contador == 1)
				contenidos.Add(listaArray[i]);
		}
		return contenidos.ToTaula();
	}
	#endregion
	#region BinaryReader
	public static byte[] ReadBytes(this BinaryReader br, uint longitud)
	{
		return br.ReadBytes(Convert.ToUInt64(longitud));
	}
	public static byte[] ReadBytes(this BinaryReader br, long longitud)
	{
		return br.ReadBytes(Convert.ToUInt64(longitud));
	}
	public static byte[] ReadBytes(this BinaryReader br, ulong longitud)
	{
		byte[] bytes = new byte[longitud];
		for (ulong i = 0; i < longitud; i++)
			bytes[i] = br.ReadByte();
		return bytes;
	}
	#endregion
	#region ColorExtension

	public static System.Drawing.Color Invertir(this System.Drawing.Color color)
	{
		int r = color.R;
		int g = color.G;
		int b = color.B;
		r = Math.Abs(r - 255);
		g = Math.Abs(g - 255);
		b = Math.Abs(b - 255);
		return System.Drawing.Color.FromArgb(color.A, r, g, b);
	}
	public static bool EsColorClaro(this System.Drawing.Color color)
	{
		return (color.R + color.G + color.B) / 3 < 255 / 2;
	}

	#endregion
	#region ComparerExtension


	public static IComparer<T> Create<T>(this Comparer<T> comparador, ComparadorEventHandler<T> delegado)
	{
		return new Comparador<T>(delegado);
	}

	#endregion
	#region ObjectClaseExtension
	/// <summary>
	/// Coge los valores de las propiedades con get y set y los asigna a un nuevo objeto
	/// </summary>
	/// <typeparam name="Tipo"></typeparam>
	/// <param name="bytes"></param>
	/// <returns></returns>
	public static Tipo ClonSuperficialConPropiedades<Tipo>(this Tipo obj) where Tipo : new()
	{
		return obj.ClonSuperficialConPropiedades(new Tipo());
	}
	public static Tipo ClonSuperficialConPropiedades<Tipo>(this Tipo obj, Tipo clonNew)
	{
		Propiedad[] propiedades = obj.Serialitze();
		for (int i = 0; i < propiedades.Length; i++)
		{
			if ((int)(clonNew.GetPropertyUsage(propiedades[i].Nombre) ^ UsoPropiedad.Get) == (int)UsoPropiedad.Set)
				clonNew.SetProperty(propiedades[i].Nombre, propiedades[i].Objeto);

		}
		return clonNew;
	}

	/// <summary>
	/// Coge los valores de las propiedades con get y set y los asigna a un nuevo objeto usa la interficie ICloneable si esta presente
	/// </summary>
	/// <typeparam name="Tipo"></typeparam>
	/// <param name="bytes"></param>
	/// <returns></returns>
	public static Tipo ClonProfundoConPropiedades<Tipo>(this Tipo obj) where Tipo : new()
	{
		return obj.ClonProfundoConPropiedades(new Tipo());
	}
	public static Tipo ClonProfundoConPropiedades<Tipo>(this Tipo obj, Tipo clonNew)
	{
		Propiedad[] propiedades = obj.Serialitze();
		Object clonObjPropiedad;
		for (int i = 0; i < propiedades.Length; i++)
		{
			if (propiedades[i].Objeto is ICloneable)
			{
				clonObjPropiedad = ((ICloneable)propiedades[i].Objeto).Clone();
			}
			else if (propiedades[i].Objeto is IClonable<Tipo>)
                clonObjPropiedad = ((IClonable<Tipo>)propiedades[i].Objeto).Clon();
			else
				clonObjPropiedad = propiedades[i].Objeto;
			if ((int)(clonNew.GetPropertyUsage(propiedades[i].Nombre) ^ UsoPropiedad.Get) == (int)UsoPropiedad.Set)
				clonNew.SetProperty(propiedades[i].Nombre, clonObjPropiedad);

		}
		return clonNew;
	}
        #endregion
        #region Stream

	public static void Write(this Stream str,string datos)
	{
		byte[] dades = Serializar.GetBytes(datos);
		str.Write(dades);
	}
	public static void Write(this Stream str, byte[] datos)
	{
		if (datos == null)
			throw new NullReferenceException();
		str.Write(datos, 0, datos.Length);
	}
        public static byte[] GetAllBytes(this Stream str)
        {
            byte[] bytes = new byte[str.Length];
            long position = str.Position;
            str.Position = 0;
            str.Read(bytes, 0, bytes.Length);
            str.Position = position;
            return bytes;
        }
        #endregion
        #region Extensiones de Internet y mias
        #region SerializeExtension
        //por probar!!
        public static bool EndOfStream(this Stream str)
	{
		return str.Position == str.Length;
	}
	public static bool[] ToBits(this byte byteToBits)
	{
		return new byte[] { byteToBits }.ToBits();
	}
	public static bool[] ToBits(this IEnumerable<byte> byteToBits)
	{
		BitArray bitsArray = new BitArray(byteToBits.ToArray());
		bool[] bits = new bool[bitsArray.Length];
		for (int i = 0; i < bits.Length; i++)
			bits[i] = bitsArray[i];
		return bits;
	}
	public static string ToHex(this IEnumerable<byte> bytesEnum)
	{
		List<char> bytesHex = new List<char>();
		List<byte> bytes = new List<byte>(bytesEnum);
		byte byteHaciendose;

		for (int bx = 0, cx = 0; bx < bytes.Count; ++bx, ++cx)
		{
			byteHaciendose = ((byte)(bytes[bx] >> 4));
			if (byteHaciendose > 9)
				bytesHex.Add((char)(byteHaciendose - 10 + 'A'));
			else
				bytesHex.Add((char)(byteHaciendose + '0'));


			byteHaciendose = ((byte)(bytes[bx] & 0x0F));
			bytesHex.Add((char)(byteHaciendose > 9 ? byteHaciendose - 10 + 'A' : byteHaciendose + '0'));
		}

		return new string(bytesHex.ToArray());
	}
	public static byte[] HexStringToByteArray(this string hexString)
	{
		byte[] bytes = new byte[hexString.Length / 2];
		int[] hexValue = new int[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05,
			0x06, 0x07, 0x08, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F
		};
		try
		{
			for (int x = 0, i = 0; i < hexString.Length; i += 2, x += 1)
			{
				bytes[x] = (byte)(hexValue[Char.ToUpper(hexString[i + 0]) - '0'] << 4 |
				                  hexValue[Char.ToUpper(hexString[i + 1]) - '0']);
			}
		}
		catch
		{
			throw new Exception("La string no tiene el formato hex");
		}

		return bytes;
	}
	public static byte[] ToByteArray(this bool[] bits)
	{
		int numBytes = bits.Length / 8;
		if (bits.Length % 8 != 0)
			numBytes++;

		byte[] bytes = new byte[numBytes];
		bool[,] mbytes = bits.ToMatriu(8, DimensionMatriz.Columna);
		for (int i = 0; i < bytes.Length; i++)
			bytes[i] = mbytes.Fila(i).Reverse().ToArray().ToByte();
		return bytes;

	}
	public static byte ToByte(this bool[] bits)
	{
		byte byteBuild = new byte();
		for (int i = 0; i < bits.Length; i++)
		{
			if (bits[i])
				byteBuild |= (byte)(1 << (7 - i));

		}
		return byteBuild;
	}
	public static byte[] ToByteArray(this BitArray bitsArray)
	{

		return bitsArray.ToBoolArray().ToByteArray();
	}
	public static bool[] ToBoolArray(this BitArray bitsArray)
	{
		bool[] bits = new bool[bitsArray.Count];
		for (int i = 0; i < bits.Length; i++)
			bits[i] = bitsArray[i];
		return bits;
	}
	public static object DeserializeObject<T>(this string toDeserialize) where T : ISerializable
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
		StringReader textReader = new StringReader(toDeserialize);
		return xmlSerializer.Deserialize(textReader);
	}

	public static string SerializeObject<T>(this T toSerialize) where T : ISerializable
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
		StringWriter textWriter = new StringWriter();
		xmlSerializer.Serialize(textWriter, toSerialize);
		return textWriter.ToString();
	}

	public static Propiedad[] Serialitze<T>(this T obj)
	{
		Llista<Propiedad> campos = new Llista<Propiedad>();
		Type tipo = typeof(T);
		PropertyInfo[] camposTipo = tipo.GetProperties();
		PropertyInfo campoTipo;
		for (int i = 0; i < camposTipo.Length; i++)
		{
			campoTipo = camposTipo[i];
			if (!campoTipo.GetCustomAttributes(false).Contains(new SerialitzeIgnoreAttribute()))// no tiene el atributo SerialitzeIgnore lo añade :)
				campos.Afegir(new Propiedad(campoTipo.Name, campoTipo.GetValue(obj)));
		}
		return campos.ToTaula();
	}
	public static void Deserialitze<T>(this T obj, SerializationInfo info)
	{//mirar si asigna las propiedades a las que hereda :D
		Type tipo = typeof(T);
		PropertyInfo[] camposTipo = tipo.GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
		PropertyInfo campoTipo;
		for (int i = 0; i < camposTipo.Length; i++)
		{
			campoTipo = camposTipo[i];
			campoTipo.SetValue(obj, info.GetValue(campoTipo.Name, campoTipo.PropertyType));
		}


	}
	public static void Deserialitze<T>(this T obj, IEnumerable<Propiedad> objetosPropiedades)
	{//mirar si asigna las propiedades a las que hereda :D
		Type tipo = typeof(T);
		PropertyInfo[] camposTipo = tipo.GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
		LlistaOrdenada<string, object> lista = new LlistaOrdenada<string, object>();
		PropertyInfo campoTipo;
		foreach (Propiedad objPropiedad in objetosPropiedades)
			if (!lista.Existeix(objPropiedad.Nombre))
				lista.Afegir(objPropiedad.Nombre, objPropiedad.Objeto);
			else
				lista[objPropiedad.Nombre] = objPropiedad.Objeto;
		for (int i = 0; i < camposTipo.Length; i++)
		{
			campoTipo = camposTipo[i];
			if (lista.Existeix(campoTipo.Name))
				campoTipo.SetValue(obj, lista[campoTipo.Name]);
		}

	}
	public static Tipo Deserialitze<Tipo>(this IEnumerable<Propiedad> objetosPropiedades) where Tipo : new()
	{//mirar si asigna las propiedades a las que hereda :D
		Tipo obj = new Tipo();
		obj.Deserialitze(objetosPropiedades);
		return obj;
	}
	public static object GetValue(this PropertyInfo properti, object obj)
	{
		return properti.GetValue(obj, null);
	}
	public static void SetValue(this PropertyInfo properti, object obj, object value)
	{
		properti.SetValue(obj, value, null);
	}
	public static void AddValue(this SerializationInfo info, Propiedad campo)
	{
		info.AddValue(campo.Nombre, campo.Objeto, campo.Tipo);
	}
	public static void AddValue(this SerializationInfo info, string nombre, object obj)
	{
		info.AddValue(nombre, obj, obj.GetType());
	}


	#endregion
	#region ReflexionExtension

	public static object GetProperty<T>(this T obj, string nombrePropiedad)
	{
		return obj.GetType().GetProperty(nombrePropiedad).GetValue(obj);
	}
	public static void SetProperty<T>(this T obj, string nombrePropiedad, object value)
	{
		obj.GetType().GetProperty(nombrePropiedad).SetValue(obj, value);
	}
	public static Type GetPropertyType<Tipo>(this Tipo obj, string nombre)
	{
		return obj.GetType().GetProperty(nombre).PropertyType;
	}
	public enum UsoPropiedad
	{
		Get = 1,
		Set = 2,
		GetSet = Get | Set
	}
	/// <summary>
	/// Obtiene Si la propiedad es Get/Set o ambos
	/// </summary>
	/// <typeparam name="Tipo"></typeparam>
	/// <param name="bytes"></param>
	/// <param name="nombre"></param>
	/// <returns></returns>
	public static UsoPropiedad GetPropertyUsage<Tipo>(this Tipo obj, string nombre)
	{
		UsoPropiedad uso;
		if (!obj.GetType().GetProperty(nombre).CanRead && obj.GetType().GetProperty(nombre).CanWrite)
			uso = UsoPropiedad.Set;
		else if (obj.GetType().GetProperty(nombre).CanRead && obj.GetType().GetProperty(nombre).CanWrite)
			uso = UsoPropiedad.GetSet;
		else
			uso = UsoPropiedad.Get;
		return uso;
	}
	#endregion
	#region ByteArrayExtension
	public static string Hash(this byte[] obj)
	{
		MemoryStream ms = new MemoryStream(obj);
		string hash = ms.GetMd5Hash();
		ms.Close();
		return hash;
	}
	public static bool HashEquals(this byte[] obj, string hash)
	{
		return ComparaHash(obj.Hash(), hash);
	}
	public static bool HashEquals(this byte[] obj1, byte[] obj2)
	{
		return ComparaHash(obj1.Hash(), obj2.Hash());
	}
	public static string SHA3(this byte[] obj)
	{
		MemoryStream ms = new MemoryStream(obj);
		string sha3 = ms.GetSha256Hash();
		ms.Close();
		return sha3;
	}
	public static bool SHA3Equals(this byte[] obj, string sha3)
	{
		return Equals(obj.SHA3(), sha3);
	}
	public static bool SHA3Equals(this byte[] obj1, byte[] obj2)
	{
		return Equals(obj1.SHA3(), obj2.SHA3());
	}
	#endregion

	#region Pointers
	public static void CopyTo(this IntPtr ptr, byte[] destino)
	{
		System.Runtime.InteropServices.Marshal.Copy(ptr, destino, 0, destino.Length);
	}
	public static void CopyTo(this byte[] source, IntPtr ptrDestino)
	{
		System.Runtime.InteropServices.Marshal.Copy(source, 0, ptrDestino, source.Length);
	}
	public static void Dispose(this IntPtr point)
	{
		System.Runtime.InteropServices.Marshal.FreeHGlobal(point);

	}
	[HandleProcessCorruptedStateExceptions]
	public static unsafe int LenghtPoint(this IntPtr point)
	{
		//por optimizar
		byte* bytePoint = (byte*)point.ToPointer();
		byte auxByte;
		int length = 1;
		bool encontrado = false;
		while (!encontrado)
		{

			try
			{

				auxByte = bytePoint[length];
				length++;
			}
			catch (System.AccessViolationException)
			{
				length--;
				encontrado = true;

			}
		}
		return length;
	}

	/*   [HandleProcessCorruptedStateExceptions]
      public static unsafe int LenghtPoint(this IntPtr point)
      {
      //por arreglar
          int max = int.MaxValue;
          int min=0;
          int medioMaxMin;
          int length;
          byte* bytePoint = (byte*)point.ToPointer();
          byte auxByte;
          while(max!=min)
          {
              medioMaxMin = (max - min) / 2 + min;
              try
              {

                  auxByte = bytePoint[medioMaxMin];
                  if (min == medioMaxMin)
                  {
                      max = min;
                  }
                  else {
                      min = medioMaxMin;
                  }
              }catch(System.AccessViolationException)
              {
                  max= medioMaxMin;
              }
          }
          length = max;
          return length;
      }*/


	#endregion
	#region StreamExtension
	public static byte[] Read(this Stream stream, int lenght)
	{
		return stream.Read(Convert.ToUInt64(lenght));
	}
	public static byte[] Read(this Stream stream, uint lenght)
	{
		return stream.Read(Convert.ToUInt64(lenght));
	}
	public static byte[] Read(this Stream stream, long lenght)
	{
		return stream.Read(Convert.ToUInt64(lenght));
	}
	public static byte[] Read(this Stream stream, ulong lenght)
	{
		List<byte> bytes = new List<byte>();
		for (ulong i = 0; i < lenght && !stream.EndOfStream(); i++)
			bytes.Add((byte)stream.ReadByte());
		return bytes.ToArray();
	}

	public static String GetMd5Hash(this Stream fs)
	{
		return getHash(fs, new MD5CryptoServiceProvider());
	}

	public static String GetSha1Hash(this Stream fs)
	{
		return getHash(fs, new SHA1Managed());
	}

	public static String GetSha256Hash(this Stream fs)
	{
		return getHash(fs, new SHA256Managed());
	}

	private static String getHash(Stream fs, HashAlgorithm hash)
	{
		Int64 currentPos = fs.Position;
		string hashCalculated = null;
		StringBuilder sb;
		byte[] hashBytes;
		try
		{
			fs.Seek(0, SeekOrigin.Begin);
			sb = new StringBuilder();
			hashBytes = hash.ComputeHash(fs);
			for (int i = 0; i < hashBytes.Length; i++)
			{
				sb.Append(hashBytes[i].ToString("X2"));
			}
			hashCalculated = sb.ToString();
		}
		finally
		{
			try
			{
				fs.Seek(currentPos, SeekOrigin.Begin);
			}
			catch
			{
			}
		}
		return hashCalculated;
	}
	#endregion
	#region TypeExtension
	public static bool IsNullableType(this Type type)
	{
		return type.IsGenericType
			&& type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
	}
        #endregion
        #region string

        public static string Right(this string param, int length)
        {

            int value = param.Length - length;
            string result = param.Substring(value, length);
            return result;
        }
        public static string Left(this string value, int maxLength)
        {
            string valorLeft;
            if (string.IsNullOrEmpty(value))
                valorLeft = value;
            else
            {
                maxLength = Math.Abs(maxLength);

                valorLeft = (value.Length <= maxLength
                       ? value
                       : value.Substring(0, maxLength)
                       );
            }
            return valorLeft;
        }
        public static string Mid(this string param, int startIndex, int length)
        {
            string result = param.Substring(startIndex, length);
            return result;
        }

        public static string Mid(this string param, int startIndex)
        {
            string result = param.Substring(startIndex);
            return result;
        }
        #endregion
       
        #endregion

    }
}



