/*
 * Creado por SharpDevelop.
 * Usuario: pc
 * Fecha: 07/04/2015
 * Hora: 16:48
 * 
 * Para cambiar esta plantilla use Herramientas | Opciones | Codificación | Editar Encabezados Estándar
 */
using Gabriel.Cat.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Gabriel.Cat
{
    public delegate ForContinue ForMethodTwoKeysList<Tkey1, Tkey2, Tvalue>(int index, Tkey1 key1, Tkey2 key2, Tvalue value);
    /// <summary>
    /// Description of TwoKeysList.
    /// </summary>
    public class TwoKeysList<TKey1, TKey2, TValue> : IDictionary<TwoKeys<TKey1, TKey2>, TValue>, IEnumerable<KeyValuePair<TwoKeys<TKey1, TKey2>, TValue>>, IList<KeyValuePair<TwoKeys<TKey1, TKey2>, TValue>>, IReadOnlyList<KeyValuePair<TwoKeys<TKey1, TKey2>, TValue>>, IColeccion<KeyValuePair<TwoKeys<TKey1, TKey2>, TValue>>
                                                        where TKey1 : IComparable<TKey1>
                                                        where TKey2 : IComparable<TKey2>
    {

        LlistaOrdenada<TKey1, TValue> llista1;
        LlistaOrdenada<TKey2, TValue> llista2;
        LlistaOrdenada<TKey2, TKey1> llistaClau2;
        LlistaOrdenada<TKey1, TKey2> llistaClau1;
        public TwoKeysList()
        {
            llista1 = new LlistaOrdenada<TKey1, TValue>();
            llista2 = new LlistaOrdenada<TKey2, TValue>();
            llistaClau1 = new LlistaOrdenada<TKey1, TKey2>();
            llistaClau2 = new LlistaOrdenada<TKey2, TKey1>();

        }
        public int Count
        { get { return llistaClau1.Count; } }

        public ICollection<TwoKeys<TKey1, TKey2>> Keys
        {
            get
            {
                return KeysToArray();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return llista1.Values;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }



        public KeyValuePair<TwoKeys<TKey1, TKey2>, TValue> this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public TValue this[TwoKeys<TKey1, TKey2> key]
        {
            get
            {
                return llista1[key.Key1];
            }

            set
            {
                llista1[key.Key1] = value;
                llista2[llistaClau1[key.Key1]] = value;
            }
        }
        public TValue this[TKey1 key1]
        {
            get { return llista1[key1]; }
            set
            {

                llista1[key1] = value;
                llista2[llistaClau1[key1]] = value;
            }
        }
        public TValue this[TKey2 key2]
        {
            get { return llista2[key2]; }
            set
            {
                llista2[key2] = value;
                llista1[llistaClau2[key2]] = value;
            }
        }

        public void Add(TKey1 key1, TKey2 key2, TValue value)
        {
            if (llista1.ContainsKey(key1))
                throw new Exception("Esta duplicada la clave1 para el valor");
            if (llista2.ContainsKey(key2))
                throw new Exception("Esta duplicada la clave2 para el valor");
            llista1.Add(key1, value);
            llista2.Add(key2, value);
            llistaClau1.Add(key1, key2);
            llistaClau2.Add(key2, key1);
        }
        public bool Remove1(TKey1 key1)
        {
            TKey2 key2 = llistaClau1[key1];
            bool removed;
            llistaClau1.Remove(key1);
            llistaClau2.Remove(key2);
            removed = llista1.Remove(key1);
            llista2.Remove(key2);
            return removed;
        }
        public bool Remove2(TKey2 key2)
        {
            TKey1 key1 = llistaClau2[key2];
            bool removed;
            llistaClau1.Remove(key1);
            llistaClau2.Remove(key2);
            removed = llista1.Remove(key1);
            llista2.Remove(key2);
            return removed;
        }
        public TValue GetValueAt(int index)
        {
            return llista1.GetValueAt(index);
        }
        public TKey1 GetTKey1At(int index)
        {
            return llistaClau2.GetValueAt(index);
        }
        public TKey2 GetTKey2At(int index)
        {
            return llistaClau1.GetValueAt(index);
        }
        public TValue GetValueWithKey1(TKey1 key)
        {
            return llista1[key];
        }
        public TValue GetValueWithKey2(TKey2 key)
        {
            return llista2[key];
        }
        public TKey1 GetTkey1WhithTkey2(TKey2 key2)
        {
            return llistaClau2[key2];
        }
        public TKey2 GetTkey2WhithTkey1(TKey1 key1)
        {
            return llistaClau1[key1];
        }

        public void ChangeKey2(TKey2 key2Old, TKey2 key2New)
        {
            if (key2Old.CompareTo(key2New) != 0)
            {
                if (ContainsKey2(key2New))
                    throw new Exception("new key2 is already in use");
                TKey1 key1 = llistaClau2[key2Old];
                TValue value = llista2[key2Old];
                Remove2(key2Old);
                Add(key1, key2New, value);
            }
        }
        public void ChangeKey1(TKey1 key1Old, TKey1 key1New)
        {
            if (key1Old.CompareTo(key1New) != 0)
            {
                if (ContainsKey1(key1New))
                    throw new Exception("new key1 is already in use");
                TKey2 key2 = llistaClau1[key1Old];
                TValue value = llista1[key1Old];
                Remove1(key1Old);
                Add(key1New, key2, value);
            }
        }

        public void Clear()
        {
            llistaClau1.Clear();
            llista2.Clear();
            llistaClau2.Clear();
            llista1.Clear();
        }

        public TValue[] ValueToArray()
        {
            return llista1.GetValues();
          
        }
        public TKey1[] Key1ToArray()
        {
            return llistaClau1.GetKeys();
        }
        public TKey2[] Key2ToArray()
        {
            return llistaClau2.GetKeys();
        }
        public KeyValuePair<TKey1, TValue>[] Key1ValuePair()
        {
            return llista1.ToArray();
        }
        public KeyValuePair<TKey2, TValue>[] Key2ValuePair()
        {
            return llista2.ToArray();
        }
        public TwoKeys<TKey1, TKey2>[] KeysToArray()
        {
            KeyValuePair<TKey1, TKey2>[] keys = llistaClau1.ToArray();
            TwoKeys<TKey1, TKey2>[] twoKeys = new TwoKeys<TKey1, TKey2>[keys.Length];
            for (int i = 0; i < keys.Length; i++)
                twoKeys[i] = new TwoKeys<TKey1, TKey2>(keys[i].Key, keys[i].Value);
            return twoKeys;
        }
        public bool ContainsKey1(TKey1 key1)
        {
            return llistaClau1.ContainsKey(key1);
        }
        public bool ContainsKey2(TKey2 key2)
        {
            return llistaClau2.ContainsKey(key2);
        }
        public IEnumerator<KeyValuePair<TwoKeys<TKey1, TKey2>, TValue>> GetEnumerator()
        {
            KeyValuePair<TwoKeys<TKey1, TKey2>, TValue>[] enumerator = new KeyValuePair<TwoKeys<TKey1, TKey2>, TValue>[llista1.Count];
            ForContinue nextStep = new ForContinue();
            For((i, tkey1, tkey2, tvalue) =>
            {
                enumerator[i] = new KeyValuePair<TwoKeys<TKey1, TKey2>, TValue>(new TwoKeys<TKey1, TKey2>(tkey1, tkey2), tvalue);
                return nextStep;
            });
            return enumerator.ObtieneEnumerador();
        }


        public bool ContainsKey(TwoKeys<TKey1, TKey2> key)
        {
            return llistaClau1.ContainsKey(key.Key1) && llistaClau2.ContainsKey(key.Key2);
        }

        public void Add(TwoKeys<TKey1, TKey2> key, TValue value)
        {
            Add(key.Key1, key.Key2, value);
        }

        public bool Remove(TwoKeys<TKey1, TKey2> key)
        {
            return Remove1(key.Key1);
        }

        bool IDictionary<TwoKeys<TKey1, TKey2>, TValue>.TryGetValue(TwoKeys<TKey1, TKey2> key, out TValue value)
        {
            return ((IDictionary<TKey1, TValue>)llista1).TryGetValue(key.Key1, out value);
        }

        public void Add(KeyValuePair<TwoKeys<TKey1, TKey2>, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TwoKeys<TKey1, TKey2>, TValue> item)
        {
            return llista1.ContainsKey(item.Key.Key1);
        }

        private void CopyTo(KeyValuePair<TwoKeys<TKey1, TKey2>, TValue>[] array, int arrayIndex = 0)
        {
            ForContinue nextStep = new ForContinue();
            For((i, tkey1, tkey2, tvalue) =>
            {
                array[arrayIndex++] = new KeyValuePair<TwoKeys<TKey1, TKey2>, TValue>(new TwoKeys<TKey1, TKey2>(tkey1, tkey2), tvalue);
                return nextStep;
            });
        }

        public void RemoveAt(int index)
        {
            Remove1(GetTKey1At(index));
        }

        public bool Remove(KeyValuePair<TwoKeys<TKey1, TKey2>, TValue> item)
        {
            return Remove1(item.Key.Key1);
        }



        public void For(ForMethodTwoKeysList<TKey1, TKey2, TValue> methodInnerFor, bool leftToRight = true)
        {
            try
            {
                llistaClau1.For((i, tkey1, tkey2) =>
                {
                    return methodInnerFor(i, tkey1, tkey2, llista1[tkey1]);
                });
            }
            catch { throw; }

        }

        public int IndexOf(KeyValuePair<TwoKeys<TKey1, TKey2>, TValue> item)
        {
            return IndexOf(item.Key.Key1);
        }
        public int IndexOf(TKey1 key)
        {
            return llistaClau1.IndexOfKey(key);
        }
        public int IndexOf(TKey2 key)
        {
            return IndexOf(llistaClau2[key]);
        }

        public void Insert(int index, KeyValuePair<TwoKeys<TKey1, TKey2>, TValue> item)
        {
            Add(item.Key, item.Value);
            llistaClau1.ChangePosicion(llistaClau1.IndexOfKey(item.Key.Key1), index);
        }
        public void ChangePosicion(int posActual, int posNueva)
        {
            llistaClau1.ChangePosicion(posActual, posNueva);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        void ICollection<KeyValuePair<TwoKeys<TKey1, TKey2>, TValue>>.CopyTo(KeyValuePair<TwoKeys<TKey1, TKey2>, TValue>[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex);
        }



    }
    public struct TwoKeys<Tkey1, Tkey2>
    {
        Tkey1 key1;
        Tkey2 key2;
        public TwoKeys(Tkey1 key1, Tkey2 key2)
        {
            this.key1 = key1;
            this.key2 = key2;
        }
        public Tkey2 Key2
        {
            get
            {
                return key2;
            }
            set
            {
                key2 = value;
            }
        }
        public Tkey1 Key1
        {
            get
            {
                return key1;
            }
        }
    }
}
