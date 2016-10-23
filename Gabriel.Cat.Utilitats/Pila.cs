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

namespace Gabriel.Cat
{
	/// <summary>
	/// Description of Pila.
	/// </summary>
	public class Pila<T>:IList<T>,IColeccion<T>
	{
        bool throwExceptionIfEmpty = false;
		Llista<T> pila;
		public Pila()
		{
      
			pila = new Llista<T>();
		}
        public Pila(IEnumerable<T> valors)
            :this(){Push(valors);}
		public int Count
		{ get { return pila.Count; } }
		public bool Empty
		{ get { return Count == 0; } }
		public T this[int index]
		{ get { return pila[index]; }
			set { pila[index] = value; } }
        public bool ThrowExceptionIfEmpty
        {
            get { return throwExceptionIfEmpty; }
            set { throwExceptionIfEmpty = value; }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Push(T valor)
		{

			pila.Push(valor);

		}
		public void Push(IEnumerable<T> valores)
		{
			foreach (T valor in valores)
				Push(valor);
		}
		public T Pop()
		{
			T valorPop = default(T);
            if (!Empty)
            {

                valorPop = pila.Pop();
            }
            else if (ThrowExceptionIfEmpty)
                throw new Exception("Is empty");
			return valorPop;
		}
		public T Peek()
		{
			return pila.Peek();
			
		}
		public void Clear()
		{

			pila.Clear();

		}

		public IEnumerator<T> GetEnumerator()
		{
			return pila.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

        public int IndexOf(T item)
        {
            return pila.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            pila.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            pila.RemoveAt(index);
        }

        public void Add(T item)
        {
            pila.Add(item);
        }

        public bool Contains(T item)
        {
           return pila.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            pila.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return pila.Remove(item);
        }

        public static implicit operator Llista<T>(Pila<T> stack)
        {
            return stack.pila;
        }
        public static implicit operator Pila<T>(Llista<T> stack)
        {
            return new Pila<T>() {pila=stack };
        }
    }
}
