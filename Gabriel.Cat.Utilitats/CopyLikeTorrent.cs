using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gabriel.Cat.Extension;

namespace Gabriel.Cat
{
    //no esta acabada
   internal class CopyLikeTorrent
    {
        //version de clase y de objeto
        /// <summary>
        /// default size is 2MB
        /// </summary>  
        public const int BUFFERDEFAULT = 2 * 1024 * 1024;
        public static event EventHandler<FileCopyPorcessEventArgs> GlobalProgress;
        int buffer;
        string destination;
        string source;
        public event EventHandler<FileCopyPorcessEventArgs> FileProgress;
        public bool HandleGlobalProgress = true;
        public CopyLikeTorrent(string source,string destination,int buffer=BUFFERDEFAULT)
        {
            Destination = destination;
            Source = source;
            Buffer = buffer;
        }

        public int Buffer
        {
            get
            {
                return buffer;
            }

            set
            {
                if (value <= 0) value = BUFFERDEFAULT;
                buffer = value;
            }
        }

        public string Destination
        {
            get
            {
                return destination;
            }

            set
            {
                if (value == null) throw new ArgumentNullException();
                destination = value;
            }
        }

        public string Source
        {
            get
            {
                return source;
            }

            set
            {
                if (value == null) throw new ArgumentNullException();
                source = value;
            }
        }

        public async Task<bool> Copy()
        {
            bool endCopySuccessful = false;
            //copy the file :D
            FileCopyPorcessEventArgs fileProgress=null;
            FileStream fsIn;
            FileStream fsOut;
            BinaryReader brIn;
            BinaryWriter brOut;
            //si existe continua por donde lo dejó :D
            fsIn = new FileStream(Source, FileMode.Open, FileAccess.Read, FileShare.Read, buffer);
            brIn = new BinaryReader(fsIn);
            fsOut = new FileStream(Destination, FileMode.Append, FileAccess.Write, FileShare.Read, buffer);
            brOut = new BinaryWriter(fsOut);
            brIn.BaseStream.Position = brOut.BaseStream.Position;//posiciono la fuente
            endCopySuccessful = fsIn.Position == fsOut.Position;
            try
            {
                do
                {
                    if (!endCopySuccessful)
                    {
                        brOut.Write(brIn.ReadBytes(buffer));//escribo los bytes que tocan
                        endCopySuccessful = fsIn.Position == fsOut.Position;
                        brOut.Flush();//escribe los datos en la stream base
                        fsOut.Flush();//escribe los datos en el archivo
                        if (FileProgress != null || HandleGlobalProgress && GlobalProgress != null)
                        {
                            fileProgress = new FileCopyPorcessEventArgs(this, brOut.BaseStream.Position, brIn.BaseStream.Length);
                            if (HandleGlobalProgress && GlobalProgress != null)
                                GlobalProgress(this, fileProgress);
                            if (FileProgress != null)
                                FileProgress(this, fileProgress);
                        }
                    }

                } while (!endCopySuccessful && (!fsIn.EndOfStream() || !fsOut.CanWrite)&&!fileProgress.Cancel);//mirar si origina excepcion si no existe...
            }
            finally
            {
                brIn.Close();
                brOut.Close();
            }
            if (fileProgress != null && fileProgress.Cancel&&File.Exists(Destination))
                File.Delete(Destination);
            return endCopySuccessful;
        }
        public bool CopySync()
        {
          
           return Copy().Result;
        }

        public static async Task<bool> Copy(string source,string destination,int buffer=BUFFERDEFAULT)
        {
            return new CopyLikeTorrent(source, destination, buffer).CopySync();
        }
        public static  bool CopySync(string source, string destination, int buffer = BUFFERDEFAULT)
        {
            return Copy(source,destination,buffer).Result;
        }
    }
    internal class FileCopyPorcessEventArgs:EventArgs
    {
        public FileCopyPorcessEventArgs(CopyLikeTorrent file, long complete, long total)
        {
            File = file;
            Complete = complete;
            Total = total;
            Cancel = false;
        }

        public CopyLikeTorrent File { get; private set; }
        public long Complete { get; private set; }
        public long Total { get; private set; }
        public double Progress { get { return Complete / (Total*1.0); } }
        public bool Cancel { get; set; }
    }
}
