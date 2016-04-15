using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat
{
    public enum Sentido
    {
        Centro = 1,
        Izquierda = 2,
        Derecha = 4,
        Abajo = 8,
        Arriba = 16,
        DiagonalIzquierdaAbajo = Izquierda + Abajo,
        DiagonalIzquierdaArriba = Izquierda + Arriba,
        DiagonalDerechaAbajo = Derecha + Abajo,
        DiagonalDerechaArriba = Derecha + Arriba
    }
    public class Vector
    {
        int inicioX;
        int inicioY;
        Sentido sentido;
        int finX;
        int finY;
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

        public int InicioY
        {
            get { return inicioY; }
            set { inicioY = value; CalculaSentido(); }
        }
        public int InicioX
        {
            get { return inicioX; }
            set { inicioX = value; CalculaSentido(); }
        }
        public int FinY
        {
            get { return finY; }
            set { finY = value; CalculaSentido(); }
        }
        public int FinX
        {
            get { return finX; }
            set { finX = value; CalculaSentido(); }
        }
        public double Longitud
        {

            get
            {
                return Math.Sqrt(Math.Pow(FinX - InicioX, 2) + Math.Pow(FinY - InicioY, 2));

            }
        }
        public Sentido Sentido
        {
            get
            {
                return sentido;
            }
        }
        /// <summary>
        /// Si la distancia entre el inicio y el fin de cada componente es el mismo entonces es pura
        /// </summary>
        public bool DiagonalPura
        {
            get
            {
                return Math.Abs(InicioX - FinX) ==Math.Abs( InicioY - FinY);
            }
        }
        private void CalculaSentido()
        {

            Sentido sentidoX, sentidoY;
            if (FinX == InicioX)
                sentidoX = Sentido.Centro;
            else
                sentidoX = FinX > InicioX ? Sentido.Derecha : Sentido.Izquierda;

            if (FinY == InicioY)
                sentidoY = Sentido.Centro;
            else
                sentidoY = FinY > InicioY ? Sentido.Arriba : Sentido.Abajo;
            //miro el sentido
            if (sentidoX == Sentido.Centro)
                sentido = sentidoY;
            else if (sentidoY == Sentido.Centro)
                sentido = sentidoX;
            else
                sentido = (Sentido)(int)sentidoX + (int)sentidoY;
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
