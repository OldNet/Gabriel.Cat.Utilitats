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
		LlistaOrdenada<IComparable,T> list;

        public ListaUnica()
		{
			list=new LlistaOrdenada<IComparable, T>();
		}

        public int Count
        {
            get { return list.Count; }
        }
        public T this[IComparable key]
        {
            get {
                return list[key];
            }
        }
        public void Vaciar()
        {
            list.Buida();
        }
        #region Existe
        public bool Existe(T obj)
		{
			return list.Existeix(obj.Clau());
		}
		public bool Existe(IComparable key)
		{
			return list.Existeix(key);
		}
		public bool ExisteObjeto(T obj)
		{
			return list.Existeix(obj.Clau());
		}
		public bool ExisteClave(IComparable key)
		{
			return list.Existeix(key);
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
			return list.Elimina(key);
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
			if(list.Existeix(obj.Clau()))
			   throw new ArgumentException("Ya existe en la lista");
			else
				list.Afegir(obj.Clau(),obj);
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
            return list.ValuesToArray().ObtieneEnumerador();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion


	}

}
