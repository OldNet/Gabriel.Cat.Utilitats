using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Gabriel.Cat.Extension;

namespace Gabriel.Cat
{
    public enum PixelColors//no se si tiene un nombre descriptivo...
    {
        Red,
        Green,
        Blue,
        GrayScale,
        Sepia,
        Inverted
    }

    public class Collage : IEnumerable<ImageFragment>
    {
        Llista<ImageFragment> fragments;

        public Collage()
        {
            fragments = new Llista<ImageFragment>();
        }
        public Collage(IEnumerable<ImageFragment> imgsCollage) : this()
        {
            fragments.AfegirMolts(imgsCollage);
        }
        public int Count
        {
            get { return fragments.Count; }
        }

        public void Add(ImageFragment imgFragment)
        {
            fragments.Afegir(imgFragment);
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
            if (imagen == null)
                throw new ArgumentNullException("Se necesita una imagen");

            ImageFragment fragment = null;
            PointZ location = new PointZ(x, y, z);
            fragment = new ImageFragment(imagen, location);
            fragments.Afegir(fragment);


            return fragment;
        }
        public void RemoveAll()
        {
            fragments.Buida();
        }
        public void Remove(ImageFragment fragmento)
        {
            fragments.Elimina(fragmento);
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

                if (this.fragments[pos].Location.Z > z)
                    acabado = true;
                else if (this.fragments[pos].Location.Z == z)
                    fragmentosCapaZero.Add(this.fragments[pos]);
                pos++;
            }
            for (int i = 0; i < fragmentosCapaZero.Count && fragmento == null; i++)
            {
                rectangle = new Rectangle(fragmentosCapaZero[i].Location.X, fragmentosCapaZero[i].Location.Y, fragmentosCapaZero[i].Image.Width, fragmentosCapaZero[i].Image.Height);
                if (rectangle.Contains(x, y))
                    fragmento = fragmentosCapaZero[i];

            }
            return fragmento;
        }
        public ImageFragment[] GetFragments(PointZ location)
        {
            return GetFragments(location.X, location.Y, location.Z);
        }
        public ImageFragment[] GetFragments(int x, int y, int z)
        {
            List<ImageFragment> fragmentosSeleccionados = new List<ImageFragment>();
            ImageFragment img;

            do
            {
                img = GetFragment(x, y, z);
                if (img != null)
                {//los quito para no molestar
                    fragmentosSeleccionados.Add(img);
                    fragments.Elimina(img);
                }
            } while (img != null);

            fragments.AfegirMolts(fragmentosSeleccionados);

            return fragmentosSeleccionados.ToArray();
        }


        public unsafe Bitmap CrearCollage()
        {
            //funciona bien ;)
            //al usar punteros ahora va 3 veces mas rapido :) //aun se puede optimizar 4 veces mas osea un total de 12 veces mas rapi:D
            const int ARGB = 4;
            int amplitudBitmapMax = 1, amplitudBitmapMin = 0;
            int alturaBitmapMax = 1, alturaBitmapMin = 0;
            int saltoLinea;
            Bitmap imagen = null;
            int puntoXInicioFila;
            if (fragments.Count != 0)
            {
                fragments.Ordena();//ordeno los fragmentos
                                   //obtengo las medidas maximas
                for (int i = 0; i < fragments.Count; i++)
                {
                    if (amplitudBitmapMax < (fragments[i].Location.X + fragments[i].Image.Width))
                        amplitudBitmapMax = (fragments[i].Location.X + fragments[i].Image.Width);
                    if (amplitudBitmapMin > fragments[i].Location.X)
                        amplitudBitmapMin = fragments[i].Location.X;
                    if (alturaBitmapMax < (fragments[i].Location.Y + fragments[i].Image.Height))
                        alturaBitmapMax = (fragments[i].Location.Y + fragments[i].Image.Height);
                    if (alturaBitmapMin > fragments[i].Location.Y)
                        alturaBitmapMin = fragments[i].Location.Y;
                }
                imagen = new Bitmap(amplitudBitmapMax + (amplitudBitmapMin * -1), alturaBitmapMax + (alturaBitmapMin * -1), fragments[0].Image.PixelFormat);
                saltoLinea = amplitudBitmapMax * ARGB;  //multiplico por 4 porque la amplitud de la tabla es en bytes no en Pixels por lo tanto Argb
                imagen.TrataBytes((MetodoTratarBytePointer)((bytesImgTotal) =>
                {
                    //pongo en el bitmap los fragmentos de forma ordenada
                    for (int i = fragments.Count - 1; i >= 0; i--)
                    {

                        puntoXInicioFila = (saltoLinea * (fragments[i].Location.Y + (alturaBitmapMin * -1))) + (fragments[i].Location.X + (amplitudBitmapMin * -1)) * ARGB;  //multiplico por 4 porque la amplitud de la tabla es en bytes no en Pixels por lo tanto Argb
                        fragments[i].Image.TrataBytes((MetodoTratarBytePointer)((bytesImgFragmento) =>
                        {
                            //pongo los fragmentos
                            for (int y = 0, yFinal = fragments[i].Image.Height, xFinal = fragments[i].Image.Width * ARGB; y < yFinal; y++, puntoXInicioFila += saltoLinea)
                            {

                                for (int x = 0; x < xFinal; x++)
                                {
                                    //ahora tengo que poner la matriz donde toca...
                                    bytesImgTotal[puntoXInicioFila + x] = bytesImgFragmento[y * xFinal + x];
                                }
                            }
                        }));
                    }
                }));

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


    }
    public static class Image
    {

        public static Bitmap ChangeColorCopy(this Bitmap bmp, PixelColors color)
        {
            Bitmap bmpClon = bmp.Clone() as Bitmap;
            ChangeColor(bmpClon, color);
            return bmpClon;
        }
        public static unsafe void ChangeColor(this Bitmap bmp, PixelColors color)
        {
            bmp.TrataBytes((rgbArray) => { ICambiaColor(rgbArray, bmp.IsArgb(), bmp.LengthBytes(), color); });
        }

        private static unsafe void ICambiaColor(byte* rgbImg, bool isArgb, int lenght, PixelColors color)
        {

            const int R = 0, G = 1, B = 2;
            byte byteR, byteG, byteB;
            int incremento = 3;
            if (isArgb) incremento++;//me salto el alfa
            for (int i = 0; i < lenght; i += incremento)
            {


                byteR = rgbImg[i + R];
                byteG = rgbImg[i + G];
                byteB = rgbImg[i + B];
                
                switch (color)
                {
                    case PixelColors.Sepia:
                        IToSepia(ref byteR, ref byteG, ref byteB);
                        break;
                    case PixelColors.Inverted:
                        ToInvertit(ref byteR, ref byteG, ref byteB);
                        break;
                    case PixelColors.GrayScale:
                        ToEscalaDeGrises(ref byteR, ref byteG, ref byteB);
                        break;
                    case PixelColors.Blue:
                        ToAzul(ref byteR, ref byteG, ref byteB);
                        break;
                    case PixelColors.Red:
                        ToRojo(ref byteR, ref byteG, ref byteB);
                        break;
                    case PixelColors.Green:
                        ToVerde(ref byteR, ref byteG, ref byteB);
                        break;


                }
                rgbImg[i + R] = byteR;
                rgbImg[i + G] = byteG;
                rgbImg[i + B] = byteB;

            }

        }
        #region Pixels
        public static Color ToRed(this Color pixel)
        {
            return ToRed(pixel.R, 0, 0);

        }
        public static Color ToBlue(this Color pixel)
        {
            return ToBlue(0, 0, pixel.B);
        }
        public static Color ToGreen(this Color pixel)
        {
            return ToGreen(0, pixel.G, 0);
        }
        public static Color ToEscalaGrises(this Color pixel)
        {
            return ToGrayScale(pixel.R, pixel.G, pixel.B);

        }
        public static Color ToInverted(this Color pixel)
        {
            return ToInverted(255 - pixel.R, 255 - pixel.G, 255 - pixel.B);
        }
        public static Color ToSepia(this Color pixel)
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
            int rInt = Convert.ToInt32(r * 0.393 + g * 0.769 + b * 0.189);
            int gInt = Convert.ToInt32(r * 0.349 + g * 0.686 + b * 0.168);
            int bInt = Convert.ToInt32(r * 0.272 + g * 0.534 + b * 0.131);
            if (rInt > 255)
                rInt = 255;
            if (gInt > 255)
                gInt = 255;
            if (bInt > 255)
                bInt = 255;
            r = (byte)rInt;
            g = (byte)gInt;
            b = (byte)bInt;
        }


        #endregion
        #endregion

    }
    public class ImageFragment : IComparable, IComparable<ImageFragment>
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
            Location = localizacion;
        }


        public byte[,] RgbValuesMatriu
        {
            get { return imagen.Matriu; }
        }
        public byte[] RgbValues
        {
            get { return imagen.Array; }
        }
        public PointZ Location
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

        public Bitmap Image
        {
            get
            {
                return imagen.Image;
            }
        }

        #region IComparable implementation
        public int CompareTo(ImageFragment other)
        {
            int compareTo;
            if (other != null)
                compareTo = Location.CompareTo(other.Location);
            else
                compareTo = -1;
            return compareTo;
        }
        public int CompareTo(Object other)
        {
            return CompareTo(other as ImageFragment);
        }
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
            this.bmp = bmp.Clone(new Rectangle(new Point(), bmp.Size), PixelFormat.Format32bppPArgb);//asi todos tienen el mismo PixelFormat :)

        }

        public byte[] Array
        {
            get
            {
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

        public Bitmap Image
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
