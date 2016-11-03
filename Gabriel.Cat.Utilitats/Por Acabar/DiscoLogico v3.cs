using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.Extension;
namespace Gabriel.Cat.Utilitats.New
{

    public delegate void ObjetoIOEventHandler(DiscoLogico disco, IOArgs archivo);
    /// <summary>
    /// En desarrollo aun no operativa, se trata de optimizar el encontrar archivos y directorios nuevos y perdidos
    /// </summary>
    public class DiscoLogico : IComparable, IComparable<DiscoLogico>,IComparable<string>
    {
        //si estan como focos activos se comprovaran automaticamente cada X tiempo -> los focos activos comprovaran sus subDirs que no esten como focos activos de forma secuencial asi ahorra recursos.
        //poder cargar y guardar en xml toda la informacion.
        #region Class Atributos y eventos
        public static event ObjetoIOEventHandler DirectorioEncontrado;
        public static event ObjetoIOEventHandler DirectorioPerdido;
        public static event ObjetoIOEventHandler ArchivoEncontrado;
        public static event ObjetoIOEventHandler ArchivoPerdido;

        //rutas omitidas,rutas poner como foco Activo aunque no tengan actividad y al reves si tienen mucha poder poner que no esten activos
        static LlistaOrdenada<string, string> discosOmitidos;//si se omite se quitan todos los subDirs y files
        static LlistaOrdenada<string, DiscoLogico> discosFocoActivoForzado;//los discos logicos C:// D:// etc estan aqui :D a no ser que esten desactivados por la fuerza...
        static LlistaOrdenada<string, DiscoLogico> discosFocoDesactivadoForzado;
        static LlistaOrdenada<string, DiscoLogico> discosPendientesDeColocar;
        static Llista<DiscoLogico> todosLosDiscos;
        #endregion
        static DiscoLogico()
        {
            NivelMinimoFocoActivo = 100000;//es una variable propiedad juntas...cosas del c# 5
            discosFocoActivoForzado = new LlistaOrdenada<string, DiscoLogico>();
            discosFocoDesactivadoForzado = new LlistaOrdenada<string, DiscoLogico>();
            discosOmitidos = new LlistaOrdenada<string, string>();
            discosPendientesDeColocar = new LlistaOrdenada<string, DiscoLogico>();
            todosLosDiscos = new Llista<DiscoLogico>();
        }
        #region Class Propiedades
        #endregion
        #region Objeto
        DiscoLogico parent;
        LlistaOrdenada<string, FileInfo> totalDirFiles;
        LlistaOrdenada<string, DirectoryInfo> totalSubDirs;
        DirectoryInfo dir;
        string dirFullName;
        Llista<DiscoLogico> subDirs;
        LlistaOrdenada<string, DiscoLogico> subDirsSorted;
        LlistaOrdenada<string, FileInfo> dirFiles;
        DateTime lastAcces;
        DateTime lastWrite;
        DateTime lastUpDateSubDirsList;
        DateTime lastUpDateFilesList;
        private DiscoLogico(DiscoLogico parent,DirectoryInfo dir)//es privado porque asi solo se usa el metodo de clase para obtener un objeto valido :) y me ahorro controlarlos...y evito duplicados :)
        {
            //falta que se autocontrole si esta activo...
            //no quito directorios que ya no existen...o si?? o pongo un mecanismo para quitarlo de la lista con un temporizador....usar DirectorioPerdido :)
            //que este pendiente aparte y que no lo contemple desde aqui.
            this.parent = parent;
            this.dir = dir;
            dirFullName = dir.FullName;
            subDirs = new Llista<DiscoLogico>();
            dirFiles = new LlistaOrdenada<string, FileInfo>();
            subDirsSorted = new LlistaOrdenada<string, DiscoLogico>();
            totalDirFiles = new LlistaOrdenada<string, FileInfo>();
            totalSubDirs = new LlistaOrdenada<string, DirectoryInfo>();
            ActualizaLists();//hacerlo async
        }
        public long Actividad
        {
            get
            {

                long actividad = -1;
                if (dir.Exists)
                {
                    actividad = dir.LastAccessTime.Ticks - lastAcces.Ticks;
                }
                return actividad;
            }
        }
        /// <summary>
        /// Si la ruta existe esta activo
        /// </summary>
        public bool Activo
        {
            get{ return dir.Exists; } 
        }

        public DirectoryInfo Directorio
        { get { return dir; } }

        public static long NivelMinimoFocoActivo { get;  set; }

        public DirectoryInfo[] TotalSubDirs()
        {
            return totalSubDirs.ValuesToArray();
        }
        public FileInfo[] TotalFiles()
        {
            return totalDirFiles.ValuesToArray();
        }
        public void ActualizaLists()
        {
            if (dir.Exists)
            {
                if (lastWrite != dir.LastWriteTime)
                {
                    lastWrite = dir.LastWriteTime;
                    ActualizaArchivosList();
                    ActualizaSubDirsList();

                }
            }
        }
        public void ActualizaSubDirsList()
        {

            const int MAXPROCESS = 5;
            DirectoryInfo[] subDirs;
            DiscoLogico discoAux;

            Llista<Tiket<DiscoLogico>> tiketsHaciendose = new Llista<Tiket<DiscoLogico>>();
            List<Tiket<DiscoLogico>> tiketsPendientes = new List<Tiket<DiscoLogico>>();

            if (dir.Exists && dir.CanRead() && lastUpDateSubDirsList != dir.LastWriteTime)
            {
                lastUpDateSubDirsList = dir.LastWriteTime;
                subDirs = dir.GetDirectories();
                for (int i = 0; i < subDirs.Length; i++)
                {
                    if (!DiscoLogico.discosOmitidos.ContainsKey(subDirs[i].FullName))
                    {
                        if (!this.subDirsSorted.ContainsKey(subDirs[i].FullName))
                        {
                            if (!discosPendientesDeColocar.ContainsKey(subDirs[i].FullName))
                            {
                                //lo aviso como nuevo encontrado
                                if (DirectorioEncontrado != null)
                                {
                                    DirectorioEncontrado(this, new IOArgs(subDirs[i].FullName, true, i, subDirs.Length));
                                }
                                discoAux = new DiscoLogico(this,subDirs[i]);
                            }
                            else
                            {
                                discoAux = discosPendientesDeColocar[subDirs[i].FullName];
                                discoAux.parent = this;
                                discosPendientesDeColocar.Remove(subDirs[i].FullName);
                            }
                            AñadeSubdirAlTotal(subDirs[i]);
                            subDirsSorted.Add(subDirs[i].FullName, discoAux);
                            this.subDirs.Add(discoAux);
                        }
                    }
                }

            }
            for (int i = 0; i < this.subDirs.Count; i++)
            {

                //si es un foco activo se mira solo
                //si deja de ser un foco activo deja de mirarse solo
                //si es un foco omitido se evitara 
                //para hacer la actividad se deja como pendiente en ThreadPool y ya se hara cuando toque...asi no gasta muchos recursos y se controla
               
                if (!discosFocoActivoForzado.ContainsKey(this.subDirs[i].dir.FullName) && !discosFocoDesactivadoForzado.ContainsKey(this.subDirs[i].dir.FullName)) //Si es no es un foco activo lo hago sino lo omito
                {
                    if (this.subDirs[i].Actividad > NivelMinimoFocoActivo)
                        this.subDirs[i].ActivarFocoActivo(); //si esta desactivado se mira si tiene mucha actividad manualmente, si tiene se activa :D
                    else
                     tiketsPendientes.Add(new Tiket<DiscoLogico>((disco) => { disco.subDirs[i].ActualizaSubDirsList(); }, this));
                }
            }
            for (int i = 0; i < tiketsPendientes.Count; i++)
            {
                while (tiketsHaciendose.Count == MAXPROCESS)System.Threading.Thread.Sleep(100);
                tiketsHaciendose.Add(tiketsPendientes[i]);
                tiketsPendientes[i].FaenaHecha += (tiket, args) => { tiketsHaciendose.Remove(tiket); };
                tiketsPendientes[i].HazFaena();
            }
            while (tiketsHaciendose.Count != 0) System.Threading.Thread.Sleep(100);
        }

        private void ActivarFocoActivo()
        {
            throw new NotImplementedException();
        }

        public void ActualizaArchivosList()
        {
            LlistaOrdenada<string, FileInfo> archivos;
            FileInfo[] files;
            if (dir.Exists && dir.CanRead() && lastUpDateFilesList != dir.LastWriteTime)
            {
                lastUpDateFilesList = dir.LastWriteTime;
                archivos = new LlistaOrdenada<string, FileInfo>();
                files = dir.GetFiles();
                for (int i = 0; i < files.Length; i++)
                {

                    if (!this.dirFiles.ContainsKey(files[i].FullName))
                    {
                        //lo aviso como nuevo encontrado
                        if (ArchivoEncontrado != null)
                            ArchivoEncontrado(this, new IOArgs(files[i].FullName, false, i, files.Length));
                        if (parent != null)
                        {
                            AñadeFileAlTotal(files[i]);
                        }

                    }
                    else
                    {
                        this.dirFiles.Remove(files[i].FullName);
                    }
                    archivos.Add(files[i].FullName, files[i]);
                }

                foreach (KeyValuePair<string, FileInfo> filePerdido in dirFiles)
                {
                    //lo aviso
                    if (ArchivoPerdido != null)
                        ArchivoPerdido(this, new IOArgs(filePerdido.Key, false, -1, -1));
                    QuitaFileAlTotal(filePerdido.Value);
                }
                dirFiles = archivos;
                for (int i = 0; i < this.subDirs.Count; i++)
                {
                    this.subDirs[i].ActualizaArchivosList();
                }
            }
        }

        private void AñadeFileAlTotal(FileInfo fileInfo)
        {
            if (parent != null)
            {
                parent.AñadeFileAlTotal(fileInfo);
            }
            totalDirFiles.Add(fileInfo.FullName, fileInfo);
        }
        private void QuitaFileAlTotal(FileInfo fileInfo)
        {
            if (parent != null)
            {
                parent.QuitaFileAlTotal(fileInfo);
            }
            totalDirFiles.Remove(fileInfo.FullName);
        }
        private void AñadeSubdirAlTotal(DirectoryInfo subDir)
        {
            if (parent != null)
            {
                parent.AñadeSubdirAlTotal(subDir);
            }
            totalSubDirs.Add(subDir.FullName, subDir);
        }
        private void QuitaSubdirAlTotal(DirectoryInfo subDir)
        {
            if (parent != null)
            {
                parent.QuitaSubdirAlTotal(subDir);
            }
            totalSubDirs.Remove(subDir.FullName);
        }
        #region CompareTo
        public int CompareTo(object obj)
        {
             return CompareTo(obj as DiscoLogico);
        }

        public int CompareTo(DiscoLogico other)
        {
            int compareTo;
            if (other != null)
            {
                compareTo = CompareTo(other.dirFullName);
            }
            else
            {
                compareTo = -1;
            }
            return compareTo;
        }
        public int CompareTo(string pathOther)
        {
          return dirFullName.CompareTo(pathOther);
        }
        #endregion
        #endregion
        #region Class Metodos
        public static FileInfo[] GetAllFiles()
        {
            Llista<FileInfo> files = new Llista<FileInfo>();
            for (int i = 0; i < todosLosDiscos.Count; i++)
                files.AddRange(todosLosDiscos[i].totalDirFiles.ValuesToArray());
            return files.ToArray();
        }
        public static DirectoryInfo[] GetAllDirs()
        {
            Llista<DirectoryInfo> dirs = new Llista<DirectoryInfo>();
            for (int i = 0; i < todosLosDiscos.Count; i++)
                dirs.AddRange(todosLosDiscos[i].totalSubDirs.ValuesToArray());
            return dirs.ToArray();
        }
        public static DiscoLogico GetDiscoLogico(string path)
        {
            if (!Directory.Exists(path))
                throw new Exception("No se puede encontrar porque no existe el directorio");
            string[] carpetasPath = path.Split(Path.AltDirectorySeparatorChar);
            DiscoLogico discoEncontrado = discosPendientesDeColocar[path], auxParent;
            text pathAux;
            bool dejarDeBuscar;
            if (discoEncontrado == null)
            {
                pathAux = carpetasPath[0];
                auxParent = todosLosDiscos.Search(pathAux);
                dejarDeBuscar = auxParent == null;//si no esta la raiz dejo de intentarlo
                for (int i = 1; discoEncontrado == null && !dejarDeBuscar && carpetasPath.Length > i; i++)
                {

                    if (auxParent.dirFullName == pathAux)
                    {
                        pathAux &= Path.AltDirectorySeparatorChar + carpetasPath[i];
                        auxParent = auxParent.subDirsSorted[pathAux];
                    }
                    else
                    {
                        dejarDeBuscar = true;
                    }
                }

                if (auxParent!=null&&auxParent.dirFullName == path)
                {
                    discoEncontrado = auxParent;
                }

                if (discoEncontrado == null)
                {
                    discosPendientesDeColocar.Add(path, new DiscoLogico(null,new DirectoryInfo(path)));
                    discoEncontrado = discosPendientesDeColocar[path];
                }
            }
            return discoEncontrado;
        }
        #endregion
    }

}
