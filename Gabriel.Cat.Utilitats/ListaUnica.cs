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
	public class ListaUnica<T>:IEnumerable<T>
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
        public void Vaciar()
        {
            listaOrdenada.Clear();
            lista.Clear();
        }
        #region Existe
        public bool Existe(T obj)
		{
			return listaOrdenada.ContainsKey(obj.Clau());
		}
		public bool Existe(IComparable key)
		{
			return listaOrdenada.ContainsKey(key);
		}
		public bool ExisteObjeto(T obj)
		{
			return listaOrdenada.ContainsKey(obj.Clau());
		}
		public bool ExisteClave(IComparable key)
		{
			return listaOrdenada.ContainsKey(key);
		}
		#endregion
		#region Elimina
		public bool[] Elimina(IEnumerable<T> objs)
		{
			if(objs==null)
				throw new NullReferenceException("Se esperaba una coleccion de objetos para quitar");
			List<bool> resultados=new List<bool>();
			foreach(T obj in objs)
				resultados.Add(Elimina(obj));
			return resultados.ToArray();
			
		}
		public bool Elimina(T obj)
		{
            lista.Remove(obj);
			return Elimina(obj.Clau());
		}
		public bool[] Elimina(IEnumerable<IComparable> objs)
		{
			if(objs==null)
				throw new NullReferenceException("Se esperaba una coleccion de objetos para quitar");
			List<bool> resultados=new List<bool>();
			foreach(T obj in objs)
				resultados.Add(Elimina(obj));
			return resultados.ToArray();
			
		}
		public bool Elimina(IComparable key)
		{
			return listaOrdenada.Remove(key);
		}
		public bool[] EliminaObjeto(IEnumerable<T> objs)
		{
			if(objs==null)
				throw new NullReferenceException("Se esperaba una coleccion de objetos para quitar");
			List<bool> resultados=new List<bool>();
			foreach(T obj in objs)
				resultados.Add(EliminaObjeto(obj));
			return resultados.ToArray();
			
		}
		public bool EliminaObjeto(T obj)
		{
			return Elimina(obj.Clau());
		}
		public bool[] EliminaClave(IEnumerable<IComparable> objs)
		{
			if(objs==null)
				throw new NullReferenceException("Se esperaba una coleccion de objetos para quitar");
			List<bool> resultados=new List<bool>();
			foreach(IComparable obj in objs)
				resultados.Add(EliminaClave(obj));
			return resultados.ToArray();
			
		}
		public bool EliminaClave(IComparable key)
		{
			return Elimina(key);
		}
		#endregion
		#region Añadir
		public void Añadir(T obj)
		{
            if (listaOrdenada.ContainsKey(obj.Clau()))
                throw new ArgumentException("Ya existe en la lista");
            else {
                listaOrdenada.Add(obj.Clau(), obj);
                lista.Add(obj);
            }
		}
		public void Añadir(IEnumerable<T> listObj)
		{
			if(listObj==null)
				throw new NullReferenceException("Se esperaba una coleccion de objetos para añadir");
			foreach(T obj in listObj)
				Añadir(obj);
		}

        public IEnumerator<T> GetEnumerator()
        {
            return lista.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion


	}

}
