using Gabriel.Cat.Extension;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat
{

    public class BitmapPointerLocated
    {
        Bitmap imagen;
        LlistaOrdenada<int, Point> pointLocatedByColorList;
        LlistaOrdenada<PointZ, Color> colorLocatedByPointerList;
        byte[] bytesImg;



        public BitmapPointerLocated(Bitmap img)
        {
            pointLocatedByColorList = new LlistaOrdenada<int, Point>();
            colorLocatedByPointerList = new LlistaOrdenada<PointZ, Color>();
            Bmp = img;

        }
        public Bitmap Bmp
        {
            get
            {
                return imagen;
            }
            set
            {
                imagen = value;
                bytesImg = imagen.GetBytes();
                pointLocatedByColorList.Clear();
                colorLocatedByPointerList.Clear();
            }
        }
        public Point GetPoint(Color color)
        {
            int colorInt = color.ToArgb();
            Point location;
            if (pointLocatedByColorList.ContainsKey(colorInt))
                location = pointLocatedByColorList[colorInt].Value;
            else
                location = GetPoint(colorInt);
            return location;
        }

        public Point GetPoint(int colorInt)
        {
            const int ARGB = 4;
            int posicion;
            byte[] bytesColor;
            Point location = default(Point);
            bool encontrado = false;
            if (pointLocatedByColorList.ContainsKey(colorInt))
                location = pointLocatedByColorList[colorInt].Value;
            else
            {
                bytesColor = Serializar.GetBytes(colorInt);
                for (int y = 0, yFin = Convert.ToInt32(imagen.Height), xFin = Convert.ToInt32(imagen.Width) * ARGB; y < yFin && !encontrado; y++)
                    for (int x = 0; x < xFin && !encontrado; x += ARGB)
                    {
                        posicion = x + (y * xFin);
                        encontrado = bytesImg[posicion] == bytesColor[0] && bytesImg[posicion + 1] == bytesColor[1] && bytesImg[posicion + 2] == bytesColor[2] && bytesImg[posicion + 3] == bytesColor[3];
                        if (encontrado)
                            location = new Point(x, y);
                    }
                if (!encontrado)
                    throw new ArgumentOutOfRangeException("El color no esta dentro de la imagen!");
                else { pointLocatedByColorList.Add(colorInt, location);
                    if (!colorLocatedByPointerList.ContainsKey(new PointZ(location, 0)))
                        colorLocatedByPointerList.Add(new PointZ(location, 0), Color.FromArgb(colorInt));
                }

            }
            return location;
        }
        public Color GetColor(Point point)
        {
            Color color;
            PointZ location = new PointZ(point, 0);
            if (colorLocatedByPointerList.ContainsKey(location))
                color = colorLocatedByPointerList[location];
            else
            {
                color = imagen.GetPixel(point.X, point.Y);
                colorLocatedByPointerList.Add(location, color);
                if (!pointLocatedByColorList.ContainsKey(color.ToArgb()))
                    pointLocatedByColorList.Add(color.ToArgb(), point);
            }
            return color;
        }
    }
}
