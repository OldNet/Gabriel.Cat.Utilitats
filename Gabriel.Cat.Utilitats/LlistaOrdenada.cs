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
    public delegate bool ConfirmacionEventHandler<TSender, TKey, TValue>(TSender sender, TKey key, TValue value);
    public delegate bool ConfirmationEventHandler<TSender, TKey>(TSender sender, TKey arg);
    public delegate bool ConfirmacionCambioClaveEventHandler<TSender, TKey>(TSender sender, TKey keyAnt,TKey keyNew);
    public delegate bool ConfirmacionEventHandler<TSender>(TSender sender);
    /// <summary>
    /// Is a SortedList multitheread compatible (use Monitor on class)
    /// </summary>
    public class LlistaOrdenada<TKey, TValue> : IDictionary<TKey, TValue>,IReadOnlyDictionary<TKey,TValue>
    {
        SortedList<TKey, TValue> llista;
        public event EventHandler Updated;
        ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>, TKey, TValue> AddConfirmation;
        ConfirmationEventHandler<LlistaOrdenada<TKey, TValue>, TKey> RemoveConfirmation;
        ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> ClearConfirmation;
        ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> UpdateConfirmatin;
        ConfirmacionCambioClaveEventHandler<LlistaOrdenada<TKey, TValue>, TKey> ChangeKeyConfirmation;
        public event EventHandler Added;
        public event EventHandler Removed;
        public LlistaOrdenada()
        {
            llista = new SortedList<TKey, TValue>();

        }
        public LlistaOrdenada(ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>, TKey, TValue> metodoAddConfirmacion, ConfirmationEventHandler<LlistaOrdenada<TKey, TValue>, TKey> metodoRemoveConfirmation, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> metodoClearConfirmation, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> metodoUpdateConfirmation, ConfirmacionCambioClaveEventHandler<LlistaOrdenada<TKey, TValue>, TKey> metodoChangeKeyConfirmation) : this()
        {
            AddConfirmation = metodoAddConfirmacion;
            RemoveConfirmation = metodoRemoveConfirmation;
            ClearConfirmation = metodoClearConfirmation;
            UpdateConfirmatin = metodoUpdateConfirmation;
            ChangeKeyConfirmation = metodoChangeKeyConfirmation;
        }
        public LlistaOrdenada(IEnumerable<KeyValuePair<TKey, TValue>> llistaOrdenada) : this()
        {
            AddRange(llistaOrdenada);
        }
        public LlistaOrdenada(IEnumerable<KeyValuePair<TKey, TValue>> llistaOrdenada, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>, TKey, TValue> metodoAddConfirmacion, ConfirmationEventHandler<LlistaOrdenada<TKey, TValue>, TKey> metodoRemoveConfirmation, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> metodoClearConfirmation, ConfirmacionEventHandler<LlistaOrdenada<TKey, TValue>> metodoUpdateConfirmation, ConfirmacionCambioClaveEventHandler<LlistaOrdenada<TKey, TValue>, TKey> metodoChangeKeyConfirmation) : this(metodoAddConfirmacion, metodoRemoveConfirmation, metodoClearConfirmation,metodoUpdateConfirmation,metodoChangeKeyConfirmation)
        {
            AddRange(llistaOrdenada);
        }
        public int Count
        {
            get
            {
                int count;
                Monitor.Enter(llista);
                count = llista.Count;
                Monitor.Exit(llista);
                return count;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                ICollection<TKey> keys;
                Monitor.Enter(llista);
                keys = llista.KeysToArray();
                Monitor.Exit(llista);
                return keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                ICollection<TValue> values;
                Monitor.Enter(llista);
                values = llista.ValuesToArray();
                Monitor.Exit(llista);
                return values;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                return Keys;
            }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                return Values;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                try
                {
                    Monitor.Enter(llista);
                    if (llista.ContainsKey(key))
                        value = llista[key];
                    else value = default(TValue);
                }
                finally
                {
                    Monitor.Exit(llista);
                }
                return value;
            }
            set
            {
                bool actualizar = UpdateConfirmatin == null || UpdateConfirmatin(this);
                if (actualizar)
                    try
                    {
                        Monitor.Enter(llista);
                        if (llista.ContainsKey(key))
                            llista[key] = value;
                    }
                    finally
                    {
                        Monitor.Exit(llista);
                        if (Updated != null)
                            Updated(this, new EventArgs());
                    }

            }
        }
        public void Add(TKey key, TValue value)
        {
            bool añadir = AddConfirmation == null || AddConfirmation(this, key, value);
            if (añadir)
                try
                {
                    Monitor.Enter(llista);

                    llista.Add(key, value);

                }
                finally
                {
                    Monitor.Exit(llista);

                    if (Updated != null)
                        Updated(this, new EventArgs());
                    if (Added != null)
                        Added(this, new EventArgs());

                }
        }
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> llista)
        {
            foreach (KeyValuePair<TKey, TValue> un in llista)
                AddOrReplace(un.Key, un.Value);
        }
        public void ChangeKey(TKey keyAnt, TKey keyNew)
        {
            if (!ContainsKey(keyAnt))
                throw new Exception("no existeix la clau a remplaçar");
            if (ContainsKey(keyNew))
                throw new Exception("ja s'utilitza la clau");
            if (ChangeKeyConfirmation == null || ChangeKeyConfirmation(this,keyAnt,keyNew))
            {
                Add(keyNew, this[keyAnt]);
                if (ContainsKey(keyNew))//si se ha cancelado no se tien que reemplazar 
                    Remove(keyAnt);
            }
        }
        public bool Remove(TKey key)
        {
            bool fer = RemoveConfirmation == null || RemoveConfirmation(this, key);
            if (fer)
                try
                {
                    Monitor.Enter(llista);
                    fer = llista.Remove(key);
                }
                catch { fer = false; }
                finally
                {
                    Monitor.Exit(llista);
                    if (fer)
                    {
                        if (Updated != null)
                            Updated(this, new EventArgs());
                        if (Removed != null)
                            Removed(this, new EventArgs());
                    }
                }
            return fer;
        }
        public bool[] RemoveRange(IEnumerable<TKey> keysToRemove)
        {
            List<bool> seHaPodidoHacer = new List<bool>();
            if (keysToRemove != null)
                foreach (TKey key in keysToRemove)
                    seHaPodidoHacer.Add(Remove(key));
            return seHaPodidoHacer.ToTaula();
        }

        public void Clear()
        {
            if (Removed == null || ClearConfirmation(this))
                try
                {
                    Monitor.Enter(llista);

                    {

                        llista.Clear();

                    }
                }
                finally
                {
                    Monitor.Exit(llista);

                    if (Updated != null)
                        Updated(this, new EventArgs());
                    if (Removed != null)
                        Removed(this, new EventArgs());

                }
        }
        public void AddOrReplace(TKey clau, TValue nouValor)
        {
            bool hecho = false;
            try
            {
                Monitor.Enter(llista);
                if (llista.ContainsKey(clau))
                {
                    Monitor.Exit(llista);//para poder usarlo en durante el método de validación
                    if (UpdateConfirmatin == null || UpdateConfirmatin(this))
                    {
                        Monitor.Enter(llista);
                        llista[clau] = nouValor;
                        hecho = true;

                    }
                }
                else
                {
                    Monitor.Exit(llista);//para poder usarlo en durante el método de validación
                    if (AddConfirmation == null || AddConfirmation(this, clau, nouValor))
                    {
                        Monitor.Enter(llista);
                        llista.Add(clau, nouValor);
                        if (Added != null)
                            Added(this, new EventArgs());
                        hecho = true;
                    }

                }
            }
            finally
            {
                try
                {
                    Monitor.Exit(llista);
                }
                finally
                {
                    if (hecho)
                    {
                        if (Updated != null)
                            Updated(this, new EventArgs());
                    }
                }

            }
        }
        public bool ContainsKey(TKey key)
        {
            bool existeix;
            Monitor.Enter(llista);
            existeix = llista.ContainsKey(key);
            Monitor.Exit(llista);
            return existeix;
        }
        public bool ContainsValue(TValue value)
        {
            bool existeix;
            Monitor.Enter(llista);
            existeix = llista.ContainsValue(value);
            Monitor.Exit(llista);
            return existeix;
        }

        #region IEnumerable implementation

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            Llista<KeyValuePair<TKey, TValue>> parelles = new Llista<KeyValuePair<TKey, TValue>>();
            Monitor.Enter(llista);
            foreach (KeyValuePair<TKey, TValue> par in llista)
                parelles.Add(par);
            Monitor.Exit(llista);
            return parelles.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            bool exist= ContainsKey(key);

            if (exist)
                value = this[key];
            else
                value = default(TValue);
            
            return exist;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            Monitor.Enter(llista);
            ((IDictionary<TKey, TValue>)llista).CopyTo(array, arrayIndex);
            Monitor.Exit(llista);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }



        #endregion
 
        public int IndexOfKey(TKey key)
        {
            int indexOf;
            Monitor.Enter(llista);
            indexOf = llista.IndexOfKey(key);
            Monitor.Exit(llista);
            return indexOf;
        }

        public int IndexOfValue(TValue value)
        {
            int indexOf;
            Monitor.Enter(llista);
            indexOf = llista.IndexOfValue(value);
            Monitor.Exit(llista);
            return indexOf;
        }
    }

}
