/*
 * Creado por SharpDevelop.
 * Usuario: Gabriel
 * Fecha: 10/01/2016
 * Hora: 1:58
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using Gabriel.Cat.Extension;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Gabriel.Cat
{
	/// <summary>
	/// Es una lista hash para ir mas rapido
	/// </summary>
	public class ListaUnica<T>:IList<T>,IReadOnlyList<T>
        where T:IClauUnicaPerObjecte
	{
		LlistaOrdenada<IComparable,T> listaOrdenada;
        Llista<T> lista;
        public ListaUnica()
		{
			listaOrdenada=new LlistaOrdenada<IComparable, T>();
            lista = new Llista<T>();
		}

        public int Count
        {
            get { return listaOrdenada.Count; }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public T this[int pos]
        {
            get { return lista[pos]; }
            set { lista[pos] = value; }
        }
        public T this[IComparable key]
        {
            get {
                return listaOrdenada[key];
            }
        }
        public void Clear()
        {
            listaOrdenada.Clear();
            lista.Clear();
        }
        #region Existe
        public bool Contains(T obj)
		{
			return listaOrdenada.ContainsKey(obj.Clau());
		}
		public bool Contains(IComparable key)
		{
			return listaOrdenada.ContainsKey(key);
		}
		#endregion
		#region Elimina
		public bool[] RemoveRange(IEnumerable<T> objs)
		{
			if(objs==null)
				throw new NullReferenceException("Se esperaba una coleccion de objetos para quitar");
			List<bool> resultados=new List<bool>();
			foreach(T obj in objs)
				resultados.Add(Remove(obj));
			return resultados.ToArray();
			
		}
		public bool Remove(T obj)
		{
			return RemoveKey(obj.Clau());
		}
		public bool RemoveKey(IComparable key)
		{
            if(listaOrdenada.ContainsKey(key))
               lista.Remove(listaOrdenada[key]);
			return listaOrdenada.Remove(key);
		}
		public bool[] RemoveKeyRange(IEnumerable<IComparable> objs)
		{
			
			
			List<bool> resultados=new List<bool>();
            if (objs != null)
                foreach (IComparable obj in objs)
				resultados.Add(RemoveKey(obj));
			return resultados.ToArray();
			
		}

		#endregion
		#region Añadir
		public void Add(T obj)
		{
            if (obj == null) throw new ArgumentNullException("item");
            if (listaOrdenada.ContainsKey(obj.Clau()))
                throw new ArgumentException("Is already on the list");
            else {
                listaOrdenada.Add(obj.Clau(), obj);
                lista.Add(obj);
            }
		}
		public void AddRange(IEnumerable<T> listObj)
		{
			if(listObj==null)
				throw new ArgumentNullException("listObj");
			foreach(T obj in listObj)
				Add(obj);
		}


        #endregion

        public IEnumerator<T> GetEnumerator()
        {
            return lista.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return lista.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (listaOrdenada.ContainsKey(((IClauUnicaPerObjecte)item).Clau()))
                lista.Insert(index, item);
            else throw new ArgumentException("item","item is already on list");
        }

        public void RemoveAt(int index)
        {
               Remove(lista[index]);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lista.CopyTo(array, arrayIndex);
        }
    }

}
