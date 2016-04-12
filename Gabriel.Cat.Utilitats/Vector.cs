using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat
{
    public class Vector
    {
        public int InicioX { get; set; }
        public int InicioY { get; set; }

        public int FinX { get; set; }
        public int FinY { get; set; }
        public Vector()
            : this(0, 0, 0, 0)
        {

        }
        public Vector(int inicioX, int inicioY, int finX, int finY)
        {
            InicioY = inicioY;
            InicioX = inicioX;

            FinX = finX;
            FinY = finY;
        }
        public double Longitud
        {

            get
            {
                return Math.Sqrt(Math.Pow(FinX - InicioX, 2) + Math.Pow(FinY - InicioY, 2));

            }
        }
        public void CalculaFin(int alfa, double longitudVector)
        {
            longitudVector = Math.Abs(longitudVector);
            alfa = Math.Abs(alfa) % 365;
            int beta;
            alfa = alfa % 90;
            beta = 90 - alfa;

            FinX = Convert.ToInt32(Math.Cos(alfa) * longitudVector + InicioX);
            FinY = Convert.ToInt32(Math.Cos(beta) * longitudVector + InicioY);


        }
        public void CalculaFin(int alfa, int ladoContiguo)
        {
            ladoContiguo = Math.Abs(ladoContiguo);
            alfa = Math.Abs(alfa) % 365;

            FinX = alfa < 180 ? InicioX + ladoContiguo : InicioX - ladoContiguo;
            FinY = Convert.ToInt32(Math.Tan(alfa) * ladoContiguo);
        }

    }
}
