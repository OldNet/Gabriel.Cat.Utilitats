/*
 * Creado por SharpDevelop.
 * Usuario: Gabriel
 * Fecha: 19/07/2015
 * Hora: 18:47
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.IO;
using System.Text;
using Gabriel.Cat.Extension;
namespace Gabriel.Cat
{
	/// <summary>
	/// Description of Chunks.
	/// </summary>
	public class Chunk:IClonable
	{
		class CRC
		{
			private static uint[] crcTable;

			public const uint INITIAL_CRC = 0xFFFFFFFF;

			private static void MakeCRCTable()
			{
				crcTable = new uint[256];
				uint c, n, k;

				for (n = 0; n < crcTable.Length; n++) {
					c = n;
					for (k = 0; k < 8; k++) {
						if ((c & 1) != 0) {
							c = 0xedb88320 ^ (c >> 1);
						} else {
							c = c >> 1;
						}
					}
					crcTable[n] = c;
				}
			}

			public static uint UpdateCRC(uint crc, byte[] bytes)
			{
				uint c = crc;
				uint n;
				if (crcTable == null)
					MakeCRCTable();
				for (n = 0; n < bytes.Length; n++) {
					c = crcTable[(c ^ bytes[n]) & 0xff] ^ (c >> 8);
				}
				return c;


			}

			public static uint Calculate(byte[] bytes)
			{
				return UpdateCRC(INITIAL_CRC, bytes);
			}
		}
		protected String error;
		protected byte[] chunkLength;
		protected byte[] chunkType;
		protected byte[] chunkData;
		protected byte[] chunkCRC;
		protected uint calculatedCRC;

		/// <summary>
		/// Default constructor
		/// </summary>
		public Chunk()
		{
			chunkLength = chunkType = chunkData = chunkCRC = null;
			error = "No Error";
		}
		public Chunk(string chunkType, byte[] chunkData)
		{
			this.ChunkType = chunkType;
			this.ChunkData = chunkData;
		}
		/// <summary>
		/// Constructor which takes an existing APNGChunk object and
		/// verifies that its type matches that which is expected
		/// </summary>
		/// <param name="chunk">The APNGChunk to copy</param>
		/// <param name="expectedType">The input APNGChunk expected type</param>
		public Chunk(Chunk chunk, String expectedType)
		{
			// Copy the existing chunks members
			chunkLength = chunk.chunkLength;
			chunkType = chunk.chunkType;
			chunkData = chunk.chunkData;
			chunkCRC = chunk.chunkCRC;

			// Verify the chunk type is as expected
			if (ChunkType != expectedType)
				throw new ArgumentException(
					String.Format("Specified chunk type is not {0} as expected", expectedType));

			// Parse the chunk's data
			ParseData(chunkData);
		}

		/// <summary>
		/// Extracts various fields specific to this chunk from the APNG's
		/// data field
		/// </summary>
		/// <param name="chunkData">An array of bytes representing the APNG's data field</param>
		protected virtual void ParseData(byte[] chunkData)
		{
			// Nothing specific to do here.  Derived classes can override this
			// to do specific field parsing.
		}

		/// <summary>
		/// Gets the array of bytes which make up the APNG chunk.  This includes:
		/// o 4 bytes of the chunk's length
		/// o 4 bytes of the chunk's type
		/// o N bytes of the chunk's data
		/// o 4 bytes of the chunk's CRC
		/// </summary>
		public byte[] ChunkBytes {
			get {
				byte[] ba = new byte[chunkLength.Length +
				                     chunkType.Length + chunkData.Length +
				                     chunkCRC.Length];
				chunkLength.CopyTo(ba, 0);
				chunkType.CopyTo(ba, chunkLength.Length);
				chunkData.CopyTo(ba, chunkLength.Length + chunkType.Length);
				chunkCRC.CopyTo(ba, chunkLength.Length + chunkType.Length + chunkData.Length);
				return ba;
			}
		}

		/// <summary>
		/// Gets the array of bytes which make up the chunk's data field
		/// </summary>
		public byte[] ChunkData {
			get {
				return chunkData;
			}
			protected set {
				chunkData = value;
				chunkLength = Serializar.GetBytes(chunkData.Length);
				Array.Reverse(chunkLength);
				if (chunkType != null) {
					UpdateCRC();
				}
			}
		}

		void UpdateCRC()
		{
			uint crc = CRC.INITIAL_CRC;
			byte[] array;
			crc = CRC.UpdateCRC(crc, chunkType);
			crc = CRC.UpdateCRC(crc, chunkData);
			// CRC is inverted
			crc = ~crc;
			array = BitConverter.GetBytes(crc);
			Array.Reverse(array);
			chunkCRC = array;
		}
		/// <summary>
		/// Gets chunk's type field as an string
		/// </summary>
		public String ChunkType {
			get {
				return new String(new char[] {
				                  	(char)chunkType[0],
				                  	(char)chunkType[1],
				                  	(char)chunkType[2],
				                  	(char)chunkType[3]
				                  });
			}
			set {
				chunkType = Encoding.ASCII.GetBytes(value);
				if (chunkData != null)
					UpdateCRC();
			}
		}

		/// <summary>
		/// Gets the length field of the chunk
		/// </summary>
		public uint ChunkLength {
			get {
				byte[] bytesLength = chunkLength.Clone() as byte[];
				Array.Reverse(bytesLength);
				return Serializar.ToUInt(bytesLength);
			}
		}

		/// <summary>
		/// Gets the CRC field of the chunk
		/// </summary>
		public uint ChunkCRC {
			get {
				return Utils.ParseUint(chunkCRC, chunkCRC.Length);
			}
		}

		/// <summary>
		/// Attempts to parse an APNGChunk for the specified stream
		/// </summary>
		/// <param name="stream">The stream containing the APNG Chunk</param>
		public void Read(Stream stream)
		{
			if (!stream.CanRead)
				throw new ArgumentException("Stream is not readable");
			long chunkStart = stream.Position;
			
			calculatedCRC = CRC.INITIAL_CRC;

			// Read the data Length
			chunkLength = Utils.ReadStream(stream, 4);

			// Read the chunk type
			chunkType = Utils.ReadStream(stream, 4);
			calculatedCRC = CRC.UpdateCRC(calculatedCRC, chunkType);

			// Read the data
			chunkData = Utils.ReadStream(stream, ChunkLength);
			calculatedCRC = CRC.UpdateCRC(calculatedCRC, chunkData);

			// Read the CRC
			chunkCRC = Utils.ReadStream(stream, 4);

			// CRC is inverted
			calculatedCRC = ~calculatedCRC;

			// Verify the CRC
			if (ChunkCRC != calculatedCRC) {
				StringBuilder sb = new StringBuilder();
				sb.AppendLine(String.Format("APNG Chunk CRC Mismatch.  Chunk CRC = {0}, Calculated CRC = {1}.",
				                            ChunkCRC, calculatedCRC));
				sb.AppendLine(String.Format("This occurred while parsing the chunk at position {0} (0x{1:X8}) in the stream.",
				                            chunkStart, chunkStart));
				throw new ApplicationException(sb.ToString());
			}
		}
		public override string ToString()
		{
			return ChunkType;
		}
		public override bool Equals(object obj)
		{
			Chunk other = obj as Chunk;
			bool iguales = other != null;
			if (other != null) {
				byte[] data = ChunkBytes;
				byte[] dataOther = other.ChunkBytes;
				if (data.Length != dataOther.Length)
					iguales = false;
				for (int i = 0; i < data.Length && iguales; i++)
					if (data[i] != dataOther[i])
						iguales = false;
			}
			
			return iguales;
		}
		public	static Chunk ReadChunk(Stream stream)
		{
			return ReadChunk(stream, false);
		}
		public	static Chunk ReadChunk(Stream stream, bool closeStreamIfThrowException)
		{
			Chunk chunk;
			chunk = new Chunk();
			try {
				chunk.Read(stream);
			} catch (Exception) {
				if (closeStreamIfThrowException)
					stream.Close();
				throw;
			}
			return chunk;
		}

		#region IClonable implementation

		public dynamic Clon()
		{
			return new Chunk() {
				chunkCRC = chunkCRC.Clone() as byte[],
				chunkData = chunkData.Clone() as byte[],
				chunkLength = chunkLength.Clone() as byte[],
				chunkType = chunkType.Clone() as byte[]
			};
		}

		#endregion
	}
	
	public class MENDChunk : Chunk
	{
		/// <summary>
		/// The ASCII name of the APNG chunk
		/// </summary>
		public const String NAME = "MEND";

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="chunk">The APNG chunk containing the data for this specific chunk</param>
		public MENDChunk(Chunk chunk)
			: base(chunk, NAME)
		{
		}
	}

	public class TERMChunk : Chunk
	{
		/// <summary>
		/// The ASCII name of the APNG chunk
		/// </summary>
		public const String NAME = "TERM";

		private uint terminationAction;
		private uint actionAfterTermination;
		private uint delay;
		private uint iterationMax;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="chunk">The APNG chunk containing the data for this specific chunk</param>
		public TERMChunk(Chunk chunk)
			: base(chunk, NAME)
		{
		}

		/// <summary>
		/// Extracts various fields specific to this chunk from the APNG's
		/// data field
		/// </summary>
		/// <param name="chunkData">An array of bytes representing the APNG's data field</param>
		protected override void ParseData(byte[] chunkData)
		{
			int offset = 0;
			terminationAction = Utils.ParseUint(chunkData, 1, ref offset);
			// If the data length is > 1 then read 9 more bytes
			if (chunkData.Length > 1) {
				actionAfterTermination = Utils.ParseUint(chunkData, 1, ref offset);
				delay = Utils.ParseUint(chunkData, 4, ref offset);
				iterationMax = Utils.ParseUint(chunkData, 4, ref offset);
			}
		}
	}

	public class BKGDChunk : Chunk
	{
		/// <summary>
		/// The ASCII name of the APNG chunk
		/// </summary>
		public const String NAME = "bKGD";

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="chunk">The APNG chunk containing the data for this specific chunk</param>
		public BKGDChunk(Chunk chunk)
			: base(chunk, NAME)
		{
		}
	}

	public class BACKChunk : Chunk
	{
		/// <summary>
		/// The ASCII name of the APNG chunk
		/// </summary>
		public const String NAME = "BACK";

		private uint redBackground;
		private uint greenBackground;
		private uint blueBackground;
		private uint mandatoryBackground;
		private uint backgroundImageId;
		private uint backgroundTiling;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="chunk">The APNG chunk containing the data for this specific chunk</param>
		public BACKChunk(Chunk chunk)
			: base(chunk, NAME)
		{
		}

		/// <summary>
		/// Extracts various fields specific to this chunk from the APNG's
		/// data field
		/// </summary>
		/// <param name="chunkData">An array of bytes representing the APNG's data field</param>
		protected override void ParseData(byte[] chunkData)
		{
			int offset = 0;
			redBackground = Utils.ParseUint(chunkData, 2, ref offset);
			greenBackground = Utils.ParseUint(chunkData, 2, ref offset);
			blueBackground = Utils.ParseUint(chunkData, 2, ref offset);

			// If the data length is > 6 then read 1 more byte
			if (chunkData.Length > 6) {
				mandatoryBackground = Utils.ParseUint(chunkData, 1, ref offset);
			}
			// If the data length is > 7 then read 2 more bytes
			if (chunkData.Length > 7) {
				backgroundImageId = Utils.ParseUint(chunkData, 2, ref offset);
			}
			// If the data length is > 9 then read 1 more byte
			if (chunkData.Length > 9) {
				backgroundTiling = Utils.ParseUint(chunkData, 1, ref offset);
			}
		}
	}

	public class IHDRChunk : Chunk
	{
		/// <summary>
		/// The ASCII name of the APNG chunk
		/// </summary>
		public const String NAME = "IHDR";

		private uint width;
		private uint height;
		private uint bitDepth;
		private uint colorType;
		private uint compressionMethod;
		private uint filterMethod;
		private uint interlaceMethod;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="chunk">The APNG chunk containing the data for this specific chunk</param>
		public IHDRChunk(Chunk chunk)
			: base(chunk, NAME)
		{
		}

		public uint Width {
			get {
				return width;
			}
			set {
				width = value;
				UpDate();
			}
		}

		public uint Height {
			get {
				return height;
			}
			set {
				height = value;
				UpDate();
			}
		}

		public uint BitDepth {
			get {
				return bitDepth;
			}
			set {
				bitDepth = value;
				UpDate();
			}
		}

		public uint ColorType {
			get {
				return colorType;
			}
			set {
				colorType = value;
				UpDate();
			}
		}

		public uint CompressionMethod {
			get {
				return compressionMethod;
			}
			set {
				compressionMethod = value;
				UpDate();
			}
		}

		public uint FilterMethod {
			get {
				return filterMethod;
			}
			set {
				filterMethod = value;
				UpDate();
			}
		}

		public uint InterlaceMethod {
			get {
				return interlaceMethod;
			}
			set {
				interlaceMethod = value;
				UpDate();
			}
		}

		void UpDate()
		{
			Llista<byte> dataBytes = new Llista<byte>();
			uint[] fields = {
				width,
				height,
				bitDepth,
				colorType,
				compressionMethod,
				filterMethod,
				interlaceMethod
			};
			for (int i = 0; i < fields.Length; i++)
				dataBytes.AfegirMolts(Serializar.GetBytes(fields[i]));
			ChunkData = dataBytes.ToTaula();
		}

		/// <summary>
		/// Extracts various fields specific to this chunk from the APNG's
		/// data field
		/// </summary>
		/// <param name="chunkData">An array of bytes representing the APNG's data field</param>
		protected override void ParseData(byte[] chunkData)
		{
			int offset = 0;
			width = Utils.ParseUint(chunkData, 4, ref offset);
			height = Utils.ParseUint(chunkData, 4, ref offset);
			bitDepth = Utils.ParseUint(chunkData, 1, ref offset);
			colorType = Utils.ParseUint(chunkData, 1, ref offset);
			compressionMethod = Utils.ParseUint(chunkData, 1, ref offset);
			filterMethod = Utils.ParseUint(chunkData, 1, ref offset);
			interlaceMethod = Utils.ParseUint(chunkData, 1, ref offset);
		}
		#region Equals and GetHashCode implementation
		public override bool Equals(object obj)
		{
			IHDRChunk other = obj as IHDRChunk;
			if (other == null)
				return false;
			return this.error == other.error && object.Equals(this.chunkLength, other.chunkLength) && object.Equals(this.chunkType, other.chunkType) && object.Equals(this.chunkData, other.chunkData) && object.Equals(this.chunkCRC, other.chunkCRC) && this.calculatedCRC == other.calculatedCRC && this.width == other.width && this.height == other.height && this.bitDepth == other.bitDepth && this.colorType == other.colorType && this.compressionMethod == other.compressionMethod && this.filterMethod == other.filterMethod && this.interlaceMethod == other.interlaceMethod;
		}

		#endregion
	}

	public class fcTLChunk : Chunk
	{
		public enum Dispose
		{
			None,
			Background,
			Previous,
		}
		public enum Blend
		{
			Source,
			Over
		}
		/// <summary>
		/// The ASCII name of the APNG chunk
		/// </summary>
		public const String NAME = "fcTL";
		uint sequenceNumber;
		uint width;
		uint height;
		uint offsetX;
		uint offsetY;
		ushort delayNum;
		ushort delayDen;
		Dispose disposeOP;
		Blend blendOP;
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="chunk">The APNG chunk containing the data for this specific chunk</param>
		public fcTLChunk(Chunk chunk)
			: base(chunk, NAME)
		{
		}

		public fcTLChunk(PNG png)
		{
			width = png.IHDR.Width;
			height = png.IHDR.Height;
			BlendOP = Blend.Source;
			ChunkType = NAME;
		}

		public uint SequenceNumber {
			get {
				return sequenceNumber;
			}
			set {
				sequenceNumber = value;
				Update();
			}
		}

		public uint Width {
			get {
				return width;
			}
			set {
				width = value;
				Update();
			}
		}

		public uint Height {
			get {
				return height;
			}
			set {
				height = value;
				Update();
			}
		}

		public uint OffsetX {
			get {
				return offsetX;
			}
			set {
				offsetX = value;
				Update();
			}
		}

		public uint OffsetY {
			get {
				return offsetY;
			}
			set {
				offsetY = value;
				Update();
			}
		}

		public ushort DelayNum {
			get {
				return delayNum;
			}
			set {
				delayNum = value;
				Update();
			}
		}

		public ushort DelayDen {
			get {
				return delayDen;
			}
			set {
				delayDen = value;
				Update();
			}
		}

		public Dispose DisposeOP {
			get {
				return disposeOP;
			}
			set {
				disposeOP = value;
				Update();
			}
		}

		public Blend BlendOP {
			get {
				return blendOP;
			}
			set {
				blendOP = value;
				Update();
			}
		}

		void Update()
		{
			Llista<byte> dataBytes = new Llista<byte>();
			dataBytes.AfegirMolts(Serializar.GetBytes(SequenceNumber));
			dataBytes.AfegirMolts(Serializar.GetBytes(Width));
			dataBytes.AfegirMolts(Serializar.GetBytes(Height));
			dataBytes.AfegirMolts(Serializar.GetBytes(OffsetX));
			dataBytes.AfegirMolts(Serializar.GetBytes(OffsetY));
			dataBytes.AfegirMolts(Serializar.GetBytes(DelayNum));
			dataBytes.AfegirMolts(Serializar.GetBytes(DelayDen));
			dataBytes.AfegirMolts(Serializar.GetBytes((byte)DisposeOP));
			dataBytes.AfegirMolts(Serializar.GetBytes((byte)BlendOP));
			ChunkData = dataBytes.ToTaula();
		}

		public bool IsEmpty {
			get{ return ChunkLength == 0; }
		}
		protected override void ParseData(byte[] chunkData)
		{
			int offset = 0;
			sequenceNumber = Utils.ParseUint(chunkData, 4, ref offset);
			width = Utils.ParseUint(chunkData, 4, ref offset);
			height = Utils.ParseUint(chunkData, 4, ref offset);
			offsetX = Utils.ParseUint(chunkData, 4, ref offset);
			offsetY = Utils.ParseUint(chunkData, 4, ref offset);
			delayNum = Serializar.ToUShort(new byte[]{ chunkData[21], chunkData[20] });
			delayDen = Serializar.ToUShort(new byte[]{ chunkData[23], chunkData[22] });
			disposeOP = (Dispose)chunkData[24];
			blendOP = (Blend)chunkData[25];

			
			
		}
	}

	public class IENDChunk : Chunk
	{
		/// <summary>
		/// The ASCII name of the APNG chunk
		/// </summary>
		public const String NAME = "IEND";

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="chunk">The APNG chunk containing the data for this specific chunk</param>
		public IENDChunk(Chunk chunk)
			: base(chunk, NAME)
		{
		}
		public IENDChunk()
			: base("IEND", new byte[]{ })
		{
		}
	}

	public class IDATChunk : Chunk
	{
		/// <summary>
		/// The ASCII name of the APNG chunk
		/// </summary>
		public const String NAME = "IDAT";

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="chunk">The APNG chunk containing the data for this specific chunk</param>
		public IDATChunk(Chunk chunk)
			: base(chunk, NAME)
		{
		}
	}

	public class fdATChunk : Chunk
	{
		/// <summary>
		/// The ASCII name of the APNG chunk
		/// </summary>
		public const String NAME = "fdAT";

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="chunk">The APNG chunk containing the data for this specific chunk</param>
		public fdATChunk(Chunk chunk)
			: base(chunk, NAME)
		{
		}
	}

	public class acTLChunk : Chunk
	{
		/// <summary>
		/// The ASCII name of the APNG chunk
		/// </summary>
		public const String NAME = "acTL";
		public const uint INFINITASREPETICIONES = 0;
		private uint numeroDeFotogramas;
		private uint numeroDeRepeticiones;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="chunk">The APNG chunk containing the data for this specific chunk</param>
		public acTLChunk(Chunk chunk)
			: base(chunk, NAME)
		{
		}
		public acTLChunk(uint numeroDeRepeticiones, uint numeroDeFotogramas)
		{
			//pngs,veces
			ChunkType = NAME;
			this.numeroDeFotogramas = numeroDeFotogramas;
			this.numeroDeRepeticiones = numeroDeRepeticiones;
			UpdateData();

		}

		void UpdateData()
		{
			ChunkData = Serializar.GetBytes(NumeroDeFotogramas).AfegirValors(Serializar.GetBytes(NumeroDeRepeticiones)).ToTaula();
		}
		public uint NumeroDeFotogramas {
			get {
				return numeroDeFotogramas;
			}
			set {
				numeroDeFotogramas = value;
				UpdateData();
			}
		}

		public uint NumeroDeRepeticiones {
			get {
				return numeroDeRepeticiones;
			}
			set {
				numeroDeRepeticiones = value;
				UpdateData();
			}
		}
		

		/// <summary>
		/// Extracts various fields specific to this chunk from the APNG's
		/// data field
		/// </summary>
		/// <param name="chunkData">An array of bytes representing the APNG's data field</param>
		protected override void ParseData(byte[] chunkData)
		{
			int offset = 0;
			numeroDeFotogramas = Utils.ParseUint(chunkData, 4, ref offset);
			numeroDeRepeticiones = Utils.ParseUint(chunkData, 4, ref offset);
		}
	}
	
	public class tEXtChunk:Chunk
	{
		
		public const string NAME = "tEXt";
		const byte NULLBYTE = (byte)'\0';
		string keyword = "";
		string information = "";
		public tEXtChunk(Chunk chunk)
			: base(chunk, NAME)
		{
		}
		public tEXtChunk(string keyword, string information)
		{
			ChunkType = NAME;
			Keyword = keyword;
			Information = information;
		}
		
		
		public string Keyword {
			get {
				return keyword;
			}
			set {
				if (value.Length > 80)
					throw new Exception("Metadata key no puede superar los 80 caracteres");
				keyword = value;
				UpdateData();
			}
		}

		public string Information {
			get {
				return information;
			}
			set {
				if (value == null)
					value = "";
				information = value;
				UpdateData();
			}
		}

		protected virtual void UpdateData()
		{
			Llista<Byte> data = new Llista<byte>();
			data.AfegirMolts(Serializar.GetBytes(Keyword));//Keyword
			data.Afegir(NULLBYTE);//null separator
			data.AfegirMolts(System.Text.ASCIIEncoding.Convert(Encoding.ASCII, System.Text.Encoding.GetEncoding(1252), Serializar.GetBytes(Information)));//text
			ChunkData = data.ToTaula();
		}
		protected override void ParseData(byte[] chunkData)
		{
			Llista<byte> bytesAConvertir = new Llista<byte>();
			long posicion = 0;
			while (chunkData[posicion] != NULLBYTE && chunkData.LongLength > posicion) {
				bytesAConvertir.Afegir(chunkData[posicion++]);
			}
			if (chunkData[posicion] == NULLBYTE) {
				keyword = Serializar.ToString(bytesAConvertir.ToTaula());
				bytesAConvertir.Buida();
				posicion++;//paso nullByte
				while (chunkData.LongLength > posicion) {
					bytesAConvertir.Afegir(chunkData[posicion++]);
				}
				Information = Serializar.ToString(System.Text.ASCIIEncoding.Convert(System.Text.Encoding.GetEncoding(1252), Encoding.ASCII, bytesAConvertir.ToTaula()));
			} else
				throw new Exception("Error al parsear los datos...no tiene la forma de tEXt");
		}
	}
	public class zTXtChunk:Chunk
	{
		public const byte COMPRESION = (byte)0;
		//por mirar...es 0 pero no se si es en ascii o en byte...
		public const string NAME = "zTXt";
		const byte NULLBYTE = (byte)'\0';
		string keyword;
		string information;
		public zTXtChunk(Chunk chunk)
			: base(chunk, NAME)
		{
		}
		public zTXtChunk(string keyword, string information)
		{
			ChunkType = NAME;
			this.keyword = keyword;
			Information = information;
		}
		
		public string Keyword {
			get {
				return keyword;
			}
			set {
				if (value.Length > 80)
					throw new Exception("Metadata key no puede superar los 80 caracteres");
				keyword = value;
				UpdateData();
			}
		}

		public string Information {
			get {
				return information;
			}
			set {
				information = value;
				UpdateData();
			}
		}

		protected virtual void UpdateData()
		{
			Llista<Byte> data = new Llista<byte>();
			data.AfegirMolts(Serializar.GetBytes(Keyword));//Keyword
			data.Afegir(NULLBYTE);//null separator
			data.Afegir(COMPRESION);
			data.AfegirMolts(System.Text.ASCIIEncoding.Convert(Encoding.ASCII, System.Text.Encoding.GetEncoding(1252), Serializar.GetBytes(Information)));//text
			ChunkData = data.ToTaula();
		}
		protected override void ParseData(byte[] chunkData)
		{
			Llista<byte> bytesAConvertir = new Llista<byte>();
			Encoding encodingSource = null;
			long posicion = 0;
			while (chunkData[posicion] != NULLBYTE && chunkData.LongLength > posicion) {
				bytesAConvertir.Afegir(chunkData[posicion++]);
			}
			if (chunkData[posicion] == NULLBYTE) {
				
				keyword = Serializar.ToString(bytesAConvertir.ToTaula());
				bytesAConvertir.Buida();
				posicion++;//paso el Null byte
				if (COMPRESION != chunkData[posicion++])//leo la compresion
					throw new Exception("Error al parsear los datos...no tiene la forma de zTXt");
				while (chunkData.LongLength > posicion) {
					bytesAConvertir.Afegir(chunkData[posicion++]);
				}
				encodingSource = System.Text.Encoding.GetEncoding(1252);//es la unica usada...
				Information = Serializar.ToString(System.Text.ASCIIEncoding.Convert(encodingSource, Encoding.ASCII, bytesAConvertir.ToTaula()));
			} else
				throw new Exception("Error al parsear los datos...no tiene la forma de zTXt");
		}
	}
	public class tIMEChunk:Chunk
	{
		public const string NAME = "tIME";
		DateTime fecha;
		public tIMEChunk(Chunk chunk)
			: base(chunk, NAME)
		{
		}
		public tIMEChunk(DateTime fecha)
			: base(NAME, new byte[]{ })
		{
			Fecha = fecha;
		}

		public DateTime Fecha {
			get {
				return fecha;
			}
			set {
				fecha = value;
				UpDateData();
			}
		}

		void UpDateData()
		{
			DateTime fecha = this.fecha.ToUniversalTime();
			byte[] bytesAño = Serializar.GetBytes((short)fecha.Year);
			ChunkData = new byte[] {
				bytesAño[1],
				bytesAño[0],
				(byte)fecha.Month,
				(byte)fecha.Day,
				(byte)fecha.Hour,
				(byte)fecha.Minute,
				(byte)fecha.Second
			};
		}
		protected override void ParseData(byte[] chunkData)
		{
			fecha = new DateTime((int)Serializar.ToShort(new byte[] {
			                                             	chunkData[1],
			                                             	chunkData[0]
			                                             }), (int)ChunkData.SubArray(2, 1)[0], (int)ChunkData.SubArray(3, 1)[0], (int)ChunkData.SubArray(4, 1)[0], (int)ChunkData.SubArray(5, 1)[0], (int)ChunkData.SubArray(6, 1)[0]).ToLocalTime();

		}
	}
}
