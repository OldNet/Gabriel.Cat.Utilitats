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
//mirar si IEnumerable puede ser IList :) asi evito el foreach que gasta mucho :)
namespace Gabriel.Cat.Extension
{
    public delegate bool MetodoWhileEach<Tvalue>(Tvalue valor);
    public delegate void MetodoTratarByteArray(byte[] byteArray);
    public unsafe delegate void MetodoTratarBytePointer(byte* prtByteArray);
    public delegate bool ComprovaEventHandler<Tvalue>(Tvalue valorAComprovar);
    public delegate T TrataObjeto<T>(T objHaTratar);
    public delegate ContinuaTratando<T> ContinuaTratandoObjeto<T>(T objHaTratar);
    public delegate TOut ConvertEventHandler<TOut, TIn>(TIn objToConvert);

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

    public enum Ordre
    {
        Consecutiu,
        ConsecutiuIAlInreves
    }

    #region Serializar???
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
        #region Cosas necesarias pero de clase// aqui es donde tengo variables y inicializo las cosas :D
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
                caracteresReservadosXml.Add(caracteresXmlReservados[i], caracteresXmlSustitutos[i]);
                caracteresReservadosXml.Add(caracteresXmlSustitutos[i], caracteresXmlReservados[i]);
            }
            #endregion
        }
        #endregion

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
            else
                vector.FinY = 0;
            if (vector.FinX > 0)
                vector.FinX = vector.FinX > width ? width : vector.FinX;
            else
                vector.FinX = 0;

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
                catch
                {
                    error = true;
                }
                return new ContinuaTratando<T>()
                {
                    Objeto = obj,
                    Continua = !error
                };
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
            if ( metodoTratarObjeto == null)
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

            Vector vectorAux = vector.ExtendPropertiesClone();

            ContinuaTratando<T> continuaObj = new ContinuaTratando<T>() { Continua = true };
            //miro las direcciones

            if (vectorAux.Sentido != Sentido.Centro)
            {//si se mueve :D
                do
                {

                    //tratoElObj
                    continuaObj = metodoTratarObjeto(matriz[vectorAux.InicioX, vectorAux.InicioY]);
                    matriz[vectorAux.InicioX, vectorAux.InicioY] = continuaObj.Objeto;
                    //muevo
                    //depende del sentido suma o resta a uno o a otro...
                    switch (vectorAux.Sentido)
                    {
                        case Sentido.Arriba:
                            vectorAux.InicioY--;
                            break;
                        case Sentido.Abajo:
                            vectorAux.InicioY++;
                            break;
                        case Sentido.Derecha:
                            vectorAux.InicioX++;
                            break;
                        case Sentido.Izquierda:
                            vectorAux.InicioY--;
                            break;
                        //va en diagonal
                        case Sentido.DiagonalDerechaAbajo:
                            if (vectorAux.DiagonalPura)
                            {
                                vectorAux.InicioX++;
                                vectorAux.InicioY++;
                            }
                            else
                            {
                                //depende del angulo
                                //menos45->X++
                                //45->diagonal
                                //mas45->Y++????????

                            }
                            break;
                        case Sentido.DiagonalDerechaArriba:
                            if (vectorAux.DiagonalPura)
                            {
                                vectorAux.InicioX++;
                                vectorAux.InicioY--;
                            }
                            else
                            {
                                //depende del angulo
                            }
                            break;
                        case Sentido.DiagonalIzquierdaAbajo:
                            if (vectorAux.DiagonalPura)
                            {
                                vectorAux.InicioX--;
                                vectorAux.InicioY++;
                            }
                            else
                            {
                                //depende del angulo
                            }
                            break;
                        case Sentido.DiagonalIzquierdaArriba:
                            if (vectorAux.DiagonalPura)
                            {
                                vectorAux.InicioX--;
                                vectorAux.InicioY--;
                            }
                            else
                            {
                                //depende del angulo
                            }
                            break;
                    }
                    if (continuaObj.Continua)
                        continuaObj.Continua = vectorAux.InicioX != vector.FinX && vectorAux.InicioY != vector.FinY;

                } while (continuaObj.Continua);
                matriz[vectorAux.FinX, vectorAux.FinY] = metodoTratarObjeto(matriz[vectorAux.FinX, vectorAux.FinY]).Objeto;
            }


        }
        #endregion
        #region IComparable
        /// <summary>
        /// Mira en la lista IEnumerable si contiene exactamente todos los elementos de la otra lista, no tiene en cuenta el orden
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="listToContain">lista a tener dentro de la otra</param>
        /// <returns></returns>
        public static bool Contains<T>(this IEnumerable<T> list, IEnumerable<T> listToContain) where T : IComparable
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
        public static void AddRange<TKey1,TKey2,TValue>(this TwoKeysList<TKey1,TKey2,TValue> lst,IList<TValue> nousValors) where TKey1:IComparable<TKey1> where TKey2:IComparable<TKey2> where TValue:IDosClausUniquesPerObjecte
        {
            for (int i = 0; i < nousValors.Count; i++)
                lst.Add((TKey1)nousValors[i].Clau,(TKey2) nousValors[i].Clau2,nousValors[i]);
        }
        public static void Add<TKey1, TKey2, TValue>(this TwoKeysList<TKey1, TKey2, TValue> lst, TValue nouValor) where TKey1 : IComparable<TKey1> where TKey2 : IComparable<TKey2> where TValue : IDosClausUniquesPerObjecte
        {
                lst.Add((TKey1)nouValor.Clau, (TKey2)nouValor.Clau2, nouValor);
        }
        public static ListaUnica<T> ToListaUnica<T>(this IEnumerable<T> enumeracion) where T : IClauUnicaPerObjecte
        {
            ListaUnica<T> lista = new ListaUnica<T>();
            lista.AddRange(enumeracion);
            return lista;
        }
        #endregion
        //cuando necesito escribir todos los caracteres en un xml y no quiero complicarme :)
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
        //cuando me interesa ejecutar cosas desde consola :)
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
        //cuando quiero metodos que solo aparezcan cuando el TValue cumpla unas determinadas caracteristicas uso metodos de extensión para poderlo hacer
        #region Llista

        public static TValue Search<TValue>(this Llista<TValue> llista, IComparable comparador) where TValue : IComparable
        {
            TValue valorEncontrado = default(TValue);
            bool encontrado = false;
            for (int i = 0; i < llista.Count && !encontrado; i++)
            {
                if (llista[i].CompareTo(comparador) == (int)CompareTo.Iguales)
                {
                    valorEncontrado = llista[i];
                    encontrado = true;
                }

            }
            return valorEncontrado;
        }
        #endregion
        #region IDictionary
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> llista, IList<IClauUnicaPerObjecte> values)
           where TValue : IClauUnicaPerObjecte
           where TKey : IComparable
        {
            for(int i=0;i<values.Count;i++)
                 llista.Add(values[i]);
        }
        public static void Add<TKey, TValue>(this IDictionary<TKey, TValue> llista, IClauUnicaPerObjecte value)
            where TValue : IClauUnicaPerObjecte
            where TKey : IComparable
        {
            llista.Add((TKey)value.Clau, (TValue)value);
        }
        public static void AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> llista, IClauUnicaPerObjecte value)
            where TValue : IClauUnicaPerObjecte
            where TKey : IComparable
        {
            llista.AddOrReplace((TKey)value.Clau, (TValue)value);
        }
        public static void AddOrReplace<TKey,TValue>(this IDictionary<TKey,TValue> diccionary,TKey key, TValue value)
            where TKey : IComparable
        {
            if (!diccionary.ContainsKey(key))
                diccionary.Add(key, value);
            else diccionary[key] = value;
        }
        #endregion
        #region ICollection<T>Extension
        public static void RemoveRange<T>(this ICollection<T> list, IEnumerable<T> elementsToRemove)
        {
            if (elementsToRemove != null)
                foreach (T element in elementsToRemove)
                    list.Remove(element);
        }
        #endregion
        #region DirectoriInfoExtension
        public static IReadOnlyList<KeyValuePair<DirectoryInfo, FileInfo[]>> GetFiles(this DirectoryInfo dir, bool recursive=false)
        {//windows si da error no se puede omit por lo tanto te quedas sin los archivos que puedes coger...es por eso que hago mi metodo...
            List<KeyValuePair<DirectoryInfo, FileInfo[]>> carpetasConSusArchivos = new List<KeyValuePair<DirectoryInfo, FileInfo[]>>();
            IReadOnlyList<DirectoryInfo> subDirs;
            bool canRead = dir.CanRead();
            if (canRead)
            {
                carpetasConSusArchivos.Add(new KeyValuePair<DirectoryInfo, FileInfo[]>(dir, dir.GetFiles()));

                if (recursive)
                {
                    subDirs = dir.SubDirectoris();
                    for (int i = 0; i < subDirs.Count; i++)
                        if (subDirs[i].CanRead())
                            carpetasConSusArchivos.Add(new KeyValuePair<DirectoryInfo, FileInfo[]>(subDirs[i], subDirs[i].GetFiles()));


                }
            }
            return carpetasConSusArchivos;
        }
        #region BuscaConHash
        public static FileInfo BuscaConHash(this DirectoryInfo dir, string fileHash, bool recursivo=false)
        {

            IReadOnlyList<FileInfo> files = BuscaConHash(dir, new string[] { fileHash }, recursivo);
            FileInfo file;
            if (files.Count > 0)
                file = files[0];
            else
                file = null;
            return file;
        }
        public static FileInfo BuscaConSHA256(this DirectoryInfo dir, string fileHash, bool recursivo=false)
        {

            IReadOnlyList<FileInfo> files = BuscaConSHA256(dir, new string[] { fileHash }, recursivo);
            FileInfo file;
            if (files.Count > 0)
                file= files[0];
            else
                file= null;
            return file;
        }
        public static IReadOnlyList<FileInfo> BuscaConSHA256(this DirectoryInfo dir, IList<string> filesSHA3, bool recursivo=false)
        {
            return dir.IBuscoConHashOSHA256(filesSHA3, recursivo, false);
        }
        public static IReadOnlyList<FileInfo> BuscaConHash(this DirectoryInfo dir, IList<string> filesHash, bool recursivo=false)
        {
            return dir.IBuscoConHashOSHA256(filesHash, recursivo, true);
        }
        static IReadOnlyList<FileInfo> IBuscoConHashOSHA256(this DirectoryInfo dir, IList<string> hashes, bool recursivo, bool isHash)
        {
            //por probar :)
            List<DirectoryInfo> dirs = new List<DirectoryInfo>();
            SortedList<string, string> llista = hashes.ToSortedList();
            List<FileInfo> filesEncontrados = new List<FileInfo>();
            FileInfo[] files = null;
            string hashArchivo = null;

            dirs.Add(dir);
            if (recursivo)
                dirs.AddRange(dir.SubDirectoris());

            for (int i = 0; i < dirs.Count && llista.Count != 0; i++)
            {
                files = dirs[i].GetFiles();
                for (int j = 0; j < files.Length && llista.Count != 0; j++)
                {

                    if (isHash)
                    {
                        hashArchivo = files[j].Hash();

                    }
                    else
                    {
                        hashArchivo = files[j].Sha256();

                    }

                    if (llista.ContainsKey(hashArchivo))
                    {
                        filesEncontrados.Add(files[j]);
                        llista.Remove(hashArchivo);

                    }
                }
            }
            return filesEncontrados;
        }
        #endregion
        public static IReadOnlyList<FileInfo> GetAllFiles(this DirectoryInfo dir)
        {
            List<DirectoryInfo> dirs = new List<DirectoryInfo>();
            List<FileInfo> files = new List<FileInfo>();
            dirs.Add(dir);
            dirs.AddRange(dir.SubDirectoris());
            for (int i = 0; i < dirs.Count; i++)
                if (dirs[i].CanRead())
                    files.AddRange(dirs[i].GetFiles());
            return files;
        }
        public static IReadOnlyList<FileInfo> GetFiles(this DirectoryInfo dir, params string[] formatsAdmessos)
        {
            List<FileInfo> files = new List<FileInfo>();
            FileInfo[] filesDir;
            SortedList<string, string> dicFormats;
            if (dir.CanRead())
            {
                filesDir = dir.GetFiles();
                for (int i = 0; i < formatsAdmessos.Length; i++)
                    formatsAdmessos[i] = DameFormatoCorrectamente(formatsAdmessos[i]); //els arreglo
                dicFormats = formatsAdmessos.ToSortedList();
                for (int i = 0; i < filesDir.Length; i++)
                {
                    if (dicFormats.ContainsKey(filesDir[i].Extension))
                        files.Add(filesDir[i]);
                }
            }
            return files;
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

        public static IReadOnlyList<FileInfo> GetFiles(this DirectoryInfo dir, bool recursivo, params string[] formatsAdmessos)
        {
            List<FileInfo> files = new List<FileInfo>();
            IReadOnlyList<DirectoryInfo> subDirs;
            if (dir.CanRead())
            {
                files.AddRange(dir.GetFiles(formatsAdmessos));
                if (recursivo)
                {
                    subDirs = dir.SubDirectoris();
                    for (int i = 0; i < subDirs.Count; i++)
                        if (subDirs[i].CanRead())
                            files.AddRange(subDirs[i].GetFiles(formatsAdmessos));
                }
            }
            return files;

        }
        public static IReadOnlyList<KeyValuePair<DirectoryInfo, IReadOnlyList<FileInfo>>> GetFilesWithDirectory(this DirectoryInfo dir)
        {
            return dir.GetFilesWithDirectory("*");
        }
        public static IReadOnlyList<KeyValuePair<DirectoryInfo, IReadOnlyList<FileInfo>>> GetFilesWithDirectory(this DirectoryInfo dir, params string[] formatsAdmessos)
        {
            return dir.GetFilesWithDirectory(false, "*");
        }
        public static IReadOnlyList<KeyValuePair<DirectoryInfo, IReadOnlyList<FileInfo>>> GetFilesWithDirectory(this DirectoryInfo dir, bool recursive, params string[] formatsAdmessos)
        {
            List<KeyValuePair<DirectoryInfo, IReadOnlyList<FileInfo>>> llistaArxiusPerCarpeta = new List<KeyValuePair<DirectoryInfo, IReadOnlyList<FileInfo>>>();
            IReadOnlyList<DirectoryInfo> subDirs;
            if (dir.CanRead())
            {
                llistaArxiusPerCarpeta.Add(new KeyValuePair<DirectoryInfo, IReadOnlyList<FileInfo>>(dir, dir.GetFiles(formatsAdmessos)));
                if (recursive)
                {
                    subDirs = dir.SubDirectoris();
                    for (int i = 0; i < subDirs.Count; i++)
                        if (subDirs[i].CanRead())
                            llistaArxiusPerCarpeta.Add(new KeyValuePair<DirectoryInfo, IReadOnlyList<FileInfo>>(subDirs[i], subDirs[i].GetFiles(formatsAdmessos)));
                }
            }
            return llistaArxiusPerCarpeta;
        }
        public static IReadOnlyList<DirectoryInfo> SubDirectoris(this DirectoryInfo dirPare)
        {
            return dirPare.ISubDirectoris();
        }
        static List<DirectoryInfo> ISubDirectoris(this DirectoryInfo dirPare)
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
        /// Copia el archivo si no esta en la carpeta (mira el nombre)
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="pathArchivo">direccion del archivo en memoria</param>
        /// <returns>devuelve la ruta final del archivo en caso de no existir el archivo devuelve null</returns>
        public static string HazSitio(this DirectoryInfo dir, string pathArchivoHaCopiar)
        {
            string direccionArchivoFinal = null;
            if (File.Exists(pathArchivoHaCopiar))
            {

                direccionArchivoFinal = dir.DamePathSinUsar(pathArchivoHaCopiar);
                File.Copy(pathArchivoHaCopiar, direccionArchivoFinal);

            }

            return direccionArchivoFinal;
        }
        public static string DamePathSinUsar(this DirectoryInfo dir, string pathArchivoHaCopiar)
        {
            FileInfo fitxerAFerLloc = new FileInfo(pathArchivoHaCopiar);

            int contadorIguales = 1;
            string nombreArchivo = Path.GetFileNameWithoutExtension(pathArchivoHaCopiar);
            string extension = Path.GetExtension(pathArchivoHaCopiar);
            string direccionArchivoFinal = dir.FullName + Path.DirectorySeparatorChar + nombreArchivo + extension;
            while (File.Exists(direccionArchivoFinal))
            {
                direccionArchivoFinal = dir.FullName + Path.DirectorySeparatorChar + nombreArchivo + "(" + contadorIguales + ")" + extension;
                contadorIguales++;
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
        public static IReadOnlyList<KeyValuePair<string, FileInfo>> HazSitioSiNoEsta(this DirectoryInfo dir, IEnumerable<string> pathsArxius)
        {//por provar con idRapido :) no es un hash que mira el archivo pero puede ser fiable...
            SortedList<string, FileInfo> idArxiusPerCopiar = new SortedList<string, FileInfo>();
            FileInfo fitxer;
            IEnumerator<FileInfo> fitxersCarpeta = dir.GetFiles().ObtieneEnumerador();
            TwoKeysList<string, string, FileInfo> pathsFinals = new TwoKeysList<string, string, FileInfo>();
            List<KeyValuePair<string, FileInfo>> llistaFinal = new List<KeyValuePair<string, FileInfo>>();

            List<FileInfo> archivosDuplicados = new List<FileInfo>();

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
                            archivosDuplicados.Add(fitxer);
                    }
            foreach (var archiuACopiar in idArxiusPerCopiar)
            {//miro archivo a copiar uno a uno  para ver si se tiene que copiar o no :)

                while (fitxersCarpeta.MoveNext() && !idArxiusPerCopiar.ContainsKey(fitxersCarpeta.Current.IdUnicoRapido()))
                    ;//mira archivo por archivo de la carpeta si su hash esta en la lista de archivos a copiar
                if (idArxiusPerCopiar.ContainsKey(fitxersCarpeta.Current.IdUnicoRapido()))
                {
                    pathsFinals.Add(fitxersCarpeta.Current.FullName, fitxersCarpeta.Current.IdUnicoRapido(), fitxersCarpeta.Current);//si el arxivo esta en la carpeta pongo la ruta
                }
                else
                {
                    contador = 1;
                    direccioFinalArxiu = dir.FullName + Path.DirectorySeparatorChar + Path.GetFileName(archiuACopiar.Value.FullName);
                    while (File.Exists(direccioFinalArxiu))//mira que no coincida en nombre con ninguno
                        direccioFinalArxiu = dir.FullName + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(archiuACopiar.Value.FullName) + "(" + (contador++) + ")" + Path.GetExtension(archiuACopiar.Value.FullName);
                    File.Copy(archiuACopiar.Value.FullName, direccioFinalArxiu);//copia el archivo con su nuevo nombre
                    pathsFinals.Add(direccioFinalArxiu, fitxersCarpeta.Current.IdUnicoRapido(), archiuACopiar.Value);
                }
            }
            llistaFinal.AddRange(pathsFinals.Key1ValuePair());
            for (int i = 0; i < archivosDuplicados.Count; i++)
            {
                llistaFinal.Add(new KeyValuePair<string, FileInfo>(pathsFinals.GetValueWithKey2(archivosDuplicados[i].IdUnicoRapido()).FullName, archivosDuplicados[i]));
            }
            return llistaFinal;

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
                {
                    sw.Close();
                    if (File.Exists(path))
                        File.Delete(path);
                }
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
        public static BitmapAnimated ToAnimatedBitmap(this IEnumerable<Bitmap> bmpsToAnimate, bool repetirSiempre = true, params int[] delay)
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
        public static MemoryStream ToStream(this Bitmap bmp, bool useRawFormat = false)
        {
            MemoryStream memory;
            ImageFormat format = useRawFormat ? bmp.RawFormat : bmp.IsArgb() ? ImageFormat.Png : ImageFormat.Jpeg;//no se porque aun pero no funciona...mejor pasarla a png
            memory = ToStream(bmp, format);
            return memory;

        }
        public static MemoryStream ToStream(this Bitmap bmp, ImageFormat format)
        {
            MemoryStream stream = new MemoryStream();
            string path;
            FileStream fs;
            try
            {
                new Bitmap(bmp).Save(stream, format);
            }
            catch (Exception ex)
            {
                path = System.IO.Path.GetRandomFileName();
                format = bmp.IsArgb() ? ImageFormat.Png : ImageFormat.Jpeg;
                new Bitmap(bmp).Save(path, format);
                fs = File.OpenRead(path);
                new Bitmap(fs).Save(stream, format);
                fs.Close();
                File.Delete(path);
            }
            return new MemoryStream(stream.GetAllBytes());
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
            return bmp.PixelFormat.IsArgb();
        }
        public static bool IsArgb(this PixelFormat format)
        {
            bool isArgb = false;
            switch (format)
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
            Bitmap bmp;
            if (!file.Exists)
                bmp = null;//si no existe
            else
            {
               bmp=Icon.ExtractAssociatedIcon(file.FullName).ToBitmap();
                
            }
            return bmp;
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
        /// Miniatura (Thumbnail Handlers)
        /// </summary>
        /// <param name="file"></param>
        /// <param name="amplitud"></param>
        /// <param name="altura"></param>
        /// <returns> si hay algun problema devuelve null</returns>
        public static Bitmap Miniatura(this FileInfo file, int amplitud, int altura)
        {
            Bitmap bmp = null;
            ShellThumbnail thub = new ShellThumbnail();
            try
            {
                bmp = thub.GetThumbnail(file.FullName, amplitud, altura).Clone() as Bitmap;
            }
            catch
            {
                bmp = null;
            }
            return bmp;
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
            string sha3;
            if (file.Exists)
            {
                FileStream stream = file.GetStream();
                sha3 = stream.Sha256Hash();
                stream.Close();
         

            }
            else
                sha3= null;
            return sha3;
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
            string hash;
            if (file.Exists)
            {
                FileStream stream = file.GetStream();
                hash = stream.Md5Hash();
                stream.Close();
            }
            else
                hash= null;
            return hash;
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
                    filas.Add(matriu[x, fila]);
            }
            return filas.ToTaula();
        }
        public static Tvalue[] Columna<Tvalue>(this Tvalue[,] matriu, int columna)
        {
            Llista<Tvalue> columnas = new Llista<Tvalue>();
            if (columna <= matriu.GetLength(DimensionMatriz.Columna))
            {
                for (int y = 0; y < matriu.GetLength(DimensionMatriz.Fila); y++)
                    columnas.Add(matriu[columna, y]);
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
        public static int IndexOf<T>(this T[] array, T[] arrayAContener) where T : IComparable
        {
            return array.IndexOf(0, arrayAContener);
        }
        public static int IndexOf<T>(this T[] array, int inicio, T[] arrayAContener) where T : IComparable
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
                    else
                        j++;

                }
            }
            return indexOf;
        }
        public static T DameElementoActual<T>(this IList<T> llista, Ordre escogerKey, int contador)
        {
           
            int posicio=0;
            if (contador < 0)
            {
                contador *= -1;
                contador = llista.Count - (contador % llista.Count);
            }
            switch (escogerKey)
            {
                case Ordre.Consecutiu:
                   posicio=contador % llista.Count;
                    break;
                case Ordre.ConsecutiuIAlInreves://repite el primero y el ultimo
                   
                    posicio = contador / llista.Count;
                    if (posicio % 2 == 0)
                    {
                        //si esta bajando
                        posicio = contador % llista.Count;
                    }
                    else
                    {
                        //esta subiendo
                        posicio = llista.Count - (contador % llista.Count) - 1;
                    }
                
                    break;
            }
            return llista[posicio];
        }
        #endregion
        #region IEnumerable
        public static bool ContainsAny<T>(this IEnumerable<T> list,IEnumerable<T> elementsToFindOne) where T:IComparable<T>
        {
            bool contains = false;
            SortedList<T, T> dicElements = elementsToFindOne.ToSortedList();
            list.WhileEach((elementList) =>
            {
                contains = dicElements.ContainsKey(elementList);
                return contains;
            });
            return contains;

        }
        public static IEnumerable<T> ConvertTo<T>(this IEnumerable enumeracion, ConvertEventHandler<T, object> metodoParaConvertirUnoAUno, bool omitirExcepciones = true)
        {
            return ConvertTo(enumeracion.Casting<object>(), metodoParaConvertirUnoAUno, omitirExcepciones);
        }
        public static IEnumerable<TOut> ConvertTo<TOut, TIn>(this IEnumerable<TIn> enumeracion, ConvertEventHandler<TOut, TIn> metodoParaConvertirUnoAUno, bool omitirExcepciones = true)
        {
            if (metodoParaConvertirUnoAUno == null) throw new ArgumentNullException();
            TOut outValue;
            foreach (TIn obj in enumeracion)
            {
                try
                {
                    outValue = metodoParaConvertirUnoAUno(obj);
                }
                catch
                {
                    if (!omitirExcepciones) throw;
                    outValue = default(TOut);
                }
                yield return outValue;

            }
        }
        public static SortedList<T, T> ToSortedList<T>(this IEnumerable<T> list) where T : IComparable<T>
        {
            SortedList<T, T> sortedList = new SortedList<T, T>();
            foreach (T element in list)
                if (!sortedList.ContainsKey(element))
                    sortedList.Add(element, element);
            return sortedList;
        }
        public static SortedList<IComparable, T> ToSortedListClauUnicaPerObjecte<T>(this IEnumerable<T> list) where T : IClauUnicaPerObjecte
        {
            SortedList<IComparable, T> sortedList = new SortedList<IComparable, T>();
            foreach (T element in list)
                if (!sortedList.ContainsKey(element.Clau))
                    sortedList.Add(element.Clau, element);
            return sortedList;
        }
        public static LlistaOrdenada<T, T> ToLlistaOrdenada<T>(this IEnumerable<T> list) where T : IComparable<T>
        {
            LlistaOrdenada<T, T> llistaOrdenada = new LlistaOrdenada<T, T>();
            foreach (T element in list)
                if (!llistaOrdenada.ContainsKey(element))
                    llistaOrdenada.Add(element, element);
            return llistaOrdenada;
        }
        public static LlistaOrdenada<IComparable, T> ToLlistaOrdenadaClauUnicaPerObjecte<T>(this IEnumerable<T> list) where T:IClauUnicaPerObjecte
        {
            LlistaOrdenada<IComparable, T> sortedList = new LlistaOrdenada<IComparable, T>();
            foreach (T element in list)
                if (!sortedList.ContainsKey(element.Clau))
                    sortedList.Add(element.Clau, element);
            return sortedList;
        }
        /// <summary>
        /// Ordena por elemetodo de orden indicado sin modificar la coleccion que se va a ordenar
        /// </summary>
        /// <param name="list"></param>
        /// <param name="orden"></param>
        /// <returns>devuelve una array ordenada</returns>
        public static T[] Sort<T>(this IEnumerable<T> list, Orden orden) where T : IComparable
        {
            return (T[])list.ToArray().Sort(orden);
        }
        /// <summary>
        /// Ordena la array actual
        /// </summary>
        /// <param name="list"></param>
        /// <param name="orden"></param>
        /// <returns></returns>
        public static IList<T> Sort<T>(this IList<T> list, Orden orden) where T : IComparable
        {
            IList<T> listSorted = null;
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
            return (T[])SortByQuickSort(list.ToArray());
        }
        public static IList<T> SortByQuickSort<T>(this IList<T> elements) where T : IComparable
        {
            int left = 0, right = elements.Count - 1;
            return ISortByQuickSort(elements, left, right);
        }
        private static IList<T> ISortByQuickSort<T>(IList<T> elements, int left, int right) where T : IComparable
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
            return (T[])SortByBubble(list.ToArray());

        }
        public static IList<T> SortByBubble<T>(this IList<T> listaParaOrdenar) where T : IComparable
        {
            //codigo de internet adaptado :)
            //Todos los derechos//http://www.c-sharpcorner.com/UploadFile/3d39b4/bubble-sort-in-C-Sharp/
            bool flag = true;
            T temp;
            int numLength = listaParaOrdenar.Count;

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
        public static List<T> Clon<T>(this IEnumerable<T> listaHaClonar) where T : IClonable<T>
        {
            List<T> clones = new List<T>();
            foreach (T item in listaHaClonar)
                clones.Add(item.Clon());
            return clones;
        }
        public static List<T> Clone<T>(this IEnumerable<T> listaHaClonar) where T : ICloneable
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
        public static List<Tvalue> Filtra<Tvalue>(this IList<Tvalue> valors, ComprovaEventHandler<Tvalue> comprovador)
        {
            if (comprovador == null)
                throw new ArgumentNullException("El metodo para realizar la comparacion no puede ser null");

            List<Tvalue> valorsOk = new List<Tvalue>();
            for (int i = 0; i < valors.Count; i++)
                if (comprovador(valors[i]))
                    valorsOk.Add(valors[i]);
            return valorsOk;

        }
        public static List<Tvalue> Desordena<Tvalue>(this IEnumerable<Tvalue> valors)
        {
            List<Tvalue> llistaOrdenada = new List<Tvalue>(valors);
            int posicionAzar;
            List<Tvalue> llistaDesordenada = new List<Tvalue>();

            for (int i = 0, total = llistaOrdenada.Count; i < total; i++)
            {
                posicionAzar = MiRandom.Next(0, llistaOrdenada.Count);
                llistaDesordenada.Add(llistaOrdenada[posicionAzar]);
                llistaOrdenada.RemoveAt(posicionAzar);

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
                catch
                {
                }
            }
            return bytes;
        }
        public static List<TResult> Casting<TResult>(this System.Collections.IEnumerable source, bool conservarNoCompatiblesCasting = false)
        {
            List<TResult> llista = new List<TResult>();
            foreach (Object obj in source)
            {
                try
                {
                    llista.Add((TResult)obj);//lo malo es cuando no es un int casting double
                }
                catch
                {
                    //mirar de poder convertir el valor en caso de que sea posible ...
                    if (conservarNoCompatiblesCasting)
                        llista.Add(default(TResult));
                }
            }


            return llista;
        }
        //filtra los IEnumerable que tienen este metodoConParametros con el where
        public static List<Tvalue> Ordena<Tvalue>(this IEnumerable<Tvalue> valors) where Tvalue : IComparable<Tvalue>
        {
            List<Tvalue> llista = new List<Tvalue>(valors);
            llista.Sort();
            return llista;
        }
        public static List<Tvalue> Treu<Tvalue>(this IEnumerable<Tvalue> valors, IEnumerable<Tvalue> valorsATreure) where Tvalue : IComparable
        {
            List<Tvalue> llista = new List<Tvalue>(valors);
            int compareTo = -1;
            if (valorsATreure != null)
            {
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
                return llista;

        }
        public static List<Tvalue> AfegirValor<Tvalue>(this IEnumerable<Tvalue> valors, Tvalue valorNou)
        {
            List<Tvalue> valorsFinals = new List<Tvalue>(valors);
            valorsFinals.Add(valorNou);
            return valorsFinals;
        }
        public static List<Tvalue> AfegirValors<Tvalue>(this IEnumerable<Tvalue> valors, IEnumerable<Tvalue> valorsNous, bool noPosarValorsJaExistents=false) where Tvalue : IComparable
        {
            List<Tvalue> llista = new List<Tvalue>(valors);
            bool valorEnLista = true;
            if (valorsNous != null)
            {
              
                if (valorsNous != null)
                    foreach (Tvalue valor in valorsNous)
                    {
                        if (noPosarValorsJaExistents)
                            valorEnLista = Contains(llista, valor);
                        if (!valorEnLista && noPosarValorsJaExistents)
                            llista.Add(valor);
                        else if (!noPosarValorsJaExistents)
                        {
                            llista.Add(valor);
                        }
                    }
             
            }
            return llista;

        }
        public static List<Tvalue> AfegirValors<Tvalue>(this IEnumerable<Tvalue> valors, IEnumerable<Tvalue> valorsNous)
        {
            List<Tvalue> llista = new List<Tvalue>(valors);
            if (valorsNous != null)
                llista.AddRange(valorsNous);
            return llista;

        }
        public static List<Tkey> GetKeys<Tkey, Tvalue>(this IEnumerable<KeyValuePair<Tkey, Tvalue>> pairs)
        {
            List<Tkey> llista = new List<Tkey>();
            foreach (KeyValuePair<Tkey, Tvalue> pair in pairs)
                llista.Add(pair.Key);
            return llista;
        }
        public static T[] ToTaula<T>(this IEnumerable<T> valors)
        {

            return valors.ToArray();
        }
        public static Coleccion<T> ToColeccion<T>(this IEnumerable<T> valors)
        {

            return valors.ToArray();
        }
        public static IList<T> ToIist<T>(this IEnumerable<T> valors)
        {

            return valors.ToArray();
        }

        public static List<T> SubList<T>(this IList<T> arrayB, int inicio)
        {
            return arrayB.SubList(inicio, arrayB.Count() - inicio);
        }
        public static List<T> SubList<T>(this IList<T> arrayB, int inicio, int longitud)
        {

            List<T> subArray;

            if (inicio < 0 || longitud <= 0)
                throw new IndexOutOfRangeException();
            if (longitud + inicio > arrayB.Count)
                throw new IndexOutOfRangeException();
            subArray = new List<T>();

            for (int i = inicio, fin = inicio + longitud; i < fin; i++)
                subArray.Add(arrayB[i]);

            return subArray;

        }
        #endregion
        #region IEnumerable<T[]>
        public static bool Contains<T>(this IList<T> list, IComparable objToFind)
        {
            return !Equals(list.Busca(objToFind),default(T));
        }
        public static T Busca<T>(this IList<T> list, IComparable objToFind)
        {
            bool contenida = false;
            object objToFindCasting = objToFind;
            T valueTrobat = default(T);
            try
            {
                bool isIComparable = list.ElementAt(0) is IComparable;
 
                for(int i=0;i<list.Count&&!contenida;i++)
                {
                    if (isIComparable)
                        contenida = (list[i] as IComparable).CompareTo(objToFind) == (int)CompareTo.Iguales;
                    else
                        contenida = (object)list[i] == objToFindCasting;
                    if (contenida)
                        valueTrobat = list[i];
                }
            }
            catch
            {
            }
            return valueTrobat;
        }
        public static T[,] ToMatriu<T>(this IList<T[]> listaTablas)
        {
            T[,] toMatriuResult;
            if (listaTablas.Count > 0)
                toMatriuResult = new T[listaTablas.LongitudMasGrande(), listaTablas.Count];
            else
                toMatriuResult = new T[0, 0];
            for (int y = 0; y < listaTablas.Count; y++)
                if (listaTablas[y] != null)
                    for (int x = 0; x < listaTablas[y].Length; x++)
                        toMatriuResult[x, y] = listaTablas[y][x];
            return toMatriuResult;
        }
        public static int LongitudMasGrande<T>(this IList<T[]> listaTablas)
        {
            int longitudMax = -1;
            for(int i=0;i<listaTablas.Count;i++)
                if (listaTablas[i] != null)
                    if (longitudMax < listaTablas[i].Length)
                        longitudMax = listaTablas[i].Length;
            return longitudMax;
        }
        #endregion
        #region IEnumerable KeyValuePair
        public static IReadOnlyList<TKey> FiltraKeys<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> pairs, ComprovaEventHandler<TKey> metodoComprovador)
        {
            return FiltraKeysOrValues(pairs, metodoComprovador, null).Casting<TKey>();
        }
        public static IReadOnlyList<TValue> FiltraValues<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> pairs, ComprovaEventHandler<TValue> metodoComprovador)
        {
            return FiltraKeysOrValues(pairs, null, metodoComprovador).Casting<TValue>();
        }
        static IReadOnlyList<object> FiltraKeysOrValues<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> pairs, ComprovaEventHandler<TKey> metodoComprovadorKeys, ComprovaEventHandler<TValue> metodoComprovarValues)
        {
            if (metodoComprovadorKeys == null && metodoComprovarValues == null)
                throw new ArgumentNullException();
            List<Object> filtro = new List<object>();
            foreach (KeyValuePair<TKey, TValue> pair in pairs)
            {
                if (metodoComprovadorKeys != null && metodoComprovadorKeys(pair.Key))
                    filtro.Add(pair.Key);
                else if (metodoComprovarValues != null && metodoComprovarValues(pair.Value))
                    filtro.Add(pair.Value);
            }
            return filtro;
        }
        public static IReadOnlyList<Tvalue> GetValues<Tkey, Tvalue>(this IEnumerable<KeyValuePair<Tkey, Tvalue>> pairs)
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
            if (File.Exists(path))
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
                    else
                    {
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
                valorsAmbPrefixISufix.Add(prefix + valor + sufix);
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
                    contenidosExcatamente.Add(ordenados[i]);
                }
            return contenidosExcatamente.ToTaula();
        }
        public static IEnumerable<string> OrdenaPorLongitud(this IEnumerable<string> lista)
        {
            List<string> listaOrdenada = new List<string>(lista);
            listaOrdenada.Sort(new Comparer<string>(delegate (string vString1, string vString2)
            {
                return vString1.Length.CompareTo(vString2.Length);
            }));
            return listaOrdenada;
        }
        public static IEnumerable<string> OrdenaPorLongitud(this IEnumerable<string> lista, bool ordenAscendente)
        {
            List<string> listaOrdenada = new List<string>(lista);
            int compareTo;
            listaOrdenada.Sort(new Comparer<string>(delegate (string vString1, string vString2)
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
            llista.Sort(new Comparer<string>((vString1, vString2) =>
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
        public static byte[] ReadBytes(this BinaryReader br, long longitud,bool fillSizeIfOverLoad = true)
        {
            byte[] bytes;
            if (fillSizeIfOverLoad && br.BaseStream.Length - br.BaseStream.Position < longitud)
                longitud = br.BaseStream.Length - br.BaseStream.Position;
            bytes = new byte[longitud];
            for (long i = 0; i < longitud&&!br.BaseStream.EndOfStream(); i++)
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


        public static IComparer<T> Create<T>(this System.Collections.Generic.Comparer<T> comparador, ComparadorEventHandler<T> delegado)
        {
            return new Comparer<T>(delegado);
        }

        #endregion
        #region ObjectClaseExtension
        /// <summary>
        /// Coge los valores de las propiedades con get y set y los asigna a un nuevo objeto
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T SimplePropertiesClone<T>(this T obj) where T : new()
        {
            return obj.SimplePropertiesClone(new T());
        }
        public static T SimplePropertiesClone<T>(this T obj, T clonNew)
        {
            Propiedad[] propiedades = obj.GetProperties();
            for (int i = 0; i < propiedades.Length; i++)
            {
                if ((int)(clonNew.GetPropertyUsage(propiedades[i].Nombre) ^ UsoPropiedad.Get) == (int)UsoPropiedad.Set)
                    clonNew.SetProperty(propiedades[i].Nombre, propiedades[i].Objeto);

            }
            return clonNew;
        }

        /// <summary>
        /// Coge los valores de las propiedades con get y set y los asigna a un nuevo objeto usa la interficie ICloneable,IClonable si esta presente
        /// </summary>
        /// <typeparam name="Tipo"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Tipo ExtendPropertiesClone<Tipo>(this Tipo obj) where Tipo : new()
        {
            return obj.ExtendPropertiesClone(new Tipo());
        }
        public static Tipo ExtendPropertiesClone<Tipo>(this Tipo obj, Tipo clonNew)
        {
            Propiedad[] propiedades = obj.GetProperties();
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
        /// <summary>
        /// Devuelve los bytes que quedan por leer
        /// </summary>
        /// <param name="str"></param>
        /// <param name="modificarPosicionAlLeer">si es true la posicion estará al final sino por donde estaba</param>
        /// <returns>bytes to end stream</returns>
        public static byte[] ReadToEnd(this Stream str, bool modificarPosicionAlLeer = false)
        {
            if (!str.CanRead)
                throw new ArgumentException("Can't Read stream", "str");
            byte[] bytesToEnd = new byte[str.Length - str.Position];
            long pos = str.Position;
            unsafe
            {
                bytesToEnd.UnsafeMethod((unsBytesToEnd) =>
                {
                    for (int i = 0; i < unsBytesToEnd.Length; i++)
                    {
                        *unsBytesToEnd.PtrArray = (byte)str.ReadByte();
                        unsBytesToEnd.PtrArray++;
                    }

                });
            }
            if (modificarPosicionAlLeer)
                str.Position = pos;
            return bytesToEnd;
        }
        /// <summary>
        ///Escribe los bytes resultado de la conversion de la string cogiendo como metodo Serializar.GetBytes(string)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="datos"></param>
        public static void Write(this Stream str, string datos)
        {
            if (datos == null)
                throw new ArgumentNullException();

            byte[] dades = Serializar.GetBytes(datos);
            str.Write(dades);
        }
        public static void Write(this Stream str, byte[] datos, int offset = 0)
        {
            if (datos == null)
                throw new NullReferenceException();
            str.Write(datos, offset, datos.Length);
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
        public static bool EndOfStream(this Stream str)
        {
            return str.Position == str.Length;
        }
        #endregion
        #region Extensiones de Internet y mias
        #region SerializeExtension


        public static bool[] ToBits(this byte byteToBits)
        {
            const int BITSBYTE = 8;
            bool[] bits = new bool[BITSBYTE];
            unsafe
            {
                bool* ptrBits;
                fixed(bool* ptBits=bits)
                {
                    ptrBits = ptBits;
                   
                        for(int i=0;i<BITSBYTE;i++)
                        {
                            *ptrBits = (byteToBits&(1 << (i % 8))) != 0;
                            ptrBits++;
                        }
                    

                }
            }
            return bits;
        }
        public static bool[] ToBits(this IList<byte> byteToBits)
        { return byteToBits.ToArray().ToBits(); }
        public static bool[] ToBits(this IEnumerable<byte> byteToBits)
        { return byteToBits.ToArray().ToBits(); }
        public static bool[] ToBits(this byte[] byteToBits)
        {
            const int BITSBYTE = 8;
            bool[] bits = new bool[byteToBits.Length * BITSBYTE];
            bool[] bitsAuxByte;
            //opero
            unsafe
            {
                bool* ptrBits, ptrBitsAuxByte;
                byte* ptrBytesToBits;
                fixed(bool* ptBits=bits)
                {
                    fixed (byte* ptBytesToBits=byteToBits)
                    {
                        ptrBytesToBits = ptBytesToBits;
                        ptrBits = ptBits;
                        for (int i = 0, f = byteToBits.Length; i < f; i++)
                        {
                            bitsAuxByte = (*ptrBytesToBits).ToBits();
                            ptrBytesToBits++;
                            fixed(bool* ptBitsAuxByte=bitsAuxByte)
                            {
                                ptrBitsAuxByte = ptBitsAuxByte;
                                for(int j=0;j<BITSBYTE;j++)
                                {
                                    *ptrBits = *ptrBitsAuxByte;
                                    ptrBits++;
                                    ptrBitsAuxByte++;
                                }
                            }
                        }
                    }
                }
            }

            return bits;
        }

        public static byte[] ToByteArray(this bool[] bits)
        {
            const int BITSBYTE = 8;

            if (bits.Length % BITSBYTE != 0)
                throw new ArgumentException();

            int numBytes = bits.Length / BITSBYTE;
            byte[] bytes = new byte[numBytes];
            int index = 0;
            
            unsafe
            {
                byte* ptrBytes;
                fixed(byte* ptBytes=bytes)
                {
                    ptrBytes = ptBytes;
                    for(int i=0;i<numBytes;i++)
                    {
                        *ptrBytes = bits.SubArray(index, BITSBYTE).ToByte();
                        index += BITSBYTE;
                        ptrBytes++;
                    }
                }

            }

            return bytes;

        }
        public static bool[] SubArray(this bool[] array,int startIndex,int length)
        {
            if (startIndex<0||startIndex + length > array.Length)
                throw new ArgumentOutOfRangeException();
            bool[] subArray = new bool[length];
            unsafe
            {
                bool* ptrArray , ptrSubArray;
                fixed (bool* ptArray = array)
                {
                    fixed (bool* ptSubArray = subArray)
                    {
                        ptrArray = ptArray;
                        ptrSubArray = ptSubArray;
                        ptrArray += startIndex;//asigno el inicio aqui :D
                        for (int j = 0,f=length; j < length; j++)
                        {
                            *ptrSubArray = *ptrArray;
                            ptrArray++;
                            ptrSubArray++;
                        }
                    }
                }
            }
            return subArray;
        }
        public static byte ToByte(this bool[] bits)
        {
            byte byteBuild = new byte();
            unsafe
            {
                bool* ptrBits;
                fixed (bool* ptBits = bits)
                {
                    ptrBits = ptBits;
                    for (int i = 0; i < bits.Length; i++)
                    {
                        if (*ptrBits)
                           byteBuild |= (byte)(1 << (7 - i));
                        ptrBits++;

                    }
                }
            }
            return byteBuild;
        }
        public static byte[] ReverseArray(this byte[] byteArrayToReverse)
        {
            byte[] byteArrayReversed = new byte[byteArrayToReverse.Length];
            unsafe
            {
                
                byte* ptrInverseBytesOut, ptrInverseBytesIn;
                byteArrayReversed.UnsafeMethod((ptrBytesOut) =>
                {
                    byteArrayToReverse.UnsafeMethod((ptrBytesIn)=>
                    {
                        ptrInverseBytesIn = ptrBytesIn.PtrArrayFin;
                        ptrInverseBytesOut = ptrBytesOut.PtrArrayFin;

                        for(long i=0,f=ptrBytesIn.Length/2;i<f;i++)
                        {
                            *ptrBytesOut.PtrArray = *ptrInverseBytesIn;
                            *ptrInverseBytesOut = *ptrBytesIn.PtrArray;
                            ptrBytesIn.PtrArray++;
                            ptrBytesOut.PtrArray++;
                            ptrInverseBytesIn--;
                            ptrInverseBytesOut--;
                        }

                    });
                });
            }
            return byteArrayReversed;
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

        public static Propiedad[] GetProperties<T>(this T obj)
        {
            Llista<Propiedad> campos = new Llista<Propiedad>();
            Type tipo = typeof(T);
            PropertyInfo[] camposTipo = tipo.GetProperties();
            PropertyInfo campoTipo;
            for (int i = 0; i < camposTipo.Length; i++)
            {
                campoTipo = camposTipo[i];
                if (!campoTipo.GetCustomAttributes(false).Contains(new SerialitzeIgnoreAttribute()))// no tiene el atributo SerialitzeIgnore lo añade :)
                    campos.Add(new Propiedad(campoTipo.Name, campoTipo.GetValue(obj)));
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
                if (!lista.ContainsKey(objPropiedad.Nombre))
                    lista.Add(objPropiedad.Nombre, objPropiedad.Objeto);
                else
                    lista[objPropiedad.Nombre] = objPropiedad.Objeto;
            for (int i = 0; i < camposTipo.Length; i++)
            {
                campoTipo = camposTipo[i];
                if (lista.ContainsKey(campoTipo.Name))
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

        public static object GetPropteryValue<T>(this T obj, string nombrePropiedad)
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
        public static byte[] Hash(this byte[] bytes, HashAlgorithm hash)
        {
            return new MemoryStream(bytes).Hash(hash);
        }
        public static byte[] Md5(this byte[] bytes)
        {
            return bytes.Hash(new MD5CryptoServiceProvider());
        }

        public static byte[] Sha1(this byte[] bytes)
        {
            return bytes.Hash(new SHA1Managed());
        }

        public static byte[] Sha256(this byte[] bytes)
        {
            return bytes.Hash(new SHA256Managed());
        }
        public static List<byte[]> Split(this byte[] array, byte byteSplit)
        {
            return Split(array, new byte[] { byteSplit });
        }
        public static List<byte[]> Split(this byte[] array, byte[] bytesSplit)
        {
            if (bytesSplit == null) throw new ArgumentNullException();
            List<byte[]> bytesSplited = new List<byte[]>();
            int posicionArray;
            int posicionArrayEncontrada;
            if (bytesSplit.Length != 0)
            {

                posicionArray =(int) array.BuscarArray(0, bytesSplit);

                //opero
                if (posicionArray > -1)
                {
                    bytesSplited.Add(array.SubArray(0, posicionArray));
                    posicionArray += bytesSplit.Length;
                    do
                    {
                        posicionArrayEncontrada =(int) array.BuscarArray(posicionArray, bytesSplit);
                        if (posicionArrayEncontrada > -1)
                        {
                            bytesSplited.Add(array.SubArray(posicionArray, posicionArrayEncontrada));
                            posicionArray = posicionArrayEncontrada + bytesSplit.Length;

                        }
                    }
                    while (posicionArrayEncontrada > -1);
                    if (posicionArray < array.Length)
                        bytesSplited.Add(array.SubArray(posicionArray, array.Length));

                }
                else
                {
                    bytesSplited.Add(array);//no la ha encontrado pues la pongo toda
                }
            }
            else bytesSplited.Add(array);//no hay bytesPara hacer split pues pongo toda
            return bytesSplited;
        }
        public static void UnsafeMethod(this byte[] array, MetodoUnsafeArray metodo)
        {
            UnsafeArray.Usar(array, metodo);
        }
        public static Hex IndexByte(this byte[] array, byte byteAEcontrar)
        {
            return IndexByte(array, 0, byteAEcontrar);
        }
        public static Hex IndexByte(this byte[] array, Hex offsetInicio, byte byteAEcontrar)
        {
            const long NOENCONTRADO = -1;
            long indexOf = NOENCONTRADO;
            long inicio = offsetInicio;//por si peta que no sea con el fixed
            unsafe
            {
                array.UnsafeMethod((unsArray) =>
                {

                    for (long i = inicio; i < unsArray.Length && indexOf == NOENCONTRADO; i++)
                        if (unsArray[i] == byteAEcontrar)
                            indexOf = i;
                });
            }
            return indexOf;
        }
        #region SubArray with pointers me gustaria no repetir codigo haciendo un metodo generico con un filtro pero de momento el c# no me lo permite hacer...
        public static byte[] SubArray(this byte[] array, int cantidad)
        {
            return SubArray(array, 0, cantidad);
        }
        public static  byte[] SubArray(this byte[] array, int inicio, int cantidad) 
        {
            if (inicio<0||cantidad + inicio > array.Length)
                throw new ArgumentOutOfRangeException();
            byte[] subArray = new byte[cantidad];
            unsafe
            {
                fixed (byte* ptrArray = array)
                {
                    fixed (byte* ptrSubArray = subArray)
                    {
                        byte* ptArray = ptrArray, ptSubArray = ptrSubArray;
                        ptArray += inicio;//asigno el inicio aqui :D
                        for (int j = 0; j < cantidad; j++)
                        {
                            *ptSubArray = *ptArray;
                            ptArray++;
                            ptSubArray++;
                        }
                    }
                }
            }
            return subArray;
        }

        public static char[] SubArray(this char[] array, Hex cantidad)
        {
            return SubArray(array, 0, cantidad);
        }
        public static char[] SubArray(this char[] array, Hex inicio, Hex cantidad)
        {
            if (cantidad + inicio > array.LongLength)
                throw new ArgumentOutOfRangeException();
            char[] subArray = new char[cantidad];
            unsafe
            {
                fixed (char* ptrArray = array)
                {
                    fixed (char* ptrSubArray = subArray)
                    {
                        char* ptArray = ptrArray, ptSubArray = ptrSubArray;
                        ptArray += inicio;//asigno el inicio aqui :D
                        for (long j = 0; j < cantidad; j++)
                        {
                            *ptSubArray = *ptArray;
                            ptArray++;
                            ptSubArray++;
                        }
                    }
                }
            }
            return subArray;
        }

        public static int[] SubArray(this int[] array, Hex cantidad)
        {
            return SubArray(array, 0, cantidad);
        }
        public static int[] SubArray(this int[] array, Hex inicio, Hex cantidad)
        {
            if (cantidad + inicio > array.LongLength)
                throw new ArgumentOutOfRangeException();
            int[] subArray = new int[cantidad];
            unsafe
            {
                fixed (int* ptrArray = array)
                {
                    fixed (int* ptrSubArray = subArray)
                    {
                        int* ptArray = ptrArray, ptSubArray = ptrSubArray;
                        ptArray += inicio;//asigno el inicio aqui :D
                        for (long j = 0; j < cantidad; j++)
                        {
                            *ptSubArray = *ptArray;
                            ptArray++;
                            ptSubArray++;
                        }
                    }
                }
            }
            return subArray;
        }

        public static uint[] SubArray(this uint[] array, Hex cantidad)
        {
            return SubArray(array, 0, cantidad);
        }
        public static uint[] SubArray(this uint[] array, Hex inicio, Hex cantidad)
        {
            if (cantidad + inicio > array.LongLength)
                throw new ArgumentOutOfRangeException();
            uint[] subArray = new uint[cantidad];
            unsafe
            {
                fixed (uint* ptrArray = array)
                {
                    fixed (uint* ptrSubArray = subArray)
                    {
                        uint* ptArray = ptrArray, ptSubArray = ptrSubArray;
                        ptArray += inicio;//asigno el inicio aqui :D
                        for (long j = 0; j < cantidad; j++)
                        {
                            *ptSubArray = *ptArray;
                            ptArray++;
                            ptSubArray++;
                        }
                    }
                }
            }
            return subArray;
        }

        public static short[] SubArray(this short[] array, Hex cantidad)
        {
            return SubArray(array, 0, cantidad);
        }
        public static short[] SubArray(this short[] array, Hex inicio, Hex cantidad)
        {
            if (cantidad + inicio > array.LongLength)
                throw new ArgumentOutOfRangeException();
            short[] subArray = new short[cantidad];
            unsafe
            {
                fixed (short* ptrArray = array)
                {
                    fixed (short* ptrSubArray = subArray)
                    {
                        short* ptArray = ptrArray, ptSubArray = ptrSubArray;
                        ptArray += inicio;//asigno el inicio aqui :D
                        for (long j = 0; j < cantidad; j++)
                        {
                            *ptSubArray = *ptArray;
                            ptArray++;
                            ptSubArray++;
                        }
                    }
                }
            }
            return subArray;
        }
        public static ushort[] SubArray(this ushort[] array, Hex cantidad)
        {
            return SubArray(array, 0, cantidad);
        }
        public static ushort[] SubArray(this ushort[] array, Hex inicio, Hex cantidad)
        {
            if (cantidad + inicio > array.LongLength)
                throw new ArgumentOutOfRangeException();
            ushort[] subArray = new ushort[cantidad];
            unsafe
            {
                fixed (ushort* ptrArray = array)
                {
                    fixed (ushort* ptrSubArray = subArray)
                    {
                        ushort* ptArray = ptrArray, ptSubArray = ptrSubArray;
                        ptArray += inicio;//asigno el inicio aqui :D
                        for (long j = 0; j < cantidad; j++)
                        {
                            *ptSubArray = *ptArray;
                            ptArray++;
                            ptSubArray++;
                        }
                    }
                }
            }
            return subArray;
        }
        public static long[] SubArray(this long[] array, Hex cantidad)
        {
            return SubArray(array, 0, cantidad);
        }
        public static long[] SubArray(this long[] array, Hex inicio, Hex cantidad)
        {
            if (cantidad + inicio > array.LongLength)
                throw new ArgumentOutOfRangeException();
            long[] subArray = new long[cantidad];
            unsafe
            {
                fixed (long* ptrArray = array)
                {
                    fixed (long* ptrSubArray = subArray)
                    {
                        long* ptArray = ptrArray, ptSubArray = ptrSubArray;
                        ptArray += inicio;//asigno el inicio aqui :D
                        for (long j = 0; j < cantidad; j++)
                        {
                            *ptSubArray = *ptArray;
                            ptArray++;
                            ptSubArray++;
                        }
                    }
                }
            }
            return subArray;
        }
        public static ulong[] SubArray(this ulong[] array, Hex cantidad)
        {
            return SubArray(array, 0, cantidad);
        }
        public static ulong[] SubArray(this ulong[] array, Hex inicio, Hex cantidad)
        {
            if (cantidad + inicio > array.LongLength)
                throw new ArgumentOutOfRangeException();
            ulong[] subArray = new ulong[cantidad];
            unsafe
            {
                fixed (ulong* ptrArray = array)
                {
                    fixed (ulong* ptrSubArray = subArray)
                    {
                        ulong* ptArray = ptrArray, ptSubArray = ptrSubArray;
                        ptArray += inicio;//asigno el inicio aqui :D
                        for (long j = 0; j < cantidad; j++)
                        {
                            *ptSubArray = *ptArray;
                            ptArray++;
                            ptSubArray++;
                        }
                    }
                }
            }
            return subArray;
        }

        #endregion
        public static byte[] AddArray(this byte[] array, params byte[][] arraysToAdd)
        {

            byte[] bytesResult;
            int espacioTotal = 0;
            if (arraysToAdd != null)
                for (int i = 0; i < arraysToAdd.Length; i++)
                    if (arraysToAdd[i] != null)
                        espacioTotal += arraysToAdd[i].Length;
            bytesResult = new byte[espacioTotal + array.Length];
            unsafe
            {
                fixed (byte* ptrArrayResult = bytesResult, ptrArray = array)
                {
                    byte* ptArrayResult = ptrArrayResult, ptArray = ptrArray, ptArrayToAdd;
                    for (int i = 0; i < array.Length; i++)
                    {//pongo los datos de la primera array
                        *ptArrayResult = *ptArray;
                        ptArray++;
                        ptArrayResult++;
                    }
                    if (arraysToAdd != null)
                        for (int i = 0; i < arraysToAdd.Length; i++)
                        {//pongo las demas arrays :D
                            if (arraysToAdd[i] != null)
                                fixed (byte* ptrArrayToAdd = arraysToAdd[i])
                                {
                                    ptArrayToAdd = ptrArrayToAdd;
                                    for (int j = 0; j < arraysToAdd[i].Length; j++)
                                    {
                                        *ptArrayResult = *ptArrayToAdd;
                                        ptArrayToAdd++;
                                        ptArrayResult++;
                                    }
                                }
                        }
                }
            }
            return bytesResult;
        }
        public static Hex BuscarArray(this byte[] datos, byte[] arrayAEncontrar)
        {
            return BuscarArray(datos, 0, arrayAEncontrar);
        }

        public static Hex BuscarArray(this byte[] datos, Hex offsetInicio, byte[] arrayAEncontrar)
        {

            if (arrayAEncontrar == null)
                throw new ArgumentNullException("bytesAEncontrar");
            if (offsetInicio < 0 || offsetInicio + arrayAEncontrar.LongLength > datos.LongLength)
                throw new ArgumentOutOfRangeException();
            const long DIRECCIONNOENCONTRADO = -1;
            long direccionBytes = DIRECCIONNOENCONTRADO;
            long posibleDireccion = DIRECCIONNOENCONTRADO;
            long posicionBytesAEncontrar = 0;
            //busco la primera aparicion de esos bytes a partir del offset dado como parametro
            unsafe
            {
                fixed (byte* ptrBytesDatos = datos, ptrBytesAEcontrar = arrayAEncontrar)
                {
                    byte* ptBytesDatos = ptrBytesDatos;
                    byte* ptBytesAEcontrar = ptrBytesAEcontrar;
                    for (long i = offsetInicio, finDatos = datos.LongLength, finArrayAEncontrar = arrayAEncontrar.LongLength, posicionFinArrayAEcontrar = finArrayAEncontrar - 1; i < finDatos && direccionBytes == DIRECCIONNOENCONTRADO && i + (finArrayAEncontrar - 1 - posicionBytesAEncontrar) < finDatos/*si los bytes que quedan por ver se pueden llegar a ver continuo sino paro*/; i++)
                    {
                        if (*ptBytesDatos == *ptBytesAEcontrar)
                        {//si no es el siguiente va al otro pero si es el primero se lo salta como si fuese malo...
                            if (posibleDireccion == DIRECCIONNOENCONTRADO)//si es la primera vez que entra
                                posibleDireccion = i;//le pongo el inicio
                            else if (posicionBytesAEncontrar == posicionFinArrayAEcontrar)//si es la última vez
                                direccionBytes = posibleDireccion;//le pongo el resultado para poder salir del bucle

                            ptBytesAEcontrar++;
                            posicionBytesAEncontrar++;
                        }
                        else
                        {
                            posibleDireccion = DIRECCIONNOENCONTRADO;
                            ptBytesAEcontrar = ptrBytesAEcontrar;//reinicio la posicion
                            posicionBytesAEncontrar = 0;
                            if (*ptBytesDatos == *ptBytesAEcontrar)
                            {//si no es el siguiente va al otro pero si es el primero se lo salta como si fuese malo...asi lo arreglo :D
                                if (posibleDireccion == DIRECCIONNOENCONTRADO)//si es la primera vez que entra
                                    posibleDireccion = i;//le pongo el inicio
                                else if (posicionBytesAEncontrar == posicionFinArrayAEcontrar)//si es la última vez
                                    direccionBytes = posibleDireccion;//le pongo el resultado para poder salir del bucle

                                ptBytesAEcontrar++;
                                posicionBytesAEncontrar++;
                            }
                        }
                        ptBytesDatos++;

                    }
                }
            }
            return direccionBytes;
        }
        public static void Remove(this byte[] datos, Hex offsetInicio, Hex longitud, byte byteEnBlanco = 0x0)
        {
            if (offsetInicio < 0 || longitud < 0 || datos.LongLength < offsetInicio + longitud)
                throw new ArgumentException();
            unsafe
            {
                fixed (byte* ptrbytesRom = datos)
                {
                    byte* ptbytesRom = ptrbytesRom;
                    ptbytesRom += offsetInicio;
                    for (long i = 0, f = longitud; i < f; i++)
                    {
                        *ptbytesRom = byteEnBlanco;
                        ptbytesRom++;
                    }
                }
            }
        }
        public static void SetArray(this byte[] datos, Hex offsetIncioArrayDatos, byte[] arrayAPoner)
        {
            Buffer.BlockCopy(arrayAPoner, 0, datos, (int)offsetIncioArrayDatos, arrayAPoner.Length);
        }
        public static void Invertir(this byte[] array)
        {
            //por testear!!
            byte aux;
            unsafe
            {
                fixed (byte* ptrArray = array)
                    for (int i = 0, f = array.Length / 2, j = array.Length - 1; i < f; i++, j--)
                {
                    aux = ptrArray[i];
                    ptrArray[i] = ptrArray[j];
                    ptrArray[j] = aux;
                }
            }
        }
        public static string Hash(this byte[] obj)
        {
            MemoryStream ms = new MemoryStream(obj);
            string hash = ms.Md5Hash();
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
            string sha3 = ms.Sha256Hash();
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
        public static void CopyTo(this IntPtr ptr, byte[] destino,int startIndex=0)
        {
            System.Runtime.InteropServices.Marshal.Copy(ptr, destino, startIndex, destino.Length);
        }
        public static void CopyTo(this byte[] source, IntPtr ptrDestino, int startIndex = 0)
        {
            System.Runtime.InteropServices.Marshal.Copy(source, startIndex, ptrDestino, source.Length);
        }
        public static void Dispose(this IntPtr point)
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(point);

        }

        #endregion
        #region StreamExtension

        public static byte[] Read(this Stream stream, Hex lenght)
        {
            byte[] bytes = new byte[lenght];
            unsafe
            {

                bytes.UnsafeMethod((ptBytes) =>
                {
                    for (long i = 0,fParcial=lenght,fTotal=stream.Length; i < fParcial && i<fTotal ; i++)
                    {
                        *ptBytes.PtrArray = ((byte)stream.ReadByte());
                        ptBytes.PtrArray++;
                    }
                });
            }
            return bytes;
        }

        public static String Md5Hash(this Stream fs)
        {
            return getHash(fs, new MD5CryptoServiceProvider());
        }

        public static String Sha1Hash(this Stream fs)
        {
            return getHash(fs, new SHA1Managed());
        }

        public static String Sha256Hash(this Stream fs)
        {
            return getHash(fs, new SHA256Managed());
        }
        public static byte[] Md5(this Stream fs)
        {
            return fs.Hash(new MD5CryptoServiceProvider());
        }

        public static byte[] Sha1(this Stream fs)
        {
            return fs.Hash(new SHA1Managed());
        }

        public static byte[] Sha256(this Stream fs)
        {
            return fs.Hash(new SHA256Managed());
        }
        public static byte[] Hash(this Stream fs, HashAlgorithm hash)
        {
            return hash.ComputeHash(fs);
        }
        private static Hex getHash(Stream fs, HashAlgorithm hash)
        {//sacado de interncet
            Int64 currentPos = fs.Position;
            byte[] hashBytes;
            try
            {
                fs.Seek(0, SeekOrigin.Begin);
                hashBytes = fs.Hash(hash);

            }catch { hashBytes = new byte[] { }; }
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
            return (Hex)hashBytes;
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
        //Encode


        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        //Decode


        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

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
        #region System.Text.Encoding
        public static string EncodeBase64(this System.Text.Encoding encoding, string text)
        {
            string strBase64;
            if (text != null)
                strBase64 = System.Convert.ToBase64String(encoding.GetBytes(text));//obtengo los bytes//los paso o base64String
            else strBase64 = null;
            return strBase64;
        }

        public static string DecodeBase64(this System.Text.Encoding encoding, string encodedText)
        {
            string textDecoded;
            if (encodedText != null)
                textDecoded = encoding.GetString(System.Convert.FromBase64String(encodedText));//obtengo los bytes decodificados//lo convierto a string la array decodificada
            else textDecoded = null;
            return textDecoded;
        }
        public static string DecodeBase64(this System.Text.Encoding encoding, byte[] encodedBytesText)
        {
            string strBase64Decoded;
            if (encodedBytesText != null)
            {
                strBase64Decoded = encoding.DecodeBase64(encoding.GetString(encodedBytesText));//obtengo la string base 64 previamente codificada
            }
            else strBase64Decoded = null;
            return strBase64Decoded;
        }
        #endregion
        #endregion
        #region TimeSpan
        public static string ToHoursMinutesSeconds(this TimeSpan time)
        {
            return time.Hours + ":" + time.Minutes + ":" + time.Seconds;
        }
        #endregion
    }
}



