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
	public class PNG : IEnumerable<Chunk>,IComparable<PNG>
	{
		public enum Metadata
		{
			Title,
			//Short (one line) title or caption for image
			Author,
			//Name of image's creator
			Description,
			//Description of image (possibly long)
			Copyright,
			//Copyright notice
			CreationTime,
			//Time of original image creation
			Software,
			//Software used to create the image
			Disclaimer,
			//Legal disclaimer
			Warning,
			//Warning of nature of content
			Source,
			//Device used to create the image
			Comment
			//Miscellaneous comment; conversion from GIF comment
				
		}
		/// <summary>
		/// The PNG file signature
		/// </summary>
		public static byte[] Header = new byte[] {
			0x89,
			0x50,
			0x4E,
			0x47,
			0x0D,
			0x0A,
			0x1A,
			0x0A
		};

		static string[] chunksCeroOIlimitados = {
			"sPLT",
			"iTXt",
			"tEXt",
			"zTXt"
		};

		static string[] chunksSinPosicion = {
			"tIME",
			"iTxt",
			"tEXt",
			"zTXt"
		};

		static string[] chunksBeforePLTE = {
			"cHRM",
			"iCCP",
			"sRGB",
			"gAMA",
			"sBIT"
		};

		static string[] chunksAfterPLTE = {
			"bKGD",
			"tRNS",
			"hIST"
		};

		static string[] chunksAfterIDAT = {
			"pHYs",
			"sPLT"
		};
		public static bool SoftwareMetadata = true;

		/// <summary>
		/// The PNG file's IHDR chunk
		/// </summary>
		private IHDRChunk ihdr;

		/// <summary>
		/// The PNG file's PLTE chunk (optional)
		/// </summary>
		private Llista<Chunk> extras;

		private Chunk iCCP;
		private fcTLChunk fctl;
//para usarlo en animaciones apng
		private Chunk sRGB;

		private tIMEChunk tIME;

		private Chunk plte;

		/// <summary>
		/// The PNG file's IDAT chunks
		/// </summary>
		private List<Chunk> idats;

		/// <summary>
		/// The PNG file's IEND chunk
		/// </summary>
		private IENDChunk iend;
		
		private LlistaOrdenada<string,Chunk> metadata;
		/// <summary>
		/// Default constructor
		/// </summary>
		public PNG()
		{
			idats = new List<Chunk>();
			extras = new Llista<Chunk>();
			metadata = new LlistaOrdenada<string, Chunk>();
		}

		public PNG(string path)
			: this()
		{
			LoadFile(path);
		}

		public PNG(Bitmap bmp)
			: this()
		{
			Load(bmp);
		}

		public PNG(Stream fStreamPng)
		{
			Load(fStreamPng);
		}

		#region Propietats
		/// <summary>
		/// Gets or Sets the PNG's IHDR chunk
		/// </summary>
		public IHDRChunk IHDR {
			get {
				return ihdr;
			}
			set {
				ihdr = value;
			}
		}

		public Chunk PLTE {
			get {
				return plte;
			}
			set {
				plte = value;
			}
		}

		public fcTLChunk FCTL {
			get {
				if (fctl == null)
					fctl = new fcTLChunk(this);
				return fctl;
			}
			set {
				fctl = value;
			}
		}
		/// <summary>
		/// Gets or Sets the PNG's IEND chunk
		/// </summary>
		public IENDChunk IEND {
			get {
				return iend;
			}
			set {
				iend = value;
			}
		}

		/// <summary>
		/// Gets the list of IDAT chunk's making up the PNG
		/// </summary>
		public List<Chunk> IDATS {
			get {
				return idats;
			}
		}

		public void Add(Chunk chunk)
		{
			Add(chunk, false);
		}

		public void Add(Chunk chunk, bool reemplazarOExcepcion)
		{
			Chunk chunkABuscar = null;
			switch (chunk.ChunkType) {
				case IDATChunk.NAME:
					idats.Add(chunk);
					break;
				case "iCCP":
					if (sRGB != null)
					if (reemplazarOExcepcion) {
						sRGB = null;
					} else
						throw new Exception("Ya existe un chunk sRGB por lo que no puede ir un iCCP");
					if (iCCP != null)
					if (!reemplazarOExcepcion)
						throw new Exception("Chunk " + chunk.ChunkType + " repetido...solo puede haber uno");
					iCCP = chunk;
					break;
				case "sRGB":
					if (iCCP != null)
					if (reemplazarOExcepcion) {
						iCCP = null;
					} else
						throw new Exception("Ya existe un chunk iCCP por lo que no puede ir un sRGB");
					if (sRGB != null)
					if (!reemplazarOExcepcion)
						throw new Exception("Chunk " + chunk.ChunkType + " repetido...solo puede haber uno");
					sRGB = chunk;
					break;
				case tIMEChunk.NAME:
					if (tIME != null && !reemplazarOExcepcion)
						throw new Exception("Solo puede haber un chunk tIME!!");
					tIME = new tIMEChunk(chunk);
					break;
				case "PLTE":
					if (PLTE != null && !reemplazarOExcepcion)
						throw new Exception("Solo puede haber un chunk PLTE!!");
					PLTE = chunk;
					break;
				case tEXtChunk.NAME:
					tEXtChunk tEXt = new tEXtChunk(chunk);
					metadata.Afegir(tEXt.Keyword, tEXt);
					break;
				case zTXtChunk.NAME:
					zTXtChunk zTXt = new zTXtChunk(chunk);
					metadata.Afegir(zTXt.Keyword, zTXt);
					break;
				default:
					if (!chunksCeroOIlimitados.Existe(chunk.ChunkType)) {
						//miro si existe
						extras.WhileEach(chunkToCompare => {
							if (chunkToCompare.Equals(chunk))
								chunkABuscar = chunk;
							return chunkABuscar == null;
						});
						//si existe lo trato
						if (chunkABuscar != null) {
							if (reemplazarOExcepcion)
								extras.Elimina(chunkABuscar);
							else
								throw new Exception("Solo puede haber uno de " + chunkABuscar);
						}
					}
					extras.Afegir(chunk);
					break;
			}
		}

		#endregion
		/// <summary>
		/// Converts the chunks making up the PNG into a single MemoryStream which
		/// is suitable for writing to a PNG file or creating a Image object using
		/// Bitmap.FromStream
		/// </summary>
		/// <returns>MemoryStream</returns>
		public MemoryStream ToStream()
		{
			MemoryStream ms = new MemoryStream();
			ms.Write(Header);
			//la firma PNG
			foreach (Chunk chunk in this)
				ms.Write(chunk.ChunkBytes);
			return ms;
		}

		public Bitmap ToBitmap()
		{
			Stream st = ToStream();
			Bitmap bmp = new Bitmap(st);
			st.Close();
			return bmp;
		}

		public void SaveFile(string fileName)
		{
			if (File.Exists(fileName))
				File.Delete(fileName);
			FileStream writer = new FileStream(fileName, FileMode.Create);
			BinaryWriter bw = new BinaryWriter(writer);
			Stream st = ToStream();
			bw.Write(st.GetAllBytes());
			bw.Close();
			writer.Close();
			st.Close();
		}

		public Chunk[] Extras {
			get {
				return extras.ToTaula();
			}
		}

		public Chunk Extra(string idExtra)
		{
			Chunk chunk = null;
			extras.WhileEach(extra => {
				if (extra.ChunkType == idExtra)
					chunk = extra;
				return chunk == null;
			});
			return chunk;
		}
		public void AddMetadata(Metadata metadata, string informacion)
		{
			if (metadata != Metadata.CreationTime)
				AddMetadata(metadata.ToString(), informacion);
			else
				AddMetadata("Creation Time", informacion);
		}
		public void AddMetadata(string metadata, string information)
		{
			this.metadata.Afegir(metadata, new tEXtChunk(metadata, information));
			
		}
		public void UpdateTIMELastModification()
		{
			UpdateTIMELastModification(DateTime.Now.ToUniversalTime());
		}

		public void UpdateTIMELastModification(DateTime fecha)
		{
			if (tIME != null)
				tIME.Fecha = fecha;
			else
				tIME = new tIMEChunk(fecha);
		}
		public DateTime LastModification()
		{
			if (tIME != null) {
				return tIME.Fecha;
			} else
				return default(DateTime);
		}

		public void LoadFile(string path)
		{
			Load(new Bitmap(path));
		}

		public void Load(Bitmap bmp)
		{
			Stream streamBmp = bmp.ToStream(ImageFormat.Png);
			Load(streamBmp);
			streamBmp.Close();
		}
		public void Load(string pathPNG)
		{
			Stream sr = new FileStream(pathPNG, FileMode.Open);
			Load(sr);
			sr.Close();
		}
		public void Load(Stream streamFilePNG)
		{
			//cargo los datos del archivo
			//firma,IHDR,IDAT,IEND
			byte[] firmaAComprobar = null;
			Chunk chunk;
			firmaAComprobar = streamFilePNG.Read(PNG.Header.Length);
			for (int i = 0; i < firmaAComprobar.Length; i++)
				if (firmaAComprobar[i] != PNG.Header[i])
					throw new Exception("El archivo no tiene la firma APNG correcta!");
			IHDR = new IHDRChunk(Chunk.ReadChunk(streamFilePNG));
			do {
				chunk = Chunk.ReadChunk(streamFilePNG);
				if (chunk.ChunkType != IENDChunk.NAME)
					Add(chunk);
			} while (chunk.ChunkType != IENDChunk.NAME);
			IEND = new IENDChunk(chunk);
		}

		#region IEnumerable implementation
		public IEnumerator<Chunk> GetEnumerator()
		{
			List<Chunk> chunksPorOrden = new List<Chunk>();
			chunksPorOrden.Add(IHDR);
			//pongo los que van antes que PLTE
			chunksPorOrden.AddRange(extras.Filtra(chunk => {
				return chunksAfterPLTE.Existe(chunk.ChunkType);
			}));
			if (iCCP != null)
				chunksPorOrden.Add(iCCP);
			else if (sRGB != null)
				chunksPorOrden.Add(sRGB);
			//pongo si esta PLTE
			if (PLTE != null)
				chunksPorOrden.Add(PLTE);
			//pongo los que van despues de PLTE
			chunksPorOrden.AddRange(extras.Filtra(chunk => {
				return chunksBeforePLTE.Existe(chunk.ChunkType);
			}));
			chunksPorOrden.AddRange(idats);
			//pongo los que van despues de IDAT
			chunksPorOrden.AddRange(extras.Filtra(chunk => {
				return chunksAfterIDAT.Existe(chunk.ChunkType);
			}));
			//pongo los que no tienen orden
			chunksPorOrden.AddRange(extras.Filtra(chunk => {
				return chunksSinPosicion.Existe(chunk.ChunkType);
			}));
			if (tIME == null)
				UpdateTIMELastModification();
			if (SoftwareMetadata) {
				metadata.AfegirORemplaçar(Metadata.Software.ToString(), new tEXtChunk(Metadata.Software.ToString(), "Gabriel.Cat.PNG"));
			}
			chunksPorOrden.Add(tIME);
			chunksPorOrden.AddRange(metadata.ValuesToArray());
			chunksPorOrden.Add(IEND);
			return chunksPorOrden.GetEnumerator();
		}

		#endregion
		#region IEnumerable implementation
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region IComparable implementation
		public int CompareTo(PNG other)
		{
			return FCTL.SequenceNumber.CompareTo(other.FCTL.SequenceNumber);
		}
		#endregion
	}
}


