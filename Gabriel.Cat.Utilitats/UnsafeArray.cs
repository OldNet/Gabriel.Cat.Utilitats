using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat
{
    public delegate void MetodoUnsafeArray(UnsafeArray unsafeArray);
    public unsafe class UnsafeArray
    {
        public byte* PtrArray;
        public readonly byte* PtrArrayInicial;
        public readonly long Length;
        public UnsafeArray(byte* ptrArrayIncial, long length)
        {
            Length = length;
            PtrArrayInicial = ptrArrayIncial;
            PtrArray = ptrArrayIncial;
        }
        public void ResetPosicion()
        {
            PtrArray = PtrArrayInicial;
        }
        public long Posicion
        {
            get
            {
                return PtrArray - PtrArrayInicial;
            }
            set
            {
                PtrArray = PtrArrayInicial + Posicion;
            }
        }
        public byte this[long posicion]
        {
            get { return PtrArrayInicial[posicion]; }
            set { PtrArrayInicial[posicion] = value; }
        }
        public byte* PtrArrayFin
        {
            get { return PtrArrayInicial + Length; }
        }
        public static unsafe explicit operator byte* (UnsafeArray unsafeArray)
        {
            return unsafeArray.PtrArray;
        }
        public static void Usar(byte[] array,MetodoUnsafeArray metodo)
        {
            if (array == null || metodo == null)
                throw new ArgumentNullException();
            Exception exAux = null;
            unsafe
            {
                fixed(byte * ptrArray=array)
                {
                    try
                    {
                        metodo(new UnsafeArray(ptrArray, array.Length));
                    }
                    catch(Exception ex)
                    {
                        exAux = ex;
                    }
            }
                //lanzo la excepcion en un contexto seguro :D asi no hay problemas con los pointers
                if (exAux != null)
                    throw exAux;
        }

    }
}
