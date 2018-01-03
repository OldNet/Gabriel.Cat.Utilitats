/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 11/12/2014
 * Hora: 21:00
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using Gabriel.Cat.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Gabriel.Cat
{

    /// <summary>
    /// Description of Llista.
    /// </summary>
    public class Llista<T> :ObjectAutoId, IList<T>, IReadOnlyList<T>, IColeccion<T>
    {
        List<T> llista;
        Semaphore semafor;
        public bool Whaiting { get; private set; }
        SortedList<IComparable, T> llistaOrdenada;//sirve para facilitar el encontrar los valores para que sea lo mas ágil posible
        /// <summary>
        /// Ocurre cuando la lista es modificada ya sea en el numero de elementos o en el orden
        /// </summary>
        public event EventHandler<ListEventArgs<T>> Updated;
        public event EventHandler<ListEventArgs<T>> Added;
        public event EventHandler<ListEventArgs<T>> Removed;
        ConfirmationEventHandler<Llista<T>, T> AddConfirmation;
        ConfirmationEventHandler<Llista<T>, IEnumerable<T>> AddRangeConfirmation;
        ConfirmationEventHandler<Llista<T>, T> RemoveConfirmation;
        ConfirmationEventHandler<Llista<T>, IEnumerable<T>> ReomveRangeConfirmation;
        ConfirmacionEventHandler<Llista<T>> ClearConfirmation;
        ConfirmacionEventHandler<Llista<T>> UpdateConfirmation;
        public Llista()
        {
            Whaiting = false;
            llista = new List<T>();
            if (typeof(T).GetInterface("IClauUnicaPerObjecte") != null)
                llistaOrdenada = new SortedList<IComparable, T>();
            semafor = new Semaphore(1, 1);
        }
        public Llista(ConfirmationEventHandler<Llista<T>, T> metodoAddConfirmation, ConfirmationEventHandler<Llista<T>, IEnumerable<T>> metodoAddRangeConfirmation, ConfirmationEventHandler<Llista<T>, T> metodoRemoveConfirmation, ConfirmationEventHandler<Llista<T>, IEnumerable<T>> metodoReomveRangeConfirmation, ConfirmacionEventHandler<Llista<T>> metodoUpdateConfirmation, ConfirmacionEventHandler<Llista<T>> metodoClearConfirmation) : this()
        {
            UpdateConfirmation = metodoUpdateConfirmation;
            AddConfirmation = metodoAddConfirmation;
            AddRangeConfirmation = metodoAddRangeConfirmation;
            RemoveConfirmation = metodoRemoveConfirmation;
            ReomveRangeConfirmation = metodoReomveRangeConfirmation;
            ClearConfirmation = metodoClearConfirmation;
        }
        public Llista(IEnumerable<T> valors)
            : this()
        {
            AddRange(valors);
        }
        public Llista(IEnumerable<T> valors, ConfirmationEventHandler<Llista<T>, T> metodoAddConfirmation, ConfirmationEventHandler<Llista<T>, IEnumerable<T>> metodoAddRangeConfirmation, ConfirmationEventHandler<Llista<T>, T> metodoRemoveConfirmation, ConfirmationEventHandler<Llista<T>, IEnumerable<T>> metodoReomveRangeConfirmation, ConfirmacionEventHandler<Llista<T>> metodoUpdateConfirmation, ConfirmacionEventHandler<Llista<T>> metodoClearConfirmation) : this(metodoAddConfirmation, metodoAddRangeConfirmation, metodoRemoveConfirmation, metodoReomveRangeConfirmation, metodoUpdateConfirmation, metodoClearConfirmation)
        {
            AddRange(valors);
        }
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
        public int Count
        {
            get
            {
                int count;
                semafor.WaitOne();
                Whaiting = true;
                count = llista.Count;
                semafor.Release();
                Whaiting = false;
                return count;
            }
        }

        int ICollection<T>.Count
        {
            get
            {
                return Count;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return IsReadOnly;
            }
        }

        T IColeccion<T>.this[int pos]
        {
            get
            {
                return this[pos];
            }
            set
            {
                this[pos] = value;
            }
        }

        public T this[int pos]
        {
            get
            {
                T value = default(T);
                try
                {
                    semafor.WaitOne();
                    Whaiting = true;
                    value = llista[pos];
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    try
                    {
                        semafor.Release();
                    }
                    catch { }
                    finally
                    {
                        Whaiting = false;

                    }
                }
                return value;

            }
            set
            {

                if (UpdateConfirmation == null || UpdateConfirmation(this))
                    try
                    {
                        semafor.WaitOne();
                        Whaiting = true;
                        llista[pos] = value;
                    }
                    finally
                    {
                        try
                        {
                            semafor.Release();
                        }
                        catch { }
                        finally
                        {
                            Whaiting = false;
                            if (Updated != null)
                            	Updated(this,new ListEventArgs<T>(this,value));
                        }
                    }
            }
        }
        public void Add(T value)
        {

            IClauUnicaPerObjecte clau;
            if (AddConfirmation == null || AddConfirmation(this, value))
                try
                {
                    semafor.WaitOne();
                    Whaiting = true;
                    llista.Add(value);
                    if (llistaOrdenada != null)
                    {
                        clau = (IClauUnicaPerObjecte)value;
                        if (!llistaOrdenada.ContainsKey(clau.Clau))
                            llistaOrdenada.Add(clau.Clau, value);
                        else
                        {
                            llistaOrdenada[clau.Clau] = value;
                        }
                    }
                }
                finally
                {
                    semafor.Release();
                    Whaiting = false;
                    if (Updated != null)
                    	Updated(this, new ListEventArgs<T>(this,value));
                    if (Added != null)
                        Added(this, new ListEventArgs<T>(this,value));
                }
        }
        public void AddRange(IEnumerable<T> values)
        {
            IClauUnicaPerObjecte clau;
            if (values != null)
                if (AddRangeConfirmation == null || AddRangeConfirmation(this, values))
                    try
                    {
                        semafor.WaitOne();
                        Whaiting = true;
                        llista.AddRange(values);
                        if (llistaOrdenada != null)
                        {
                            foreach (T value in values)
                            {
                                clau = (IClauUnicaPerObjecte)value;
                                if (!llistaOrdenada.ContainsKey(clau.Clau))
                                    llistaOrdenada.Add(clau.Clau, value);
                                else llistaOrdenada[clau.Clau] = value;
                            }

                        }
                    }
                    finally
                    {
                        semafor.Release();
                        Whaiting = false;
                        if (Updated != null)
                            Updated(this,new ListEventArgs<T>(this,values));
                        if (Added != null)
                            Added(this,new ListEventArgs<T>(this,values));
                    }
        }
        public bool Remove(T value)
        {
            bool removeDone = false;
            if (RemoveConfirmation == null || RemoveConfirmation(this, value))
                try
                {
                    semafor.WaitOne();
                    Whaiting = true;
                    if (llistaOrdenada != null)
                        llistaOrdenada.Remove((value as IClauUnicaPerObjecte).Clau);
                    removeDone = llista.Remove(value);
                }
                finally
                {
                    semafor.Release();
                    Whaiting = false;
                    if (Updated != null)
                        Updated(this, new ListEventArgs<T>(this,value));
                    if (Removed != null)
                        Removed(this, new ListEventArgs<T>(this,value));

                }
            return removeDone;
        }
        public bool[] RemoveRange(IEnumerable<T> values)
        {
            List<bool> result = new List<bool>();
            if (values != null)
                if (ReomveRangeConfirmation == null || ReomveRangeConfirmation(this, values))
                    try
                    {
                        semafor.WaitOne();
                        Whaiting = true;
                        if (llistaOrdenada != null)
                        {
                            foreach (T value in values)
                            {
                                llistaOrdenada.Remove((value as IClauUnicaPerObjecte).Clau);
                                result.Add(llista.Remove(value));
                            }
                        }
                        else
                        {
                            foreach (T value in values)
                                result.Add(llista.Remove(value));
                        }
                    }
                    finally
                    {
                        semafor.Release();
                        Whaiting = false;
                        if (Updated != null)
                            Updated(this, new ListEventArgs<T>(this,values));
                        if (Removed != null)
                            Removed(this, new ListEventArgs<T>(this,values));
                    }
            return result.ToArray();
        }
        public void RemoveAt(int pos)
        {
        	T value=llista[pos];
            if (RemoveConfirmation == null || RemoveConfirmation(this, this[pos]))
                try
                {
                    semafor.WaitOne();
                    Whaiting = true;
                    if (llistaOrdenada != null)
                        llistaOrdenada.Remove((value as IClauUnicaPerObjecte).Clau);
                    llista.RemoveAt(pos);
                }
                finally
                {
                    semafor.Release();
                    Whaiting = false;
                    if (Updated != null)
                        Updated(this, new ListEventArgs<T>(this,value));
                    if (Removed != null)
                        Removed(this,new ListEventArgs<T>(this,value));
                }
        }
        public T FirstOrDefault(IComparable keyToFindItem)
        {
            T value;
            semafor.WaitOne();
            Whaiting = true;
            if (llistaOrdenada == null)
            {
                value = llista.FirstOrDefault((item) =>
                {
            	  return Equals(keyToFindItem,item);
                });
            }
            else
            {
                value = llistaOrdenada[keyToFindItem];
            }
            semafor.Release();
            Whaiting = false;
            return value;
        }
        public int IndexOf(T value)
        {
            int index;
            semafor.WaitOne();
            Whaiting = true;
            index = llista.IndexOf(value);
            Whaiting = false;
            semafor.Release();
            return index;
        }
        public bool Contains(T value)
        {
            bool existeix = false;

            try
            {
                semafor.WaitOne();
                Whaiting = true;
                if (llistaOrdenada != null)
                    existeix = llistaOrdenada.ContainsKey(((IClauUnicaPerObjecte)value).Clau);
                else
                    existeix = llista.Contains(value);
            }
            finally
            {
                try
                {
                    semafor.Release();
                }
                finally
                {
                    Whaiting = false;
                }
            }
            return existeix;

        }
        public bool Contains(IComparable value)
        {
            bool existeix = false;

            try
            {
                semafor.WaitOne();
                Whaiting = true;
                if (llistaOrdenada != null)
                    existeix = llistaOrdenada.ContainsKey(value);
                else
                    existeix = llista.Contains(value);
            }
            finally
            {
                try
                {
                    semafor.Release();
                }
                finally
                {
                    Whaiting = false;
                }
            }
            return existeix;

        }
        public void Sort()
        {
            Sort(Comparer<T>.Default);//por mirar si funciona
        }
        public void Sort(IComparer<T> comparador)
        {
            if (UpdateConfirmation == null || UpdateConfirmation(this))
                try
                {
                    semafor.WaitOne();
                    Whaiting = true;
                    llista.Sort(comparador);
                }
                finally
                {
                    try
                    {
                        semafor.Release();
                    }
                    finally
                    {
                        Whaiting = false;
                        if (Updated != null)
                            Updated(this, new ListEventArgs<T>(this,this));
                    }
                }
        }
        public void Disorder()
        {
            if (UpdateConfirmation == null || UpdateConfirmation(this))
                try
                {
                    semafor.WaitOne();
                    Whaiting = true;
                    IEnumerable<T> desorden = llista.Desordena();
                    this.llista.Clear();
                    this.llista.AddRange(desorden);
                }
                finally
                {
                    semafor.Release();
                    Whaiting = false;
                    if (Updated != null)
                        Updated(this, new ListEventArgs<T>(this,this));
                }
        }

        public void Clear()
        {
            if (ClearConfirmation == null || ClearConfirmation(this))
            {
                semafor.WaitOne();
                Whaiting = true;
                llista.Clear();
                if (llistaOrdenada != null)
                    llistaOrdenada.Clear();
                semafor.Release();
                Whaiting = false;

                if (Updated != null)
                    Updated(this, new ListEventArgs<T>(this));
                if (Removed != null)
                    Removed(this, new ListEventArgs<T>(this));
            }
        }

        public void Insert(int posicio, T valor)
        {
            if (AddConfirmation == null || AddConfirmation(this, valor))
                try
                {
                    semafor.WaitOne();
                    Whaiting = true;
                    llista.Insert(posicio, valor);
                }
                finally
                {
                    try
                    {
                        semafor.Release();
                    }
                    finally
                    {
                        Whaiting = false;
                        if (Updated != null)
                        	Updated(this, new ListEventArgs<T>(this,valor));
                        if (Added != null)
                            Added(this, new ListEventArgs<T>(this,valor));
                    }
                }
        }
        public void Push(T valor)
        {
            Insert(0, valor);
        }

        public T Peek()
        {
            T peek = default(T);
            try
            {
                semafor.WaitOne();
                Whaiting = true;
                peek = llista[0];
            }
            catch { }
            finally
            {
                semafor.Release();
                Whaiting = false;
            }
            return peek;
        }
        public T Pop()
        {
            T primero = Peek();
            RemoveAt(0);
            return primero;
        }


        #region Interficies implementation

        public IEnumerator<T> GetEnumerator()
        {
            T valor;
            T[] taula;
            semafor.WaitOne();
            Whaiting = true;
            taula = llista.ToArray();
            semafor.Release();
            Whaiting = false;
            for (int i = 0, taulaLength = taula.Length; i < taulaLength; i++)
            {
                valor = taula[i];
                yield return valor;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            try
            {
                semafor.WaitOne();
                Whaiting = true;
                llista.CopyTo(array, arrayIndex);
            }
            finally
            {
                Whaiting = false;
                semafor.Release();
            }

        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        void ICollection<T>.Clear()
        {
            Clear();
        }

        bool ICollection<T>.Contains(T item)
        {
            return Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            return Remove(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
    public class ListEventArgs<T>:EventArgs
    {
    	IList<T> items;
    	object list;
    	public ListEventArgs(object list):this(list,new T[0])
    	{}
    	public ListEventArgs(object list,T item):this(list,new T[]{item})
    	{}
    	public ListEventArgs(object list,IEnumerable<T> items):this(list,items.ToTaula())
    	{}
    	public ListEventArgs(object list,IList<T> items)
    	{
    		this.list=list;
    		this.items=items;
    	}
		public object List {
			get {
				return list;
			}
		}
		public IList<T> Items {
			get {
				return items;
			}
		}
    }
 

}
