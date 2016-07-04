//http://en.wikipedia.org/wiki/APNG
//namespace SprinterPublishing original coder
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Text;
using System.Collections;
using System.Drawing;
using System.IO;
using Gabriel.Cat.Extension;
namespace Gabriel.Cat
{
    public class Utils
    {
        /// <summary>
        /// Attempts to read count bytes of data from the supplied stream.
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <param name="count">The number of bytes to read</param>
        /// <returns>A byte[] containing the data or null if an error occurred</returns>
        public static byte[] ReadStream(Stream stream, uint count)
        {
            byte[] bytes = new byte[count];
            try
            {
                stream.Read(bytes, 0, (int)count);
            }
            catch (IOException)
            {
                throw;
            }
            return bytes;
        }

        /// <summary>
        /// Attempts to parse an unsigned integer value from the array of bytes
        /// provided.  The most significant byte of the unsigned integer is
        /// parsed from the first element in the array.
        /// </summary>
        /// <param name="buffer">An array of bytes from which the value is to be extracted</param>
        /// <param name="uintLengthInBytes">The number of bytes to parse (must be <= sizeof(uint))</param>
        /// <returns>The extracted unsigned integer returned in a uint</returns>
        public static uint ParseUint(byte[] buffer, int uintLengthInBytes)
        {
            int offset = 0;
            return ParseUint(buffer, uintLengthInBytes, ref offset);
        }

        /// <summary>
        /// Attempts to parse an unsigned integer value from the array of bytes
        /// provided.  The most significant byte of the unsigned integer is
        /// parsed from the specified offset in the array.
        /// </summary>
        /// <param name="buffer">An array of bytes from which the value is to be extracted</param>
        /// <param name="uintLengthInBytes">The number of bytes to parse (must be <= sizeof(uint))</param>
        /// <param name="offset">The offset in the array of bytes where parsing shall begin</param>
        /// <returns>The extracted unsigned integer returned in a uint</returns>
        public static uint ParseUint(byte[] buffer, int uintLengthInBytes, ref int offset)
        {
            uint value = 0;
            if (uintLengthInBytes > sizeof(uint))
                throw new ArgumentException(
                    String.Format("Function can only be used to parse up to {0} bytes from the buffer",
                        sizeof(uint)));
            if (buffer.Length - offset < uintLengthInBytes)
                throw new ArgumentException(
                    String.Format("buffer is not long enough to extract {0} bytes at offset {1}",
                        sizeof(uint), offset));
            int i, j;
            for (i = offset + uintLengthInBytes - 1, j = 0; i >= offset; i--, j++)
                value |= (uint)(buffer[i] << (8 * j));
            offset += uintLengthInBytes;
            return value;
        }
    }

    public class APNG : IEnumerable<PNG>
    {
        /// <summary>
        /// List of chunks in the APNG
        /// </summary>
        List<Chunk> chunks;
        List<Chunk> metadata;//se puede poner metadata :D
                             /// <summary>
                             /// List of PNGs embedded in the APNG
                             /// </summary>
        List<PNG> pngs;
        /// <summary>
        /// The APNG's MHDRChunk
        /// </summary>
        IENDChunk iendChunk;

        public APNG()
        {
            SaltarPrimerFotograma = false;
            pngs = new List<PNG>();
            chunks = new List<Chunk>();
            NumeroDeRepeticiones = 0;
            metadata = new List<Chunk>();
        }
        public APNG(string pathApng)
            : this()
        {
            Load(pathApng);
        }
        public APNG(IEnumerable<Bitmap> frames)
            : this()
        {
            Add(frames);
        }
        public APNG(IEnumerable<PNG> frames)
            : this()
        {
            Add(frames);
        }
        /// <summary>
        /// Gets the number of embedded PNGs within the APNG
        /// </summary>
        public int Count
        {
            get { return pngs.Count; }
        }

        public uint NumeroDeRepeticiones
        {
            get;
            set;
        }
        public bool SaltarPrimerFotograma
        {
            get;
            set;
        }
        public acTLChunk HeaderChunk
        {
            get;
            private set;
        }

        public IHDRChunk IhdrChunk
        {
            get;
            private set;
        }

        public IENDChunk IendChunk {
            get {
                if (iendChunk == null)
                    iendChunk = new IENDChunk();
                return iendChunk;
                    }
            private set { iendChunk = value; }
        }
        public Chunk[] Chunks
        {
            get
            {
                return chunks.ToTaula();
            }
        }
        public PNG[] PNGS
        {
            get { return pngs.ToTaula(); }
        }
        public Bitmap this[int Index]
        {
            get
            {
                return ToBitmap(Index);
            }
        }
        #region add/remove frames
        public void Add(Bitmap fragment, int index)
        {
            Add(new PNG(fragment), index);
        }
        public void Add(PNG png, int index)
        {
            pngs.Insert(index, png);
        }
        public void Add(Bitmap bmp)
        {
            Add(new PNG(bmp));
        }
        public void Add(IEnumerable<Bitmap> bmps)
        {
            foreach (Bitmap bmp in bmps)
                Add(bmp);
        }
        public void Add(PNG png)
        {
            if (IhdrChunk == null)
            {
                IhdrChunk = png.IHDR;
                IendChunk = png.IEND;
            }
            if (png.IHDR.Equals(IhdrChunk))
                pngs.Add(png);
            else
                throw new Exception("la imagen no tiene el ihdr igual");
            pngs.Ordena();
        }
        public void Add(IEnumerable<PNG> pngs)
        {
            foreach (PNG png in pngs)
                Add(png);
        }
        public void Remove(int index)
        {
            pngs.RemoveAt(index);
        }
        public void Remove(PNG png)
        {
            pngs.Remove(png);

        }
        public void Remove(Bitmap bmp)
        {
            string hashAQuitar = bmp.GetBytes().Hash();
            PNG pngAQuitar = null;
            pngs.WhileEach((pngAComprar) =>
            {

                bool quitar = pngAComprar.ToBitmap().GetBytes().HashEquals(hashAQuitar);
                if (quitar)
                    pngAQuitar = pngAComprar;
                return quitar;

            });
            Remove(pngAQuitar);
        }
        #endregion

        /// <summary>
        /// Creates a Bitmap object containing the embedded PNG at the specified
        /// index in the APNG's list of embedded PNGs
        /// </summary>
        /// <param name="index">The embedded PNG index</param>
        /// <returns>Bitmap</returns>
        public Bitmap ToBitmap(int index)
        {
            // Verify the index
            if (index > Count || index < 0)
                throw new IndexOutOfRangeException();
            
            // Create the bitmap
            return pngs[index].ToBitmap();
        }
        public Bitmap[] ToBitmapArray()
        {
            List<Bitmap> bmps = new List<Bitmap>();
            for (int i = 0; i < Count; i++)
                bmps.Add(ToBitmap(i));
            return bmps.ToArray();
        }
        public Chunk[] GetChunks()
        {
            Llista<Chunk> chunks = new Llista<Chunk>();
            Chunk chunkIDAT = null;
            uint orden = 3;//no se porque va de 3 en 3....
            if (pngs.Count > 0)
            {
                //	pngs.Ordena();
                chunks.Afegir(IhdrChunk);//IHDR
                chunks.Afegir(new acTLChunk(NumeroDeRepeticiones, (uint)pngs.Count));//acTL

                               //pongo los pngs
                               //fcTL
                if (!SaltarPrimerFotograma)
                {
                    pngs[0].FCTL.SequenceNumber = orden; orden += 3;
                    chunks.Afegir(pngs[0].FCTL);
                }
                //Idata
                for (int j = 0; j < pngs[0].IDATS.Count; j++)
                {
                    pngs[0].IDATS[j].ChunkType = IDATChunk.NAME;//el primero es IDAT
                    chunks.Afegir(pngs[0].IDATS[j]);
                }
                for (int i = 1; i < pngs.Count; i++)
                {
                    //fcTL

                    pngs[i].FCTL.SequenceNumber = orden; orden += 3;
                    chunks.Afegir(pngs[i].FCTL);
                    //Idata
                    for (int j = 0; j < pngs[i].IDATS.Count; j++)
                    {
                        chunkIDAT = pngs[i].IDATS[j].Clon();
                        chunkIDAT.ChunkType = fdATChunk.NAME;// los demás fdAT
                        chunks.Afegir(chunkIDAT);
                    }

                }
                chunks.AfegirMolts(metadata);
                chunks.Afegir(IendChunk);
            }
            return chunks.ToTaula();
        }
        /// <summary>
        /// Save in apng format
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>true si esta guardado</returns>
        public bool SaveFile(string fileName)
        {
            bool guardado = false;
            if (pngs.Count > 0)
            {
                //guarda en formato apng
                FileStream fs = new FileStream(fileName, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                Chunk[] chunks = GetChunks();
                bw.Write(PNG.Header);//lo primero es la firma del formato png
                for (int i = 0, maxLength = chunks.Length; i < maxLength; i++)
                {
                    bw.Write(chunks[i].ChunkBytes);
                }

                bw.Close();
                fs.Close();
                guardado = true;
            }
            return guardado;

        }

        void AñadirChunk(Chunk chunk)
        {
            //controlo que esten bien puestos y todo eso...
            switch (chunk.ChunkType)
            {
                case IHDRChunk.NAME:
                    if (IhdrChunk != null)
                        throw new Exception("Solo puede haber uno por archivo");
                    IhdrChunk = new IHDRChunk(chunk);
                    break;
                case acTLChunk.NAME:
                    if (HeaderChunk != null)
                        throw new Exception("Solo puede haber uno por archivo");
                    HeaderChunk = new acTLChunk(chunk);
                    break;
                case fcTLChunk.NAME:
                    chunks.Add(new fcTLChunk(chunk));
                    break;
                case IDATChunk.NAME:
                    chunks.Add(chunk);
                    break;
                case IENDChunk.NAME:
                    if (IendChunk != null)
                        throw new Exception("Solo puede haber uno por archivo");
                    IendChunk = new IENDChunk(chunk);
                    break;
                case tEXtChunk.NAME:
                    metadata.Add(new tEXtChunk(chunk)); break;
                default:
                    chunks.Add(chunk);//pos si hay de extras no contemplados...
                    break;
            }
        }
        /// <summary>
        /// Hace un reset de la clase eliminando los chunks y los pngs
        /// </summary>
        public void ResetChuks()
        {
            chunks.Clear();
            pngs.Clear();
            HeaderChunk = null;
            IhdrChunk = null;
            IendChunk = null;
            metadata.Clear();
        }
        /// <summary>
        /// Attempts to load an APNG from the specified file name
        /// </summary>
        /// <param name="filename">Name of the APNG file to load</param>
        public void Load(string filename)
        {

            // Open the file for reading
            Stream stream = File.OpenRead(filename);
            Chunk chunk;
            PNG png = null;
            byte[] firmaAComprobar = null;
            ResetChuks();
            //leo la firma png
            try
            {
                firmaAComprobar = stream.Read(PNG.Header.Length);
                for (int i = 0; i < firmaAComprobar.Length; i++)
                    if (firmaAComprobar[i] != PNG.Header[i])
                        throw new Exception("El archivo no tiene la firma APNG correcta!");

                //leo el IHDR
                chunk = Chunk.ReadChunk(stream);

                if (chunk.ChunkType != IHDRChunk.NAME)
                    throw new ApngFormatException(chunk, IHDRChunk.NAME);
                AñadirChunk(chunk);
                //leo el acTL
                chunk = Chunk.ReadChunk(stream);

                if (chunk.ChunkType != acTLChunk.NAME)
                    throw new ApngFormatException(chunk, acTLChunk.NAME);

                AñadirChunk(chunk);
                //leo los PNGS

                png = new PNG();
                chunk = Chunk.ReadChunk(stream);
                //compruebo que sea los chunks que toca
                if (chunk.ChunkType == IDATChunk.NAME)
                {
                    png.IHDR = IhdrChunk;
                    //	png.FCTL.SequenceNumber=0;
                    png.Add(chunk);
                    SaltarPrimerFotograma = true;
                }
                else if (chunk.ChunkType != fcTLChunk.NAME)//si se salta el primer fotograma este no tiene fcTL
                    throw new ApngFormatException(chunk, fcTLChunk.NAME);
                else
                {
                    png.FCTL = new fcTLChunk(chunk);
                }
                AñadirChunk(chunk);
                do
                {

                    //leo los IDATs
                    do
                    {
                        chunk = Chunk.ReadChunk(stream);
                        if (chunk.ChunkType == fdATChunk.NAME || chunk.ChunkType == IDATChunk.NAME)
                        {
                            chunk.ChunkType = IDATChunk.NAME;
                            png.Add(chunk);
                            AñadirChunk(chunk);
                        }
                        else if (chunk.ChunkType != fcTLChunk.NAME && chunk.ChunkType != IENDChunk.NAME)//recojo chunks no contemplados...
                            AñadirChunk(chunk);
                    } while (chunk.ChunkType != fcTLChunk.NAME && chunk.ChunkType != IENDChunk.NAME);
                    pngs.Add(png);
                    if (chunk.ChunkType == fcTLChunk.NAME)
                    {
                        png = new PNG();
                        png.FCTL = new fcTLChunk(chunk);
                        AñadirChunk(chunk);
                    }
                    if (HeaderChunk.NumeroDeFotogramas < pngs.Count)
                        throw new ApngFormatException();

                } while (chunk.ChunkType != IENDChunk.NAME);
                AñadirChunk(chunk);
                //y pongo los ends y los ihdrs a los pngs
                for (int i = 0; i < pngs.Count; i++)
                {
                    pngs[i].IEND = IendChunk;
                    pngs[i].IHDR = IhdrChunk;
                }
                pngs.Ordena();
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                stream.Close();
            }
        }

        /// <summary>
        /// Creates a string containing the names of all the chunks in the APNG
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0, chunksCount = chunks.Count; i < chunksCount; i++)
            {
                sb.AppendLine(chunks[i].ChunkType);
            }
            return sb.ToString();
        }
        #region IEnumerable implementation

        public IEnumerator<PNG> GetEnumerator()
        {
            return pngs.GetEnumerator();
        }

        #endregion

        #region IEnumerable implementation

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
    public class ApngFormatException : Exception
    {
        public Chunk Chunk { get; private set; }
        public ApngFormatException(Chunk chunk, string chunkEsperado) : base("Error de formato se encuentra " + chunk.ChunkType + " y se esperaba " + chunkEsperado)
        { Chunk = chunk; }

        public ApngFormatException():base("Error en el formato del archivo")
        {
        }
    }
}
