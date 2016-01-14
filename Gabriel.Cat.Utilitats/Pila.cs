/*
 * Creado por SharpDevelop.
 * Usuario: pc
 * Fecha: 16/04/2015
 * Hora: 22:39
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of Pila.
	/// </summary>
	public class Pila<Tvalue>:IEnumerable<Tvalue>
	{
        bool throwExceptionIfEmpty = false;


		Llista<Tvalue> pila;
		public Pila()
		{
			pila = new Llista<Tvalue>();
		}
        public Pila(IEnumerable<Tvalue> valors)
            :this(){Push(valors);}
		public int Count
		{ get { return pila.Count; } }
		public bool Empty
		{ get { return Count == 0; } }
		public Tvalue this[int index]
		{ get { return pila[index]; }
			set { pila[index] = value; } }
        public bool ThrowExceptionIfEmpty
        {
            get { return throwExceptionIfEmpty; }
            set { throwExceptionIfEmpty = value; }
        }
		public void Push(Tvalue valor)
		{

			pila.Push(valor);

		}
		public void Push(IEnumerable<Tvalue> valores)
		{
			foreach (Tvalue valor in valores)
				Push(valor);
		}
		public Tvalue Pop()
		{
			Tvalue valorPop = default(Tvalue);
            if (!Empty)
            {

                valorPop = pila.Pop();
            }
            else if (ThrowExceptionIfEmpty)
                throw new Exception("Is empty");
			return valorPop;
		}
		public Tvalue Peek()
		{
			return pila.Peek();
			
		}
		public void Clear()
		{

			pila.Buida();

		}

		public IEnumerator<Tvalue> GetEnumerator()
		{
			return pila.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
