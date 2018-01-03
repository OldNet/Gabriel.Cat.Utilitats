/*
 * Creado por SharpDevelop.
 * Usuario: Pingu
 * Fecha: 11/12/2014
 * Hora: 20:42
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using Gabriel.Cat.Extension;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Gabriel.Cat
{


	public delegate ForContinue ForMethodLlistaOrdenada<TKey, TValue>(int actualIndex, TKey key, TValue value);
	public delegate bool ConfirmacionEventHandler<TSender, TKey, TValue>(TSender sender, TKey key, TValue value);
	public delegate bool ConfirmationEventHandler<TSender, TKey>(TSender sender, TKey arg);
	public delegate bool ConfirmacionCambioClaveEventHandler<TSender, TKey>(TSender sender, TKey keyAnt, TKey keyNew);
	public delegate bool ConfirmacionEventHandler<TSender>(TSender sender);
	/// <summary>
	/// Is a SortedList multitheread compatible (use Monitor on class)
	/// </summary>
	public class LlistaOrdenada<TKey, TValue> :ObjectAutoId ,IDictionary<TKey, TValue>,IReadOnlyDictionary<TKey,TValue>, IList<KeyValuePair<TKey,TValue>>, IReadOnlyList<KeyValuePair<TKey,TValue>>, IColeccion<KeyValuePair<TKey,TValue>>
	{
		protected  SortedList<TKey, TValue> llistaOrdenada;
		internal List<TKey> llista;
		public event EventHandler<DicEventArgs<TKey,TValue>> Updated;
		ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>, TKey, TValue> AddConfirmation;
		ConfirmationEventHandler<LlistaOrdenada<TKey, TValue>, TKey> RemoveConfirmation;
		ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> ClearConfirmation;
		ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> UpdateConfirmatin;
		ConfirmacionCambioClaveEventHandler<LlistaOrdenada<TKey, TValue>, TKey> ChangeKeyConfirmation;
		public event EventHandler<DicEventArgs<TKey,TValue>> Added;
		public event EventHandler<DicEventArgs<TKey,TValue>> Removed;
		public LlistaOrdenada()
		{
			llistaOrdenada = new SortedList<TKey, TValue>();
			llista = new List<TKey>();

		}



		public LlistaOrdenada(ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>, TKey, TValue> metodoAddConfirmacion, ConfirmationEventHandler<LlistaOrdenada<TKey, TValue>, TKey> metodoRemoveConfirmation, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> metodoClearConfirmation, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> metodoUpdateConfirmation, ConfirmacionCambioClaveEventHandler<LlistaOrdenada<TKey, TValue>, TKey> metodoChangeKeyConfirmation)
			: this()
		{
			AddConfirmation = metodoAddConfirmacion;
			RemoveConfirmation = metodoRemoveConfirmation;
			ClearConfirmation = metodoClearConfirmation;
			UpdateConfirmatin = metodoUpdateConfirmation;
			ChangeKeyConfirmation = metodoChangeKeyConfirmation;
		}
		public LlistaOrdenada(IList<KeyValuePair<TKey, TValue>> llistaOrdenada)
			: this()
		{
			AddRange(llistaOrdenada);
		}
		public LlistaOrdenada(IList<KeyValuePair<TKey, TValue>> llistaOrdenada, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>, TKey, TValue> metodoAddConfirmacion, ConfirmationEventHandler<LlistaOrdenada<TKey, TValue>, TKey> metodoRemoveConfirmation, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> metodoClearConfirmation, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> metodoUpdateConfirmation, ConfirmacionCambioClaveEventHandler<LlistaOrdenada<TKey, TValue>, TKey> metodoChangeKeyConfirmation)
			: this(metodoAddConfirmacion, metodoRemoveConfirmation, metodoClearConfirmation, metodoUpdateConfirmation, metodoChangeKeyConfirmation)
		{
			AddRange(llistaOrdenada);
		}
		public int Count {
			get {
				int count;
				Monitor.Enter(llistaOrdenada);
				count = llistaOrdenada.Count;
				Monitor.Exit(llistaOrdenada);
				return count;
			}
		}

		public ICollection<TKey> Keys {
			get {
				ICollection<TKey> keys;
				Monitor.Enter(llista);
				keys = llista.ToArray();
				Monitor.Exit(llista);
				return keys;
			}
		}

		public ICollection<TValue> Values {
			get {
				ICollection<TValue> values;
				Monitor.Enter(llistaOrdenada);
				values = llistaOrdenada.ValuesToArray();
				Monitor.Exit(llistaOrdenada);
				return values;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                //coge la propiedad que devuelve IColeccionable<TKey>
                return Keys;
            }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            { 
                //coge la propiedad que devuelve IColeccionable<TValue>
                return Values;
            }
        }

        public   KeyValuePair<TKey, TValue> this[int index]
        {
            get
            {
                return new KeyValuePair<TKey, TValue>(GetKey(index), GetValue(GetKey(index)));
            }

            set
            {
                if (ContainsKey(value.Key))
                {
                    try
                    {
                        Monitor.Enter(llistaOrdenada);
                        llistaOrdenada[value.Key] = value.Value;
                    }
                    catch { throw; }
                    finally
                    {
                        Monitor.Exit(llistaOrdenada);
                    }

                    if (IndexOfKey(value.Key) != index)
                    {
                        ChangePosicion(IndexOfKey(value.Key), index);
                    }
                }
                else Insert(index, value.Key, value.Value);
            }
        }

        public TValue this[TKey key] {
			get {
				return GetValue(key);
			}
			set {
				SetValue(key, value);

			}
		}

		public void Add(TKey key, TValue value)
		{
			bool añadir = AddConfirmation == null || AddConfirmation(this, key, value);
			if (añadir)
				try {
					Monitor.Enter(llistaOrdenada);

					llistaOrdenada.Add(key, value);
					llista.Add(key);

				} finally {
					Monitor.Exit(llistaOrdenada);

					if (Updated != null)
						Updated(this, new DicEventArgs<TKey,TValue>(key,value));
					if (Added != null)
						Added(this, new DicEventArgs<TKey,TValue>(key,value));

				}
		}
		public void AddRange(IList<KeyValuePair<TKey, TValue>> llista, bool throwException = false)
		{
			if (llista == null)
				throw new ArgumentNullException();
			for (int i = 0; i < llista.Count; i++)
				try {
					Add(llista[i].Key, llista[i].Value);
				} catch {
					if (throwException)
						throw;
				}
		}
		public void AddOrReplaceRange(IList<KeyValuePair<TKey, TValue>> llista)
		{
			if (llista == null)
				throw new ArgumentNullException();
			for (int i = 0; i < llista.Count; i++)
				AddOrReplace(llista[i].Key, llista[i].Value);
		}
		public void ChangeKey(TKey keyAnt, TKey keyNew)
		{
			if (!ContainsKey(keyAnt))
				throw new Exception("no existeix la clau a remplaçar");
			if (ContainsKey(keyNew))
				throw new Exception("ja s'utilitza la clau");
			if (ChangeKeyConfirmation == null || ChangeKeyConfirmation(this, keyAnt, keyNew)) {
				Add(keyNew, this[keyAnt]);
				if (ContainsKey(keyNew))//si se ha cancelado no se tien que reemplazar 
                    Remove(keyAnt);
				if(Updated!=null)
					Updated(this,new DicEventArgs<TKey,TValue>(keyNew,this[keyNew]));
			}
		}
		public bool Remove(TKey key)
		{
			bool fer = RemoveConfirmation == null || RemoveConfirmation(this, key);
			TValue value=this[key];
			if (fer)
				try {
					Monitor.Enter(llistaOrdenada);
					llista.Remove(key);
					fer = llistaOrdenada.Remove(key);
				} catch {
					fer = false;
				} finally {
					Monitor.Exit(llistaOrdenada);
					if (fer) {
						if (Updated != null)
							Updated(this, new DicEventArgs<TKey,TValue>(key,value));
						if (Removed != null)
							Removed(this, new DicEventArgs<TKey,TValue>(key,value));
					}
				}
			return fer;
		}
		public bool[] RemoveRange(IList<TKey> keysToRemove)
		{
			if (keysToRemove == null)
				throw new ArgumentNullException();
			bool[] seHaPodidoHacer = new bool[keysToRemove.Count];
			for (int i = 0; i < keysToRemove.Count; i++)
				seHaPodidoHacer[i] = Remove(keysToRemove[i]);
			return seHaPodidoHacer;
		}

		public void Clear()
		{
			IList<KeyValuePair<TKey,TValue>> items=null;
			if (Removed == null || ClearConfirmation(this))
				try {
					Monitor.Enter(llistaOrdenada);

					{

						if(Removed!=null)
							items=new Llista<KeyValuePair<TKey,TValue>>(this);
						llistaOrdenada.Clear();
						llista.Clear();

					}
				} finally {
					Monitor.Exit(llistaOrdenada);

					if (Updated != null)
						Updated(this, new DicEventArgs<TKey,TValue>(new KeyValuePair<TKey,TValue>[0]));
					if (Removed != null)
						Removed(this, new DicEventArgs<TKey,TValue>(items));

				}
		}
		public void AddOrReplace(TKey clau, TValue nouValor)
		{
			bool hecho = false;
			try {
				Monitor.Enter(llistaOrdenada);
				if (llistaOrdenada.ContainsKey(clau)) {
					Monitor.Exit(llistaOrdenada);//para poder usarlo en durante el método de validación
					if (UpdateConfirmatin == null || UpdateConfirmatin(this)) {
						Monitor.Enter(llistaOrdenada);
						llistaOrdenada[clau] = nouValor;
						hecho = true;

					}
				} else {
					Monitor.Exit(llistaOrdenada);//para poder usarlo en durante el método de validación
					if (AddConfirmation == null || AddConfirmation(this, clau, nouValor)) {
						Monitor.Enter(llistaOrdenada);
						llistaOrdenada.Add(clau, nouValor);
						llista.Add(clau);
						if (Added != null)
							Added(this, new DicEventArgs<TKey,TValue>(clau,nouValor));
						hecho = true;
					}

				}
			} finally {
				try {
					Monitor.Exit(llistaOrdenada);
				} finally {
					if (hecho) {
						if (Updated != null)
							Updated(this, new DicEventArgs<TKey,TValue>(clau,nouValor));
					}
				}

			}
		}
		public void ChangePosicion(int posActual, int posToInsert)
		{
			if (posActual < 0 || posActual >= Count)
				throw new ArgumentOutOfRangeException("posActual");
			if (posToInsert < 0 || posToInsert >= Count)
				throw new ArgumentOutOfRangeException("posToInsert");
			TKey key;
			Monitor.Enter(llista);
			key = llista[posActual];
			llista.RemoveAt(posActual);
			llista.Insert(posToInsert, key);
			Monitor.Exit(llista);
		}
		public void Insert(int index, TKey key, TValue value)
		{
			Add(key, value);
			ChangePosicion(IndexOfKey(key), index);//si pongo como posActual Count-1 puedo tener problemas si se usan threads y hay cola para el add
		}
		public bool ContainsKey(TKey key)
		{
			bool existeix;
			Monitor.Enter(llistaOrdenada);
			existeix = llistaOrdenada.ContainsKey(key);
			Monitor.Exit(llistaOrdenada);
			return existeix;
		}
		public bool ContainsValue(TValue value)
		{
			bool existeix;
			Monitor.Enter(llistaOrdenada);
			existeix = llistaOrdenada.ContainsValue(value);
			Monitor.Exit(llistaOrdenada);
			return existeix;
		}

		#region IEnumerable implementation

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			Llista<KeyValuePair<TKey, TValue>> parelles = new Llista<KeyValuePair<TKey, TValue>>();
			Monitor.Enter(llistaOrdenada);
			for (int i = 0; i < llista.Count; i++)
				parelles.Add(new KeyValuePair<TKey, TValue>(llista[i], llistaOrdenada[llista[i]]));
			Monitor.Exit(llistaOrdenada);
			return parelles.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
		{
			return TryGetValue(key, out value);
		}
		bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
		{
			return TryGetValue(key, out value);
		}
		private bool TryGetValue(TKey key, out TValue value)
		{
			bool exist = ContainsKey(key);

			if (exist)
				value = GetValue(key);
			else
				value = default(TValue);

			return exist;
		}
		public TKey GetKey(int index)
		{
			TKey key;
			try {
				Monitor.Enter(llista);
				key = llista[index];

			} catch {
				throw;
			} finally {
				Monitor.Exit(llista);
			}
			return key;
		}
		public TValue GetValue(TKey key)
		{
			TValue value;
			try {
				Monitor.Enter(llistaOrdenada);
				value = llistaOrdenada[key];
			} catch {
				throw;
			} finally {
				Monitor.Exit(llistaOrdenada);
			}
			return value;
		}
		public void SetValue(TKey key, TValue value)
		{
			bool actualizar = UpdateConfirmatin == null || UpdateConfirmatin(this);
			if (actualizar)
				try {
					Monitor.Enter(llistaOrdenada);
					if (llistaOrdenada.ContainsKey(key))
						llistaOrdenada[key] = value;
				} finally {
					Monitor.Exit(llistaOrdenada);
					if (Updated != null && llistaOrdenada.ContainsKey(key))
						Updated(this, new DicEventArgs<TKey,TValue>(key,value));
				}
		}
		public TValue GetValueAt(int index)
		{
			TValue value;
			try {
				Monitor.Enter(llista);
				value = this[llista[index]];
			} catch {
				throw;
			} finally {
				Monitor.Exit(llista);
			}
			return value;
		}
		public void SetValueAt(int index, TValue value)
		{
           
			try {
				Monitor.Enter(llista);
				this[llista[index]] = value;
			} catch {
				throw;
			} finally {
				Monitor.Exit(llista);
			}
		}


		public void Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return ContainsKey(item.Key);
		}

		 void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			Monitor.Enter(llistaOrdenada);
			((IDictionary<TKey, TValue>)llistaOrdenada).CopyTo(array, arrayIndex);
			Monitor.Exit(llistaOrdenada);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			return Remove(item.Key);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="methodInnerFor"></param>
		/// <param name="leftToRight"> i ;lt llista.Count or i ;gt= 0 </llista.Count> </param>
		public void For(ForMethodLlistaOrdenada<TKey,TValue> methodInnerFor, bool leftToRight = true)
		{
			if (methodInnerFor == null)
				throw new ArgumentNullException("methodInnerFor");
			ForContinue nextStep = new ForContinue();
			try {
				Monitor.Enter(llista);
				Monitor.Enter(llistaOrdenada);
				if (leftToRight) {
					for (int i = 0; i < llista.Count && !nextStep.Finished; i += nextStep.IncrementOrDecrement) {
						nextStep = methodInnerFor(i, llista[i], llistaOrdenada[llista[i]]);
					}

				} else {
					for (int i = llista.Count - 1; i >= 0 && !nextStep.Finished; i -= nextStep.IncrementOrDecrement) {
						nextStep = methodInnerFor(i, llista[i], llistaOrdenada[llista[i]]);
					}
				}
			} catch {
				throw;
			} finally {
				Monitor.Exit(llista);
				Monitor.Exit(llistaOrdenada);
			}
		}


		#endregion
 
		public int IndexOfKey(TKey key)
		{
			int indexOf;
			Monitor.Enter(llista);
			indexOf = llista.IndexOf(key);
			Monitor.Exit(llista);
			return indexOf;
		}

		int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item)
		{
			return IndexOfKey(item.Key);
		}
		void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item)
		{

			Insert(index,item.Key,item.Value);
		}
	
    	public	void RemoveAt(int index)
		{
    		if(index<0||index>=Count)
    			throw new ArgumentOutOfRangeException("index");
    		TKey key;
    		Monitor.Enter(llista);
    		key=llista[index];
    		Monitor.Exit(llista);
    		Remove(key);
		}

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

    

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

    }
	public class LlistaOrdenada<T>:LlistaOrdenada<IComparable,T> where T:IComparable
	{
		public void Add(T value)
		{
			base.Add(value, value);
		}
		public void AddRange(IList<T> values, bool throwException = false)
		{
			if (values == null)
				throw new ArgumentNullException();
			for (int i = 0; i < values.Count; i++)
				try {
					base.Add(values[i], values[i]);
				} catch {
					if (throwException)
						throw;
				}
		}
		public void AddOrReplaceRange(IList<T> values, bool throwException = false)
		{
			if (values == null)
				throw new ArgumentNullException();
			for (int i = 0; i < values.Count; i++)
				try {
					base.AddOrReplace(values[i], values[i]);
				} catch {
					if (throwException)
						throw;
				}
		}
		public bool Remove(T value)
		{
			return base.Remove(value);
		}
		public bool[] Remove(IList<T> values)
		{
			if (values == null)
				throw new ArgumentNullException();
			bool[] resultRemove = new bool[values.Count];
			for (int i = 0; i < values.Count; i++)
				resultRemove[i] = base.Remove(values[i]);
			return resultRemove;
		}
	}
	
	public class DicEventArgs<TKey,TValue>:EventArgs
	{
		IList<KeyValuePair<TKey,TValue>> items;

		public DicEventArgs(TKey key, TValue value):this(new KeyValuePair<TKey,TValue>[]{new KeyValuePair<TKey,TValue>(key,value)})
		{
		
		}

		public DicEventArgs(IList<KeyValuePair<TKey, TValue>> items)
		{
			this.items=items;
		}

		public IList<KeyValuePair<TKey, TValue>> Items {
			get {
				return items;
			}
		}
	}

}
