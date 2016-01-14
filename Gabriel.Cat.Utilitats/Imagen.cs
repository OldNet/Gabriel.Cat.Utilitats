using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Gabriel.Cat;
using Gabriel.Cat.Extension;

namespace Gabriel.Cat
{

    /// <summary>
    /// El Collage no se ha testeado...aun...
    /// </summary>
    public class Image:IEnumerable<ImageFragment>
    {
        public enum PixelColors
        {
            Red,
            Green,
            Blue,
            GrayScale,
            Sepia,
            Inverted
        }

        public struct PixelByte
        {
            public int X;
            public int Y;
            public byte Data;
        }

        Llista<ImageFragment> fragments;
        public Image()
        {
            fragments = new Llista<ImageFragment>();
        }

        public ImageFragment Add(Bitmap imatge, PointZ localizacio)
        {
            if (localizacio == null)
                throw new ArgumentNullException("Se necesita una localizacion");
            return Add(imatge, localizacio.X, localizacio.Y, localizacio.Z);
        }

        /// <summary>
        /// Añade una imagen al mosaico
        /// </summary>
        /// <param name="imagen">imagen para poner</param>
        /// <param name="localizacion">localización en la imagen</param>
        /// <param name="capa">produndidad a la que esta</param>
        /// <returns>devuelve null si no lo a podido añadir</returns>
        public ImageFragment Add(Bitmap imatge, Point localizacio, int capa = 0)
        {
            if (localizacio == null)
                throw new ArgumentNullException("Se necesita una localizacion");
            return Add(imatge, localizacio.X, localizacio.Y, capa);
        }
        public ImageFragment Add(Bitmap imagen, int x = 0, int y = 0, int z = 0)
        {
            ImageFragment fragment = null;
            if (imagen != null)
            {
                fragment = new ImageFragment(imagen, x, y, z);
                try
                {
                    fragments.Afegir(fragment);
                }
                catch
                {
                    fragment = null;
                }
            }
            return fragment;
        }
        public ImageFragment Remove(int x = 0, int y = 0, int z = 0)
        {
            return Remove(new PointZ(x, y, z));
        }
        public ImageFragment Remove(Point localizacion, int capa = 0)
        {
            return Remove(new PointZ(localizacion.X, localizacion.Y, capa));
        }
        public ImageFragment Remove(PointZ localizacion)
        {
            if (localizacion == null)
                throw new ArgumentNullException("Se necesita un punto para localizar el fragmento");

            ImageFragment fragmentoQuitado = GetFragment(localizacion);

            if (fragmentoQuitado != null)
                fragments.Elimina(fragmentoQuitado);

            return fragmentoQuitado;
        }
        public ImageFragment GetFragment(PointZ location)
        {
            return GetFragment(location.X, location.Y, location.Z);
        }
        public ImageFragment GetFragment(int x, int y, int z)
        {
            List<ImageFragment> fragmentosCapaZero = new List<ImageFragment>();
            bool acabado = false;
            int pos = 0;
            Rectangle rectangle;
            ImageFragment fragmento = null;

            fragments.Ordena();
            while (pos < this.fragments.Count && !acabado)
            {
                if (this.fragments[pos].Localizacion.Z > z)
                    acabado = true;
                else
                    fragmentosCapaZero.Add(this.fragments[pos]);
            }
            for (int i = 0; i < fragmentosCapaZero.Count && fragmento == null; i++)
            {
                rectangle = new Rectangle(fragmentosCapaZero[i].Localizacion.X, fragmentosCapaZero[i].Localizacion.Y, fragmentosCapaZero[i].Imagen.Width, fragmentosCapaZero[i].Imagen.Height);
                if (rectangle.Contains(x, y))
                    fragmento = fragmentosCapaZero[i];

            }
            return fragmento;
        }

        public Bitmap CrearCollage()
        {//funciona bien ;)
            const int ARGB = 4;
            int amplitudBitmapMax = 1, amplitudBitmapMin=0;
            int alturaBitmapMax = 1,alturaBitmapMin=0;
            int saltoLinea;
            Bitmap imagen=null;
            byte[,] matrizFragmento;
            int  puntoXInicioFila;
            if (fragments.Count != 0)
            {
                fragments.Ordena();//ordeno los fragmentos
                                   //obtengo las medidas maximas
                for (int i = 0; i < fragments.Count; i++)
                {
                  
                    if (amplitudBitmapMax < (fragments[i].Localizacion.X  + fragments[i].Imagen.Width))
                        amplitudBitmapMax = (fragments[i].Localizacion.X + fragments[i].Imagen.Width);
                    if (amplitudBitmapMin > fragments[i].Localizacion.X)
                        amplitudBitmapMin = fragments[i].Localizacion.X;
                    if (alturaBitmapMax < (fragments[i].Localizacion.Y + fragments[i].Imagen.Height))
                        alturaBitmapMax = (fragments[i].Localizacion.Y + fragments[i].Imagen.Height) ;
                     if (alturaBitmapMin> fragments[i].Localizacion.Y)
                        alturaBitmapMin = fragments[i].Localizacion.Y;
                }
                imagen = new Bitmap(amplitudBitmapMax + (amplitudBitmapMin*-1), alturaBitmapMax +(alturaBitmapMin*-1), fragments[0].Imagen.PixelFormat);
                saltoLinea=amplitudBitmapMax*ARGB;  //multiplico por 4 porque la amplitud de la tabla es en bytes no en Pixels por lo tanto Argb
                imagen.TrataBytes((bytes) =>
                {
                //pongo en el bitmap los fragmentos de forma ordenada
                for (int i = fragments.Count - 1; i >= 0; i--)
                    {
                        matrizFragmento = fragments[i].RgbValuesMatriu;
                        puntoXInicioFila = (saltoLinea * (fragments[i].Localizacion.Y+(alturaBitmapMin*-1))) + (fragments[i].Localizacion.X+(amplitudBitmapMin*-1))*ARGB;  //multiplico por 4 porque la amplitud de la tabla es en bytes no en Pixels por lo tanto Argb
                        //pongo los fragmentos
                        for (int y = 0, yFinal = matrizFragmento.GetLength(DimensionMatriz.Y), xFinal = matrizFragmento.GetLength(DimensionMatriz.X); y < yFinal; y++, puntoXInicioFila += saltoLinea)
                            for (int x = 0; x < xFinal; x++) 
                            {
                                //ahora tengo que poner la matriz donde toca...
                                bytes[puntoXInicioFila + x] = matrizFragmento[x, y];
                            }

                }
                });
            }
            return imagen;
        }
        public IEnumerator<ImageFragment> GetEnumerator()
        {
            fragments.Ordena();
            return fragments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Metodos de clase
        public static Bitmap ChangeColorCopy(Bitmap bmp, PixelColors color)
        {
            if (bmp == null)
                throw new ArgumentNullException();
            Bitmap bmpClon = bmp.Clone() as Bitmap;
            ChangeColor(bmpClon, color);
            return bmpClon;
        }
        public static void ChangeColor(Bitmap bmp, PixelColors color)
        {
            if (bmp == null)
                throw new ArgumentNullException();
            bmp.TrataBytes((rgbArray) => { ICambiaColor(rgbArray, color); });

        }

        private static void ICambiaColor(byte[] rgbImg, PixelColors color)
        {
            if (rgbImg.Length % 3!=0)
                throw new ArgumentException("la tabla rgb no tiene la longitud divisible enteramente en 3 bytes");
            //de momento no vale la pena usar threads porque va mas lento...
            const int R = 0, G = 1, B = 2;
            byte r, g, b;
            for (int i = 0; i < rgbImg.Length; i += 3)
            {

                r = rgbImg[i + R];
                g = rgbImg[i + G];
                b = rgbImg[i + B];

                switch (color)
                {
                    case PixelColors.Sepia:
                        IToSepia(ref r, ref g, ref b);
                        break;
                    case PixelColors.Inverted:
                        ToInvertit(ref r, ref g, ref b);
                        break;
                    case PixelColors.GrayScale:
                        ToEscalaDeGrises(ref r, ref g, ref b);
                        break;
                    case PixelColors.Blue:
                        ToAzul(ref r, ref g, ref b);
                        break;
                    case PixelColors.Red:
                        ToRojo(ref r, ref g, ref b);
                        break;
                    case PixelColors.Green:
                        ToVerde(ref r, ref g, ref b);
                        break;


                }
                rgbImg[i + R] = r;
                rgbImg[i + G] = g;
                rgbImg[i + B] = b;

            }

        }
        #region Pixels
        public static Color ToRed(Color pixel)
        {
            return ToRed(0, 0, pixel.R);

        }
        public static Color ToBlue(Color pixel)
        {
            return ToBlue(pixel.B, 0, 0);
        }
        public static Color ToGreen(Color pixel)
        {
            return ToGreen(0, pixel.G, 0);
        }
        public static Color ToEscalaGrises(Color pixel)
        {
            return ToGrayScale(pixel.R, pixel.G, pixel.B);

        }
        public static Color ToInverted(Color pixel)
        {
            return ToInverted(255 - pixel.R, 255 - pixel.G, 255 - pixel.B);
        }
        public static Color ToSepia(Color pixel)
        {
            return ToSepia(pixel.R, pixel.G, pixel.B);
        }
        public static Color ToRed(int r, int g, int b)
        {
            return Color.FromArgb(0, 0, r);
        }
        public static Color ToBlue(int r, int g, int b)
        {
            return Color.FromArgb(b, 0, 0);
        }
        public static Color ToGreen(int r, int g, int b)
        {
            return Color.FromArgb(0, g, 0);
        }
        public static Color ToGrayScale(int r, int g, int b)
        {
            int v = Convert.ToInt32(0.2126 * r + 0.7152 * g + 0.0722 * b);
            return Color.FromArgb(v, v, v);

        }

        public static Color ToInverted(int r, int g, int b)
        {
            return Color.FromArgb(255 - r, 255 - g, 255 - b);
        }
        public static Color ToSepia(int r, int g, int b)
        {
            byte rB = (byte)r, gB = (byte)g, bB = (byte)b;
            IToSepia(ref rB, ref gB, ref bB);
            return Color.FromArgb(rB, gB, bB);
        }

        #region Optimizacion

        static void ToRojo(ref byte r, ref byte g, ref byte b)
        {
            g = 0;
            r = 0;
        }
        static void ToAzul(ref byte r, ref byte g, ref byte b)
        {
            g = 0;
            b = 0;
        }
        static void ToVerde(ref byte r, ref byte g, ref byte b)
        {
            r = 0;
            b = 0;
        }
        static void ToInvertit(ref byte r, ref byte g, ref byte b)
        {
            r = (byte)(255 - r);
            g = (byte)(255 - g);
            b = (byte)(255 - b);
        }
        static void ToEscalaDeGrises(ref byte r, ref byte g, ref byte b)
        {
            b = (byte)Convert.ToInt32(0.2126 * r + 0.7152 * g + 0.0722 * b);
            g = b;
            r = g;
        }
        static void IToSepia(ref byte r, ref byte g, ref byte b)
        {
           int  rInt = Convert.ToInt32(r * 0.393 + g * 0.769 + b * 0.189);
           int gInt = Convert.ToInt32(r * 0.349 + g * 0.686 + b * 0.168);
           int bInt = Convert.ToInt32(r * 0.272 + g * 0.534 + b * 0.131);
            if (rInt > 255)
                rInt = 255;
            if (gInt > 255)
                gInt = 255;
            if (bInt > 255)
                bInt = 255;
            r = (byte)rInt;
            g =(byte) gInt;
            b = (byte)bInt;
        }


        #endregion
        #endregion

        #endregion


    }

    public class ImageFragment : IComparable, IComparable<ImageFragment>, IClauUnicaPerObjecte
    {
        PointZ localizacion;
        ImageBase imagen;

        public ImageFragment(Bitmap imagen, int x = 0, int y = 0, int z = 0)
            : this(imagen, new PointZ(x, y, z))
        {

        }
        public ImageFragment(Bitmap imagen, Point localizacion, int capa = 0)
            : this(imagen, new PointZ(localizacion != default(Point) ? localizacion.X : 0, localizacion != default(Point) ? localizacion.Y : 0, capa))
        {

        }
        public ImageFragment(Bitmap imagen, PointZ localizacion)
        {
            if (localizacion == null)
                throw new ArgumentNullException("Necesita una localizacion!");
            if (imagen == null)
                throw new NullReferenceException("La imagen no puede ser null");
            this.imagen = new ImageBase(imagen);
            Localizacion = localizacion;
        }


        public byte[,] RgbValuesMatriu
        {
            get { return imagen.Matriu; }
        }
        public byte[] RgbValues
        {
            get { return imagen.Array; }
        }
        public PointZ Localizacion
        {
            get
            {
                return localizacion;
            }
            set
            {
                localizacion = value;
            }
        }

        public Bitmap Imagen
        {
            get
            {
                return imagen.Imatge;
            }
        }

        #region IComparable implementation
        public int CompareTo(ImageFragment other)
        {
            int compareTo;
            if (other != null)
                compareTo = Localizacion.CompareTo(other.Localizacion);
            else
                compareTo = -1;
            return compareTo;
        }

        #region IClauUnicaPerObjecte implementation


        public IComparable Clau()
        {
            return Localizacion as IComparable;
        }

        public int CompareTo(object obj)
        {
            return CompareTo(obj as ImageFragment);
        }


        #endregion

        #endregion



    }
    public class ImageBase
    {
        Bitmap bmp;
        byte[,] matriuBmp;
        byte[] bmpArray;
        public ImageBase(Bitmap bmp)
        {
            if (bmp == null)
                throw new NullReferenceException("La imagen no puede ser null");
            this.bmp = bmp.Clone(new Rectangle(new Point(),bmp.Size),PixelFormat.Format32bppPArgb);//asi todos tienen el mismo PixelFormat :)

        }

        public byte[] Array {
            get {
                if (bmpArray == null)
                    bmpArray = bmp.GetBytes();
                return bmpArray;
            }
            set
            {
                bmp.SetBytes(value);
                bmpArray = value;
                matriuBmp = null;
            }
        }

        public Bitmap Imatge
        {
            get
            {
                return bmp;
            }
        }

        public byte[,] Matriu
        {
            get
            {
                if (matriuBmp == null)
                    matriuBmp = bmp.GetMatriuBytes();
                return matriuBmp;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                bmp.SetMatriuBytes(value);
                matriuBmp = value;
                bmpArray = null;
            }

        }


    }

}
